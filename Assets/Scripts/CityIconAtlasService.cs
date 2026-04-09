using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[Serializable]
public class CityIconAtlasManifestEntry
{
    public string label;
    public int index;
    public string itemType;
}

[Serializable]
public class CityIconAtlasManifest
{
    public string cityName;
    public string atlasKey;
    public int columns;
    public int rows;
    public CityIconAtlasManifestEntry[] entries;
}

public class CityIconAtlasBuildResult
{
    public Texture2D Texture;
    public CityIconAtlasManifest Manifest;
}

public static class CityIconAtlasService
{
    public const string ResourceFolder = "CityIconAtlases";
    private const int DefaultColumns = 6;

    private static readonly Dictionary<string, Texture2D> AtlasTextureCache = new Dictionary<string, Texture2D>();
    private static readonly Dictionary<string, CityIconAtlasManifest> AtlasManifestCache = new Dictionary<string, CityIconAtlasManifest>();
    private static readonly Dictionary<string, Dictionary<string, Sprite>> AtlasSpriteCache = new Dictionary<string, Dictionary<string, Sprite>>();
    private static readonly Dictionary<string, Dictionary<string, int>> EntryIndexCache = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

    public static async UniTask PreloadCityAtlasAsync(CityRuntimeProfile profile, bool allowRemoteGeneration)
    {
        if (profile == null)
        {
            return;
        }

        string atlasKey = GetAtlasKey(profile.CityName);
        if (AtlasSpriteCache.ContainsKey(atlasKey))
        {
            return;
        }

        if (TryLoadOfflineAtlas(profile.CityName))
        {
            return;
        }

        if (!allowRemoteGeneration)
        {
            return;
        }

        CityIconAtlasBuildResult buildResult = await GenerateCityAtlasAsync(profile);
        if (buildResult != null)
        {
            RegisterAtlas(buildResult.Texture, buildResult.Manifest);
        }
    }

    public static bool TryGetSprite(string cityName, string label, out Sprite sprite)
    {
        string atlasKey = GetAtlasKey(cityName);

        Global global = GameObject.FindFirstObjectByType<Global>();
        if (global != null && global.CityItemIcons != null)
        {
            for (int i = 0; i < global.CityItemIcons.Length; i++)
            {
                AtlaCollection atlas = global.CityItemIcons[i];
                if (atlas == null)
                {
                    continue;
                }

                string key = atlas.name;
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                if (string.Equals(key, atlasKey, StringComparison.OrdinalIgnoreCase) && atlas.sprites != null && atlas.sprites.Length > 0)
                {
                    if (TryResolveEntryIndex(cityName, atlasKey, label, out int index) && index >= 0 && index < atlas.sprites.Length)
                    {
                        Sprite s = atlas.sprites[index];
                        if (s != null)
                        {
                            sprite = s;
                            return true;
                        }
                    }
                    break;
                }
            }
        }

        if (!AtlasSpriteCache.TryGetValue(atlasKey, out Dictionary<string, Sprite> citySprites))
        {
            if (!TryLoadOfflineAtlas(cityName))
            {
                sprite = null;
                return false;
            }

            citySprites = AtlasSpriteCache[atlasKey];
        }

        return citySprites.TryGetValue(label, out sprite);
    }

