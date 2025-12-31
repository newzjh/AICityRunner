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

        // 计算单个帧的宽高（自动扣除3像素间隔）
        int singleFrameWidth = (sourceTex.width - (ColCount - 1) * FrameSpacing) / ColCount;
        int singleFrameHeight = (sourceTex.height - (RowCount - 1) * FrameSpacing) / RowCount;
        List<Sprite> frameSprites = new List<Sprite>();

        // 分割九宫格 → 生成9个精灵帧（按【从左到右、从上到下】排序，对应跑酷动作帧序列）
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                int posX = col * (singleFrameWidth + FrameSpacing);
                int posY = (RowCount-1-row) * (singleFrameHeight + FrameSpacing);
                Rect rect = new Rect(posX+ SubFrameSpacing, posY+ SubFrameSpacing, singleFrameWidth- SubFrameSpacing*2, singleFrameHeight- SubFrameSpacing*2);
                Sprite frame = Sprite.Create(sourceTex, rect, new Vector2(0.5f, 0.5f), 100);
                frame.name = $"RunFrame_{row * ColCount + col}";
                frameSprites.Add(frame);

                // 保存分割后的单帧精灵到工程

                string spriteFolder = $"Assets/KeyFrameAnimations/Sprites/{sourceName}";
                if (!Directory.Exists(spriteFolder))
                    Directory.CreateDirectory(spriteFolder);

                string spritePath = $"Assets/KeyFrameAnimations/Sprites/{sourceName}/{frame.name}.asset";
                AssetDatabase.CreateAsset(frame, spritePath);
            }
        }

        //// ========== 生成精灵图集 SpriteAtlas ==========
        //SpriteAtlas atlas = new SpriteAtlas();
        //atlas.Add(frameSprites.ToArray());
        //// 图集设置：C4D风格清晰无模糊
        //SpriteAtlasPackingSettings packSetting = atlas.GetPackingSettings();
        //packSetting.padding = 2;
        //atlas.SetPackingSettings(packSetting);
        //// 保存精灵图集
        //string atlasPath = $"Assets/KeyFrameAnimations/Atlas/{sourceName}.spriteatlas";
        //AssetDatabase.CreateAsset(atlas, atlasPath);

        AtlaCollection atlas= new AtlaCollection();
        atlas.sprites = frameSprites.ToArray();
        string atlasPath = $"Assets/KeyFrameAnimations/Atlas/{sourceName}.asset";
        AssetDatabase.CreateAsset(atlas, atlasPath);

        // ========== 生成跑酷动画剪辑 AnimationClip ==========
        AnimationClip animClip = new AnimationClip();
        animClip.frameRate = 12; // 跑酷动画帧率，可改（8-15帧最佳）
        animClip.wrapMode = WrapMode.Loop; // 循环播放跑酷动作
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[frameSprites.Count];
        for (int i = 0; i < frameSprites.Count; i++)
        {
            keyFrames[i] = new ObjectReferenceKeyframe();
            keyFrames[i].time = (float)i / animClip.frameRate;
            keyFrames[i].value = frameSprites[i];
        }
        AnimationUtility.SetObjectReferenceCurve(animClip, curveBinding, keyFrames);

        // 保存动画剪辑
        string animPath = $"Assets/KeyFrameAnimations/Animations/{sourceName}.anim";
        AssetDatabase.CreateAsset(animClip, animPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", "跑酷精灵图集+分割帧+循环动画 全部生成完成！", "确定");
    }
}