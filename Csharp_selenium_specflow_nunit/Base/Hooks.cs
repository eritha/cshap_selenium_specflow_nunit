﻿using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using BoDi;
using Csharp_selenium_specflow_nunit.Utility;
using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Configuration;
using System.IO;
using TechTalk.SpecFlow;
using static Csharp_selenium_specflow_nunit.Utility.Helper;
using Sikuli4Net.sikuli_UTIL;

namespace Csharp_selenium_specflow_nunit.Base
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private static IWebDriver _driver;
        //private static TestData _testData;
        private static DriverFactory _driverFactory;

        // Extent report
        private static ExtentTest _feature; // Node for the Feature
        private static ExtentTest _scenario; // Node for the Scenario
        private static ExtentReports _extent; // ExtentReports object to be created

        // Here I am saving in the bin / debug folder of the project, the report file called index.html with timeStamp for each run
        private static string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        public static string pathReport;
        public static string reportHtml;

        // Sikuli
        //static APILauncher launcher = new APILauncher();

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            // Init data config 
            Configs.InitDataConfig();

            // Init report index.html
            pathReport = $"{AppDomain.CurrentDomain.BaseDirectory}" + timeStamp + Configs.varBrowser + Path.AltDirectorySeparatorChar;
            reportHtml = pathReport + Configs.varReportHtlm;

            // Create folder report
            Directory.CreateDirectory(Path.Combine(pathReport));

            // Here I inform the path of the file that will be generated by creating an ExtentHtmlReporter object
            var reporter = new ExtentHtmlReporter(reportHtml);
            reporter.Configuration().Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Dark;
            reporter.Configuration().ChartLocation = ChartLocation.Top;
            reporter.Configuration().ChartVisibilityOnOpen = true;
            reporter.Configuration().DocumentTitle = "Extent/NUnit Samples";
            reporter.Configuration().ReportName = "Extent/NUnit";

            // Instantiate the ExtentReports object
            _extent = new ExtentReports();
            _extent.AddSystemInfo("Host Name", Configs.varOrgName);
            _extent.AddSystemInfo("Environment", Configs.varEnvironment);
            _extent.AddSystemInfo("QC", Configs.varQcName);

            // Then attach in ExtentHtmlReporter
            _extent.AttachReporter(reporter);

            // Sikuli launch
            //launcher.Start();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            // Open folder & report after finishing test
            System.Diagnostics.Process.Start(pathReport);
            System.Diagnostics.Process.Start(reportHtml);
        }

        [BeforeFeature]
        public static void BeforeFeature()
        { 
            _driverFactory = new DriverFactory();
            _driver = _driverFactory.CreateDriver();

            _feature = _extent.CreateTest<Feature>(FeatureContext.Current.FeatureInfo.Title);
            _feature.AssignCategory(FeatureContext.Current.FeatureInfo.Tags);
        }

        [AfterFeature]
        public static void AfterFeature()
        {
            _driver?.Quit();
            //_driver?.Close();
            //extent.Flush();
            //extent.Close();

            _extent.Flush();
            // Sikuli Stop
            //launcher.Stop();
        }

        [BeforeScenario(Order = 0)]
        public void BeforeScenario()
        {
            // Handle Wait & Maximize the Browser
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Constant.TIME_IMPLICITWAIT);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(Constant.TIME_PAGELOADWAIT);
            _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(Constant.TIME_ASYNJAVASCRIPTWAIT);
            _driver.Manage().Window.Maximize();
            _objectContainer.RegisterInstanceAs(_driver);

            //_objectContainer.RegisterInstanceAs(_testData);

            //Create dynamic scenario name
            _scenario = _feature.CreateNode<Scenario>(ScenarioContext.Current.ScenarioInfo.Title);
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            // TODO
        }

        [AfterStep]
        public void InsertReportingStepsAndScreenshot()
        {
            var takesScreenshot = _driver as ITakesScreenshot;
            var pathFileName = "";

            if (ScenarioContext.Current.TestError != null)
            {
                if (takesScreenshot != null)
                {
                    var screenshot = takesScreenshot.GetScreenshot();
                    pathFileName = Path.Combine(pathReport, $"{ScenarioContext.Current.ScenarioInfo.Title}.jpg");
                    screenshot.SaveAsFile(pathFileName, ScreenshotImageFormat.Jpeg);

                    Console.WriteLine($"SCREENSHOT[ file:///{pathFileName} ]SCREENSHOT");

                    _scenario.AddScreenCaptureFromPath(pathFileName);


                    ////Comment("Expected: User has the ability to view all roles in the system.");
                    //TestContext.WriteLine(ScenarioContext.Current.ScenarioInfo.Title);
                    //TestContext.Write("<<<<<<<<< Out GIVEN WHEN THEN AND >>>>>>>>>");
                    //TestContext.Error.WriteLine("<<<<<<<<< Error GIVEN WHEN THEN AND >>>>>>>>>");
                    //TestContext.Out.WriteLine("<<<<<<<<< Out GIVEN WHEN THEN AND >>>>>>>>>");
                    ////TestContext.AddFormatter(ValueFormatterFactory(fun _->ValueFormatter(sprintf "%A")));
                    //var s = takesScreenshot.GetScreenshot();
                    //var v = @"C:\Screenshots\1.Jpeg";
                    //s.SaveAsFile(v, ScreenshotImageFormat.Jpeg);
                    //TestContext.AddTestAttachment(v, "Description Test Attachment");
                    //// Add that file to NUnit results
                    //TestContext.AddTestAttachment(pathFileName, ScenarioContext.Current.ScenarioInfo.Title); 
                }
            }

            // Add Gherkin to report
            switch (ScenarioStepContext.Current.StepInfo.StepDefinitionType)
            {
                case TechTalk.SpecFlow.Bindings.StepDefinitionType.Given:
                    _scenario.StepDefinitionGiven(pathFileName); // extension method Given
                    break;

                case TechTalk.SpecFlow.Bindings.StepDefinitionType.Then:
                    _scenario.StepDefinitionThen(pathFileName); // extension method Then
                    break;

                case TechTalk.SpecFlow.Bindings.StepDefinitionType.When:
                    _scenario.StepDefinitionWhen(pathFileName); // extension method When
                    break;
            }
        }
    }
}