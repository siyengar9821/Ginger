using Amdocs.Ginger.CoreNET.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using GingerTestHelper;
using System.Globalization;
using Ginger.Reports.GingerExecutionReport;
using System.IO;

namespace Ginger.Reports.Tests
{
    [TestClass]
    public class ReportsTest
    {
        [TestMethod]  [Timeout(60000)]
        public void ActivityReportTest()
        {


            string ActivityReportFile = GingerTestHelper.TestResources.GetTestResourcesFile(@"Reports" + Path.DirectorySeparatorChar + "Activity.txt");
            try
            {

                ActivityReport AR = (ActivityReport)JsonLib.LoadObjFromJSonFile(ActivityReportFile, typeof(ActivityReport));
                Assert.AreEqual("Passed", AR.RunStatus);
                Assert.AreEqual(2044, AR.Elapsed);
            }

            catch (Exception Ex)
            {
                Assert.Fail(Ex.Message);
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void BusinessflowReportTest()
        {
            //Arrange
            string BusinessFlowReportFile = GingerTestHelper.TestResources.GetTestResourcesFile(@"Reports" + Path.DirectorySeparatorChar + "BusinessFlow.txt");
            try
            {

                BusinessFlowReport BFR = (BusinessFlowReport)JsonLib.LoadObjFromJSonFile(BusinessFlowReportFile, typeof(BusinessFlowReport));
                Assert.AreEqual("Failed", BFR.RunStatus);
                Assert.AreEqual(float.Parse("36.279", CultureInfo.InvariantCulture), BFR.ElapsedSecs.Value);
            }

            catch (Exception Ex)
            {
                Assert.Fail(Ex.Message);
            }
        }


        [TestMethod]
        //[Timeout(60000)]
        public void GenrateLastExecutionHTMLReportTest()
        {
            //Arrange
            string BusinessFlowReportFolder = GingerTestHelper.TestResources.GetTestResourcesFolder(@"Reports" + Path.DirectorySeparatorChar + "AutomationTab_LastExecution"+ Path.DirectorySeparatorChar);
            ReportInfo RI = new ReportInfo(BusinessFlowReportFolder);
            string templatesFolder = (ExtensionMethods.getGingerEXEFileName() + @"Reports"+ Path.DirectorySeparatorChar  + "GingerExecutionReport" + Path.DirectorySeparatorChar).Replace("Ginger.exe", "");
            HTMLReportConfiguration selectedHTMLReportConfiguration = HTMLReportConfiguration.SetHTMLReportConfigurationWithDefaultValues("DefaultTemplate", true);
            string hTMLOutputFolder = TestResources.GetTempFolder("HTMLReports");


            //Act
            string report = Ginger.Reports.GingerExecutionReport.ExtensionMethods.NewFunctionCreateGingerExecutionReport(RI, selectedHTMLReportConfiguration, templatesFolder, hTMLOutputFolder );

            //Assert

        }

    }
}
