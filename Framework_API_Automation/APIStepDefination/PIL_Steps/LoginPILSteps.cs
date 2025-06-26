using Framework_API_Automation.Reusables;
using Framework_API_Automation.Utilities.Reporting;
using System;
using System.Threading.Tasks;
using Reqnroll;
using Framework_API_Automation.Utilities;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using Microsoft.Playwright;
using Newtonsoft.Json;
using Xunit;
using System.Net.Http;
using System.Web.UI.WebControls;
using AventStack.ExtentReports.Model;
using System.Text.Json.Nodes;

namespace Framework_API_Automation.APIStepDefination.PIL_Steps
{

    [Binding]
    public class LoginPILSteps
    {

        public static readonly string URL = "https://pi-stage.serviceranger4.com/graphql";
        //  public static HttpResponseMessage response;


        

        [When(@"the user sends a POST request for login to ""(.*)"" with ""(.*)"" and ""(.*)""")]
        public async Task WhenTheUserSendsAPOSTRequestForLoginTo(string endpoint, string email, string password)
        {
            string jsonBody = $@"{{ ""email"": ""{email}"", ""password"": ""{password}"" }}";
            ExtentReportHelper.GetTest()?.Info("Credential: " + jsonBody);
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequest(endpoint, null, null, null, null, null, null, 4, jsonBody, null);
        }

        



        [When(@"the user login to PIL console with ""(.*)"" and ""(.*)""")]
        public async Task WhenTheUserSendsAPOSTRequestForLogin(string email, string password)
        {
            string jsonBody = $@"{{ ""email"": ""{email}"", ""password"": ""{password}"" }}";
            ExtentReportHelper.GetTest()?.Info("Credential: " + jsonBody);
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequest("/login-user", null, null, null, null, null, null, 4, jsonBody, null);              
        }




        [When(@"the user sends a POST request with GraphQL query from ""(.*)"" for SendResetPasswordLink to ""(.*)""")]
        public async Task WhenTheUserSendsAPostRequestForSendResetPasswordLink(string graphqlFileName, string emailId)
        {
            string graphqlQuery = JsonUtils.ReadFileFromSubfolders(graphqlFileName);
            var variables = new JObject
            {
                ["email"] = emailId
            };

            var graphqlvaribles = variables.ToString();
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequestWithGraphqlAsync(URL, null, null, null, null, null, null, 3, graphqlQuery, graphqlvaribles, null);
        }



        [Then(@"the response message should say ""Missing credentials""")]
        public async Task ValidateResponseBody()
        {
            string ExpectedResponseBody = "Missing credentials";
            String ActualResponseBody = await CommonSteps.response.Content.ReadAsStringAsync();
            Assert.Equal(ExpectedResponseBody, ActualResponseBody);
            ExtentReportHelper.GetTest()?.Pass("Reset password response body has been successfully validated.");

        }


        [Then(@"the response message should say ""Login failed with given credentials.""")]
        public async Task ValidateLoginInvalidResponseBody()
        {
            string ExpectedResponseBody = "Login failed with given credentials.";
            String ActualResponseBody = await CommonSteps.response.Content.ReadAsStringAsync();
            Assert.Equal(ExpectedResponseBody, ActualResponseBody);
            ExtentReportHelper.GetTest()?.Pass("Login Invalid response body has been successfully validated.");

        }






    }
}

    

