/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Text.Json.Serialization;
using System.Net;
using System.Net.Security;
using System.IO;

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
        public void GetExamplesList()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                (sender, certificate, chain, error) => true
            );
            WebRequest request = WebRequest.Create(apiUrl);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            request.Headers.Add("Host", "api.github.com");
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            string message = reader.ReadToEnd();
            System.Console.WriteLine(message);
        }
    }
}