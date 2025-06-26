using Framework_API_Automation.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Framework_API_Automation.Utilities.Reporting;
using Framework_API_Automation.Reusables;
using Newtonsoft.Json;
using Reqnroll;

namespace Framework_API_Automation.APIStepDefination.PIL_Steps
{


    [Binding]
    public class CommonSteps
    {
        public static string baseUri;
        public static HttpResponseMessage response;
        public static String URL = "https://pi-stage.serviceranger4.com";
        public static String GraphQLURL = "https://pi-stage.serviceranger4.com/graphql";



        [Given(@"the base URI is ""(.*)""")]
        public void GivenTheBaseUriIs(string uri)
        {
            baseUri = uri == "URL" ? URL : uri;
            ExtentReportHelper.GetTest()?.Info("Base URI is " + baseUri);
        }

        [Given(@"the base graphql URI is ""(.*)""")]
        public void GivenTheBaseGraphQLUriIs(string uri)
        {
            baseUri = uri == "URL" ? GraphQLURL : uri;
            ExtentReportHelper.GetTest()?.Info("GraphQL Base URI is " + baseUri);
        }


       



        [When(@"the user sends a GET request to ""(.*)""")]
        public async Task WhenTheUserSendsAGetRequestTo(string endpoint)
        {
             await PIL_ApiRequestsReusables.SendGetRequestAsync(endpoint, null, null, null, null, null, null, 4, null);
            //  response = await PIL_ApiRequestsReusables.SendGetRequestAsync(endpoint, null, null, null, null, null, null, 4, null);
            // ExtentReportHelper.GetTest()?.Info("GET Response: " + await response.Content.ReadAsStringAsync());
        }

        [When(@"the user sends a POST request to ""(.*)"" with JSON body from ""(.*)""")]
        public async Task WhenTheUserSendsAPostRequestToWithJsonBodyFrom(string url, string jsonFileName)
        {
            string jsonBody = JsonUtils.ReadFileFromSubfolders(jsonFileName);
            await PIL_ApiRequestsReusables.SendPostRequest(url, null, null, null, null, null, null, 3, jsonBody, null);
            /// response = await PIL_ApiRequestsReusables.SendPostRequest(url, null, null, null, null, null, null, 3, jsonBody, null);
            //  ExtentReportHelper.GetTest()?.Info("POST Response Status: " + (int)response.StatusCode);
            //  ExtentReportHelper.GetTest()?.Info("POST Response Body: " + await response.Content.ReadAsStringAsync());
        }

        [Then(@"the response body should match the expected JSON response from ""(.*)""")]
        public async Task ThenTheResponseBodyShouldMatchTheExpectedResponseFrom(string expectedJsonFile)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Framework_API_Automation"));
            string baseFolderPath = Path.Combine(projectRoot, "TestData");

            var baseFolder = new DirectoryInfo(baseFolderPath);
            var expectedFile = JsonUtils.FindFileInSubfolders(baseFolder, expectedJsonFile);

            if (expectedFile == null)
                throw new FileNotFoundException($"Expected JSON file not found: {expectedJsonFile}");

            var expectedData = JToken.Parse(File.ReadAllText(expectedFile.FullName));
            var responseText = await response.Content.ReadAsStringAsync();

            bool comparisonResult = JsonUtils.CompareExpectedKeysAndValues(
                responseText, expectedData, "sid", "id", "deletedAt", "assets", "devices");

            Assert.True(comparisonResult, "Response body should match the expected data");

            string passMsg = $"The response body from the server matches the expected data in the JSON file: {expectedJsonFile}";
            ExtentReportHelper.LogPass(passMsg);
        }

        [Then(@"the response body should match the expected response from ""(.*)""")]
        public async Task ThenTheResponseBodyShouldMatchTheExpectedResponse(string expectedJsonFile)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\Framework_API_Automation"));
            string baseFolderPath = Path.Combine(projectRoot, "TestData");

            var baseFolder = new DirectoryInfo(baseFolderPath);
            var expectedFile = JsonUtils.FindFileInSubfolders(baseFolder, expectedJsonFile);

            if (expectedFile == null)
                throw new FileNotFoundException($"Expected JSON file not found: {expectedJsonFile}");

            var expectedFileContent = File.ReadAllText(expectedFile.FullName).Trim();
            JToken expectedData;

            try
            {
                expectedData = JToken.Parse(expectedFileContent);
            }
            catch (JsonReaderException)
            {
                expectedData = JValue.CreateString(expectedFileContent);
            }

            var responseText = await response.Content.ReadAsStringAsync();

            bool comparisonResult = JsonUtils.CompareExpectedResponse(
                responseText, expectedData, "sid", "id", "deletedAt");

            Assert.True(comparisonResult, "Response body should match the expected data");

