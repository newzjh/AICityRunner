//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;
//using Newtonsoft.Json;
//using Cysharp.Threading.Tasks;
//using System.Buffers.Text;
//using ArNav.Rendering.Controllers;
//using ArNav.Rendering.Managers;
//using ArNav.Rendering.Models;
//using UnityEditor.Experimental.GraphView;
//using static UnityEditor.Searcher.SearcherWindow.Alignment;





//#if UNITY_EDITOR
//using UnityEditor;
//[CustomEditor(typeof(ImageDetection))]
//public class ImageDetectionEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        ImageDetection id = (ImageDetection)target;

//        base.OnInspectorGUI();

//        if (Application.isPlaying)
//        {
//            if (GUILayout.Button("Detect T4"))
//            {
//                id.DetectCoordinate("T4");
//            }

//            if (GUILayout.Button("Detect Part"))
//            {
//                id.DetectPart("what is the part in the human body of these medical images?");
//            }

//            if (GUILayout.Button("Detect All"))
//            {
//                id.DetectAll();
//            }

//        }
//    }
//}
//#endif

//public class ImageDetection : MonoBehaviour
//{
//    [NonSerialized]
//    private string apiKey = "ba0e3461-60b9-49d2-87e6-d6e6d95e10bd";
//    [NonSerialized]
//    private string apiUrl = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";

//    public async void DetectCoordinate(string joint)
//    {
//        var vc = GameObject.FindFirstObjectByType<VolumeController>();
//        if (vc)
//        {
//            var scs = vc.GetComponentsInChildren<SliceController>(true);
//            if (scs != null && scs.Length==3)
//            {
//                RenderTexture[] rts = new RenderTexture[3];
//                string OrientationNames = string.Empty;
//                for (int i = 0; i < 3; i++)
//                {
//                    rts[i] = scs[i].RenderTextureManager.SliceRenderTexture;
//                    if (i==0)
//                        OrientationNames += scs[i].name + ",";
//                    else if (i == 1)
//                        OrientationNames += scs[i].name + " and ";
//                    else
//                        OrientationNames += scs[i].name;
//                }
//                //string question = "what is the coordinate (x,y) of " + joint + " respectively in these " + OrientationNames + " medical images?";
//                string question = "what are the coordinates (x,y) of " + joint + " respectively in these " + OrientationNames + " medical images?";
//                var result = await SendStreamRequestLocation(rts, question);
//                var content = result.Item1;
//                var index0 = 0;
//                if (index0 >= 0)
//                {
//                    var index1 = content.IndexOf("**", index0);
//                    if (index1 > 0)
//                    {
//                        var index2 = content.IndexOf("**", index1 + 1);
//                        if (index2 > 0)
//                        {
//                            var ret = content.Substring(index1 + 2, index2 - index1 - 2);
//                        }
//                    }
//                }

//            }
//        }
//    }

//    public async UniTask DetectAll()
//    {
//        string part = await DetectPart("what is the part in the human body of these medical images?");
//        if (part.Contains("chest"))
//        {
//            DetectCoordinate("T1 to T12");
//        }
//        else if (part.Contains("cervical spine") || part.Contains("neck"))
//        {
//            DetectCoordinate("C1 to C7");
//        }
//        else if (part.Contains("lumbar ") || part.Contains("abdomen"))
//        {
//            DetectCoordinate("L1 to L5");
//        }
//        else if (part.Contains("thoracic") || part.Contains("spine"))
//        {
//            DetectCoordinate("T1 to T12");
//        }
//        else if (part.Contains("sacrum"))
//        {
//            DetectCoordinate("S1 to S5");
//        }
//        else if (part.Contains("brain") || part.Contains("head"))
//        {
//            DetectCoordinate("left noise and right noise");
//        }
//    }

