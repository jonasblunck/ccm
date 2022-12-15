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
        public void TestNextIsFunction()
        {
            string code = "function Get-NextFunction([string] $body) { Write-Host $body } ";

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
