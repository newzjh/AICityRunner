using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CityIconAtlasEditorWindow : EditorWindow
{
    private bool _isDownloading;
    private Vector2 _scroll;
    private string _log = string.Empty;

    [MenuItem("Tools/城市图标图集下载器")]
    public static void Open()
    {
        GetWindow<CityIconAtlasEditorWindow>("城市图标图集");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("珠三角城市图标图集", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("点击按钮后，会为珠三角城市逐个请求一张整图图集，并保存到 Assets/Resources/CityIconAtlases。运行时可直接离线加载。", MessageType.Info);

        GUI.enabled = !_isDownloading;
        if (GUILayout.Button("下载全部珠三角城市图标图集", GUILayout.Height(40)))
        {
            DownloadAllAtlases().Forget();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private async UniTaskVoid DownloadAllAtlases()
    {
        _isDownloading = true;
        _log = string.Empty;

        try
        {
            var profiles = CityRuntimeContent.GetAllProfiles();
            for (int i = 0; i < profiles.Count; i++)
            {
                CityRuntimeProfile profile = profiles[i];
                string title = "下载城市图标图集";
                string info = "正在处理 " + profile.CityName + " (" + (i + 1) + "/" + profiles.Count + ")";
                EditorUtility.DisplayProgressBar(title, info, (i + 1f) / profiles.Count);

                CityIconAtlasBuildResult buildResult = await CityIconAtlasService.GenerateCityAtlasAsync(profile);
                if (buildResult == null || buildResult.Texture == null || buildResult.Manifest == null)
                {
                    AppendLog("失败: " + profile.CityName);
                    continue;
                }

                SaveBuildResult(buildResult);
                AppendLog("完成: " + profile.CityName);
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            _isDownloading = false;
        }
    }

    private void SaveBuildResult(CityIconAtlasBuildResult buildResult)
    {
        string folder = "Assets/Resources/CityIconAtlases";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string atlasPath = folder + "/" + buildResult.Manifest.atlasKey + "_atlas.png";
        string manifestPath = folder + "/" + buildResult.Manifest.atlasKey + "_manifest.json";
        File.WriteAllBytes(atlasPath, buildResult.Texture.EncodeToPNG());
        File.WriteAllText(manifestPath, JsonUtility.ToJson(buildResult.Manifest, true));

        AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private void AppendLog(string line)
    {
        _log += line + "\n";
        Repaint();
    }
}
