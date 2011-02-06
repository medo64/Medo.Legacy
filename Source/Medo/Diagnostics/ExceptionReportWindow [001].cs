//Josip Medved <jmedved@jmedved.com> http://www.jmedved.com

//2010-05-28: New version.


using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace Medo.Diagnostics {

    /// <summary>
    /// Sending error reports to Internet.
    /// This class is thread-safe.
    /// </summary>
    public static class ExceptionReportWindow {

        private static readonly object SyncRoot = new object();
        private static readonly string LogLineSeparator = Environment.NewLine + new string('-', 72) + Environment.NewLine + Environment.NewLine;


        /// <summary>
        /// Stores exception text in file
        /// </summary>
        /// <param name="text">Exception report text.</param>
        public static bool SaveToTemp(string text) {
            lock (SyncRoot) {
                var logFileName = Path.Combine(Path.GetTempPath(), string.Format(CultureInfo.InvariantCulture, "ExceptionReport [{0}].log", GetProductTitle(Assembly.GetEntryAssembly())));
                if (System.IO.File.Exists(logFileName)) {
                    System.IO.File.AppendAllText(logFileName, LogLineSeparator);
                }
                System.IO.File.AppendAllText(logFileName, text);
                return true;
            }
        }

        /// <summary>
        /// Executes POST request.
        /// </summary>
        /// <param name="text">Text to send.</param>
        /// <param name="address">Destination URL.</param>
        public static bool PostToWeb(string text, Uri address) {
            return PostToWeb(text, address, null);
        }

        /// <summary>
        /// Executes POST request.
        /// </summary>
        /// <param name="text">Text to send.</param>
        /// <param name="address">Destination URL.</param>
        /// <param name="additionalParameters">Additional parameters.</param>
        public static bool PostToWeb(string text, Uri address, NameValueCollection additionalParameters) {
            if (additionalParameters == null) {
                additionalParameters = new NameValueCollection();
            }
            var assembly = Assembly.GetEntryAssembly();
            additionalParameters.Add("Product", GetProductTitle(assembly));
            additionalParameters.Add("Version", assembly.GetName().Version.ToString());
            additionalParameters.Add("Message", text);

            try {

                var request = WebRequest.Create(address);
                request.Method = "POST";
                request.Proxy = WebRequest.DefaultWebProxy;
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                var sbPostData = new StringBuilder();
                for (int i = 0; i < additionalParameters.Count; ++i) {
                    if (sbPostData.Length > 0) { sbPostData.Append("&"); }
                    sbPostData.Append(Uri.EscapeUriString(additionalParameters.GetKey(i)) + "=" + Uri.EscapeUriString(additionalParameters[i]));
                }

                request.ContentType = "application/x-www-form-urlencoded";
                byte[] byteArray = Encoding.UTF8.GetBytes(sbPostData.ToString());
                request.ContentLength = byteArray.Length;
                using (var dataStream = request.GetRequestStream()) {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse()) {
                    if (response.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new StreamReader(response.GetResponseStream())) {
                            string responseFromServer = reader.ReadToEnd();
                            if (responseFromServer.Length == 0) { //no data is outputed in case of real 200 response (instead of 500 wrapped in generic 200 page)
                                return true; //correct response
                            } else {
                                return false; //something weird
                            }
                        }
                    } else {
                        return false; //Status code is something wrong
                    }
                }

            } catch (WebException ex) {
                System.Diagnostics.Debug.WriteLine("W: " + ex.Message + ".    {{Medo.Diagnostics.ExceptionReportWindow}}");
                return false;
            }
        }




        private static string GetProductTitle(Assembly assembly) {
            object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            if ((productAttributes != null) && (productAttributes.Length >= 1)) {
                return ((AssemblyProductAttribute)productAttributes[productAttributes.Length - 1]).Product;
            } else {
                object[] titleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
                if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
                    return ((AssemblyTitleAttribute)titleAttributes[titleAttributes.Length - 1]).Title;
                } else {
                    return assembly.GetName().Name;
                }
            }
        }

    }

}