//    public async UniTask<string> DetectPart(string question)
//    {
//        string part = "unknown";
//        var vc = GameObject.FindFirstObjectByType<VolumeController>();
//        if (vc)
//        {
//            var scs = vc.GetComponentsInChildren<SliceController>(true);
//            if (scs != null && scs.Length == 3)
//            {
//                RenderTexture[] rts = new RenderTexture[3];
//                string OrientationNames = string.Empty;
//                for (int i = 0; i < 3; i++)
//                {
//                    rts[i] = scs[i].RenderTextureManager.SliceRenderTexture;
//                    if (i == 0)
//                        OrientationNames += scs[i].name + ",";
//                    else if (i == 1)
//                        OrientationNames += scs[i].name + " and ";
//                    else
//                        OrientationNames += scs[i].name;
//                }
//                var result = await SendStreamRequestCommon(rts, question);
//                var content = result.Item1;
//                var index0 = content.IndexOf("depicts the **");
//                if (index0 < 0)
//                    index0 = content.IndexOf("depict the **");
//                if (index0 < 0)
//                    index0 = content.IndexOf("shows the **");
//                if (index0 < 0)
//                    index0 = content.IndexOf("show the **");
//                if (index0 >= 0)
//                {
//                    var index1 = content.IndexOf("**", index0);
//                    if (index1 > 0)
//                    {
//                        var index2 = content.IndexOf("**", index1 + 1);
//                        if (index2 > 0)
//                        {
//                            part = content.Substring(index1 + 2, index2 - index1 - 2).ToLower();
//                        }
//                    }
//                }
//            }
//        }

//        Debug.Log("part=" + part);
//        return part;
//    }

//    private string encodeTexture(RenderTexture rt)
//    {
//        var tex = new Texture2D(rt.width, rt.height);
//        var oldrt = RenderTexture.active;
//        RenderTexture.active = rt;
//        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
//        tex.Apply();
//        RenderTexture.active = oldrt;

//        var bytes = tex.EncodeToPNG();
//        var ret = Convert.ToBase64String(bytes);
//        return ret;
//    }

//    async UniTask<ValueTuple<string,string,float>> SendStreamRequestCommon(SliceOrientation orientation, RenderTexture rt, string question)
//    {
//        string base64_image = encodeTexture(rt);

//        var requestbody = new ImageDetectRequestCommon
//        {
//            messages = new List<ImageDetectMessagesItem>
//            {
//                new ImageDetectMessagesItem
//                {
//                    content = new List<ImageDetectMessagesContentBase>
//                    {
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image}",
//                            }
//                        },
//                        new ImageDetectMessagesContentText
//                        {
//                            type = "text",
//                            text = question
//                        },
//                    },
//                    role = "user"
//                }
//            },
//            model = "doubao-seed-1-6-vision-250815",
//            stream = true
//        };

//        string jsonBody = JsonConvert.SerializeObject(requestbody);

//        Debug.Log("question:" + question);

//        var fullContent = new StringBuilder();
//        var fullReason = new StringBuilder();

//        float t1 = Time.time;
//        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
//        {
//            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
//            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            request.downloadHandler = new DownloadHandlerBuffer();

//            request.SetRequestHeader("Content-Type", "application/json");
//            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//            await request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError($"request error: {request.error}");
//                return ("","",0);
//            }

//            string response = request.downloadHandler.text;

//            string[] lines = response.Split('\n');
//            foreach (string line in lines)
//            {
//                if (string.IsNullOrWhiteSpace(line)) continue;
//                if (line.StartsWith("data:"))
//                {
//                    string jsonData = line.Substring(5).Trim();

//                    if (jsonData == "[DONE]")
//                    {
//                        Debug.Log($"fullContent: {fullContent}");
//                        Debug.Log($"fullReason: {fullReason}");
//                        break;
//                    }

//                    try
//                    {
//                        var fragment = JsonConvert.DeserializeObject<ResponseRoot>(jsonData);

//                        if (fragment.choices.Count > 0)
//                        {
//                            string content = fragment.choices[0].delta.content;
//                            fullContent.Append(content);

