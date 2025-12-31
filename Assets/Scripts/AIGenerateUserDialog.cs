using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

public class AIGenerateUserDialog : MonoBehaviour
{
    public InputField inputfield;
    public GameObject progressUI;

    private int RowCount = 5;
    private int ColCount = 5;
    private int FrameSpacing = 25; // 动作单元3像素间隔
    private int SubFrameSpacing = 20; // 动作单元3像素间隔

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Application.isPlaying)
            tex = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnable()
    {
        if (Application.isPlaying)
            tex = null;
    }

    public Texture2D tex = null;

    public async void OnGenerate()
    {
        if (progressUI)
            progressUI.gameObject.SetActive(true);

        string prompt = $"C4D风格的16帧跑步动画序列网格图，宽度和高度分别是1024像素和2048像素，4x4的网格布局，每个格子占512*1024像素，每个格子内是一个跑步动作的关键帧，帧与帧之间动作要连贯斜街，角色是侧面视角的游戏角色，{inputfield.text}。第1-5帧：站立起步姿势，面向右，第6-10帧：抬起左腿，向右跑动，第11-15帧：跑步中，向右跑动。所有帧的角色比例保持一致，每个格子背景为纯黑色，格子与格子之间不留空间，格子之间如果有任何其他内容需要替换为纯黑色";
        tex = await TextToImage2.SendStreamRequestCommon(prompt);

        var bytes = tex.EncodeToJPG();
        string userTexPath = Application.persistentDataPath + "/newUser.jpg";
        System.IO.File.WriteAllBytes(userTexPath, bytes);
#if UNITY_EDITOR
        Application.OpenURL("file://" + userTexPath);
#endif

        var answer = await ImageDetection2.SendStreamRequestCommon(tex, "图上的格子有多少行，多少列？仅以(行数,列数)格式返回结果，不要除此之外其他答案内容");
        var answer2 = answer.Item1.Replace("(","").Replace(")","");
        var answer3 = answer2.Split(",");

        RowCount = 5;
        ColCount = 5;
        if (answer3.Length == 2 )
        {
            int.TryParse(answer3[0], out RowCount);
            int.TryParse(answer3[1], out ColCount);
        }

        //var answer = await ImageDetection2.SendStreamRequestCommon(tex, "图上有多少个格子，以文本列表的形式返回各个格子在图中的像素范围，不要除此之外其他答案内容");

        //var grids = answer.Item1.Split("\n");
        //List<Rect> rects = new List<Rect>();
        //foreach (var grid in grids)
        //{
        //    var _grid = grid;
        //    if (_grid.Contains("."))
        //    {
        //        var gridsplit = _grid.Split(".");
        //        _grid = gridsplit[gridsplit.Length-1];
        //    };

        //    var coords = _grid.Split(",");
        //    List<int> values = new List<int>();
        //    foreach (var coord in coords)
        //    {
        //        var _coord = coord;
        //        if (_coord.Contains(":"))
        //        {
        //            var coordsplit = _coord.Split(":");
        //            _coord = coordsplit[coordsplit.Length - 1];
        //        };

        //        var ranges= _coord.Split("-");
        //        foreach (var range in ranges)
        //        {
        //            int value = -1;
        //            if (int.TryParse(range, out value))
        //            {
        //                values.Add(value);
        //            }
        //        }
        //    }
        //    Rect rc = new Rect();
        //    if (values.Count==4)
        //    {
        //        rc.x = values[0];
        //        rc.y = values[2];
        //        rc.width = values[1] - values[0];
        //        rc.height = values[3] - values[2];
        //    }
        //    rects.Add(rc);
        //}

        List<Sprite> frameSprites = new List<Sprite>();
        string sourceName = "NewUser";

        //if (rects.Count > 0)
        //{
        //    int index = 0;
        //    foreach (var rect in rects)
        //    {
        //        var _rect = rect;
        //        _rect.y = tex.height - 1 - rect.height - _rect.y;
        //        Sprite frame = Sprite.Create(tex, _rect, new Vector2(0.5f, 0.5f), 100);
        //        frame.name = $"RunFrame_{index}";
        //        frameSprites.Add(frame);
        //        index++;
        //    }
        //}
        //else
        {
            // 计算单个帧的宽高（自动扣除3像素间隔）
            int singleFrameWidth = (tex.width - (ColCount - 1) * FrameSpacing) / ColCount;
            int singleFrameHeight = (tex.height - (RowCount - 1) * FrameSpacing) / RowCount;

            // 分割九宫格 → 生成9个精灵帧（按【从左到右、从上到下】排序，对应跑酷动作帧序列）
            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColCount; col++)
                {
                    int posX = col * (singleFrameWidth + FrameSpacing);
                    int posY = (RowCount - 1 - row) * (singleFrameHeight + FrameSpacing);
                    Rect rect = new Rect(posX + SubFrameSpacing, posY + SubFrameSpacing, singleFrameWidth - SubFrameSpacing * 2, singleFrameHeight - SubFrameSpacing * 2);
                    Sprite frame = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100);
                    frame.name = $"RunFrame_{row * ColCount + col}";
                    frameSprites.Add(frame);

                    //// 保存分割后的单帧精灵到工程
                    //string spriteParentFolder = $"{Application.persistentDataPath}/Sprites";
                    //if (!Directory.Exists(spriteParentFolder))
                    //    Directory.CreateDirectory(spriteParentFolder);

                    //string spriteFolder = $"{Application.persistentDataPath}/Sprites/{sourceName}";
                    //if (!Directory.Exists(spriteFolder))
                    //    Directory.CreateDirectory(spriteFolder);

                    //string spritePath = $"{Application.persistentDataPath}/Sprites/{sourceName}/{frame.name}.asset";
                }
            }
        }


        AtlaCollection atlas = new AtlaCollection();
        atlas.sprites = frameSprites.ToArray();

        var global = GameObject.FindFirstObjectByType<Global>(FindObjectsInactive.Include);
        if (global)
        {
            global.CurrentUser = atlas;
            global.CurrentSpeed = 12;
        }

        if (progressUI)
            progressUI.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }

}
