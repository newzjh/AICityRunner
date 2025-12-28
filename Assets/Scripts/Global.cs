using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Global : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Texture2D bg;
    public static string street = "广州阅江东路";

    public Sprite[] Coins;

    public Material CoinMat;

    public Texture2D[] bgs;

    public static async UniTask BuildAISceneContent()
    {
        List<string> streets = new();
        streets.Add("广州阅江东路");
        streets.Add("广州沿江路");
        streets.Add("广州体育中心");
        streets.Add("广州珠江新城");
        streets.Add("广州白云山");
        streets.Add("深圳后海");
        streets.Add("深圳前海");
        streets.Add("深圳市民中心");
        streets.Add("香港中环");
        streets.Add("香港科学园");
        streets.Add("香港红磡");

        if (streets.Count > 0)
        {
            street = streets[UnityEngine.Random.Range(0, streets.Count - 1)];
        }

        //{
        //    var prompt = "生成" + street + "街景背景图片，立体像素，像素风，16-bitpixel art，复古，平铺展示，像素游戏风格，远景，平视角度，道路在最前方平行于图片放置且严格占比不超过图片总高度的1/4，最终图片为横向循环贴图，最右侧内容能无缝拼接最左侧内容";
        //    var tex = await TextToImage2.SendStreamRequestCommon(prompt);

        //    bg = tex;
        //}

        //{
        //    var prompt = "生成" + street + "横版游戏街景风格，水平透视视角，C4D风格，超高清，大师级建筑渲染，真实还原该城市比例与道路结构，远景为该城市山脉，中景包含几个该城市特色地标建筑，最终图片为横向循环贴图，最右侧内容能无缝拼接最左侧内容，构图整洁，建筑比例精准，色彩多彩鲜明，材质丰富，去掉拼接线，去掉文字";
        //    var tex = await TextToImage2.SendStreamRequestCommon(prompt);

        //    bg = tex;
        //}

        //{
        //    var req = Resources.LoadAsync<Texture2D>("generatebg2");
        //    await req;
        //    bg = req.asset as Texture2D;
        //}

        {
            var global = GameObject.FindFirstObjectByType<Global>();
            bg = global.bgs[UnityEngine.Random.Range(0, global.bgs.Length-1)];
        }

        {
            var req = Resources.LoadAsync<GameObject>("coins");
            await req;
            var sprites = req.asset as GameObject;
        }
    }
}
