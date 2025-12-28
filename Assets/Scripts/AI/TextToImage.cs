using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Linq;
using Cysharp.Threading.Tasks;


//curl https://ark.cn-beijing.volces.com/api/v3/chat/completions \
//  -H "Content-Type: application/json" \
//  -H "Authorization: Bearer ba0e3461-60b9-49d2-87e6-d6e6d95e10bd" \
//  -d $'{
//    "model": "doubao-seed-1-6-vision-250815",
//    "messages": [
//        {
//            "content": [
//                {
//                    "image_url": {
//                        "url": "https://ark-project.tos-cn-beijing.ivolces.com/images/view.jpeg"
//                    },
//                    "type": "image_url"
//                },
//                {
//    "text": "图片主要讲了什么?",
//                    "type": "text"
//                }
//            ],
//            "role": "user"
//        }
//    ]
//}'

// 定义请求参数对应的结构体，按照文档里请求格式要求来组织数据
[System.Serializable]
public struct ImageGenerationRequest
{
    public string req_key;
    public string prompt;
    public string model_version;
    public string req_schedule_conf;
    public int seed;
    public float scale;
    public int ddim_steps;
    public int width;
    public int height;
    public bool use_sr;
    public bool return_url;
    public LogoInfo logo_info;
}

// 定义请求里logo相关信息的结构体
[System.Serializable]
public struct LogoInfo
{
    public bool add_logo;
    public int position;
    public int language;
    public float opacity;
    public string logo_text_content;
}

// 定义返回数据对应的结构体，按照文档里返回格式要求来解析响应内容
[System.Serializable]
public struct ImageGenerationResponse
{
    public int code;
    public ResponseData data;
    public string message;
    public string request_id;
    public int status;
    public string time_elapsed;
}

[System.Serializable]
public struct ResponseData
{
    public AlgorithmBaseResp algorithm_base_resp;
    public List<string> binary_data_base64;
    public string pe_result;
    public string predict_tags_result;
    public string rephraser_result;
    public string request_id;
}

[System.Serializable]
public struct AlgorithmBaseResp
{
    public int status_code;
    public string status_message;
}

public class TextToImage : MonoBehaviour
{
    // 火山引擎文生图服务的API请求地址，替换为你申请到的真实地址
    public static string apiUrl = "https://your-volcengine-api-url.com/generate";
    // 你的火山引擎应用的访问密钥，根据实际申请填写
    public static string accessKey = "your-access-key";
    // 你的火山引擎应用的秘密密钥，根据实际申请填写
    public static string secretKey = "your-secret-key";
    // 用于输入文生图的文本提示，比如描述画面内容等
    public static string textPrompt = "美丽的自然风光";
    // 用于展示生成图片的Unity UI的Image组件，在场景中挂载并赋值到此处
    public Image resultImage;
    // 火山引擎服务对应的区域，需替换为实际使用的区域名称
    private static string region = "cn-north-1";
    // 火山引擎服务对应的服务名称，需替换为实际的服务名称（比如文生图相关服务的名称）
    private static string service = "cv";

