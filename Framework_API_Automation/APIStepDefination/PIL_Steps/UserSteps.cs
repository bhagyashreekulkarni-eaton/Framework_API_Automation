using Framework_API_Automation.Reusables;
using Framework_API_Automation.Utilities.Reporting;
using Framework_API_Automation.Utilities;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Reqnroll;



namespace Framework_API_Automation.APIStepDefination.PIL_Steps
{

    [Binding]
    public class UserSteps
    {

        public static int PublisherId;
        public static int AdminId;
        public static readonly string URL = "https://pi-stage.serviceranger4.com/graphql";


  

        [When(@"the user sends a POST request with GraphQL query from ""(.*)"" and variables: firstName=""(.*)"", lastName=""(.*)"", email=""(.*)"", role=""(.*)""")]
        public async Task WhenTheUserSendsAPostRequestWithGraphQLQueryAndVariablesforpublisherUser(string graphqlFileName, string firstName, string lastName, string email, string role)
        {
            string graphqlQuery = JsonUtils.ReadFileFromSubfolders(graphqlFileName);
            var variables = new JObject
            {
                ["firstName"] = firstName,
                ["lastName"] = lastName,
                ["email"] = email,
                ["role"] = role
            };
            string graphqlVariablesJson = variables.ToString();
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequestWithGraphqlAsync(URL, null, null, null, null, null, null, 3, graphqlQuery, graphqlVariablesJson, null);
            var rawResponse = await CommonSteps.response.Content.ReadAsStringAsync();
            var jsonData = JToken.Parse(rawResponse);
            PublisherId = jsonData["data"]?["createNewUser"]?["id"]?.Value<int>() ?? 0;
            ExtentReportHelper.GetTest()?.Info("Extracted userId: " + PublisherId);            
           
        }
    

        [When(@"the user sends a POST request with GraphQL query for delete a user")]
        public async Task WhenTheUserSendsAPostRequestForDeleteAUser()
        {
            string graphqlQuery = JsonUtils.ReadFileFromSubfolders("DeleteUserQuery.graphql");

            var variables = new JObject
            {
                ["id"] = PublisherId
            };

            string graphqlVariablesJson = variables.ToString();
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequestWithGraphqlAsync(URL, null, null, null, null, null, null, 3, graphqlQuery, graphqlVariablesJson, null);
        }


        [When(@"the user sends a POST request with GraphQL query for create a admin user from ""(.*)"" and variables: firstName=""(.*)"", lastName=""(.*)"", email=""(.*)"", role=""(.*)""")]
        public async Task WhenTheUserSendsAPostRequestforadminWithGraphQLQueryAndVariables(string graphqlFileName, string firstName, string lastName, string email, string role)
        {
            string graphqlQuery = JsonUtils.ReadFileFromSubfolders(graphqlFileName);
            var variables = new JObject
            {
                ["firstName"] = firstName,
                ["lastName"] = lastName,
                ["email"] = email,
                ["role"] = role
            };
            string graphqlVariablesJson = variables.ToString();
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequestWithGraphqlAsync(URL, null, null, null, null, null, null, 3, graphqlQuery, graphqlVariablesJson, null);
            var rawResponse = await CommonSteps.response.Content.ReadAsStringAsync();
            var jsonData = JToken.Parse(rawResponse);
            AdminId = jsonData["data"]?["createNewUser"]?["id"]?.Value<int>() ?? 0;
            ExtentReportHelper.GetTest()?.Info("Extracted adminId: " + AdminId);

        }



        [When(@"the user sends a POST request with GraphQL query for delete a admin user")]
        public async Task WhenTheUserSendsAPostRequestForDeleteAAdminUser()
        {
            string graphqlQuery = JsonUtils.ReadFileFromSubfolders("DeleteUserQuery.graphql");
            var variables = new JObject
            {
                ["id"] = AdminId
            };

            string graphqlVariablesJson = variables.ToString();
            CommonSteps.response = await PIL_ApiRequestsReusables.SendPostRequestWithGraphqlAsync(URL, null, null, null, null, null, null, 3, graphqlQuery, graphqlVariablesJson, null);
      
        }




    }




}


       
