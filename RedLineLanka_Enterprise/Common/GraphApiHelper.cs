using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace RedLineLanka_Enterprise.Common
{
    public class GraphApiHelper
    {
        private const string ServiceClientId = "a8610987-a1a6-4f12-b898-f04e5e7c642f";
        private const string ServiceClientSecret = "._3aU65~Wr3iCG.pN~AnK4lNc_qr.36aWS";
        private const string ServiceTenantName = "a6ec0f1c-2a34-41a9-ad11-2275a4888497";
        private const string UserEmail = "";
        private const string FromEmail = "";
        private const string ImagesFolder = "";
        private const string QualificationAttachmentsFolder = "";

        private static string accessToken = null;
        private static DateTime? accessTokenExpiresAt = null;

        private static async Task<string> GetAccessTokenAsync()
        {
            if (accessToken != null && accessTokenExpiresAt > DateTime.Now)
                return accessToken;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(
                                      "https://login.microsoftonline.com/{0}/oauth2/v2.0/token",
                                      ServiceTenantName));

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            string postData = @"grant_type=client_credentials";
            postData += "&scope=" + HttpUtility.UrlEncode("https://graph.microsoft.com/.default");
            postData += "&client_id=" + HttpUtility.UrlEncode(ServiceClientId);
            postData += "&client_secret=" + HttpUtility.UrlEncode(ServiceClientSecret);
            byte[] data = encoding.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AzureActiveDirectoryTokenFormat));
                    AzureActiveDirectoryTokenFormat tokenResp =
                        (AzureActiveDirectoryTokenFormat)(serializer.ReadObject(stream));

                    string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", tokenResp.token_type, " ",
                        tokenResp.access_token);

                    accessToken = tokenResp.access_token;
                    accessTokenExpiresAt = DateTime.Now.AddSeconds(tokenResp.expires_in);
                    AzureActiveDirectoryTokenFormat.Instance.access_token = tokenResp.access_token;
                }
            }
            return accessToken;
        }

        private static GraphServiceClient graphClient = null;

        public static async Task<GraphServiceClient> GetGraphClientAsync()
        {
            if (graphClient == null || accessTokenExpiresAt < DateTime.Now)
            {
                var token = await GetAccessTokenAsync();

                graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                   async (requestMessage) =>
                   {
                       await Task.Run(() =>
                       {
                           requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                       });
                   }));
            }

            AzureActiveDirectoryTokenFormat.Instance.graphServiceClient = graphClient;
            return graphClient;
        }

        public static MemoryStream GetImage(string imageName, string defaultImageName)
        {
            MemoryStream ms = null;

            Task.Run(async () =>
            {
                ms = await GetImageAsync(imageName, defaultImageName);
            }).GetAwaiter().GetResult();

            return ms;
        }

        public static async Task<MemoryStream> GetImageAsync(string imageName, string defaultImageName)
        {
            await GetGraphClientAsync();

            try
            {
                using (var downloadStream = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                                                .Root
                                                .ItemWithPath($"{ImagesFolder}/{imageName}")
                                                .Content
                                                .Request()
                                                .GetAsync())
                {
                    using (var outStream = new MemoryStream())
                    {
                        await downloadStream.CopyToAsync(outStream);
                        return outStream;
                    }
                }
            }
            catch (Exception)
            {
                using (var downloadStream = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                                                .Root
                                                .ItemWithPath($"{ImagesFolder}/{defaultImageName}")
                                                .Content
                                                .Request()
                                                .GetAsync())
                {
                    using (var outStream = new MemoryStream())
                    {
                        await downloadStream.CopyToAsync(outStream);
                        return outStream;
                    }
                }
            }

        }

        public static async Task<string> GetImageUrlAsync(string imageName, string defaultImageName)
        {
            await GetGraphClientAsync();

            try
            {
                var item = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                            .Root
                            .ItemWithPath($"{ImagesFolder}/{imageName}")
                            .Request()
                            .GetAsync();
                return item.WebUrl;
            }
            catch (Exception)
            {
                var item = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                            .Root
                            .ItemWithPath($"{ImagesFolder}/{defaultImageName}")
                            .Request()
                            .GetAsync();
                return item.WebUrl;
            }

        }

        public static void SaveImage(byte[] binData, string fileName)
        {
            Task.Run(async () =>
            {
                await SaveImageAsync(binData, fileName);
            }).GetAwaiter().GetResult();
        }

        public static async Task SaveImageAsync(byte[] binData, string fileName)
        {
            await GetGraphClientAsync();

            using (var inStream = new MemoryStream(binData))
            {
                var uploadedItem = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                                        .Root
                                        .ItemWithPath($"{ImagesFolder}/{fileName}")
                                        .Content
                                        .Request()
                                        .PutAsync<DriveItem>(inStream);
            }
        }

        public static string GetQualificationAttachment(int enrollId, string uniqueFileName)
        {
            string path = null;

            Task.Run(async () =>
            {
                path = await GetQualificationAttachmentAsync(enrollId, uniqueFileName);
            }).GetAwaiter().GetResult();

            return path;
        }

        public static async Task<string> GetQualificationAttachmentAsync(int enrollId, string uniqueFileName)
        {
            await GetGraphClientAsync();

            try
            {
                var item = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                            .Root
                            .ItemWithPath($"{QualificationAttachmentsFolder}/{enrollId}-{uniqueFileName}")
                            .Request()
                            .GetAsync();

                var permission = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                    .Items[item.Id].CreateLink("edit", "anonymous").Request().PostAsync();

                return permission.Link.WebUrl;
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        public static void SaveQualificationAttachment(byte[] binData, int enrollId, string uniqueFileName)
        {
            Task.Run(async () =>
            {
                await SaveQualificationAttachmentAsync(binData, enrollId, uniqueFileName);
            }).GetAwaiter().GetResult();
        }

        public static async Task SaveQualificationAttachmentAsync(byte[] binData, int enrollId, string uniqueFileName)
        {
            await GetGraphClientAsync();

            using (var inStream = new MemoryStream(binData))
            {
                var uploadedItem = await AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Drives[UserEmail]
                                        .Root
                                        .ItemWithPath($"{QualificationAttachmentsFolder}/{enrollId}-{uniqueFileName}")
                                        .Content
                                        .Request()
                                        .PutAsync<DriveItem>(inStream);
            }
        }

        public static void SendMail(HttpContextBase context, string toAddress, string subject, string mailAction, RouteValueDictionary routeValues = null)
        {
            Task.Run(async () =>
            {
                await SendMailAsync(context, toAddress, subject, mailAction, routeValues);
            }).GetAwaiter().GetResult();
        }

        public static async Task SendMailAsync(HttpContextBase context, string toAddress, string subject, string mailAction, RouteValueDictionary routeValues = null)
        {
            await GetGraphClientAsync();

            var htmlContent = await RenderViewToStringAsync(context, mailAction, routeValues);

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlContent
                },
                ToRecipients = new List<Recipient>()
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = toAddress
                        }
                    }
                }
            };

            var request = AzureActiveDirectoryTokenFormat.Instance.graphServiceClient.Users[FromEmail].SendMail(message);
            await request.Request().PostAsync();
        }

        private static async Task<string> RenderViewToStringAsync(HttpContextBase context, string mailAction, RouteValueDictionary routeValues)
        {
            var urlHelper = new System.Web.Mvc.UrlHelper(context.Request.RequestContext);
            routeValues = routeValues ?? new RouteValueDictionary();
            if (routeValues.ContainsKey("area"))
                routeValues["area"] = "Base";
            else
                routeValues.Add("area", "Base");
            var datUrl = urlHelper.Action(mailAction, "Mail", routeValues, context.Request.Url.Scheme);

            HttpWebRequest request = WebRequest.Create(datUrl) as HttpWebRequest;
            request.Method = "GET";
            var body = string.Empty;
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("iso-8859-1"));
                body = await reader.ReadToEndAsync();
            }

            return body;
        }
    }

    [DataContract]
    internal class AzureActiveDirectoryTokenFormat
    {
        [DataMember]
        internal string token_type { get; set; }
        [DataMember]
        internal string access_token { get; set; }
        [DataMember]
        internal int expires_in { get; set; }

        public IGraphServiceClient graphServiceClient { get; set; }

        public DriveItem folder = new DriveItem();

        public List<string> userEmails = new List<string>();

        public static AzureActiveDirectoryTokenFormat Instance { get; } = new AzureActiveDirectoryTokenFormat();

        public string CreateToken()
        {
            return Instance.access_token;
        }
    }
}