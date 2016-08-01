using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ConsoleApplication1
{
    /// <summary>
    /// This class is an alternative when you can't use Service References. It allows you to invoke Web Methods on a given Web Service URL.
    /// Based on the code from http://stackoverflow.com/questions/9482773/web-service-without-adding-a-reference
    /// </summary>
    public class CustomWebService
    {
        public string Url { get; private set; }
        public string Method { get; private set; }
        public Dictionary<string, string> Params = new Dictionary<string, string>();
        public XDocument ResponseSOAP = XDocument.Parse("<root/>");
        public XDocument ResultXML = XDocument.Parse("<root/>");
        public string ResultString = String.Empty;

        //private Cursor InitialCursorState;

        public CustomWebService()
        {
            Url = String.Empty;
            Method = String.Empty;
        }
        public CustomWebService(string baseUrl)
        {
            Url = baseUrl;
            Method = String.Empty;
        }
        public CustomWebService(string baseUrl, string methodName)
        {
            Url = baseUrl;
            Method = methodName;
        }

        // Public API

        /// <summary>
        /// Adds a parameter to the WebMethod invocation.
        /// </summary>
        /// <param name="name">Name of the WebMethod parameter (case sensitive)</param>
        /// <param name="value">Value to pass to the paramenter</param>
        public void AddParameter(string name, string value)
        {
            Params.Add(name, value);
        }

        public void Invoke()
        {
            Invoke(Method, true);
        }

        /// <summary>
        /// Using the base url, invokes the WebMethod with the given name
        /// </summary>
        /// <param name="methodName">Web Method name</param>
        public void Invoke(string methodName)
        {
            Invoke(methodName, true);
        }

        /// <summary>
        /// Cleans all internal data used in the last invocation, except the WebService's URL.
        /// This avoids creating a new WebService object when the URL you want to use is the same.
        /// </summary>
        public void CleanLastInvoke()
        {
            ResponseSOAP = ResultXML = null;
            ResultString = Method = String.Empty;
            Params = new Dictionary<string, string>();
        }

        #region Helper Methods

        /// <summary>
        /// Checks if the WebService's URL and the WebMethod's name are valid. If not, throws ArgumentNullException.
        /// </summary>
        /// <param name="methodName">Web Method name (optional)</param>
        private void AssertCanInvoke(string methodName = "")
        {
            if (Url == String.Empty)
                throw new ArgumentNullException("You tried to invoke a webservice without specifying the WebService's URL.");
            if ((methodName == "") && (Method == String.Empty))
                throw new ArgumentNullException("You tried to invoke a webservice without specifying the WebMethod.");
        }

        private void ExtractResult(string methodName)
        {
            var _namespace = "http://www.webserviceX.NET";  // This will need to be either input or dynamically obtained
            //var _namespace = "http://tempuri.org";

            // Selects just the elements with namespace http://tempuri.org/ (i.e. ignores SOAP namespace)
            var namespMan = new XmlNamespaceManager(new NameTable());
            namespMan.AddNamespace("foo", _namespace);

            XElement webMethodResult = ResponseSOAP.XPathSelectElement("//foo:" + methodName + "Result", namespMan);
            // If the result is an XML, return it and convert it to string
            if (webMethodResult.FirstNode.NodeType == XmlNodeType.Element)
            {
                ResultXML = XDocument.Parse(webMethodResult.FirstNode.ToString());
                ResultXML = Utils.RemoveNamespaces(ResultXML);
                ResultString = ResultXML.ToString();
            }
            // If the result is a string, return it and convert it to XML (creating a root node to wrap the result)
            else
            {
                ResultString = webMethodResult.FirstNode.ToString();

                ResultString = Utils.UnescapeString(ResultString);
                //ResultXML = XDocument.Parse("<root>" + ResultString + "</root>");
                ResultXML = XDocument.Parse(ResultString);
            }
        }

        /// <summary>
        /// Invokes a Web Method, with its parameters encoded or not.
        /// </summary>
        /// <param name="methodName">Name of the web method you want to call (case sensitive)</param>
        /// <param name="encode">Do you want to encode your parameters? (default: true)</param>
        private void Invoke(string methodName, bool encode)
        {
            var _namespace = "http://www.webserviceX.NET";  // This will need to be either input or dynamically obtained
            //var _namespace = "http://tempuri.org";

            AssertCanInvoke(methodName);
            var soapStr =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                   xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                   xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                  <soap:Body>
                    <{0} xmlns=""{namespace}"">
                      {1}
                    </{0}>
                  </soap:Body>
                </soap:Envelope>";
            soapStr = soapStr.Replace("{namespace}", _namespace);

            var req = (HttpWebRequest)WebRequest.Create(Url);
            
            req.Headers.Add("SOAPAction", "\"" + _namespace + "/" + methodName + "\"");
            req.ContentType = "text/xml;charset=\"utf-8\"";
            req.Accept = "text/xml";
            req.Method = "POST";

            using (var stm = req.GetRequestStream())
            {
                string postValues = "";
                foreach (var param in Params)
                {
                    if (encode) postValues += string.Format("<{0}>{1}</{0}>", HttpUtility.HtmlEncode(param.Key), HttpUtility.HtmlEncode(param.Value));
                    else postValues += string.Format("<{0}>{1}</{0}>", param.Key, param.Value);
                }

                soapStr = string.Format(soapStr, methodName, postValues);
                using (var stmw = new StreamWriter(stm))
                {
                    stmw.Write(soapStr);
                }
            }
            try
            {
                using (var response = req.GetResponse())
                using (var responseReader = new StreamReader(response.GetResponseStream()))
                {
                    var result = responseReader.ReadToEnd();
                    ResponseSOAP = XDocument.Parse(result);
                    //var tmp = Utils.UnescapeString(result);
                    //tmp = tmp.Replace("?><", "?>\n<");
                    //ResponseSOAP = XDocument.Parse(tmp);
                    ExtractResult(methodName);
                }
            }
            catch (WebException webEx)
            {
                var stream = (webEx as WebException).Response.GetResponseStream();
                var xmr = System.Xml.XmlReader.Create(stream);
                var message = Message.CreateMessage(xmr, (int)stream.Length, MessageVersion.Soap11);
                var mf = MessageFault.CreateFault(message, (int)stream.Length);
                var fe = new FaultException(mf);
                var result = fe.Message;
                message.Close();
                throw new Exception(result);
            }
        }

        /// <summary>
        /// This method should be called before each Invoke().
        /// </summary>
        internal void PreInvoke()
        {
            CleanLastInvoke();
            //InitialCursorState = Cursor.Current;
            //Cursor.Current = Cursor.WaitCursor;
            // feel free to add more instructions to this method
        }

        /// <summary>
        /// This method should be called after each (successful or unsuccessful) Invoke().
        /// </summary>
        internal void PosInvoke()
        {
            //Cursor.Current = InitialCursorState;
            // feel free to add more instructions to this method
        }

        #endregion
    }
}
