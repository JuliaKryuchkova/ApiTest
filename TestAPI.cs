using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace JsonBlobTest
{
    [TestClass]
    public class TestAPI
    {
        public string redirect;
        public string redirect1;
        public string id;

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(redirect);
                request.Method = "DELETE";
                WebResponse response = request.GetResponse();
            }
            catch { }
        }

        public static void Request(string people, string age, out string redirect, out string id)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://jsonblob.com/api/jsonBlob");
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.Write(Message(people, age));
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            redirect = String.Format(response.Headers["Location"]);
            id = String.Format(response.Headers["X-jsonblob"]);
            response.Close();
        }

        static string Message(string people, string age)
        {
            Message message = new Message { People = people, Age = age };
            string json = JsonConvert.SerializeObject(message);
            return json;
        }

        // Send a POST request to /api/jsonBlob
        [TestMethod]
        public void TestMethodPost()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://jsonblob.com/api/jsonBlob");
            request.Method = "POST";
            request.ContentType = "application/json";

            using (var requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.Write(Message("Ivanov_T", "10.5"));
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            redirect = String.Format(response.Headers["Location"]);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
            response.Close();
        }

        // Send a GET request to /api/jsonBlob/<blobId>
        [TestMethod]
        public void TestMethodGet()
        {
            Request("Maria", "18", out redirect, out id);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(redirect);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
            {
                string responseText = reader.ReadToEnd();
                string mes = Message("Maria", "18");
                Assert.AreEqual(responseText, mes);
            }

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            response.Close();
        }

        // Send a DELETE request to /api/jsonBlob/<blobId>
        [TestMethod]
        public void TestMethodDelete()
        {
            Request("Maria", "18", out redirect, out id);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(redirect);
            request.Method = "DELETE";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            response.Close();
        }

        // Send a PUT request to /api/jsonBlob/ using the X-jsonblob header
        [TestMethod]
        public void TestMethodPUTWOid()
        {
            Request("Maria", "18", out redirect, out id);
            var request = (HttpWebRequest)WebRequest.Create("https://jsonblob.com/api/company/employees/engineers");
            request.Method = "PUT";
            request.ContentType = "application/json";
            request.Headers["X-jsonblob"] = id;

            using (var requestStream = request.GetRequestStream())
            using (var writer = new StreamWriter(requestStream))
            {
                writer.Write(Message("Ivanov_T", "10.5"));
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
            {
                string responseText = reader.ReadToEnd();
                string mes = Message("Ivanov_T", "10.5");
                Assert.AreEqual(responseText, mes);
            }

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            response.Close();
        }

        // Send a POST request to /api/jsonBlob twice
        [TestMethod]
        public void TestMethodDublicate()
        {
            Request("Maria", "18", out redirect1, out id);
            Request("Maria", "18", out redirect, out id);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(redirect1);
            request.Method = "DELETE";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            response.Close();
        }

        // Send a DELETE request to not existing  /api/jsonBlob/<blobId>
        [TestMethod]
        public void TestMethodDelNotFound()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://jsonblob.com/api/jsonBlob/1e07ee2e-a9de-11e8-a4ff-7931f5e9b90b");
            request.Method = "DELETE";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                e.ToString().Contains("The remote server returned an error: (404) Not Found");
            }
        
        }

        // Send a GET request to not existing  /api/jsonBlob/<blobId>
        [TestMethod]
        public void TestMethodGetNotFound()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://jsonblob.com/api/jsonBlob/1e07ee2e-a9de-11e8-a4ff-7931f5e9b90b");
            request.Method = "GET";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                e.ToString().Contains("The remote server returned an error: (404) Not Found");
            }

        }

        // Send a POST request to not existing  /api/jsonBlob/<blobId>
        [TestMethod]
        public void TestMethodPutNotFound()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://jsonblob.com/api/jsonBlob/1e07ee2e-a9de-11e8-a4ff-7931f5e9b90b");
            request.Method = "PUT";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                e.ToString().Contains("The remote server returned an error: (404) Not Found");
            }

        }
        
    }
}
