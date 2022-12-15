using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;

namespace CCMTests
{
    [TestClass]
    public class PSParserTests
    {
        [TestMethod]
        public void TestNextIsFunction_WithSignatureParameters()
        {
            string code = "function Get-NextFunction([string] $body) { Write-Host $body } ";

            LookAheadLangParser textParser = LookAheadLangParser.CreatePowerShellParser(TestUtil.GetTextStream(code));
            var parser = new PSParser(textParser);

            Assert.IsTrue(parser.NextIsFunction());
        }

        [TestMethod]
        public void TestNextIsFunction_WithBodyParameters()
        {
            string code = "function Write-Body { param([string] $body) Write-Host $body } ";

            LookAheadLangParser textParser = LookAheadLangParser.CreatePowerShellParser(TestUtil.GetTextStream(code));
            var parser = new PSParser(textParser);

            Assert.IsTrue(parser.NextIsFunction());
        }

        [TestMethod]
        public void TestAdvanceToFunction()
        {
            try
            {
                string code = "function Get-NextFunction([string] $body) { Write-Host $body } ";

                LookAheadLangParser textParser = LookAheadLangParser.CreatePowerShellParser(TestUtil.GetTextStream(code));
                var parser = new PSParser(textParser);

                parser.AdvanceToNextFunction();
                Assert.Fail(); // we should not get here
            }
            catch (CCCParserSuccessException success)
            {
                Assert.AreEqual("Get-NextFunction", success.Function);
            }
        }

    }
}

/*
 * 
 * Example functions we should support
 * 

function Echo-BodyParams
{
    param
    (
       [string] $InputText
    )

    Write-Host $InputText
}

function Echo-HeaderParams
(
    [string] $InputText
)
{
   Write-Host $InputText
}

*/