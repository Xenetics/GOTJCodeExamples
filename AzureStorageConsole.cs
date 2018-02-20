using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;

namespace AzureStorageConsole
{
    internal class BlobHelper
    {
        #region Variables, Constructors, Helpers

        string _Account;

        public BlobHelper()
        {
            _Account = RESTHelper.GetAccount(string.Empty);
        }

        public BlobHelper(string Account)
        {
            _Account = RESTHelper.GetAccount(Account);
        }

        private HttpWebResponse RESTExec(string Command, string Resource)
        {
            return RESTExec(Command, Resource, string.Empty, null);
        }

        private HttpWebResponse RESTExec(string Command, string Resource, string RequestBody, SortedList<string, string> MetadataList)
        {
            DateTime now = DateTime.UtcNow;
            string Method = Command;

            Command = Command.ToUpper();

            switch (Command)
            {
                case "GETCONTAINERACL":
                    Method = "HEAD";
                    Resource += "?comp=acl";
                    break;
                case "SETCONTAINERACL":
                    Method = "PUT";
                    Resource += "?comp=acl";
                    break;
                case "SETCONTAINERMETADATA":
                case "SETBLOBMETADATA":
                    Method = "PUT";
                    Resource += "?comp=metadata";
                    break;
                default:
                    break;
            }

            string uri = @"http://" + _Account + ".blob.core.windows.net/" + Resource;

            // setup the web request
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = Method;
            request.ContentLength = 0;
            request.Headers.Add("x-ms-date", now.ToString("R", System.Globalization.CultureInfo.InvariantCulture));


            // Add the authorization header
            if ((Command == "SETCONTAINERMETADATA" || Command == "SETBLOBMETADATA") && MetadataList != null)
            {
                for (int i = 0; i < MetadataList.Count; i++)
                {
                    request.Headers.Add("x-ms-meta-" + MetadataList.Keys[i], MetadataList[MetadataList.Keys[i]]);
                }
            }

            else if (Command == "SETCONTAINERACL")
            {
                request.Headers.Add("x-ms-prop-publicaccess", RequestBody);
                RequestBody = string.Empty;
            }

            request.Headers.Add("Authorization", RESTHelper.SharedKey(_Account, GenSignature(Method, Resource, now, request)));

            if (!string.IsNullOrEmpty(RequestBody))
            {
                // Set the request body and content length
                byte[] ba = new ASCIIEncoding().GetBytes(RequestBody);

                request.ContentLength = ba.Length;
                request.GetRequestStream().Write(ba, 0, ba.Length);
                request.GetRequestStream().Close();
            }


            return (HttpWebResponse)request.GetResponse();
        }

        private string GenSignature(string Method, string Resource, DateTime now, HttpWebRequest request)
        {
            // For a table, you need to use this Canonical form:
            //  VERB + "\n" +
            //  Content - MD5 + "\n" +
            //  Content - Type + "\n" +
            //  Date + "\n" +
            //  CanonicalizedResources;

            // Verb
            string signature = Method + "\n";

            // Content-MD5
            signature += "\n";

            // Content-Type
            if (request.ContentType != string.Empty)
                signature += request.ContentType;

            signature += "\n";

            // Date
            signature += "\n";

            // Add the datetime header
            // Add the datetime header
            signature += "x-ms-date:" + now.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "\n";

            // add the meta data headers (if any)
            SortedList<string, string> list = new SortedList<string, string>();
            for (int i = 0; i < request.Headers.Count; i++)
            {
                if (request.Headers.Keys[i].StartsWith("x-ms-"))
                {
                    if (request.Headers.Keys[i] != "x-ms-date")
                        list.Add(request.Headers.Keys[i], request.Headers[i]);
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                signature += list.Keys[i] + ":" + list[list.Keys[i]] + "\n";
            }

            // Canonicalized Resource
            // Format is /{0}/{1} where 0 is name of the account and 1 is resources URI path
            signature += "/" + _Account + "/" + Resource;

            return signature;
        }

        #endregion

        #region Container Methods

        public void CreateContainer(string Container)
        {
            HttpWebResponse response = RESTExec("PUT", Container);
            response.Close();
        }

        public void DeleteContainer(string Container)
        {
            HttpWebResponse response = RESTExec("DELETE", Container);
            response.Close();
        }

        public bool GetContainerACL(string Container)
        {
            using (HttpWebResponse response = RESTExec("GetContainerACL", Container))
            {
                bool b;
                bool.TryParse(response.Headers["x-ms-prop-publicaccess"], out b);
                return b;
            }
        }

        public void DisplayContainerProperties(string Container)
        {
            try
            {
                // create the request
                using (HttpWebResponse response = RESTExec("HEAD", Container))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string key;

                        Console.WriteLine("Meta Data for Container: " + Container);

                        for (int i = 0; i < response.Headers.Count; i++)
                        {
                            key = response.Headers.Keys[i];
                            if (key.StartsWith("x-ms-meta-"))
                            {
                                Console.WriteLine(key.Substring(10) + ": " + response.Headers[i]);
                            }
                        }

                        Console.WriteLine();
                    }
                }
            }
            catch (WebException ex)
            {
                RESTHelper.DisplayWebException(ex);
            }
        }

