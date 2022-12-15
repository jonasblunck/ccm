using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;

namespace CCMTests
{
  [TestClass]
  public class JSParserTests
  {
    [TestMethod]
    public void TestFunctionWithTwoArgumentsIsRecognizedAsFunction()
    {
      string code = "function Test(segmentA, segmentB) { }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("function", textParser.PeekNextKeyword());
    }

    [TestMethod]
    public void TestGlobalAssignmentNotRecognizedAsFunction()
    {
      string code = "Backbone.Event = { };";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsFalse(parser.NextIsFunction());
    }

    [TestMethod]
    public void TestMemberFunctionisRecognizedAsFunction()
    {
      string code = "bind : function(ev, callback) { ... }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("bind", textParser.PeekNextKeyword()); // parser should not have consumed any of the tokens
    }

    [TestMethod]
    public void GlobalScopeCodeNotRecognizedAsFunction()
    {
      string code = "if (x) { ... }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsFalse(parser.NextIsFunction());
    }

    [TestMethod]
    public void TestCanExtractNameOfFunctionWithTwoParams()
    {
      string code = "function Test(segmentA, segmentB) { }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.AreEqual("Test(segmentA,segmentB)", parser.GetFunctionName());
      Assert.AreEqual("{", textParser.NextKeyword());
    }

    [TestMethod]
    public void TestMemberFunctionNameIsRetrieved()
    {
      string code = "bind : function(ev, callback) { ... }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.AreEqual("bind(ev,callback)", parser.GetFunctionName());
    }

    [TestMethod]
    public void TestFunctionNameWithColon()
    {
      string code = "bind: function(ev, callback) { ... }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.AreEqual("bind(ev,callback)", parser.GetFunctionName());
    }
   
    [TestMethod]
    public void TestFunctionAssignment()
    {
      string code = "Backbone.View = function(options) {}";
      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("Backbone.View(options)", parser.GetFunctionName());
    }

    [TestMethod]
    public void TestAnonymousFunctionIsRecognizedAsFunction()
    {
      string code = "function () { }";
      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsFunction());
    }

    [TestMethod]
    public void TestAnonymousFunctionIsNameAnonymous()
    {
      string code = "function (par1,par2) { }";
      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);
      Assert.IsTrue(parser.NextIsFunction());

      Assert.AreEqual("function(par1,par2)", parser.GetFunctionName());
    }

    [TestMethod]
    public void TestMethodNamesWithDoubleNamesAreRecognized()
    {
      string code = "AFunction: function BFunction() {}";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("BFunction()", parser.GetFunctionName());
      Assert.AreEqual("{", textParser.PeekNextKeyword());
    }

    [TestMethod]
    [ExpectedException(typeof(JSParserException))]
    public void TestAdvanceAndReturnThrowsIfNextIsNotFunction()
    {
      string code = "class X { function BFunction() {}";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      parser.GetFunctionName();
    }

    [TestMethod]
    public void TestNextFunctionIsWiredUpAndReturnsName()
    {
      string code = "function Test(segmentA, segmentB) { }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      try
      {
        parser.AdvanceToNextFunction();

        Assert.Fail(); // should not get here
      }
      catch (CCCParserSuccessException function)
      {
        Assert.AreEqual("Test(segmentA,segmentB)", function.Function);
      }
    }

    [TestMethod]
    public void TestFindFunctionInClass()
    {
      string code = "class MyClass { function foo() { } }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      try
      {
        parser.AdvanceToNextFunction();
        Assert.Fail();
      }
      catch (CCCParserSuccessException function)
      {
        Assert.AreEqual("MyClass::foo()", function.Function);
      }
    }

    [TestMethod]
    public void TestAnonFunctionWithoutParametersRecognizedAsFunction()
    {
      string code = "greet() { }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("greet()", parser.GetFunctionName());
      Assert.AreEqual("{", textParser.PeekNextKeyword());
    }

    [TestMethod]
    public void TestNestedClassesWithFunctionFindFunction()
    {
      string code = "class MyClass { class MyClass2 { function foo() { } } }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      try
      {
        parser.AdvanceToNextFunction();
        Assert.Fail();
      }
      catch (CCCParserSuccessException function)
      {
        Assert.AreEqual("MyClass::MyClass2::foo()", function.Function);
      }

      Assert.AreEqual("{", textParser.PeekNextKeyword());
    }

    [TestMethod]
    public void TestModulesRecognizedAsContext()
    {
      string code = "module MyNamespace { export class X { grey() {} } }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      try
      {
        parser.AdvanceToNextFunction();
        Assert.Fail();
      }
      catch (CCCParserSuccessException function)
      {
        Assert.AreEqual("MyNamespace::X::grey()", function.Function);
      }

      Assert.AreEqual("{", textParser.PeekNextKeyword());
    }

    [TestMethod]
    public void TestConsumesTypeScriptInterfaces()
    {
      string code = "interface Thing { intersect: (ray: Ray); normal: (pos: Vector); } NEXT";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      Assert.IsTrue(parser.NextIsInterface());

      parser.ConsumeInterface();

      Assert.AreEqual("NEXT", textParser.NextKeyword());

    }

    [TestMethod]
    [ExpectedException(typeof(JSParserException))]
    public void TestThrowsIfCallingConsumeInterfaceWhenNextIsNotInterface()
    {
      string code = "class MyClass { class MyClass2 { function foo() { } } }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);
      parser.ConsumeInterface();
    }

    [TestMethod]
    public void TestFindsFunctionAfterInterfaceDefinition()
    {
      string code = "interface Thing { member: ();  foo() {} } boo() {}";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);

      try
      {
        parser.AdvanceToNextFunction();
        Assert.Fail();
      }
      catch (CCCParserSuccessException function)
      {
        Assert.AreEqual("boo()", function.Function);
      }

      Assert.AreEqual("{", textParser.NextKeyword());
    }

    [TestMethod]
    public void TestCanFindAnonTypeScriptFunctionWithReturnType()
    {
      string code = "greet(): int { }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);
      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("greet()", parser.GetFunctionName());
      Assert.AreEqual("{", textParser.NextKeyword());
    }

    [TestMethod]
    public void TestCanFindTypeScriptFunctionWithReturnType()
    {
      string code = "function greet(): int { }";

      LookAheadLangParser textParser = LookAheadLangParser.CreateJavascriptParser(TestUtil.GetTextStream(code));
      var parser = new JSParser(textParser);
      Assert.IsTrue(parser.NextIsFunction());
      Assert.AreEqual("greet()", parser.GetFunctionName());
      Assert.AreEqual("{", textParser.NextKeyword());
    }
  }
}
