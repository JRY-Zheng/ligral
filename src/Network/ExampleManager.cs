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
        private Logger logger = new Logger("ExampleManager");
        public void ShowExampleList()
        {
            GetExamplesList();
            logger.Prompt($"Here are {ExamplesList.Count} examples available:\n\t{string.Join("\n\t", ExamplesList.ConvertAll(info=>info.Name))}");
        }
        public void DownloadProject(string projectName)
        {
            if (Directory.Exists(projectName))
            {
                logger.Warn($"Folder {projectName} has already existed, will be overrode.");
                Directory.Delete(projectName, true);
            }
            if (ExamplesList == null)
            {
                GetExamplesList();
            }
            projectName = projectName.ToLower();
            if (ExamplesList.Exists(info => info.Name == projectName))
            {
                DownloadFolder(projectName);
            }
            else
            {
                ShowExampleList();
                throw logger.Error(new LigralException($"No example project named {projectName}"));
            }
        }
        public void DownloadAllProjects()
        {
            GetExamplesList();
            foreach (var info in ExamplesList)
            {
                DownloadProject(info.Name);
            }
        }
        private void GetExamplesList()
        {
            using (var examples = GetHttpResponse(apiUrl))
            {
                var examplesInfo = JsonSerializer.DeserializeAsync<GitInfo[]>(examples);
                ExamplesList = new List<GitInfo>(examplesInfo.Result);
            }
        }
        private void DownloadFile(string path)
        {
            string fileUrl = downloadUrl+"/"+path;
            logger.Prompt($"Downloading file: {fileUrl}");
            using (var rawContentStream = GetHttpResponse(fileUrl))
            {
                using (StreamReader reader = new StreamReader(rawContentStream))
                {
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
        private void DownloadFolder(string path)
        {
            Directory.CreateDirectory(path);
            using (var items = GetHttpResponse(apiUrl+"/"+path))
            {
                var itemsInfo = JsonSerializer.DeserializeAsync<GitInfo[]>(items);
                foreach (var info in itemsInfo.Result)
                {
                    string newPath = path+"/"+info.Name;
                    if (info.Type == "file")
                    {
                        DownloadFile(newPath);
                    }
                    else
                    {
                        DownloadFolder(newPath);
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