    private static bool TryResolveEntryIndex(string cityName, string atlasKey, string label, out int index)
    {
        index = -1;
        if (string.IsNullOrWhiteSpace(atlasKey) || string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        if (EntryIndexCache.TryGetValue(atlasKey, out Dictionary<string, int> cached) && cached != null && cached.TryGetValue(label, out index))
        {
            return true;
        }

        Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.Ordinal);

        if (AtlasManifestCache.TryGetValue(atlasKey, out CityIconAtlasManifest manifest) && manifest != null && manifest.entries != null && manifest.entries.Length > 0)
        {
            for (int i = 0; i < manifest.entries.Length; i++)
            {
                CityIconAtlasManifestEntry entry = manifest.entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.label))
                {
                    continue;
                }

                map[entry.label] = entry.index;
            }
        }
        else
        {
            CityRuntimeProfile profile = CityRuntimeContent.ResolveProfile(cityName);
            List<CityIconAtlasManifestEntry> entries = BuildEntries(profile);
            for (int i = 0; i < entries.Count; i++)
            {
                CityIconAtlasManifestEntry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.label))
                {
                    continue;
                }

                map[entry.label] = entry.index;
            }
        }

        EntryIndexCache[atlasKey] = map;
        return map.TryGetValue(label, out index);
    }

    public static bool TryLoadOfflineAtlas(string cityName)
    {
        string atlasKey = GetAtlasKey(cityName);
        if (AtlasSpriteCache.ContainsKey(atlasKey))
        {
            return true;
        }

        Texture2D atlasTexture = Resources.Load<Texture2D>(ResourceFolder + "/" + atlasKey + "_atlas");
        TextAsset manifestAsset = Resources.Load<TextAsset>(ResourceFolder + "/" + atlasKey + "_manifest");
        if (atlasTexture == null || manifestAsset == null)
        {
            return false;
        }

        CityIconAtlasManifest manifest = JsonUtility.FromJson<CityIconAtlasManifest>(manifestAsset.text);
        if (manifest == null || manifest.entries == null || manifest.entries.Length == 0)
        {
            return false;
        }

        RegisterAtlas(atlasTexture, manifest);
        return true;
    }

    public static async UniTask<CityIconAtlasBuildResult> GenerateCityAtlasAsync(CityRuntimeProfile profile)
    {
        if (profile == null)
        {
            return null;
        }

        List<CityIconAtlasManifestEntry> entries = BuildEntries(profile);
        if (entries.Count == 0)
        {
            return null;
        }

        int columns = DefaultColumns;
        int rows = Mathf.CeilToInt(entries.Count / (float)columns);
        string prompt = BuildPrompt(profile, entries, columns, rows);
        Texture2D texture = await TextToImage2.SendStreamRequestCommon(prompt);
        if (texture == null)
        {
            return null;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        CityIconAtlasManifest manifest = new CityIconAtlasManifest
        {
            cityName = profile.CityName,
            atlasKey = GetAtlasKey(profile.CityName),
            columns = columns,
            rows = rows,
            entries = entries.ToArray()
        };

        return new CityIconAtlasBuildResult
        {
            Texture = texture,
            Manifest = manifest
        };
    }

    public static List<CityIconAtlasManifestEntry> BuildEntries(CityRuntimeProfile profile)
    {
        List<CityIconAtlasManifestEntry> entries = new List<CityIconAtlasManifestEntry>();
        HashSet<string> unique = new HashSet<string>();

        AddEntries(entries, unique, profile.ScoreItems, DynamicStreetItemType.ScorePickup);
        AddEntries(entries, unique, profile.ObstacleItems, DynamicStreetItemType.Obstacle);
        AddEntries(entries, unique, profile.LifeItems, DynamicStreetItemType.LifePickup);
        AddEntries(entries, unique, profile.SpeedItems, DynamicStreetItemType.SpeedPickup);
        AddEntries(entries, unique, profile.CheckInItems, DynamicStreetItemType.CheckInPickup);
        AddEntries(entries, unique, profile.FoodStalls, DynamicStreetItemType.FoodStall);
        AddEntries(entries, unique, profile.ArcadeSigns, DynamicStreetItemType.ArcadeSign);
        AddEntries(entries, unique, profile.SharedBikeSpots, DynamicStreetItemType.SharedBikeSpot);
        AddEntries(entries, unique, profile.FlowerMarkets, DynamicStreetItemType.FlowerMarket);
        AddEntries(entries, unique, profile.TransitStops, DynamicStreetItemType.TransitStop);

        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].index = i;
        }

        return entries;
    }

    public static string GetAtlasKey(string cityName)
    {
        switch (cityName)
        {
            case "广州": return "guangzhou";
            case "深圳": return "shenzhen";
            case "佛山": return "foshan";
            case "东莞": return "dongguan";
            case "珠海": return "zhuhai";
            case "中山": return "zhongshan";
            case "惠州": return "huizhou";
            case "江门": return "jiangmen";
            case "肇庆": return "zhaoqing";
            case "香港": return "hongkong";
            case "澳门": return "macau";
            default: return "guangzhou";
        }
    }

    private static void RegisterAtlas(Texture2D atlasTexture, CityIconAtlasManifest manifest)
    {
        if (atlasTexture == null || manifest == null || manifest.entries == null)
        {
            return;
        }

        atlasTexture.wrapMode = TextureWrapMode.Clamp;
        atlasTexture.filterMode = FilterMode.Bilinear;

        AtlasTextureCache[manifest.atlasKey] = atlasTexture;
        AtlasManifestCache[manifest.atlasKey] = manifest;
        AtlasSpriteCache[manifest.atlasKey] = SliceSprites(atlasTexture, manifest);
    }

    private static Dictionary<string, Sprite> SliceSprites(Texture2D atlasTexture, CityIconAtlasManifest manifest)
    {
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        float cellWidth = atlasTexture.width / (float)manifest.columns;
        float cellHeight = atlasTexture.height / (float)manifest.rows;
        float padX = cellWidth * 0.08f;
        float padY = cellHeight * 0.08f;

        foreach (CityIconAtlasManifestEntry entry in manifest.entries)
        {
            int column = entry.index % manifest.columns;
            int row = entry.index / manifest.columns;
            float x = column * cellWidth + padX;
            float y = atlasTexture.height - (row + 1) * cellHeight + padY;
            float width = Mathf.Max(8f, cellWidth - padX * 2f);
            float height = Mathf.Max(8f, cellHeight - padY * 2f);
            Rect rect = new Rect(x, y, width, height);
            Sprite sprite = Sprite.Create(atlasTexture, rect, new Vector2(0.5f, 0.5f), 100f);
            sprite.name = manifest.atlasKey + "_" + entry.label;
            sprites[entry.label] = sprite;
        }

        return sprites;
    }

    private static string BuildPrompt(CityRuntimeProfile profile, List<CityIconAtlasManifestEntry> entries, int columns, int rows)
    {
        List<string> lines = new List<string>();
        for (int i = 0; i < entries.Count; i++)
        {
            CityIconAtlasManifestEntry entry = entries[i];
            lines.Add((i + 1) + "." + entry.label);
        }

        string entryText = string.Join("；", lines.ToArray());
        return "为" + profile.CityName + "生成一张2D游戏道具图集，" +
               "整张图是规则网格，" + columns + "列" + rows + "行，从左到右从上到下按顺序放置图标。" +
               "每个格子只放一个完整图标，主体居中，留白一致，黑色背景方便后续切图。" +
               "图标风格统一，像素卡通文旅道具风格，适合横版跑酷游戏，图标上不要文字，总共"+ entries.Count + "个图标，不多不少。图标顺序如下：" + entryText;
    }

    private static void AddEntries(List<CityIconAtlasManifestEntry> entries, HashSet<string> unique, string[] labels, DynamicStreetItemType itemType)
    {
        if (labels == null)
        {
            return;
        }

        for (int i = 0; i < labels.Length; i++)
        {
            string label = labels[i];
            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            string uniqueKey = itemType + "|" + label;
            if (!unique.Add(uniqueKey))
            {
                continue;
            }

            entries.Add(new CityIconAtlasManifestEntry
            {
                label = label,
                itemType = itemType.ToString()
            });
        }
    }
}
