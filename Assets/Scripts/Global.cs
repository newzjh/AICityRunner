using Cysharp.Threading.Tasks;
using UnityEngine;

public class Global : MonoBehaviour
{
    public static Texture2D bg;
    public static Texture2D bridge;
    public static string CurrentStreet = "广州";
    public static string CurrentCity = "广州";

    public Sprite[] Coins;

    public Material CoinMat;

    public Texture2D[] bgs;

    public Texture2D[] bridges;

    public AtlaCollection DefaultUser;
    public AtlaCollection CurrentUser;
    public float CurrentSpeed = 24;

    public static bool runtimegeneration = false;

    public static async UniTask BuildAISceneContent(string street)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            street = "广州";
        }

        CurrentStreet = street;
        CurrentCity = CityRuntimeContent.ResolveCityName(street);
        CityRuntimeProfile cityProfile = CityRuntimeContent.ResolveProfile(street);
        Global global = GameObject.FindFirstObjectByType<Global>();

        if (runtimegeneration)
        {
            string prompt = "生成" + street + "城市横板跑酷街景，水平侧视角，C4D风格，保留城市天际线和道路结构，包含地标、街边绿化与水岸层次，适合横向无限卷轴拼接，无明显接缝，整体色彩体现" + cityProfile.CityName + "气质。";
            var tex = await TextToImage2.SendStreamRequestCommon(prompt);
            tex.wrapMode = TextureWrapMode.Mirror;
            bg = tex;

            if (global != null && global.bridges != null && global.bridges.Length > 0)
            {
                bridge = global.bridges[UnityEngine.Random.Range(0, global.bridges.Length - 1)];
            }
        }
        else if (global != null)
        {
            if (global.bgs != null && global.bgs.Length > 0)
            {
                bg = global.bgs[UnityEngine.Random.Range(0, global.bgs.Length - 1)];
            }

            if (global.bridges != null && global.bridges.Length > 0)
            {
                bridge = global.bridges[UnityEngine.Random.Range(0, global.bridges.Length - 1)];
            }
        }

        {
            var req = Resources.LoadAsync<GameObject>("coins");
            await req;
            var sprites = req.asset as GameObject;
        }
    }
}
