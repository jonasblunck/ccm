using System;
using System.Collections.Generic;
using System.Text;
using CCMEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace CCMTests
{
  [TestClass]
  public class BlockAnalyzerTests
  {
    [TestMethod]
    public void AnalyzerConsumesBlockOnly()
    {
      string code = "" +
        "{              \r\n" +
        "  int x = 0;   \r\n" +
        "}              \r\n" +
        "{}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.ConsumeBlockCalculateAdditionalComplexity();

      Assert.AreEqual("{", parser.NextKeyword());
      Assert.AreEqual("}", parser.NextKeyword());
    }


    [TestMethod]
    public void AnalyzerConsumesNestedBlocks()
    {
      string code = "" +
        "{              \r\n" +
        "  { int x = 0; } \r\n" +
        "}              \r\n" +
        "{}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.ConsumeBlockCalculateAdditionalComplexity();

      Assert.AreEqual("{", parser.NextKeyword());
      Assert.AreEqual("}", parser.NextKeyword());
    }

    [TestMethod]
    public void ExpressionCounterConsumesExpression()
    {
      string code = "((x))(y)";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.CalcNumExpressionConditions();

      Assert.AreEqual("(", parser.NextKeyword());
      Assert.AreEqual("y", parser.NextKeyword());
      Assert.AreEqual(")", parser.NextKeyword());
    }

    [TestMethod]
    public void ExpressionCounterConsumes()
    {
      string code = "(x)\r\n" +
                    "more";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.CalcNumExpressionConditions();

      Assert.AreEqual("more", parser.NextKeyword());
    }

    [TestMethod]
    public void ExpressionCountAndOr()
    {
      string code = "((x > 2) && (y>2) || (b))";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(2, analyzer.CalcNumExpressionConditions());
    }


    [TestMethod]
    public void ConditionalBranchConsumesBranchOnly()
    {
      string code = "" +
        "if (x > 2) \r\n" +
        "{ y = x; } \r\n" +
        "else";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.CalcCCOnConditionalBranch();

      Assert.AreEqual("else", parser.PeekNextKeyword());
    }

    [TestMethod]
    public void EmptyStatement()
    {
      string code = "{ }";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(0, analyzer.ConsumeBlockCalculateAdditionalComplexity()); // I'm not adding a CC value based on { and }
    }

    [TestMethod]
    public void IfStatements()
    {
      string code = "" +
        "{          \r\n" +
        "if (x > 2) \r\n" +
        "{ y = x; } \r\n" +
        "else if (x > 3)\r\n" +
        " y = 4; \r\n" +
        "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity()); // I'm not adding a CC value based on { and }
    }

    [TestMethod]
    public void SwitchStatementConsumes()
    {
      string code = "" +
        "{ switch(x)   \r\n" +
        " {           \r\n" +
        "   case 1: return 2; \r\n" +
        "   case 2: return 3; \r\n" +
        " } } text \r\n";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.ConsumeBlockCalculateAdditionalComplexity();

      Assert.AreEqual("text", parser.NextKeyword());
    }

    [TestMethod]
    public void StructInitializationAndAssignment()
    {
      string code = "" +
        "{ \r\n" +
        " RECT rect = { 0, 0, 0, 0 }; \r\n" +
        " if (x) return; \r\n" +
        "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(1, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void SwitchStatementCalcsCases()
    {
      string code = "" +
        "{ switch(x)   \r\n" +
        " {           \r\n" +
        "   case 1: return 2; \r\n" +
        "   case 2: return 3; \r\n" +
        " } } } \r\n";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void SwitchCaseWithEmbeddedConditionals()
    {
      string code = "" +
        "{ switch(x)   \r\n" +
        " {           \r\n" +
        "   case 1: return 2; break;\r\n" +
        "   case 2:          " +
        "           if (x > u) return 3; \r\n" +
        "           break;" + 
        " } } }\r\n";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(3, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void VariablesWithInitializationLists()
    {
      string code = "" +
        "{           " +
        "  this.x = new List<string>( new string[] { \"if\", \"while\" } ); \r\n" +
        "  if (this.x.y) " +
        "    return u; " +
        "} more";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(1, analyzer.ConsumeBlockCalculateAdditionalComplexity());
      Assert.AreEqual("more", parser.NextKeyword());

    }

    [TestMethod]
    public void ConditionalExpressionEmbeddedCalls()
    {
      string code = "" +
        "(x != this.parser.PeekNext()) \r\n" +
        "this.parser.NextKeyword()";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      analyzer.CalcNumExpressionConditions();

      Assert.AreEqual("this.parser.NextKeyword", parser.NextKeyword());
    }

    [TestMethod]
    public void WhileWithoutScoping()
    {
        string code = "" +
        "{           " +
        "  while(true)\r\n" +
        "    if (this.x.y)\r\n" +
        "      return u;\r\n" +
        "  return x;\r\n" + 
        "} ";

        LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

        BlockAnalyzer analyzer = new BlockAnalyzer(parser);

        Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void SwitchEmbeddedIntoWhile()
    {
      string code = "{             " +
                    " while (true) {   " +
                    "  switch(x)) {   " +
                    "     case 3: break; " +
                    "     case 4: break; " +
                    "     default: break; " +
                    "  } " +
                    " }" +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(3, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void CatchCountedAsPath()
    {
      string code = "{             " +
                    "  try { \r\n  " +
                    "  } " +
                    "  catch (const ex&){} " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(1, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void PragmasRemoved()
    {
      string code = "{             " +
                    "  #pragma warning (disable: 333) \r\n  " +
                    "  int j; \r\n " +
                    "  return j; \r\n " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(0, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void MultipleCatchCountedAsDifferentPaths()
    {
      string code = "{             " +
                    "  try { \r\n  " +
                    "  } " +
                    "  catch (const ex&){} " +
                    "  catch (const ex2&){} " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void CodeInCatchCounted()
    {
      string code = "{             " +
                    "  try {       " +
                    "    int* p = 0 " +
                    "    *p = 2; " +
                    "  } " +
                    "  catch (const ex&) " +
                    "  {  " +
                    "     if (x > y) x++ " +
                    "  } " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void ReturnWithCreate()
    {
      string code = "{  " +
                    " return new ParentOCModifiedEventArgs(items) { IsCommitResult = contextInCommitStage }; " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));

      BlockAnalyzer analyzer = new BlockAnalyzer(parser);

      Assert.AreEqual(0, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    
    }

    [TestMethod]
    public void TestCaseInsideSwitchCounted_WhenSwitchBehaviorTraditionalInclude()
    {
      string code = "{             " +
                    "  switch(x)) {   " +
                    "     case 3: break; " +
                    "     case 4: break; " +
                    "     default: break; " +
                    "  } " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
      BlockAnalyzer analyzer = new BlockAnalyzer(parser, null, null, ParserSwitchBehavior.TraditionalInclude);

      Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void TestCaseInsideSwitchNotCounted_WhenSwitchBehaviorIgnoreCases()
    {
      string code = "{             " +
                    "  switch(x)) {   " +
                    "     case 3: break; " +
                    "     case 4: break; " +
                    "     default: break; " +
                    "  } " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
      BlockAnalyzer analyzer = new BlockAnalyzer(parser, null, null, ParserSwitchBehavior.IgnoreCases);

      Assert.AreEqual(0, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

    [TestMethod]
    public void TestBranchInCaseIncludedButNotCaseStatements_WhenSwitchBehaviorIgnoreCases()
    {
      // with ParserSwitchBehavior.IgnoreCases we ignore switch cases and just include branches inside of case statements

      string code = "{             " +
                    "  switch(x)) {   " +
                    "     case 3: if (a) { // do something } ; " +
                    "     case 4: break; " +
                    "     default: break; " +
                    "  } " +
                    "}";

      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
      BlockAnalyzer analyzer = new BlockAnalyzer(parser, null, null, ParserSwitchBehavior.IgnoreCases);

      Assert.AreEqual(1, analyzer.ConsumeBlockCalculateAdditionalComplexity());
    }

        [TestMethod]
        public void TestPowerShellBlock_Elseif()
        {
            string code = @"
{
   if ($var -gt 0)
   {
        Console.WriteLine('Greater');
   }
   elseif ($var -gt 0)
   {
        Console.WriteLine('Lesser');
   }
}
";
            LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
            BlockAnalyzer analyzer = new BlockAnalyzer(parser, null, null, ParserSwitchBehavior.IgnoreCases);

            Assert.AreEqual(2, analyzer.ConsumeBlockCalculateAdditionalComplexity());
        }

    }
}
