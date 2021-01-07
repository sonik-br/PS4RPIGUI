using Newtonsoft.Json;
using PS4RPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PS4RPI
{
    internal class RestClient
    {
        //public const string APPLICATIONJSON = "application/json";
        private HttpClient _client = new HttpClient();

        public RestClient(string baseAddress)
        {
            _client.BaseAddress = new Uri($"http://{baseAddress}:12800/api/");
        }

        private T _streamToObject<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var reader = new StreamReader(stream))
                using (var jsonreader = new JsonTextReader(reader))
                    return new JsonSerializer().Deserialize<T>(jsonreader);
        }

        private async Task<string> _streamToStringAsync(Stream stream)
        {
            string toReturn = null;

            if (stream != null)
                using (var reader = new StreamReader(stream))
                    toReturn = await reader.ReadToEndAsync();

            return toReturn;
        }

        private async Task<T> _parseResponseAsync<T>(HttpResponseMessage response)
        {

            using (var stream = await response.Content.ReadAsStreamAsync())
                if (response.IsSuccessStatusCode)
                    return _streamToObject<T>(stream);
                else
                    throw new Exception($"{response.StatusCode}\n{await _streamToStringAsync(stream)}");
        }

        //private async Task<T> get<T>(string path, CancellationToken token)
        //{
        //    using (var request = new HttpRequestMessage(HttpMethod.Get, path))
        //        using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token))
        //            return await _parseResponseAsync<T>(response);
        //}

        private async Task<T> post<T>(string path, object obj, CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, path))
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, MediaTypeNames.Application.Json);
                var timeOutToken = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeOutToken.CancelAfter(5000);
                using (var response = await _client.SendAsync(request, timeOutToken.Token))
                    return await _parseResponseAsync<T>(response);
            }
        }

        internal async Task<ResponseExists> IsExists(RequestTitleID title, CancellationToken token)
        {
            return await post<ResponseExists>($"is_exists", title, token);
        }

        internal async Task<ResponseTaskTitle> Install(RequestInstall install, CancellationToken token)
        {
            return await post<ResponseTaskTitle>($"install", install, token);
        }

        //internal async Task<ResponseExists> UninstallGame(RequestTitleID title, CancellationToken token)
        //{
        //    return await post<ResponseExists>($"uninstall_game", title, token);
        //}

        internal async Task<ResponseTaskStatus> GetTaskProcess(RequestTaskID task, CancellationToken token)
        {
            return await post<ResponseTaskStatus>($"get_task_progress", task, token);
        }

    }
}
