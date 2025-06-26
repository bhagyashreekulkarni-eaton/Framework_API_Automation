
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using Microsoft.Playwright;
using Reqnroll;
using System;
using System.IO;
using System.Threading;

namespace Framework_API_Automation.Utilities.Reporting
{
    public class ExtentReportHelper
    {
        public static ExtentReports _extent;
        public static ExtentSparkReporter _sparkReporter;

        public static AsyncLocal<ExtentTest> _test = new AsyncLocal<ExtentTest>();


        public static readonly string TestResultsPath = FindTestResultsPath();
        public static readonly string ScreenshotPath = Path.Combine(TestResultsPath, "screenshots");

        static ExtentReportHelper()
        {
            Directory.CreateDirectory(TestResultsPath);
            Directory.CreateDirectory(ScreenshotPath);

            string reportPath = Path.Combine(TestResultsPath, "API-Tests-Execution-Report.html");

            _sparkReporter = new ExtentSparkReporter(reportPath);
            _sparkReporter.Config.DocumentTitle = "Test Execution Summary";
            _sparkReporter.Config.ReportName = "Automation Test Dashboard";

            _extent = new ExtentReports();
            _extent.AttachReporter(_sparkReporter);
            _extent.AddSystemInfo("Environment", "QA");
            _extent.AddSystemInfo("User", Environment.UserName);
        }

        private static string FindTestResultsPath()
        {
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                var potentialPath = Path.Combine(dir.FullName, "Framework_API_Automation", "TestResults");
                if (Directory.Exists(Path.Combine(dir.FullName, "Framework_API_Automation")) &&
                    Directory.Exists(potentialPath))
                {
                    return potentialPath;
                }
                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate Framework_API_Automation\\TestResults folder.");
        }

        private static readonly object _lock = new object();

        public static void StartTest(string testName, string description = "", string[] tags = null)
        {
            lock (_lock)
            {
                var test = _extent.CreateTest(testName, description);
                if (tags != null)
                {
                    foreach (var tag in tags)
                        test.AssignCategory(tag);
                }
                _test.Value = test;
            }
        }


        public static void LogPass(string message = "Test passed")
        {
            _test.Value?.Pass(message);
        }

        public static void LogFail(string message, Exception ex = null)
        {
            _test.Value?.Fail(message);
            if (ex != null)
                _test.Value?.Fail(ex);
        }

        
        public static void LogSkip(string reason)
        {
            _test.Value?.Skip(reason);
        }

        public static void LogInfo(string message)
        {
            _test.Value?.Info(message);
        }

        public static void CaptureScreenshot(IPage page, string testName)
        {
            string filePath = Path.Combine(ScreenshotPath, $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            page.ScreenshotAsync(new PageScreenshotOptions { Path = filePath }).Wait();
            _test.Value?.AddScreenCaptureFromPath(filePath);
        }

        public static void FlushReport()
        {
            _extent.Flush();
        }

        public static ExtentTest GetTest()
        {
            return _test.Value;
        }

        public static void AddSummary(int total, int passed, int failed, int skipped)
        {
            var summary = _extent.CreateTest("Test Summary");
            summary.Info($"Total: {total}");
            summary.Info($"Passed: {passed}");
            summary.Info($"Failed: {failed}");
            summary.Info($"Skipped: {skipped}");
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            ExtentReportHelper.FlushReport();
        }
    }
}