//                            string reason = fragment.choices[0].delta.reasoning_content;
//                            fullReason.Append(reason);
//                        }
//                    }
//                    catch (System.Exception e)
//                    {
//                        Debug.LogWarning($"detect fail: {e.Message}\ndata: {jsonData}");
//                    }
//                }
//            }
//        }

//        float t2 = Time.time;
//        Debug.Log("detect time = " + (t2 - t1) + " s");

//        return (fullContent.ToString(), fullReason.ToString(),t2-t1);
//    }

//    async UniTask<ValueTuple<string, string, float>> SendStreamRequestCommon(RenderTexture[] rts, string question)
//    {
//        if (rts == null || rts.Length != 3)
//            return ("", "", 0);

//        string base64_image0 = encodeTexture(rts[0]);
//        string base64_image1 = encodeTexture(rts[1]);
//        string base64_image2 = encodeTexture(rts[2]);

//        var requestbody = new ImageDetectRequestCommon
//        {
//            messages = new List<ImageDetectMessagesItem>
//            {
//                new ImageDetectMessagesItem
//                {
//                    content = new List<ImageDetectMessagesContentBase>
//                    {
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image0}",
//                            }
//                        },
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image1}",
//                            }
//                        },
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image2}",
//                            }
//                        },
//                        new ImageDetectMessagesContentText
//                        {
//                            type = "text",
//                            text = question
//                        },
//                    },
//                    role = "user"
//                }
//            },
//            model = "doubao-seed-1-6-vision-250815",
//            stream = true
//        };

//        string jsonBody = JsonConvert.SerializeObject(requestbody);

//        Debug.Log("question:" + question);

//        var fullContent = new StringBuilder();
//        var fullReason = new StringBuilder();
//        float t1 = Time.time;

//        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
//        {
//            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
//            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            request.downloadHandler = new DownloadHandlerBuffer();

//            request.SetRequestHeader("Content-Type", "application/json");
//            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//            await request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError($"request error: {request.error}");
//                return ("", "", 0);
//            }

//            string response = request.downloadHandler.text;

//            string[] lines = response.Split('\n');
//            foreach (string line in lines)
//            {
//                if (string.IsNullOrWhiteSpace(line)) continue;
//                if (line.StartsWith("data:"))
//                {
//                    string jsonData = line.Substring(5).Trim();

//                    if (jsonData == "[DONE]")
//                    {
//                        Debug.Log($"fullContent: {fullContent}");
//                        Debug.Log($"fullReason: {fullReason}");
//                        break;
//                    }

//                    try
//                    {
//                        var fragment = JsonConvert.DeserializeObject<ResponseRoot>(jsonData);

//                        if (fragment.choices.Count > 0)
//                        {
//                            string content = fragment.choices[0].delta.content;
//                            fullContent.Append(content);

//                            string reason = fragment.choices[0].delta.reasoning_content;
//                            fullReason.Append(reason);
//                        }
//                    }
//                    catch (System.Exception e)
//                    {
//                        Debug.LogWarning($"detect fail: {e.Message}\ndata: {jsonData}");
//                    }
//                }
//            }
//        }

//        float t2 = Time.time;
//        Debug.Log("detect time = " + (t2 - t1) + " s");

//        return (fullContent.ToString(), fullReason.ToString(), t2-t1);
//    }

//    async UniTask<ValueTuple<string, string, float>> SendStreamRequestLocation(RenderTexture[] rts, string question)
//    {
//        if (rts == null || rts.Length != 3)
//            return ("", "", 0);

//        string base64_image0 = encodeTexture(rts[0]);
//        string base64_image1 = encodeTexture(rts[1]);
//        string base64_image2 = encodeTexture(rts[2]);

