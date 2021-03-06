﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

using Interfaces.Delegates.Constrain;

using Interfaces.Controllers.Network;
using Interfaces.Controllers.Cookies;

using Interfaces.Status;

using Models.Network;

using Interfaces.Controllers.Uri;

namespace Controllers.Network
{
    public sealed class NetworkController : INetworkController
    {
        HttpClient client;
        ICookiesController cookieController;
        readonly IUriController uriController;
        IConstrainAsyncDelegate<string> constrainRequestRateAsyncDelegate;

        public NetworkController(
            ICookiesController cookieController,
            IUriController uriController,
            IConstrainAsyncDelegate<string> constrainRequestRateAsyncDelegate)
        {
            this.cookieController = cookieController;
            this.uriController = uriController;
            this.constrainRequestRateAsyncDelegate = constrainRequestRateAsyncDelegate;

            var httpHandler = new HttpClientHandler
            {
                UseDefaultCredentials = false
            };
            client = new HttpClient(httpHandler);
            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.Add(Headers.UserAgent, HeaderDefaultValues.UserAgent);
        }

        public async Task<string> GetResourceAsync(
            IStatus status,
            string baseUri,
            IDictionary<string, string> parameters = null)
        {
            var uri = uriController.ConcatenateUriWithKeyValueParameters(baseUri, parameters);

            using (var response = await RequestResponseAsync(status, HttpMethod.Get, uri))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return await reader.ReadToEndAsync();
            }
        }

        public async Task<HttpResponseMessage> RequestResponseAsync(
            IStatus status,
            HttpMethod method,
            string uri,
            HttpContent content = null)
        {
            await constrainRequestRateAsyncDelegate.ConstrainAsync(uri, status);

            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Headers.Add(Headers.Accept, HeaderDefaultValues.Accept);
            requestMessage.Headers.Add(Headers.Cookie, await cookieController.GetCookiesStringAsync(status));

            if (content != null) requestMessage.Content = content;
            var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            if (response.Headers.Contains(Headers.SetCookie))
                await cookieController.SetCookiesAsync(response.Headers.GetValues(Headers.SetCookie), status);

            return response;
        }

        public async Task<string> PostDataToResourceAsync(
            IStatus status,
            string baseUri,
            IDictionary<string, string> parameters = null,
            string data = null)
        {
            var uri = uriController.ConcatenateUriWithKeyValueParameters(baseUri, parameters);

            if (data == null) data = string.Empty;
            var content = new StringContent(data, Encoding.UTF8, HeaderDefaultValues.ContentType);

            using (var response = await RequestResponseAsync(status, HttpMethod.Post, uri, content))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return await reader.ReadToEndAsync();
            }
        }
    }
}
