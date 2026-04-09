using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.IO;

public class CreateRunAtlasEditor : Editor
{
    // 适配你的需求：九宫格 3行3列 + 每个帧间距3像素
    private const int RowCount = 5;
    private const int ColCount = 5;
    private const int FrameSpacing = 10; // 动作单元3像素间隔
    private const int SubFrameSpacing = 5; // 动作单元3像素间隔
    private const int DefaultPixelsPerUnit = 100;

    [MenuItem("Tools/一键生成跑酷精灵图集&动画(可视切图)")]
    public static void CreateRunSpriteAtlasWithSlicer()
    {
        Texture2D sourceTex = Selection.activeObject as Texture2D;
        if (sourceTex == null)
        {
            EditorUtility.DisplayDialog("提示", "请先选中你的跑酷动作图(Texture2D)！", "确定");
            return;
        }

        RunSpriteSheetSlicerWindow.Show(
            sourceTex,
            RowCount,
            ColCount,
            FrameSpacing,
            SubFrameSpacing,
            SubFrameSpacing,
            SubFrameSpacing,
            SubFrameSpacing,
            DefaultPixelsPerUnit,
            settings =>
            {
                string sourcePath = AssetDatabase.GetAssetPath(settings.Texture);
                string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
                GenerateRunAtlas(settings, sourceName);
            }
        );
    }

    [MenuItem("Tools/一键生成跑酷精灵图集&动画")]
    public static void CreateRunSpriteAtlas()
    {
        // 选中你导入的九宫格跑酷图
        Texture2D sourceTex = Selection.activeObject as Texture2D;
        if (sourceTex == null)
        {
            EditorUtility.DisplayDialog("提示", "请先选中你的九宫格跑酷动作图！", "确定");
            return;
        }

        var sourcePath = AssetDatabase.GetAssetPath(sourceTex);
        var sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        RunSpriteSheetSliceSettings settings = new RunSpriteSheetSliceSettings(
            sourceTex,
            new RectInt(0, 0, sourceTex.width, sourceTex.height),
            RowCount,
            ColCount,
            FrameSpacing,
            SubFrameSpacing,
            SubFrameSpacing,
            SubFrameSpacing,
            SubFrameSpacing,
            DefaultPixelsPerUnit
        );
        GenerateRunAtlas(settings, sourceName);
    }

    private static void GenerateRunAtlas(RunSpriteSheetSliceSettings settings, string sourceName)
    {
        Texture2D sourceTex = settings.Texture;
        if (sourceTex == null)
        {
            return;
        }

        RectInt cropRect = settings.CropRect;
        cropRect.xMin = Mathf.Clamp(cropRect.xMin, 0, sourceTex.width);
        cropRect.xMax = Mathf.Clamp(cropRect.xMax, 0, sourceTex.width);
        cropRect.yMin = Mathf.Clamp(cropRect.yMin, 0, sourceTex.height);
        cropRect.yMax = Mathf.Clamp(cropRect.yMax, 0, sourceTex.height);

        int rows = Mathf.Max(1, settings.Rows);
        int cols = Mathf.Max(1, settings.Cols);
        int frameSpacing = Mathf.Max(0, settings.FrameSpacing);
        int paddingLeft = Mathf.Max(0, settings.PaddingLeft);
        int paddingRight = Mathf.Max(0, settings.PaddingRight);
        int paddingTop = Mathf.Max(0, settings.PaddingTop);
        int paddingBottom = Mathf.Max(0, settings.PaddingBottom);
        int pixelsPerUnit = Mathf.Max(1, settings.PixelsPerUnit);

        string spriteFolder = $"Assets/KeyFrameAnimations/Sprites/{sourceName}";
        EnsureFolder(spriteFolder);
        EnsureFolder("Assets/KeyFrameAnimations/Atlas");
        EnsureFolder("Assets/KeyFrameAnimations/Animations");

        int singleFrameWidth = (cropRect.width - (cols - 1) * frameSpacing) / cols;
        int singleFrameHeight = (cropRect.height - (rows - 1) * frameSpacing) / rows;
        if (singleFrameWidth <= paddingLeft + paddingRight || singleFrameHeight <= paddingTop + paddingBottom)
        {
            EditorUtility.DisplayDialog("提示", "参数不合法：单元尺寸过小，无法应用内缩或行列/间距不匹配。", "确定");
            return;
        }

        List<Sprite> frameSprites = new List<Sprite>(rows * cols);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int cellX = cropRect.x + col * (singleFrameWidth + frameSpacing);
                int cellY = cropRect.y + (rows - 1 - row) * (singleFrameHeight + frameSpacing);
                Rect rect = new Rect(
                    cellX + paddingLeft,
                    cellY + paddingBottom,
                    singleFrameWidth - paddingLeft - paddingRight,
                    singleFrameHeight - paddingTop - paddingBottom
                );

                Sprite frame = Sprite.Create(sourceTex, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
                frame.name = $"RunFrame_{row * cols + col}";
                frameSprites.Add(frame);

                string spritePath = $"{spriteFolder}/{frame.name}.asset";
                if (AssetDatabase.LoadAssetAtPath<Sprite>(spritePath) != null)
                {
                    AssetDatabase.DeleteAsset(spritePath);
                }
                AssetDatabase.CreateAsset(frame, spritePath);
            }
        }

        string atlasPath = $"Assets/KeyFrameAnimations/Atlas/{sourceName}.asset";
        if (AssetDatabase.LoadAssetAtPath<AtlaCollection>(atlasPath) != null)
        {
            AssetDatabase.DeleteAsset(atlasPath);
        }

        AtlaCollection atlas = ScriptableObject.CreateInstance<AtlaCollection>();
        atlas.sprites = frameSprites.ToArray();
        AssetDatabase.CreateAsset(atlas, atlasPath);

        AnimationClip animClip = new AnimationClip();
        animClip.frameRate = 12;
        animClip.wrapMode = WrapMode.Loop;
        EditorCurveBinding curveBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[frameSprites.Count];
        for (int i = 0; i < frameSprites.Count; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = (float)i / animClip.frameRate,
                value = frameSprites[i]
            };
        }
        AnimationUtility.SetObjectReferenceCurve(animClip, curveBinding, keyFrames);

        string animPath = $"Assets/KeyFrameAnimations/Animations/{sourceName}.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath) != null)
        {
            AssetDatabase.DeleteAsset(animPath);
        }
        AssetDatabase.CreateAsset(animClip, animPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", "跑酷精灵图集+分割帧+循环动画 全部生成完成！", "确定");
    }

    private static void EnsureFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }
}
