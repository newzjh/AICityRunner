using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class DynamicStreetSpriteFactory
{
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
    private static readonly Dictionary<string, Sprite> RemoteSpriteCache = new Dictionary<string, Sprite>();

    public static Sprite GetSprite(DynamicStreetItemType itemType, string label, Color primaryColor, Color accentColor, bool decorative)
    {
        int width = decorative ? 160 : 128;
        int height = decorative ? 128 : 128;
        string cacheKey = itemType + "|" + label + "|" + ColorUtility.ToHtmlStringRGBA(primaryColor) + "|" + ColorUtility.ToHtmlStringRGBA(accentColor) + "|" + decorative;

        if (SpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        Texture2D texture = BuildTexture(width, height, itemType, label, primaryColor, accentColor, decorative);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = cacheKey;
        SpriteCache[cacheKey] = sprite;
        return sprite;
    }

    public static async UniTask<Sprite> TryGetEnhancedSpriteAsync(DynamicStreetItemType itemType, string label, Color primaryColor, Color accentColor, bool decorative)
    {
        if (CityIconAtlasService.TryGetSprite(Global.CurrentCity, label, out Sprite atlasSprite))
        {
            return atlasSprite;
        }

        string cacheKey = "remote|" + itemType + "|" + label + "|" + ColorUtility.ToHtmlStringRGBA(primaryColor) + "|" + ColorUtility.ToHtmlStringRGBA(accentColor) + "|" + decorative;
        if (RemoteSpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        Sprite webSprite = await TryBuildEmojiWebSpriteAsync(itemType, label, primaryColor, accentColor, decorative);
        if (webSprite != null)
        {
            RemoteSpriteCache[cacheKey] = webSprite;
            return webSprite;
        }

        return null;
    }

    private static Texture2D BuildTexture(int width, int height, DynamicStreetItemType itemType, string label, Color primaryColor, Color accentColor, bool decorative)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clear;
        }

        texture.SetPixels(pixels);

        int radius = decorative ? 14 : 16;
        DrawRoundedRect(texture, 10, 10, width - 20, height - 20, radius, new Color(0f, 0f, 0f, 0.28f));
        DrawRoundedRect(texture, 6, 14, width - 12, height - 24, radius, primaryColor);
        DrawRoundedRectOutline(texture, 6, 14, width - 12, height - 24, radius, accentColor, 4);

        int stripeHeight = decorative ? 22 : 18;
        int stripeColorOffset = Mathf.Abs((label ?? string.Empty).GetHashCode()) % 24;
        Color stripeColor = Color.Lerp(accentColor, Color.white, 0.18f + stripeColorOffset / 120f);
        DrawRoundedRect(texture, 14, height - stripeHeight - 18, width - 28, stripeHeight, stripeHeight / 2, stripeColor);

        switch (itemType)
        {
            case DynamicStreetItemType.ScorePickup:
                DrawCoin(texture, width / 2, height / 2 + 6, decorative ? 34 : 28, accentColor, Color.white);
                DrawSparkle(texture, width / 2 + 18, height / 2 + 24, 7, Color.white);
                break;
            case DynamicStreetItemType.Obstacle:
                DrawBarrier(texture, width / 2, height / 2 + 6, decorative ? 62 : 52, new Color(0.93f, 0.45f, 0.12f), Color.white);
                break;
            case DynamicStreetItemType.LifePickup:
                DrawHeart(texture, width / 2, height / 2 + 8, decorative ? 28 : 24, accentColor, Color.white);
                DrawCross(texture, width / 2, height / 2 + 8, decorative ? 22 : 18, Color.white, 6);
                break;
            case DynamicStreetItemType.SpeedPickup:
                DrawBolt(texture, width / 2, height / 2 + 6, decorative ? 34 : 28, accentColor);
                DrawMotionLine(texture, width / 2 - 34, height / 2 + 2, 18, 5, Color.white);
                DrawMotionLine(texture, width / 2 - 34, height / 2 - 12, 26, 5, Color.white);
                break;
            case DynamicStreetItemType.CheckInPickup:
                DrawPin(texture, width / 2, height / 2 + 10, decorative ? 30 : 24, accentColor, Color.white);
                break;
            case DynamicStreetItemType.FoodStall:
                DrawShop(texture, width / 2, height / 2 + 4, decorative ? 72 : 56, accentColor, Color.white);
                break;
            case DynamicStreetItemType.ArcadeSign:
                DrawSign(texture, width / 2, height / 2 + 6, decorative ? 60 : 46, accentColor, Color.white);
                break;
            case DynamicStreetItemType.SharedBikeSpot:
                DrawBike(texture, width / 2, height / 2 + 2, decorative ? 60 : 46, accentColor, Color.white);
                break;
            case DynamicStreetItemType.FlowerMarket:
                DrawFlower(texture, width / 2, height / 2 + 6, decorative ? 30 : 24, accentColor, Color.white);
                break;
            case DynamicStreetItemType.TransitStop:
                DrawBus(texture, width / 2, height / 2 + 6, decorative ? 62 : 48, accentColor, Color.white);
                break;
        }

        texture.Apply();
        return texture;
    }

    private static async UniTask<Sprite> TryBuildEmojiWebSpriteAsync(DynamicStreetItemType itemType, string label, Color primaryColor, Color accentColor, bool decorative)
    {
        string emojiCode = GetEmojiCode(itemType);
        if (string.IsNullOrEmpty(emojiCode))
        {
            return null;
        }

        string url = "https://raw.githubusercontent.com/hfg-gmuend/openmoji/master/color/618x618/" + emojiCode + ".png";
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                return null;
            }

            Texture2D downloaded = DownloadHandlerTexture.GetContent(request);
            if (downloaded == null)
            {
                return null;
            }

            int width = decorative ? 160 : 128;
            int height = decorative ? 128 : 128;
            Texture2D composed = BuildTexture(width, height, itemType, label, primaryColor, accentColor, decorative);
            OverlayTexture(composed, downloaded, decorative ? 68 : 58, decorative ? 58 : 56, decorative ? 70 : 66);
            composed.Apply();

            Sprite sprite = Sprite.Create(composed, new Rect(0, 0, composed.width, composed.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "emoji|" + emojiCode + "|" + itemType;
            return sprite;
        }
    }

    private static async UniTask<Sprite> TryBuildAISpriteAsync(DynamicStreetItemType itemType, string label, Color primaryColor, Color accentColor, bool decorative)
    {
        if (!Global.runtimegenerationIcon || decorative)
        {
            return null;
        }

        string prompt = BuildAIPrompt(itemType, label);
        if (string.IsNullOrEmpty(prompt))
        {
            return null;
        }

        Texture2D aiTexture = await TextToImage2.SendStreamRequestCommon(prompt);
        if (aiTexture == null)
        {
            return null;
        }

        int width = decorative ? 160 : 128;
        int height = decorative ? 128 : 128;
        Texture2D composed = BuildTexture(width, height, itemType, label, primaryColor, accentColor, decorative);
        Texture2D prepared = PrepareDownloadedTexture(aiTexture);
        OverlayTexture(composed, prepared, decorative ? 68 : 58, decorative ? 58 : 56, decorative ? 70 : 66);
        composed.Apply();

        Sprite sprite = Sprite.Create(composed, new Rect(0, 0, composed.width, composed.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = "ai|" + itemType + "|" + label;
        return sprite;
    }

    private static Texture2D PrepareDownloadedTexture(Texture2D source)
    {
        Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.SetPixels(source.GetPixels());
        Color[] pixels = readable.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            if (c.r > 0.94f && c.g > 0.94f && c.b > 0.94f)
            {
                c.a = 0f;
            }
            pixels[i] = c;
        }
        readable.SetPixels(pixels);
        readable.Apply();
        return readable;
    }

    private static void OverlayTexture(Texture2D target, Texture2D source, int centerX, int centerY, int maxSize)
    {
        if (target == null || source == null)
        {
            return;
        }

        int drawWidth;
        int drawHeight;
        if (source.width >= source.height)
        {
            drawWidth = maxSize;
            drawHeight = Mathf.Max(1, Mathf.RoundToInt((float)source.height / source.width * maxSize));
        }
        else
        {
            drawHeight = maxSize;
            drawWidth = Mathf.Max(1, Mathf.RoundToInt((float)source.width / source.height * maxSize));
        }

        int startX = centerX - drawWidth / 2;
        int startY = centerY - drawHeight / 2;
        for (int y = 0; y < drawHeight; y++)
        {
            for (int x = 0; x < drawWidth; x++)
            {
                float u = drawWidth <= 1 ? 0f : (float)x / (drawWidth - 1);
                float v = drawHeight <= 1 ? 0f : (float)y / (drawHeight - 1);
                Color src = source.GetPixelBilinear(u, v);
                if (src.a <= 0.02f)
                {
                    continue;
                }

                int px = startX + x;
                int py = startY + y;
                if (!InBounds(target, px, py))
                {
                    continue;
                }

                Color dst = target.GetPixel(px, py);
                Color blended = Color.Lerp(dst, src, src.a);
                blended.a = Mathf.Max(dst.a, src.a);
                target.SetPixel(px, py, blended);
            }
        }
    }

    private static string GetEmojiCode(DynamicStreetItemType itemType)
    {
        switch (itemType)
        {
            case DynamicStreetItemType.ScorePickup:
                return "1F4B0";
            case DynamicStreetItemType.Obstacle:
                return "1F6A7";
            case DynamicStreetItemType.LifePickup:
                return "2764";
            case DynamicStreetItemType.SpeedPickup:
                return "26A1";
            case DynamicStreetItemType.CheckInPickup:
                return "1F4CD";
            case DynamicStreetItemType.FoodStall:
                return "1F3EA";
            case DynamicStreetItemType.ArcadeSign:
                return "1F3F7";
            case DynamicStreetItemType.SharedBikeSpot:
                return "1F6B2";
            case DynamicStreetItemType.FlowerMarket:
                return "1F33C";
            case DynamicStreetItemType.TransitStop:
                return "1F68F";
            default:
                return null;
        }
    }

    private static string BuildAIPrompt(DynamicStreetItemType itemType, string label)
    {
        string descriptor = label;
        switch (itemType)
        {
            case DynamicStreetItemType.ScorePickup:
                descriptor += "，做成单个游戏积分道具徽章图标";
                break;
            case DynamicStreetItemType.Obstacle:
                descriptor += "，做成单个跑酷障碍物图标";
                break;
            case DynamicStreetItemType.LifePickup:
                descriptor += "，做成单个回血补给道具图标";
                break;
            case DynamicStreetItemType.SpeedPickup:
                descriptor += "，做成单个加速道具图标";
                break;
            case DynamicStreetItemType.CheckInPickup:
                descriptor += "，做成单个文旅打卡纪念道具图标";
                break;
            default:
                descriptor += "，做成单个街景元素图标";
                break;
        }

        return "生成一个" + descriptor + "，正视角，居中构图，适合2D横版跑酷游戏使用，图标清晰，主体完整，纯净浅色背景，边缘清楚，C4D卡通道具风格。";
    }

    private static void DrawCoin(Texture2D texture, int cx, int cy, int radius, Color mainColor, Color innerColor)
    {
        DrawCircle(texture, cx, cy, radius, mainColor);
        DrawCircle(texture, cx, cy, radius - 6, innerColor);
        DrawCircle(texture, cx, cy, radius - 11, mainColor);
    }

    private static void DrawBarrier(Texture2D texture, int cx, int cy, int width, Color stripeColor, Color accentColor)
    {
        int height = width / 2;
        DrawRect(texture, cx - width / 2, cy - height / 2, width, height, accentColor);
        for (int i = -2; i <= 2; i++)
        {
            DrawDiagonalStripe(texture, cx - width / 2 + i * 18, cy - height / 2, width / 2, height, stripeColor, 8);
        }
        DrawRect(texture, cx - width / 2 + 8, cy - height / 2 + 8, width - 16, height - 16, new Color(0.2f, 0.2f, 0.2f, 0.18f));
    }

    private static void DrawHeart(Texture2D texture, int cx, int cy, int size, Color mainColor, Color accentColor)
    {
        DrawCircle(texture, cx - size / 2, cy + size / 4, size / 2, mainColor);
        DrawCircle(texture, cx + size / 2, cy + size / 4, size / 2, mainColor);
        DrawTriangle(texture, new Vector2(cx - size, cy + size / 6), new Vector2(cx + size, cy + size / 6), new Vector2(cx, cy - size), mainColor);
        DrawHeartOutline(texture, cx, cy, size + 3, accentColor);
    }

    private static void DrawPin(Texture2D texture, int cx, int cy, int radius, Color mainColor, Color accentColor)
    {
        DrawCircle(texture, cx, cy + radius / 3, radius, mainColor);
        DrawCircle(texture, cx, cy + radius / 3, radius / 2, accentColor);
        DrawTriangle(texture, new Vector2(cx - radius / 2, cy), new Vector2(cx + radius / 2, cy), new Vector2(cx, cy - radius - 10), mainColor);
    }

    private static void DrawBolt(Texture2D texture, int cx, int cy, int size, Color color)
    {
        Vector2[] polygon =
        {
            new Vector2(cx - size / 4, cy + size),
            new Vector2(cx + size / 8, cy + size / 8),
            new Vector2(cx - size / 10, cy + size / 8),
            new Vector2(cx + size / 4, cy - size),
            new Vector2(cx, cy - size / 4),
            new Vector2(cx + size / 10, cy - size / 4)
        };

        DrawPolygon(texture, polygon, color);
    }

    private static void DrawShop(Texture2D texture, int cx, int cy, int size, Color accentColor, Color foreground)
    {
        int width = size;
        int height = Mathf.RoundToInt(size * 0.56f);
        DrawRect(texture, cx - width / 2, cy - height / 2, width, height, foreground);
        DrawRect(texture, cx - width / 2, cy + height / 2 - 12, width, 12, accentColor);
        for (int i = 0; i < 4; i++)
        {
            DrawRect(texture, cx - width / 2 + i * (width / 4), cy + height / 2 - 12, width / 8, 12, Color.white);
        }
        DrawRect(texture, cx - 10, cy - height / 2, 20, height / 2, accentColor);
        DrawCircle(texture, cx - width / 3, cy - 2, 8, accentColor);
        DrawCircle(texture, cx + width / 3, cy - 2, 8, accentColor);
    }

    private static void DrawSign(Texture2D texture, int cx, int cy, int size, Color accentColor, Color foreground)
    {
        DrawRect(texture, cx - size / 2, cy - size / 4, size, size / 2, foreground);
        DrawRect(texture, cx - 5, cy - size / 2, 10, size / 2, accentColor);
        DrawRect(texture, cx - size / 2 + 8, cy + 4, size - 16, 10, accentColor);
        DrawRect(texture, cx - size / 2 + 8, cy - 14, size / 2, 8, accentColor);
    }

    private static void DrawBike(Texture2D texture, int cx, int cy, int size, Color accentColor, Color foreground)
    {
        int wheelRadius = size / 5;
        DrawCircleOutline(texture, cx - size / 3, cy - size / 4, wheelRadius, accentColor, 4);
        DrawCircleOutline(texture, cx + size / 3, cy - size / 4, wheelRadius, accentColor, 4);
        DrawLine(texture, cx - size / 3, cy - size / 4, cx - 4, cy + 8, foreground, 4);
        DrawLine(texture, cx - 4, cy + 8, cx + size / 5, cy - size / 4, foreground, 4);
        DrawLine(texture, cx + size / 5, cy - size / 4, cx + size / 3, cy - size / 4, foreground, 4);
        DrawLine(texture, cx - 4, cy + 8, cx + 12, cy + 18, foreground, 4);
        DrawLine(texture, cx + 12, cy + 18, cx + size / 4, cy + 18, foreground, 4);
        DrawLine(texture, cx - 10, cy + 18, cx - 20, cy + 24, foreground, 4);
    }

    private static void DrawFlower(Texture2D texture, int cx, int cy, int radius, Color accentColor, Color foreground)
    {
        DrawCircle(texture, cx, cy, radius / 2, foreground);
        DrawCircle(texture, cx - radius, cy, radius / 2, accentColor);
        DrawCircle(texture, cx + radius, cy, radius / 2, accentColor);
        DrawCircle(texture, cx, cy + radius, radius / 2, accentColor);
        DrawCircle(texture, cx, cy - radius, radius / 2, accentColor);
        DrawRect(texture, cx - 3, cy - radius - 18, 6, 18, foreground);
    }

    private static void DrawBus(Texture2D texture, int cx, int cy, int size, Color accentColor, Color foreground)
    {
        int width = size;
        int height = Mathf.RoundToInt(size * 0.46f);
        DrawRoundedRect(texture, cx - width / 2, cy - height / 2, width, height, 10, foreground);
        DrawRoundedRect(texture, cx - width / 2 + 8, cy, width - 16, height / 3, 6, accentColor);
        DrawRect(texture, cx - width / 3, cy - height / 2 - 6, width / 6, 8, accentColor);
        DrawRect(texture, cx + width / 6, cy - height / 2 - 6, width / 6, 8, accentColor);
        DrawCircle(texture, cx - width / 4, cy - height / 2, 8, accentColor);
        DrawCircle(texture, cx + width / 4, cy - height / 2, 8, accentColor);
    }

    private static void DrawSparkle(Texture2D texture, int cx, int cy, int size, Color color)
    {
        DrawRect(texture, cx - 1, cy - size, 3, size * 2, color);
        DrawRect(texture, cx - size, cy - 1, size * 2, 3, color);
        DrawLine(texture, cx - size, cy - size, cx + size, cy + size, color, 2);
        DrawLine(texture, cx - size, cy + size, cx + size, cy - size, color, 2);
    }

    private static void DrawCross(Texture2D texture, int cx, int cy, int size, Color color, int thickness)
    {
        DrawRect(texture, cx - thickness / 2, cy - size / 2, thickness, size, color);
        DrawRect(texture, cx - size / 2, cy - thickness / 2, size, thickness, color);
    }

    private static void DrawMotionLine(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        DrawRoundedRect(texture, x, y, width, height, height / 2, color);
    }

    private static void DrawDiagonalStripe(Texture2D texture, int startX, int startY, int width, int height, Color color, int thickness)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int px = startX + x + y;
                int py = startY + y;
                if (Mathf.Abs(x - width / 4) <= thickness && InBounds(texture, px, py))
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private static void DrawHeartOutline(Texture2D texture, int cx, int cy, int size, Color color)
    {
        for (int angle = 0; angle < 360; angle += 2)
        {
            float t = angle * Mathf.Deg2Rad;
            float x = 16f * Mathf.Pow(Mathf.Sin(t), 3f);
            float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);
            int px = Mathf.RoundToInt(cx + x * size / 34f);
            int py = Mathf.RoundToInt(cy + y * size / 34f);
            if (InBounds(texture, px, py))
            {
                texture.SetPixel(px, py, color);
            }
        }
    }

    private static void DrawPolygon(Texture2D texture, Vector2[] points, Color color)
    {
        int minX = Mathf.RoundToInt(points[0].x);
        int maxX = minX;
        int minY = Mathf.RoundToInt(points[0].y);
        int maxY = minY;

        for (int i = 1; i < points.Length; i++)
        {
            minX = Mathf.Min(minX, Mathf.RoundToInt(points[i].x));
            maxX = Mathf.Max(maxX, Mathf.RoundToInt(points[i].x));
            minY = Mathf.Min(minY, Mathf.RoundToInt(points[i].y));
            maxY = Mathf.Max(maxY, Mathf.RoundToInt(points[i].y));
        }

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (IsPointInPolygon(points, x, y))
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static bool IsPointInPolygon(Vector2[] polygon, int x, int y)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            bool intersect = ((polygon[i].y > y) != (polygon[j].y > y)) &&
                             (x < (polygon[j].x - polygon[i].x) * (y - polygon[i].y) / (polygon[j].y - polygon[i].y + 0.001f) + polygon[i].x);
            if (intersect)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static void DrawTriangle(Texture2D texture, Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        DrawPolygon(texture, new[] { a, b, c }, color);
    }

    private static void DrawRoundedRect(Texture2D texture, int x, int y, int width, int height, int radius, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                if (!InBounds(texture, px, py))
                {
                    continue;
                }

                bool insideHorizontal = px >= x + radius && px < x + width - radius;
                bool insideVertical = py >= y + radius && py < y + height - radius;
                if (insideHorizontal || insideVertical)
                {
                    texture.SetPixel(px, py, color);
                    continue;
                }

                int cornerX = px < x + radius ? x + radius : x + width - radius - 1;
                int cornerY = py < y + radius ? y + radius : y + height - radius - 1;
                if ((px - cornerX) * (px - cornerX) + (py - cornerY) * (py - cornerY) <= radius * radius)
                {
                    texture.SetPixel(px, py, color);
                }
            }
        }
    }

    private static void DrawRoundedRectOutline(Texture2D texture, int x, int y, int width, int height, int radius, Color color, int thickness)
    {
        DrawRoundedRect(texture, x, y, width, height, radius, color);
        DrawRoundedRect(texture, x + thickness, y + thickness, width - thickness * 2, height - thickness * 2, Mathf.Max(1, radius - thickness), new Color(0f, 0f, 0f, 0f));
    }

    private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        int sqrRadius = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= sqrRadius)
                {
                    SetPixelSafe(texture, cx + x, cy + y, color);
                }
            }
        }
    }

    private static void DrawCircleOutline(Texture2D texture, int cx, int cy, int radius, Color color, int thickness)
    {
        int outer = radius * radius;
        int innerRadius = Mathf.Max(1, radius - thickness);
        int inner = innerRadius * innerRadius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int dist = x * x + y * y;
                if (dist <= outer && dist >= inner)
                {
                    SetPixelSafe(texture, cx + x, cy + y, color);
                }
            }
        }
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                SetPixelSafe(texture, px, py, color);
            }
        }
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircle(texture, x0, y0, Mathf.Max(1, thickness / 2), color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private static void SetPixelSafe(Texture2D texture, int x, int y, Color color)
    {
        if (InBounds(texture, x, y))
        {
            texture.SetPixel(x, y, color);
        }
    }

    private static bool InBounds(Texture2D texture, int x, int y)
    {
        return x >= 0 && x < texture.width && y >= 0 && y < texture.height;
    }
}
