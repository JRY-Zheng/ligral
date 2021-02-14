/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Collections.Generic;

namespace Ligral.Network
{
    struct GitInfo
    {
        [JsonPropertyName("name")]
        public string Name {get; set;}
        [JsonPropertyName("type")]
        public string Type {get; set;}
    }
    class ExampleManager
    {
        private const string apiUrl = "https://api.github.com/repos/jry-zheng/ligral/contents/examples";
        private const string downloadUrl = "https://gitee.com/junruoyu-zheng/ligral/raw/master/examples";
        private List<GitInfo> ExamplesList;
        public void GetExamplesList()
        {
            using (var examples = GetHttpResponse(apiUrl))
            {
                var examplesInfo = JsonSerializer.DeserializeAsync<GitInfo[]>(examples);
                ExamplesList = new List<GitInfo>(examplesInfo.Result);
            }
            DownloadFile("linearization/sys.lig");
        }
        private void DownloadFile(string path)
        {
            string fileUrl = downloadUrl+"/"+path;
            System.Console.WriteLine(fileUrl);
            using (var rawContentStream = GetHttpResponse(fileUrl))
            {
                using (StreamReader reader = new StreamReader(rawContentStream))
                {
                    // System.Console.Write(reader.ReadToEnd());
                    using (FileStream fileStream = new FileStream(path, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            int index = 0;
                            char[] buffer = new char[1024];
                            while (!reader.EndOfStream)
                            {
                                int count = reader.ReadBlock(buffer, 0, 1024);
                                writer.Write(buffer, 0, count);
                                index += 1024;
                            }
                        }
                    }
                }
            }
        }
        private Stream GetHttpResponse(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                (sender, certificate, chain, error) => true
            );
            WebRequest request = WebRequest.Create(url);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            string host = url.Substring(8);
            host = host.Substring(0, host.IndexOf('/'));
            request.Headers.Add("Host", host);
            WebResponse response = request.GetResponse();
            return response.GetResponseStream();
        }
    }
}