using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

/**
 * Copyright (year) Beijing Volcano Engine Technology Ltd.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace VolcEngineExample.CV
{
    public class Sign
    {
        private static readonly HashSet<char> UrlEncoder = new HashSet<char>();
        private const string ConstEncode = "0123456789ABCDEF";
        public static readonly Encoding Utf8 = Encoding.UTF8;

        private readonly string _region;
        private readonly string _service;
        private readonly string _schema;
        private readonly string _host;
        private readonly string _path;
        private readonly string _ak;
        private readonly string _sk;

        static Sign()
        {
            // 初始化URL编码允许的字符集
            for (int i = 97; i <= 122; i++) UrlEncoder.Add((char)i); // a-z
            for (int i = 65; i <= 90; i++) UrlEncoder.Add((char)i); // A-Z
            for (int i = 48; i <= 57; i++) UrlEncoder.Add((char)i); // 0-9
            UrlEncoder.Add('-');
            UrlEncoder.Add('_');
            UrlEncoder.Add('.');
            UrlEncoder.Add('~');
        }

        public static void Main(string[] args)
        {
            try
            {
                // 火山官网密钥信息, 注意sk结尾有==
                string accessKeyId = "AK*****";
                string secretAccessKey = "******==";
                // 请求域名
                string endpoint = "visual.volcengineapi.com";
                string path = "/"; // 路径，不包含 Query
                // 请求接口信息
                string service = "cv";
                string region = "cn-north-1";
                string schema = "https";

                var sign = new Sign(region, service, schema, endpoint, path, accessKeyId, secretAccessKey);

                // 参考接口文档Query参数
                string action = "CVProcess";
                string version = "2022-08-31";
                DateTime date = DateTime.UtcNow;

                // 参考接口文档Body参数
                var req = new Dictionary<string, object>
                {
                    { "req_key", "xxx" },
                    { "prompt", "******" }
                };
                byte[] body = Utf8.GetBytes(JsonConvert.SerializeObject(req));

                sign.DoRequest("POST", new Dictionary<string, string>(), body, date, action, version);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public Sign(string region, string service, string schema, string host, string path, string ak, string sk)
        {
            _region = region;
            _service = service;
            _host = host;
            _schema = schema;
            _path = path;
            _ak = ak;
            _sk = sk;
        }

        public void DoRequest(string method, Dictionary<string, string> queryList, byte[] body,
                              DateTime date, string action, string version)
        {
            body ??= Array.Empty<byte>();

            string xContentSha256 = HashSha256(body);
            string xDate = date.ToString("yyyyMMdd'T'HHmmss'Z'");
            string shortXDate = xDate.Substring(0, 8);
            string contentType = "application/json";
            string signHeader = "host;x-date;x-content-sha256;content-type";

            // 构建有序查询参数
            SortedDictionary<string, string> realQueryList = new SortedDictionary<string, string>(queryList);
            realQueryList["Action"] = action;
            realQueryList["Version"] = version;

            StringBuilder querySb = new StringBuilder();
            foreach (var kvp in realQueryList)
            {
                querySb.Append(SignStringEncoder(kvp.Key))
                       .Append("=")
                       .Append(SignStringEncoder(kvp.Value))
                       .Append("&");
            }
            if (querySb.Length > 0)
                querySb.Length--; // 移除最后一个&

            // 构建规范请求字符串
            string canonicalString = $"{method}\n{_path}\n{querySb}\n" +
                                     $"host:{_host}\n" +
                                     $"x-date:{xDate}\n" +
                                     $"x-content-sha256:{xContentSha256}\n" +
                                     $"content-type:{contentType}\n\n" +
                                     $"{signHeader}\n" +
                                     $"{xContentSha256}";

            Console.WriteLine(canonicalString);

            // 计算签名字符串
            string hashCanonicalString = HashSha256(Utf8.GetBytes(canonicalString));
            string credentialScope = $"{shortXDate}/{_region}/{_service}/request";
            string signString = $"HMAC-SHA256\n{xDate}\n{credentialScope}\n{hashCanonicalString}";

            // 生成签名密钥并计算签名
            byte[] signKey = GenSigningSecretKeyV4(_sk, shortXDate, _region, _service);
            string signature = BitConverter.ToString(HmacSha256(signKey, signString))
                .Replace("-", "")
                .ToUpperInvariant();

            // 构建请求URL
            string url = $"{_schema}://{_host}{_path}?{querySb}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Host = _host;
            request.Headers["X-Date"] = xDate;
            request.Headers["X-Content-Sha256"] = xContentSha256;
            request.ContentType = contentType;
            request.Headers["Authorization"] =
                $"HMAC-SHA256 Credential={_ak}/{credentialScope}, " +
                $"SignedHeaders={signHeader}, Signature={signature}";

            // 写入请求体
            if (method != "GET" && body.Length > 0)
            {
                request.ContentLength = body.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(body, 0, body.Length);
                }
            }

            // 发送请求并获取响应
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    ProcessResponse(response);
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errorResponse)
            {
                ProcessResponse(errorResponse);
            }
        }

        /// <summary>
        /// 处理响应内容
        /// </summary>
        private void ProcessResponse(HttpWebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                string responseBody = reader.ReadToEnd();
                Console.WriteLine((int)response.StatusCode);
                Console.WriteLine(responseBody);
            }
        }

        /// <summary>
        /// 签名专用URL编码
        /// </summary>
        private string SignStringEncoder(string source)
        {
            if (source == null) return null;

            StringBuilder buf = new StringBuilder();
            byte[] bytes = Utf8.GetBytes(source);

            foreach (byte b in bytes)
            {
                int c = b & 0xFF;
                if (UrlEncoder.Contains((char)c))
                {
                    buf.Append((char)c);
                }
                else if (c == 32) // 空格
                {
                    buf.Append("%20");
                }
                else
                {
                    buf.Append('%');
                    buf.Append(ConstEncode[c >> 4]);
                    buf.Append(ConstEncode[c & 0x0F]);
                }
            }

            return buf.ToString();
        }

        /// <summary>
        /// SHA256哈希计算
        /// </summary>
        public static string HashSha256(byte[] content)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(content);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to compute hash while signing request: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// HMAC-SHA256计算
        /// </summary>
        public static byte[] HmacSha256(byte[] key, string content)
        {
            try
            {
                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    return hmac.ComputeHash(Utf8.GetBytes(content));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to calculate a request signature: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 生成V4版本签名密钥
        /// </summary>
        private byte[] GenSigningSecretKeyV4(string secretKey, string date, string region, string service)
        {
            byte[] kDate = HmacSha256(Utf8.GetBytes(secretKey), date);
            byte[] kRegion = HmacSha256(kDate, region);
            byte[] kService = HmacSha256(kRegion, service);
            return HmacSha256(kService, "request");
        }
    }
}