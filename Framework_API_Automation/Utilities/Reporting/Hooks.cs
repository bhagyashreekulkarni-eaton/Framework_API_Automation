using Framework_API_Automation.APIStepDefination.PIL_Steps;
using Framework_API_Automation.Reusables;
using Framework_API_Automation.Utilities.Reporting;
using Newtonsoft.Json.Linq;
using Reqnroll;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Framework_API_Automation.Utilities.Reporting
{
    [Binding]
    public class Hooks
    {
       // public static string Jtoken;
       // public static string ViewingSessionId;

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            // Initialize HTTP clients
            PIL_ApiRequestsReusables.InitializeClients();

            // Perform login to get session cookie
            string jsonBody = @"{ ""email"": ""lokeshrsontakke@eaton.com"", ""password"": ""Krunal@123"" }";
            var response = await PIL_ApiRequestsReusables.SendPostRequest("/login-user", null, null, null, null, null, null, 4, jsonBody, null);
            var responseText = await response.Content.ReadAsStringAsync();

            // Extract session cookie
            //if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            //{
            //    foreach (var header in cookieHeaders)
            //    {
            //        if (header.StartsWith("connect.sid"))
            //        {
            //            Jtoken = header.Split(';')[0].Split('=')[1].Trim();
            //            break;
            //        }
            //    }
            //}

            //Console.WriteLine("🔐 Session token initialized.");
        }

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            var title = scenarioContext.ScenarioInfo.Title;
            var tags = scenarioContext.ScenarioInfo.Tags;
            ExtentReportHelper.StartTest(title, "Scenario execution started", tags);
            ExtentReportHelper.LogInfo($"🚀 Starting scenario: {title}");
        }

        //[AfterStep]
        //public void AfterStep(ScenarioContext scenarioContext)
        //{
        //    var stepText = scenarioContext.StepContext.StepInfo.Text;
        //    if (scenarioContext.TestError != null)
        //    {
        //        ExtentReportHelper.LogFail($"❌ Step failed: {stepText}", scenarioContext.TestError);
        //    }
        //    else
        //    {
        //        ExtentReportHelper.LogInfo($"✅ Step passed: {stepText}");
        //    }
        //}

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            if (scenarioContext.TestError != null)
            {
                ExtentReportHelper.LogFail("❌ Scenario failed", scenarioContext.TestError);
            }
            else
            {
                ExtentReportHelper.LogPass("✅ Scenario passed");
            }

            ExtentReportHelper.LogInfo("📄 Scenario execution completed.");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            ExtentReportHelper.FlushReport();
        }
    }
}