            string passMsg = $"The response body from the server matches the expected data in the JSON file: {expectedJsonFile}";
            ExtentReportHelper.LogPass(passMsg);
        }



        private static readonly Dictionary<int, string> StatusCodeDescriptions = new Dictionary<int, string>
{
    {100, "100 : Continue – The server has received the request headers, and the client should proceed to send the request body."},
    {101, "101 : Switching Protocols – The requester has asked the server to switch protocols."},
    {102, "Processing – WebDAV; the server has received and is processing the request."},
    {103, "Early Hints – Used to return some response headers before final HTTP message."},
    {200, "200 : OK – The request has succeeded."},
    {201, "Created – The request has been fulfilled and resulted in a new resource being created."},
    {202, "Accepted – The request has been accepted for processing, but the processing is not complete."},
    {203, "Non-Authoritative Information – The server is returning information from a third party."},
    {204, "No Content – The server successfully processed the request, but is not returning any content."},
    {205, "Reset Content – Tells the client to reset the document view."},
    {206, "Partial Content – The server is delivering only part of the resource."},
    {300, "Multiple Choices – There are multiple options for the resource."},
    {301, "Moved Permanently – The resource has been moved to a new URL permanently."},
    {302, "Found – The resource resides temporarily under a different URI."},
    {303, "See Other – The response can be found under a different URI."},
    {304, "Not Modified – The resource has not been modified since the last request."},
    {305, "Use Proxy – The requested resource must be accessed through the proxy."},
    {307, "Temporary Redirect – The request should be repeated with another URI."},
    {308, "Permanent Redirect – The request and all future requests should be repeated using another URI."},
    {400, "400 : Bad Request – The server could not understand the request due to invalid syntax."},
    {401, "401 : Unauthorized – Authentication is required and has failed or not been provided."},
    {402, "Payment Required – Reserved for future use."},
    {403, "403 : Forbidden – The client does not have access rights to the content."},
    {404, "404 : Not Found – The server can not find the requested resource."},
    {405, "Method Not Allowed – The request method is known by the server but is not supported."},
    {406, "Not Acceptable – The server cannot produce a response matching the list of acceptable values."},
    {407, "Proxy Authentication Required – The client must first authenticate itself with the proxy."},
    {408, "Request Timeout – The server timed out waiting for the request."},
    {409, "Conflict – The request could not be completed due to a conflict."},
    {410, "Gone – The resource is no longer available and will not be available again."},
    {411, "Length Required – The request did not specify the length of its content."},
    {412, "Precondition Failed – The server does not meet one of the preconditions."},
    {413, "Payload Too Large – The request is larger than the server is willing or able to process."},
    {414, "URI Too Long – The URI provided was too long for the server to process."},
    {415, "Unsupported Media Type – The media format is not supported."},
    {416, "Range Not Satisfiable – The range specified cannot be fulfilled."},
    {417, "Expectation Failed – The server cannot meet the requirements of the Expect request-header field."},
    {418, "I'm a teapot – The server refuses to brew coffee because it is a teapot."},
    {422, "Unprocessable Entity – The request was well-formed but was unable to be followed due to semantic errors."},
    {429, "Too Many Requests – The user has sent too many requests in a given amount of time."},
    {500, "Internal Server Error – The server has encountered a situation it doesn't know how to handle."},
    {501, "Not Implemented – The request method is not supported by the server."},
    {502, "Bad Gateway – The server received an invalid response from the upstream server."},
    {503, "Service Unavailable – The server is not ready to handle the request."},
    {504, "Gateway Timeout – The server is acting as a gateway and cannot get a response in time."},
    {505, "HTTP Version Not Supported – The HTTP version used in the request is not supported."},
    {507, "Insufficient Storage – The server is unable to store the representation needed to complete the request."},
    {511, "Network Authentication Required – The client needs to authenticate to gain network access."}
};






        [Then(@"the response status code should be (.*)")]
        public async Task ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.NotNull(response);
            int actualStatusCode = (int)response.StatusCode;
            string statusMeaning = StatusCodeDescriptions.TryGetValue(actualStatusCode, out var desc)
            ? desc
            : "No description available for this status code.";

            string responseBody = await response.Content.ReadAsStringAsync();

            if (actualStatusCode == expectedStatusCode)
            {
                ExtentReportHelper.GetTest()?.Pass($"✅ API Tests PASS: {statusMeaning}");
                ExtentReportHelper.GetTest()?.Pass($"Actual Response received from server: {actualStatusCode}, and Expected Response should be: {expectedStatusCode} — Test PASSED ✅");
            }
            else
            {
                ExtentReportHelper.GetTest()?.Fail($"❌ FAILED API Tests: {statusMeaning}");
                ExtentReportHelper.GetTest()?.Fail($"Actual Response received from server: {actualStatusCode}, Expected: {expectedStatusCode} — Test FAILED ❌");
            }

            try
            {
                var json = JToken.Parse(responseBody);
            }
            catch (JsonReaderException) { }
            Assert.Equal(expectedStatusCode, actualStatusCode);
        }



    }


}
