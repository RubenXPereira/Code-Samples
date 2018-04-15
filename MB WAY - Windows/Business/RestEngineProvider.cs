using System;
using System.Threading;
using Windows.Storage.Streams;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using Newtonsoft.Json;
using SIBS.MBWAY.Business.Network.Utils;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;
using Windows.Networking.Connectivity;
using Windows.Web;
using System.IO;
using System.Text;
using Windows.Web.Http;
using SIBS.MBWAY.Business.Network.Gamification.Engine.Inputs;
using SIBS.MBWAY.Business.AppSettings;

namespace SIBS.MBWAY.Business.Utils
{
    public abstract class RestEngineProvider
    {
        public static string GamificationWebServerEndpoint = AppSettingsManager.GetAppSettings().engineWebViewEndPoint + "/#/";
        
        // WebView Requests
        public static string GamificationHome = "home";

        // Server Requests
        public static string GamificationLogin = "Login";

        protected GenericEngineInput input;
        
        protected HttpClient GetConfiguredHttpClient()
        {
            try
            {
                HttpBaseProtocolFilter protolFilter = new HttpBaseProtocolFilter();
                
                protolFilter.AllowAutoRedirect = false;

                HttpClient httpClient = new HttpClient(protolFilter);

                httpClient.DefaultRequestHeaders.Accept.Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.TryAppendWithoutValidation("Accept-Charset", RestConfig.OUTPUT_STREAM_CHARSET);

                return httpClient;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected async void postRequest()
        {
            string jsonInput = JsonConvert.SerializeObject(input);

            LogManager.WriteLine("jsonInput: " + jsonInput);
            HttpResponseMessage responseMessage = new HttpResponseMessage();

            CancellationTokenSource postTimeout = new CancellationTokenSource(); // 50 seconds
            postTimeout.CancelAfter(RestConfig.SOCKET_TIMEOUT_MS);

            CancellationTokenSource readTimeout = new CancellationTokenSource(); // 2 seconds
            readTimeout.CancelAfter(RestConfig.CONNECTION_TIMEOUT_MS);

            // POST REQUEST
            string GamificationLoginEndpoint = AppSettingsManager.GetAppSettings().engineServerEndPoint + GamificationLogin;
            LogManager.WriteLine("Gamification Login EndPoint: " + GamificationLoginEndpoint);
            Uri gamificationServerUri = new Uri(GamificationLoginEndpoint);
            HttpClient client = GetConfiguredHttpClient();

            Encoding latinISO = Encoding.GetEncoding(RestConfig.OUTPUT_STREAM_CHARSET);
            byte[] messageArray = latinISO.GetBytes(jsonInput);

            MemoryStream ms = new MemoryStream(messageArray);

            IInputStream inStream = ms.AsInputStream();

            var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Post, gamificationServerUri);

            request.Content = new HttpStreamContent(inStream);
            ((HttpStreamContent)request.Content).Headers.ContentType = new HttpMediaTypeHeaderValue("application/json");

            LogManager.WriteLine("Request: " + request);

            try
            {
                responseMessage = await client.PostAsync(request.RequestUri, request.Content).AsTask(postTimeout.Token);
                
                // READ RESPONSE
                try
                {
                    IInputStream responseStream = await responseMessage.Content.ReadAsInputStreamAsync().AsTask(readTimeout.Token);

                    string responseContent;
                    using (Stream receiveStream = responseStream.AsStreamForRead())
                    {
                        Encoding utf8 = Encoding.GetEncoding(RestConfig.INPUT_STREAM_CHARSET);
                        using (StreamReader readStream = new StreamReader(receiveStream, utf8))
                        {
                            responseContent = readStream.ReadToEnd();
                        }
                    }

                    string response = responseContent;

                    LogManager.WriteLine("response: " + response);

                    handleResponse(response);
                }

                catch (Exception e)
                {
                    WebErrorStatus status = WebError.GetStatus(e.HResult);
                    
                    LogManager.WriteLine("Exception on Read Request: " + e);
                    handleResponse(e.Message);
                }
            }

            catch (Exception e)
            {
                WebErrorStatus status = WebError.GetStatus(e.HResult);

                LogManager.WriteLine("Error: " + status);

                LogManager.WriteLine("Exception on Post Request: " + e);
                handleResponse(e.Message);
            }

            // Once your app is done using the HttpClient object, we call 'Dispose' to
            // free up system resources (the underlying socket and memory used for the object)
            client.Dispose();
        }
        
        private bool IsInternetConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = (connections != null) && (connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
            return internet;
        }

        protected abstract void handleRequest();

        protected abstract void handleResponse(string response);
    }
}
