using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SsisUnit;
using SsisUnit.Packages;
using System;
using System.IO;

namespace SSIS.Tests
{
    [TestClass]
    public class DemoTest
    {
        ConfigurationSettings config;

        [TestInitialize]
        public void GetTestData()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");
            config = JsonConvert.DeserializeObject<ConfigurationSettings>(File.ReadAllText(filepath));
        }

        [TestMethod]
        public void TestMethod()
        {
            var testSuite = new SsisTestSuite();
            var projectPath = config.ProjectPath;
            var template = "Product.dtsx";
            var templatePath = config.TemplatePath;
            PackageRef p = new PackageRef("ProductPackage", projectPath, template, SsisUnit.Enums.PackageStorageType.FileSystem);

            var conn = new System.Data.SqlClient.SqlConnectionStringBuilder(config.ConnectionString);

            var DatabaseConnectionString = $"{ conn.ConnectionString};Provider=SQLNCLI11.1;Auto Translate=False;Integrated Security =SSPI;";

            ConnectionRef con1 = new ConnectionRef("Destination",
                DatabaseConnectionString,
                ConnectionRef.ConnectionTypeEnum.ConnectionString
                );

            var originalProductListConnection = new PropertyCommand(testSuite)
            {
                Name= "OriginalProductList",
                PropertyPath= @"\Package.Connections[OriginalProductList].Properties[ConnectionString]",
                Operation= PropertyCommand.PropertyOperation.Set,
                Value= config.OriginalFilePath
            };

            //var stagedProductListConnection = new PropertyCommand(testSuite)
            //{
            //    Name="StagedProductList",
            //    PropertyPath = @"\Package.Connections[StagedProductList].Properties[ConnectionString]",
            //    Operation =PropertyCommand.PropertyOperation.Set,
            //    Value= @"C:\Users\nishant-mishra\source\repos\SSIS_UnitTesting\temp\ProductList.txt"
            //};

            testSuite.SetupCommands.Commands.Add(originalProductListConnection);
            //testSuite.SetupCommands.Commands.Add(stagedProductListConnection);
            testSuite.ConnectionList.Add(con1.ReferenceName, con1);
            testSuite.PackageList.Add(p.Name, p);

            //VariableCommand var1 = new VariableCommand(testSuite)
            //{
            //    Name = "fileExists",
            //    Operation = VariableCommand.VariableOperation.Set,
            //    Value = "false"
            //};

            //testSuite.SetupCommands.Commands.Add(var1);

            Test test1 = new Test(testSuite, "Verify No of RecordsinTable", templatePath+template, null, "{74BF5B2C-1017-4015-8B4A-36EEEFD44C06}");
            testSuite.Tests.Add(test1.Name, test1);

            //SqlCommand command1 = new SqlCommand(testSuite, "Destination", false, "select count(1) from dbo.Product");

            //test1.TestSetup.Commands.Add(command1);

            var assert1 = new SsisAssert(testSuite, test1, "Assert: Product has records", 6, false)
            {
                Command = new SqlCommand(testSuite, "Destination", true, "select count(1) from dbo.Product;")
            };
            

            test1.Asserts.Add(assert1.Name,assert1);
            testSuite.Execute();

            Assert.AreEqual(2, testSuite.Statistics.GetStatistic(SsisUnitBase.Enums.StatisticEnum.AssertPassedCount));
        }
    }
}


