using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Medo.Web.GData {

    public class GDataDocs {

        public GDataDocs(string authorizationToken) {
            this.AuthorizationToken = authorizationToken;

            ServicePointManager.ServerCertificateValidationCallback += delegate(
                object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors) {
                return true;
            };
        }


        public string AuthorizationToken { get; private set; }


        public string GetRawRes() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://docs.google.com/feeds/default/private/full");
            request.ProtocolVersion = HttpVersion.Version10;
            request.Headers.Add("GData-Version: 3.0");
            request.Headers.Add("Authorization: GoogleLogin auth=" + this.AuthorizationToken);
            request.Proxy = WebRequest.DefaultWebProxy;
            request.Proxy.Credentials = CredentialCache.DefaultCredentials;




            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();

            using (var stream = new StreamReader(response.GetResponseStream())) {
                return stream.ReadToEnd();
            }
        }

    }

}
