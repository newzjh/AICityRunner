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

    async UniTask<ValueTuple<string, string, float>> SendStreamRequestCommon(RenderTexture rt, string question)
    {
        string base64_image = encodeTexture(rt);

        var requestbody = new ImageDetectRequestCommon
        {
            messages = new List<ImageDetectMessagesItem>
            {
                new ImageDetectMessagesItem
                {
                    content = new List<ImageDetectMessagesContentBase>
                    {
                        new ImageDetectMessagesContentImage
                        {
                            type = "image_url",
                            image_url = new ImageDetectMessagesUrl
                            {
                                url = $"data:image/png;base64,{base64_image}",
                            }
                        },
                        new ImageDetectMessagesContentText
                        {
                            type = "text",
                            text = question
                        },
                    },
                    role = "user"
                }
            },
            model = "doubao-seed-1-6-vision-250815",
            stream = true
        };

        string jsonBody = JsonConvert.SerializeObject(requestbody);

        Debug.Log("question:" + question);

        var fullContent = new StringBuilder();
        var fullReason = new StringBuilder();

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
                return ("", "", 0);
            }

            string response = request.downloadHandler.text;

            string[] lines = response.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("data:"))
                {
                    string jsonData = line.Substring(5).Trim();

                    if (jsonData == "[DONE]")
                    {
                        Debug.Log($"fullContent: {fullContent}");
                        Debug.Log($"fullReason: {fullReason}");
                        break;
                    }

                    try
                    {
                        var fragment = JsonConvert.DeserializeObject<ResponseRoot>(jsonData);

                        if (fragment.choices.Count > 0)
                        {
                            string content = fragment.choices[0].delta.content;
                            fullContent.Append(content);

                            string reason = fragment.choices[0].delta.reasoning_content;
                            fullReason.Append(reason);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"detect fail: {e.Message}\ndata: {jsonData}");
                    }
                }
            }
        }

        float t2 = Time.time;
        Debug.Log("detect time = " + (t2 - t1) + " s");

        return (fullContent.ToString(), fullReason.ToString(), t2 - t1);
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
public class ImageDetectMessagesItem
{
    /// <summary>
    /// 
    /// </summary>
    public List<ImageDetectMessagesContentBase> content;
    /// <summary>
    /// 
    /// </summary>
    public string role;
}

[Serializable]
public class ImageDetectMessagesUrl
{
    /// <summary>
    /// 
    /// </summary>
    public string url = "https://ark-project.tos-cn-beijing.ivolces.com/images/view.jpeg";
    /// <summary>
    /// 
}

[Serializable]
public class ImageDetectMessagesContentBase
{
    /// <summary>
    /// 
    /// </summary>
    public string type;
}

[Serializable]
public class ImageDetectMessagesContentImage : ImageDetectMessagesContentBase
{
    /// <summary>
    /// 
    /// </summary>
    public ImageDetectMessagesUrl image_url;
}

[Serializable]
public class ImageDetectMessagesContentText : ImageDetectMessagesContentBase
{
    /// <summary>
    /// 
    /// </summary>
    public string text;
}


[Serializable]
public class ImageDetectRequestCommon
{
    /// <summary>
    /// 
    /// </summary>
    public List<ImageDetectMessagesItem> messages;
    /// <summary>
    /// 
    /// </summary>
    public string model;
    /// <summary>
    /// 
    /// </summary>
    public bool stream;
}

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

#region response
[Serializable]
public class Delta
{
    /// <summary>
    /// 
    /// </summary>
    public string content;

    /// <summary>
    /// 
    /// </summary>
    public string reasoning_content;

    /// <summary>
    /// 
    /// </summary>
    public string role;
}
[Serializable]
public class ChoicesItem
{
    /// <summary>
    /// 
    /// </summary>
    public Delta delta;
    /// <summary>
    /// 
    /// </summary>
    public int index;
}
[Serializable]
public class ResponseRoot
{
    /// <summary>
    /// 
    /// </summary>
    public List<ChoicesItem> choices;
    /// <summary>
    /// 
    /// </summary>
    public int created;
    /// <summary>
    /// 
    /// </summary>
    public string id;
    /// <summary>
    /// 
    /// </summary>
    public string model;
    /// <summary>
    /// 
    /// </summary>
    public string service_tier;
    /// <summary>
    /// 
    /// </summary>
    public string @object;
    /// <summary>
    /// 
    /// </summary>
    public string usage;
}

#endregion