using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Text;

namespace Verified_Demo_App
{
    class Program
    {

        private string endpoint = "https://app.verified.eu/api";
        private string token = "";

        static void Main(string[] args)
        {

            Program program = new Program();
            program.run();

        }

        private void run()
        {

            this.token = "JWT " + this.Authenticate();

            string envelopeUid = this.CreateEnvelope("default");
            string documentUid = this.GetDocumentUid(envelopeUid);
            string fileUploadUrl = this.GetFileUploadUrl(documentUid);
            this.UploadFile(fileUploadUrl);

            Console.WriteLine("Finished");

            Console.ReadKey();
        }

        private string Authenticate()
        {

            Console.WriteLine("Verified Email:");
            string email = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("Password:");

            string password = Console.ReadLine();

            Console.WriteLine();

            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(email + ":" + password));

            var client = new RestClient(this.endpoint + "/auth");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic " + credentials);
            request.AddHeader("content-type", "application/json");
            IRestResponse response = client.Execute(request);

            dynamic body = JsonConvert.DeserializeObject(response.Content);

            return body.token;

        }

        private string CreateEnvelope(string descriptor)
        {

            var client = new RestClient(this.endpoint + "/envelope-descriptors/" + descriptor + "/envelopes");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", this.token);
            request.AddHeader("content-type", "application/json");

            // Include a document in the envelope so that we don't have to make a separate call for creating the document later
            request.AddParameter("application/json", "{\"documents\": {\"1\": {\"data\": {}}}}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            dynamic body = JsonConvert.DeserializeObject(response.Content);

            return body.uid;

        }

        private string GetDocumentUid(string envelopeUid)
        {

            var client = new RestClient(this.endpoint + envelopeUid + "/documents");
            var request = new RestRequest(Method.GET);
            request.AddHeader("authorization", this.token);
            request.AddHeader("content-type", "application/json");

            IRestResponse response = client.Execute(request);

            dynamic body = JsonConvert.DeserializeObject(response.Content);

            return body[0].uid;

        }

        private string GetFileUploadUrl(string documentUid)
        {

            var client = new RestClient(this.endpoint + documentUid + "/files");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", this.token);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", "{\"name\": \"TEST.pdf\",\"fileType\": \"document\"}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            dynamic body = JsonConvert.DeserializeObject(response.Content);

            return body.url;

        }

        private void UploadFile(string uploadUrl) {

            var client = new RestClient(uploadUrl);
            var request = new RestRequest(Method.PUT);
            request.AddHeader("content-type", "application/pdf");
            request.AddParameter("application/octect-stream", File.ReadAllBytes(@"C:\test.pdf"), ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

        }


    }
}
