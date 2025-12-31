using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

[System.Serializable]
public struct ImageURLElement
{
    public string url;
}

[System.Serializable]
public struct ImageGenerationResponse2
{
    /// <summary>
    /// 
    /// </summary>
    public string model;

    public string created;

    public List<ImageURLElement> data;
}




public class TextToImage2 : MonoBehaviour
{
    [NonSerialized]
    private static string apiKey = "ba0e3461-60b9-49d2-87e6-d6e6d95e10bd";
    [NonSerialized]
    private static string apiUrl = "https://ark.cn-beijing.volces.com/api/v3/images/generations";

    private static string encodeTexture(RenderTexture rt)
    {
        var tex = new Texture2D(rt.width, rt.height);
        var oldrt = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldrt;

        var bytes = tex.EncodeToPNG();
        var ret = Convert.ToBase64String(bytes);
        return ret;
    }


    public static async UniTask<Texture2D> SendStreamRequestCommon(string _prompt)
    {
        //var requestbody = new TextToImageRequestCommon
        //{
        //    model = "doubao-seedream-4-5-251128",
        //    prompt = _prompt,
        //    sequential_image_generation = "disabled",
        //    response_format = "b64_json",
        //    size = "2k",
        //    stream = true,
        //    watermark = true,
        //};

        var requestbody = new TextToImageRequestCommon
        {
            model = "doubao-seedream-4-5-251128",
            prompt = _prompt,
            sequential_image_generation = "disabled",
            response_format = "url",
            size = "2k",
            stream = false,
            watermark = true,
        };

        string jsonBody = JsonConvert.SerializeObject(requestbody);

        Debug.Log("prompt:" + _prompt);

        float t1 = Time.time;

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"request error: {request.error}");
                return null;
            }

            string responseJson = request.downloadHandler.text;

            ImageGenerationResponse2 response = JsonUtility.FromJson<ImageGenerationResponse2>(responseJson);

            if (response.data.Count>0)
            {
                var request2 = UnityWebRequestTexture.GetTexture(response.data[0].url);
                request2.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                await request2.SendWebRequest();

                if (request2.isDone)
                {
                    var handler = request2.downloadHandler as DownloadHandlerTexture;
                    var tex = handler.texture;

                    float t2 = Time.time;
                    Debug.Log("detect time = " + (t2 - t1) + " s");

                    request2.Dispose();
                    return tex;
                }
            }
  
        }

        {
            float t2 = Time.time;
            Debug.Log("detect time = " + (t2 - t1) + " s");
        }

        return null;
    }


}

#region request

[Serializable]
public class TextToImageRequestCommon
{
    /// <summary>
    /// 
    /// </summary>
    public string model;

    public string prompt;

    public string sequential_image_generation;

    public string response_format;

    public string size;

    public bool stream;

    public bool watermark;
}

#endregion

