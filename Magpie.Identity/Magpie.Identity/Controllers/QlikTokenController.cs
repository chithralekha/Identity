using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QlikAuthNet;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using static QlikAuthNet.Ticket;
using System.Web.Http;
using System.Configuration;
using System.Threading.Tasks;
using Magpie.Logging;


//http://localhost:50443/qliktoken/?proxyRestUri=https://ip-172-31-8-24.us-west-2.compute.internal:4243/qps/tick/&targetId=1477be43-ec64-4b0f-b0cd-803af9258fa4


namespace Magpie.Identity.Controllers
{
    public class QlikTokenController : BaseApiController
    {
        public string EndPoint { get; set; }

        private readonly string EndPointDefault = "ticket";
        private StoreLocation CertificateLocation { get; set; }
        private string CertificateName { get; set; }

        private string proxyRestUri = string.Empty;

        private string userId = string.Empty;

        private string userDir = string.Empty;

        private string systemUserId = string.Empty;

        private X509Certificate2 certificate { get; set; }

        protected ILog logger = new Logging.Logger.Log(typeof(QlikTokenController));

        public QlikTokenController()
        {
            proxyRestUri = ConfigurationManager.AppSettings["QlikTokenProviderUri"];
            string endPoint = ConfigurationManager.AppSettings["QlikTokenProviderEndPoint"];

            userId = ConfigurationManager.AppSettings["QlikUserId"];
            userDir = ConfigurationManager.AppSettings["QlikUserDir"];

            logger.Info("QlikTokenController", systemUserId);


            CertificateLocation = StoreLocation.CurrentUser;
            CertificateName = "QlikClient";

            if (string.IsNullOrWhiteSpace(endPoint))
            {
                EndPoint = EndPointDefault;
            }
            else
            {
                EndPoint = endPoint;
            }
        }

        public QlikTokenController(string tokenProviderUri, string endPoint, string userDir)
        {
            //UserProfileController userProfileController = new Controllers.UserProfileController();
           // var temp = userProfileController.GetSystemUser();

            this.proxyRestUri = tokenProviderUri;
           // this.userId = userId;
            this.userDir = userDir;

            CertificateLocation = StoreLocation.CurrentUser;
            CertificateName = "QlikClient";

            if (string.IsNullOrWhiteSpace(endPoint))
            {
                EndPoint = EndPointDefault;
            }
            else
            {
                EndPoint = endPoint;
            }
        }

        [AllowAnonymous]
        [Route("qliktoken")]
        public async Task<IHttpActionResult> Get(string proxyRestUri, string targetId)
        {
            systemUserId = await GetUserGuid("System");
            
            logger.Info($"qliktoken proxyRestUri: {proxyRestUri}, targetId: {targetId}", systemUserId);

            #region Preconditions

            if (targetId == null)
            {
                throw new ArgumentNullException();
            }


            if (proxyRestUri == null)
            {
                throw new ArgumentNullException();
            }

            #endregion

            try
            {
                // log.Write($"UserName: {User.Identity.Name}");

                //  string currentUserId = await GetUserGuid(User.Identity.Name);
                // log.Write($"currentUserId: {currentUserId}");

                //  if (!string.IsNullOrWhiteSpace(currentUserId))
                // {
                //    userId = currentUserId;
                //  }
                logger.Info($"userId: {userId}", systemUserId);

                if (!proxyRestUri.Contains("https://"))
                {
                    proxyRestUri = "https://" + proxyRestUri + ":4243";
                }

                var ticketRequest = new Ticket()
                {
                    UserDirectory = userDir,
                    UserId = userId,
                    TargetId = targetId
                };

                ticketRequest.ProxyRestUri = proxyRestUri;


                StringBuilder ticketMessage = new StringBuilder();
                ticketMessage.Append($"ticketRequest ProxyRestUri: {ticketRequest.ProxyRestUri}");
                ticketMessage.Append($"ticketRequest UserDirectory: {ticketRequest.UserDirectory}");
                ticketMessage.Append($"ticketRequest UserId: {ticketRequest.UserId}");
                ticketMessage.Append($"ticketRequest targetId: {ticketRequest.TargetId}");
                logger.Info(ticketMessage.ToString(), systemUserId);

                //Request ticket
                Stream stream = TicketRequest(ticketRequest);

                //Stream stream = Execute(request);

                if (stream == null)
                {
                    throw new Exception("Ticket Request was not valid");
                }

                var result = JsonConvert.DeserializeObject<ResponseData>(new StreamReader(stream).ReadToEnd());

                StringBuilder resultMessage = new StringBuilder();
                resultMessage.Append($"result Ticket: {result.Ticket}");
                resultMessage.Append($"result TargetUri: {result.TargetUri}");
                resultMessage.Append($"result UserDirectory: {result.UserDirectory}");
                logger.Info(resultMessage.ToString(), systemUserId);

                //Return ticket only due to lack of TargetUri
                if (String.IsNullOrEmpty(result.TargetUri))
                    return Ok(result);

                //Add ticket to TargetUri
                string redirectUrl;
                if (result.TargetUri.Contains("?"))
                    redirectUrl = result.TargetUri + "&qlikTicket=" + result.Ticket;
                else
                    redirectUrl = result.TargetUri + "?qlikTicket=" + result.Ticket;


                var uri = new Uri(redirectUrl);

                return Redirect(uri);

                //ticket = req.TicketRequest();
            }

            catch (Exception ex)
            {
                logger.Error($"Exception {ex}", systemUserId);
                return InternalServerError(ex);

            }
        }