        public void ListContainers()
        {
            try
            {
                // create the request
                using (HttpWebResponse response = RESTExec("GET", "?comp=list"))
                {
                    // load the xml
                    XmlDocument doc = new XmlDocument();
                    doc.Load(response.GetResponseStream());

                    XmlNodeList nodes = doc.GetElementsByTagName("Container");

                    if (nodes.Count > 0)
                    {
                        Console.WriteLine("Containers in Account: " + _Account);

                        // process each row
                        foreach (XmlNode n in nodes)
                        {
                            // process the columns
                            foreach (XmlNode n2 in n.ChildNodes)
                            {
                                if (n2.Name == "Name")
                                {
                                    Console.WriteLine(n2.InnerText);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No Containers Defined\n");
                    }
                }
            }
            catch (WebException ex)
            {
                RESTHelper.DisplayWebException(ex);
            }

            Console.WriteLine();
        }

        public void SetContainerACL(string Container, bool PublicAccess)
        {
            HttpWebResponse response = RESTExec("SetContainerACL", Container, PublicAccess.ToString(), null);
            response.Close();
        }

        public void SetContainerMetadata(string Container, SortedList<string, string> MetadataList)
        {
            HttpWebResponse response = RESTExec("SetContainerMetadata", Container, string.Empty, MetadataList);
            response.Close();
        }

        #endregion

        #region Blob Methods

        public string ListBlobs(string container)
        {
            string returnString = "";
            try
            {
                // create the request
                using (HttpWebResponse response = RESTExec("GET", container + "?comp=list"))
                {
                    // load the xml
                    XmlDocument doc = new XmlDocument();
                    doc.Load(response.GetResponseStream());

                    XmlNodeList nodes = doc.GetElementsByTagName("Blob");

                    if (nodes.Count > 0)
                    {
                        // process each row (blob)
                        foreach (XmlNode n in nodes)
                        {
                            // process each column
                            foreach (XmlNode n2 in n.ChildNodes)
                            {
                                if (n2.Name == "Name")
                                {
                                    returnString += n2.InnerText;
                                    returnString += "@";
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No Blobs Defined\n");
                    }
                    return returnString;
                }
            }
            catch (WebException ex)
            {
                RESTHelper.DisplayWebException(ex);
            }

            return returnString;
        }

        public void DeleteBlob(string container, string blob)
        {
            HttpWebResponse response = RESTExec("DELETE", container + "/" + blob);
            response.Close();
        }

        public HttpWebResponse GetBlob(string container, string blob)
        {
            return RESTExec("GET", container + "/" + blob);
        }

        public void DisplayBlobMetaData(string Container, string Blob)
        {
            try
            {
                using (HttpWebResponse response = RESTExec("HEAD", Container + "/" + Blob + "?comp=metadata"))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string key;

                        Debug.Log("Meta Data for Blob: " + Blob);

                        for (int i = 0; i < response.Headers.Count; i++)
                        {
                            key = response.Headers.Keys[i];
                            if (key.StartsWith("x-ms-meta-"))
                            {
                                Debug.Log(key.Substring(10) + ": " + response.Headers[i]);
                            }
                        }

                        Debug.Log("");
                    }
                }
            }
            catch (WebException ex)
            {
                RESTHelper.DisplayWebException(ex);
            }
        }

        public void DisplayBlobProperties(string Container, string Blob)
        {
            try
            {
                // create the request
                using (HttpWebResponse response = RESTExec("HEAD", Container + "/" + Blob))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string key;

                        Debug.Log("Properties for Blob: " + Blob);

                        for (int i = 0; i < response.Headers.Count; i++)
                        {
                            key = response.Headers.Keys[i];
                            if (key.StartsWith("x-ms-meta-"))
                            {
                                Debug.Log(key.Substring(10) + ": " + response.Headers[i]);
                            }
                        }

                        Debug.Log("");
                    }
                }
            }
            catch (WebException ex)
            {
                RESTHelper.DisplayWebException(ex);
            }
        }

        public void PutBlob(string Container, string Blob, string Data)
        {
            HttpWebResponse response = RESTExec("PUT", Container + "/" + Blob, Data, null);
            response.Close();
        }

        public void SetBlobMetaData(string Container, string Blob, SortedList<string, string> MetadataList)
        {
            HttpWebResponse response = RESTExec("SetBlobMetaData", Container + "/" + Blob, string.Empty, MetadataList);
            response.Close();
        }

        #endregion

    }

    internal class RESTHelper
    {
        public static string DisplayWebException(WebException ex)
        {
            Console.WriteLine(ex.Message);

            string s = string.Empty;

            if (ex.Response != null)
            {
                s = new System.IO.StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                Console.WriteLine(s + "\n");
            }
            Console.WriteLine();

            return s;
        }

        public static string GetAccount()
        {
            return GetAccount(string.Empty);
        }

        public static string GetAccount(string Account)
        {
            // default account
            if (string.IsNullOrEmpty(Account) || Account.Equals("gotjreplay", StringComparison.OrdinalIgnoreCase))
                return "gotjreplay";

            if (Account.Equals("Account2", StringComparison.OrdinalIgnoreCase))
                return "Account2";

            if (Account.Equals("Account3", StringComparison.OrdinalIgnoreCase))
                return "Account3";

            throw new Exception("Invalid Account: " + Account);
        }

        public static string GetSecret(string Account)
        {
            if (Account == "gotjreplay")
                return "3OOsHeOgT3elXpfh/4Yv+wLzj87fWpBpXFQHt2W8xHOmOC4+y+k7Y7cjjOynfO6EHiNyYABs64cD9bqaP8FOAw==";

            if (Account == "Account2")
                return "Secret2";

            if (Account == "Account3")
                return "Secret3";

            throw new Exception("Invalid Account: " + Account);
        }

        public static string SharedKey(string Account, string signature)
        {
            // Hash-based Message Authentication Code (HMAC) using SHA256 hash
            System.Security.Cryptography.HMACSHA256 hasher = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(GetSecret(Account)));

            byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(signature));

            // return the Shared Key
            return "SharedKey " + Account + ":" + System.Convert.ToBase64String(hash);
        }
    }
}