//        var requestbody = new ImageDetectRequestCommon
//        {
//            messages = new List<ImageDetectMessagesItem>
//            {
//                new ImageDetectMessagesItem
//                {
//                    content = new List<ImageDetectMessagesContentBase>
//                    {
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image0}",
//                            }
//                        },
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image1}",
//                            }
//                        },
//                        new ImageDetectMessagesContentImage
//                        {
//                            type = "image_url",
//                            image_url = new ImageDetectMessagesUrl
//                            {
//                                url = $"data:image/png;base64,{base64_image2}",
//                            }
//                        },
//                        new ImageDetectMessagesContentText
//                        {
//                            type = "text",
//                            text = question
//                        },
//                    },
//                    role = "user"
//                }
//            },
//            model = "doubao-seed-1-6-vision-250815",
//            stream = true
//        };

//        string jsonBody = JsonConvert.SerializeObject(requestbody);

//        Debug.Log("question:" + question);

//        var fullContent = new StringBuilder();
//        var fullReason = new StringBuilder();
//        float t1 = Time.time;

//        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
//        {
//            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
//            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//            request.downloadHandler = new DownloadHandlerBuffer();

//            request.SetRequestHeader("Content-Type", "application/json");
//            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

//            await request.SendWebRequest();

//            if (request.result != UnityWebRequest.Result.Success)
//            {
//                Debug.LogError($"request error: {request.error}");
//                return ("", "", 0);
//            }

//            string response = request.downloadHandler.text;

//            string[] lines = response.Split('\n');
//            foreach (string line in lines)
//            {
//                if (string.IsNullOrWhiteSpace(line)) continue;
//                if (line.StartsWith("data:"))
//                {
//                    string jsonData = line.Substring(5).Trim();

//                    if (jsonData == "[DONE]")
//                    {
//                        Debug.Log($"fullContent: {fullContent}");
//                        Debug.Log($"fullReason: {fullReason}");
//                        break;
//                    }

//                    try
//                    {
//                        var fragment = JsonConvert.DeserializeObject<ResponseRoot>(jsonData);

//                        if (fragment.choices.Count > 0)
//                        {
//                            string content = fragment.choices[0].delta.content;
//                            fullContent.Append(content);

//                            string reason = fragment.choices[0].delta.reasoning_content;
//                            fullReason.Append(reason);
//                        }
//                    }
//                    catch (System.Exception e)
//                    {
//                        Debug.LogWarning($"detect fail: {e.Message}\ndata: {jsonData}");
//                    }
//                }
//            }
//        }

//        float t2 = Time.time;
//        Debug.Log("detect time = " + (t2 - t1) + " s");

//        return (fullContent.ToString(), fullReason.ToString(), t2-t1);
//    }
//}

//#region request

//[Serializable]
//public class ImageDetectMessagesItem
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public List<ImageDetectMessagesContentBase> content;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string role;
//}

//[Serializable]
//public class ImageDetectMessagesUrl
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public string url = "https://ark-project.tos-cn-beijing.ivolces.com/images/view.jpeg";
//    /// <summary>
//    /// 
//}

//[Serializable]
//public class ImageDetectMessagesContentBase
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public string type;
//}

//[Serializable]
//public class ImageDetectMessagesContentImage: ImageDetectMessagesContentBase
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public ImageDetectMessagesUrl image_url;
//}

//[Serializable]
//public class ImageDetectMessagesContentText: ImageDetectMessagesContentBase
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public string text;
//}


//[Serializable]
//public class ImageDetectRequestCommon
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public List<ImageDetectMessagesItem> messages;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string model;
//    /// <summary>
//    /// 
//    /// </summary>
//    public bool stream;
//}
//#endregion

//#region response
//[Serializable]
//public class Delta
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public string content;

//    /// <summary>
//    /// 
//    /// </summary>
//    public string reasoning_content;

//    /// <summary>
//    /// 
//    /// </summary>
//    public string role;
//}
//[Serializable]
//public class ChoicesItem
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public Delta delta;
//    /// <summary>
//    /// 
//    /// </summary>
//    public int index;
//}
//[Serializable]
//public class ResponseRoot
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public List<ChoicesItem> choices;
//    /// <summary>
//    /// 
//    /// </summary>
//    public int created;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string id;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string model;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string service_tier;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string @object;
//    /// <summary>
//    /// 
//    /// </summary>
//    public string usage;
//}

//#endregion