        private string TicketRequestOrig(Ticket ticket)
        {
            try
            {
                //Execute request
                Stream stream = Execute(ticket);

                if (stream != null)
                {
                    var res = JsonConvert.DeserializeObject<ResponseData>(new StreamReader(stream).ReadToEnd());

                    //Return ticket only due to lack of TargetUri
                    if (String.IsNullOrEmpty(res.TargetUri))
                        return "qlikTicket=" + res.Ticket;

                    //Add ticket to TargetUri
                    string redirectUrl;
                    if (res.TargetUri.Contains("?"))
                        redirectUrl = res.TargetUri + "&qlikTicket=" + res.Ticket;
                    else
                        redirectUrl = res.TargetUri + "?qlikTicket=" + res.Ticket;

                    //Redirect user
                    // HttpContext.Current.Response.Redirect(redirectUrl);
                }
                else
                {
                    throw new Exception("Unknown error");
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }


        private void LocateCertificate()
        {
            try
            {
                // First locate the Qlik Sense certificate
                X509Store store = new X509Store(StoreName.My, CertificateLocation);
                store.Open(OpenFlags.ReadOnly);

                logger.Info($"LocateCertificate: {CertificateName}", systemUserId);

                //foreach (var item in store.Certificates.Cast<X509Certificate2>())
                //{
                //    logger.Info($"Certificate: {item.FriendlyName}, {item.GetName()}", systemUserId);
                //}

                certificate = store.Certificates.Cast<X509Certificate2>().FirstOrDefault(c => c.FriendlyName == CertificateName);
                store.Close();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }
            catch (Exception ex)
            {
                logger.Error($"Exception {ex}", systemUserId);
                certificate = null;
            }
        }

        public Stream TicketRequest(Ticket ticketRequest)
        {
            Uri url = CombineUri(ticketRequest.ProxyRestUri, EndPoint);

            //Get certificate
            LocateCertificate();

            if (certificate == null)
            {
                logger.Info("Current User Certificate not found! Verify AppPool credentials.", systemUserId);
                CertificateLocation = StoreLocation.LocalMachine;
                LocateCertificate();
            }

            if (certificate == null)
            {
                logger.Info("Local Machine Certificate not found! Verify AppPool credentials.", systemUserId);
                throw new Exception("Certificate not found! Verify AppPool credentials.");
            }

            //Create the HTTP Request and add required headers and content in Xrfkey
            string xrfkey = GenerateXrfKey();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?Xrfkey=" + xrfkey);

            // Add the method to authentication the user
            request.Method = "POST";
            request.Accept = "application/json";
            request.Headers.Add("X-Qlik-Xrfkey", xrfkey);

            request.ClientCertificates.Add(certificate);

            string body = string.Empty;
            byte[] bodyBytes;

            if (string.IsNullOrWhiteSpace(ticketRequest.TargetId))
            {
                body = "{ 'UserId':'" + ticketRequest.UserId + "','UserDirectory':'" + ticketRequest.UserDirectory + "','Attributes': [] }";
            }
            else
            {
                //  ticketRequest.Attributes = new List<Dictionary<string, string>>();
                ticketRequest.AddGroups("Users");
                body = JsonConvert.SerializeObject(ticketRequest);
            }

            if (!string.IsNullOrEmpty(body))
            {
                bodyBytes = Encoding.UTF8.GetBytes(body);

                request.ContentType = "application/json";
                request.ContentLength = bodyBytes.Length;

                // Write Request object
                StringBuilder message = new StringBuilder();
                message.Append($"URL: {request.RequestUri}");
                message.Append($"Request Headers: {request.Headers}");
                message.Append($"Body: {body}");
                logger.Info(message.ToString(), systemUserId);

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
                requestStream.Close();
            }

            // make the web request and return the content
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.GetResponseStream();
            }
            catch (WebException ex)
            {
                logger.Error($"WebException {ex}", systemUserId);
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string error = ex.ToString() + reader.ReadToEnd();
                    throw new Exception(error);
                }
            }
        }

        private Stream Execute(Ticket ticketRequest)
        {
            // Get data as json
            var json = JsonConvert.SerializeObject(ticketRequest);

            //Create URL to REST endpoint for tickets
            Uri url = CombineUri(ticketRequest.ProxyRestUri, EndPoint);

            //Get certificate
            LocateCertificate();

            //Create the HTTP Request and add required headers and content in Xrfkey
            string xrfkey = GenerateXrfKey();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?Xrfkey=" + xrfkey);

            // Add the method to authentication the user
            request.Method = "POST";
            request.Accept = "application/json";
            request.Headers.Add("X-Qlik-Xrfkey", xrfkey);

            if (certificate == null)
            {
                throw new Exception("Certificate not found! Verify AppPool credentials.");
            }

            request.ClientCertificates.Add(certificate);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

            if (!string.IsNullOrEmpty(json))
            {
                request.ContentType = "application/json";
                request.ContentLength = bodyBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bodyBytes, 0, bodyBytes.Length);
                requestStream.Close();
            }

            // make the web request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream();
        }

        /// <summary>
        /// Generates a randomized string to be used as XrfKey 
        /// </summary>
        /// <returns>16 character randomized string</returns>
        private string GenerateXrfKey()
        {
            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            var chars = new char[16];
            var rd = new Random();

            for (int i = 0; i < 16; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        private static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
        {
            if (!baseUri.EndsWith("/"))
                baseUri += "/";

            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
        }


        private async Task<string> GetUserGuid(string username)
        {
            try
            {
                var user = await this.AppUserManager.FindByNameAsync(username);

                if (user != null)
                {
                   var userProfile = this.TheModelFactory.Create(user);

                    return userProfile.Id;
                }
                return string.Empty;
            }

            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}