    // 获取时间戳，用于请求签名等操作（按常见的时间戳生成方式示例）
    private static long GetTimestamp()
    {
        System.DateTime startTime = System.DateTime.Now.ToUniversalTime();
        return (long)(startTime - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
    }

    // 生成签名，按照火山引擎要求完善后的签名算法
    private static string GenerateSignature(long timestamp, string accessKey, string secretKey, string method, string path, string contentType, string requestBody)
    {
        // 假设这里有对应的区域和服务名称变量，需要根据实际情况正确赋值

        // 第一步：获取格式化后的X-Date，格式为YYYYMMDD'T'HHmmss'Z'，用于后续多处时间相关的标识
        string xDate = GetFormattedXDate(timestamp);

        // 第二步：构建规范请求字符串（CanonicalRequest）
        StringBuilder canonicalRequestBuilder = new StringBuilder();

        // 1. 添加请求方法（HTTPRequestMethod），按照文档要求转换为大写
        canonicalRequestBuilder.Append(method.ToUpper()).Append("\n");

        // 2. 添加规范化URI（CanonicalURI）
        // 根据文档，如果URI为空，使用"/"作为绝对路径；若不为空，需按照RFC3986规范进行编码
        string canonicalURI;
        if (string.IsNullOrEmpty(path))
        {
            canonicalURI = "/";
        }
        else
        {
            canonicalURI = System.Web.HttpUtility.UrlEncode(path, System.Text.Encoding.UTF8);
        }
        canonicalRequestBuilder.Append(canonicalURI).Append("\n");


        canonicalRequestBuilder.Append("Action=CVProcess&Version=2022-08-31").Append("\n");

        // 4. 添加规范化请求头（CanonicalHeaders）
        // 遍历要参与签名的请求头，将请求头名称转换为小写，去除值的前后空格，然后按照“名称:值\n”的格式添加到构建器中
        // 注意最后要额外添加一个换行符，确保格式符合文档要求，且host和x-date如果存在于请求头中则必选参与签名计算
        StringBuilder canonicalHeadersBuilder = new StringBuilder();
        AddCanonicalHeader(canonicalHeadersBuilder, "host", new Uri(apiUrl).Host);
        AddCanonicalHeader(canonicalHeadersBuilder, "x-content-sha256", HashSHA256(Encoding.UTF8.GetBytes(requestBody)));
        AddCanonicalHeader(canonicalHeadersBuilder, "x-date", xDate);
        // AddCanonicalHeader(canonicalHeadersBuilder, "content-type", contentType);
        string canonicalHeaders = canonicalHeadersBuilder.ToString() + "\n";
        canonicalRequestBuilder.Append(canonicalHeaders);

        // 5. 添加参与签名的请求头列表（SignedHeaders）
        // 将参与签名的请求头名称转换为小写并用“;”连接，确保顺序正确且包含host和x-date（如果存在），用于后续签名计算中明确参与的请求头
        string signedHeaders = "host;x-content-sha256;x-date";
        canonicalRequestBuilder.Append(signedHeaders).Append("\n");
        // 6. 添加请求体哈希值（HexEncode(Hash(RequestPayload))）
        // 计算请求体的哈希值（使用SHA256算法），并将结果转换为十六进制编码后添加到规范请求字符串中
        canonicalRequestBuilder.Append(HashSHA256(Encoding.UTF8.GetBytes(requestBody)));

        string canonicalRequest = canonicalRequestBuilder.ToString();
        Debug.Log(canonicalRequest);

        // 第三步：计算规范请求的哈希值（HexEncode(Hash(CanonicalRequest))）
        string hashedCanonicalRequest = HashSHA256(Encoding.UTF8.GetBytes(canonicalRequest));

        // 第四步：构建凭证范围字符串（CredentialScope）
        // 按照文档要求，取X-Date中的日期部分（前8位），与region、service和固定的“request”字符串拼接，用于界定签名的有效范围
        string shortDate = xDate.Substring(0, 8);
        string credentialScope = $"{shortDate}/{region}/{service}/request";

        // 第五步：构建待签名字符串（StringToSign）
        string stringToSign = $"HMAC-SHA256\n{xDate}\n{credentialScope}\n{hashedCanonicalRequest}";
        Debug.Log(stringToSign);
        // 第六步：计算签名密钥（kSigning）
        // 通过多层HMAC计算，使用秘密密钥（secretKey）和相关的日期、区域、服务信息逐步派生得到签名密钥
        byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        byte[] kDate = HmacSha256(secretKeyBytes, shortDate);
        byte[] kRegion = HmacSha256(kDate, region);
        byte[] kService = HmacSha256(kRegion, service);
        byte[] kSigning = HmacSha256(kService, "request");

        // 第七步：计算签名（Signature）
        // 使用派生得到的签名密钥（kSigning）和待签名字符串（StringToSign）进行HMAC计算，然后将结果转换为十六进制编码

        byte[] signatureBytes = HmacSha256(kSigning, stringToSign);
        string signature = ByteArrayToHexString(signatureBytes);
        return signature;
    }

    // 辅助方法，用于添加规范化的请求头到构建器中
    private static void AddCanonicalHeader(StringBuilder builder, string headerName, string headerValue)
    {
        builder.Append(headerName.ToLower()).Append(":").Append(headerValue.Trim()).Append("\n");
    }

    // 辅助方法，执行HMAC-SHA256计算，返回字节数组结果
    private static byte[] HmacSha256(byte[] key, string data)
    {
        using (HMACSHA256 hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }

    // 辅助方法，将字节数组转换为十六进制编码的字符串
    private static string ByteArrayToHexString(byte[] bytes)
    {
        StringBuilder hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            hex.AppendFormat("{0:x2}", b);
        }
        return hex.ToString();
    }

    // 辅助方法，用于将时间戳转换为符合要求的X-Date格式字符串
    private static string GetFormattedXDate(long timestamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToUniversalTime();
        return dateTimeOffset.ToString("yyyyMMdd'T'HHmmss'Z'");
    }

    // 计算字节数组的SHA256哈希值，并返回十六进制字符串表示
    private static string HashSHA256(byte[] content)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] hashBytes = sha256Hash.ComputeHash(content);
            StringBuilder sb = new StringBuilder();
            for (int i = 0, n = hashBytes.Length; i < n; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public static async UniTask<Texture> GenerateImage()
    {
        long timestamp = GetTimestamp();
        string method = "POST"; // 请求方法，固定为POST，可根据实际情况调整
        string path = ""; // 这里假设你的请求路径是/generate，需根据实际API调整
        string contentType = "application/json"; // 根据请求体内容类型设置

        // 构建请求的JSON格式数据，按照文档里请求格式要求填充各参数
        ImageGenerationRequest requestData = new ImageGenerationRequest
        {
            req_key = "high_aes_general_v20_L",
            prompt = textPrompt,
            model_version = "doubao-seedream-4-5-251128",
            req_schedule_conf = "general_v20_9B_rephraser",
            seed = -1,
            scale = 3.5f,
            ddim_steps = 16,
            width = 512,
            height = 512,
            use_sr = true,
            return_url = true,
            logo_info = new LogoInfo
            {
                add_logo = false,
                position = 0,
                language = 0,
                opacity = 0.3f,
                logo_text_content = "这里是明水印内容"
            }
        };
        string requestBody = JsonUtility.ToJson(requestData);

        // 生成签名
        string signature = GenerateSignature(timestamp, accessKey, secretKey, method, path, contentType, requestBody);
        //string signature = "1caf8e82d88a9fc8f094f6d800616522e7c78f23f7e4ee568625b9b825e15eb0";
        //Debug.Log(signature);
        UnityWebRequest request = new UnityWebRequest(apiUrl, method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        // 设置Content-Type请求头
        request.SetRequestHeader("Content-Type", contentType);

        //设置Host请求头，从API请求地址中提取主机部分
        //string host = new Uri(apiUrl).Host;
        // request.SetRequestHeader("host", host);

        // 设置X-Date请求头，使用获取到的时间戳转换后的格式
        string xDate = GetFormattedXDate(timestamp);
        request.SetRequestHeader("X-Date", xDate);

        // 计算并设置X-Content-Sha256请求头，对请求体内容计算SHA256哈希值
        string xContentSha256 = HashSHA256(bodyRaw);
        request.SetRequestHeader("X-Content-Sha256", xContentSha256);
        Debug.Log(xContentSha256);

        // 构建并设置Authorization请求头，包含签名等认证信息
        string credentialScope = $"{xDate.Substring(0, 8)}/{region}/{service}/request";
        string authorizationHeader = $"HMAC-SHA256 Credential={accessKey}/{credentialScope}, SignedHeaders=host;x-content-sha256;x-date, Signature={signature}";


        request.SetRequestHeader("Authorization", authorizationHeader);

        await request.SendWebRequest();

        // 按照新的方式判断请求结果，使用isDone和isNetworkError、isHttpError属性来替代原来的.result属性判断
        if (request.isDone)
        {
            //
            // 解析返回的JSON数据，获取图片相关内容并处理显示（按文档返回格式要求操作）
            string responseJson = request.downloadHandler.text;
            Debug.Log(responseJson);
            ImageGenerationResponse response = JsonUtility.FromJson<ImageGenerationResponse>(responseJson);
            Debug.Log(response.data.binary_data_base64.Count);
            if (response.data.binary_data_base64 != null && response.data.binary_data_base64.Count > 0)
            {
                byte[] imageBytes = System.Convert.FromBase64String(response.data.binary_data_base64[0]);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                //Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                //resultImage.sprite = sprite;
                return texture;
            }
            else
            {
                Debug.LogError("没有获取到有效的图片数据");
            }
        }
        else
        {
            Debug.LogError("请求出错: " + request.error);
        }

        return null;
    }


}