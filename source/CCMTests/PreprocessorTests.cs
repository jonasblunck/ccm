using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;

namespace CCMTests
{
  [TestClass]
  public class PreprocessorTests
  {
    [TestMethod]
    public void NoDirectives()
    {
      string text = "the lazy brown fox";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));

      StreamReader result = preprocessor.Process();

      Assert.AreEqual(text, result.ReadToEnd());
    }

    [TestMethod]
    public void NextIsIfDef()
    {
      string text = "#ifdef";

      Assert.AreEqual(false, Preprocessor.NextIsIfndef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
      Assert.AreEqual(true, Preprocessor.NextIsIfdef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
    }

    [TestMethod]
    public void NextIsIfNDef()
    {
      string text = "#ifndef";

      Assert.AreEqual(false, Preprocessor.NextIsIfdef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
      Assert.AreEqual(true, Preprocessor.NextIsIfndef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
    }

    [TestMethod]
    public void NextIsIfDefWithSpace()
    {
      string text = "# ifdef";

      Assert.AreEqual(true, Preprocessor.NextIsIfdef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
    }

    [TestMethod]
    public void NextIsIf()
    {
      string text = "#if";

      Assert.AreEqual(true, Preprocessor.NextIsIfdef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
    }

    [TestMethod]
    public void NextIsNotIfDef()
    {
      string text = "#pragma";

      Assert.AreEqual(false, Preprocessor.NextIsIfdef(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
    }

    [TestMethod]
    public void Evalute1()
    {
      string text = "1";

      Preprocessor expression = new Preprocessor(TestUtil.GetTextStream(text));

      Assert.AreEqual(true, expression.EvaluateExpression());
    }

    [TestMethod]
    public void Evalute0()
    {
      string text = "0";

      Preprocessor expression = new Preprocessor(TestUtil.GetTextStream(text));

      Assert.AreEqual(false, expression.EvaluateExpression());
    }
    [TestMethod]
    public void IsEndDirective()
    {
      string text = "#endif";

      Assert.AreEqual(true, Preprocessor.NextIsEndif(LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text))));
    }
    [TestMethod]
    public void IfDef1()
    {
      string text = "#ifdef 1 \r\n" +
                    "the lazy brown fox \r\n" +
                    "#endif\r\n";

      string expect =
                    "         \r\n" +
                    "the lazy brown fox \r\n" +
                    "      \r\n";


      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.AreEqual(expect, result);
      Assert.IsTrue(result.Contains("lazy brown fox"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void PragmaRemoved()
    {
      string text = "#pragma warning(x: disable)\r\n" +
                    "text";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("text"));
      Assert.IsFalse(result.Contains("disable"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void IfDef0()
    {
      string text = "#ifdef 0 \r\n" +
                    " the lazy brown fox \r\n" +
                    "#endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsFalse(result.Contains("#"));
      Assert.IsFalse(result.Contains("lazy brown fox"));
      Assert.IsFalse(result.Contains("ifdef"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void IfDef0InsideText()
    {
      string text = "text" +
                    "#ifdef 0 \r\n" +
                    " the lazy brown fox \r\n" +
                    "#endif\r\n" +
                    "more text";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.StartsWith("text"));
      Assert.IsTrue(result.EndsWith("more text"));
      Assert.IsFalse(result.Contains("if"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void IfElse()
    {
      string text = "  #ifdef 0 \r\n" +
                    " the lazy brown fox \r\n" +
                    "#else \r\n" +
                    " energizing bunny \r\n" +
                    "#endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("energizing bunny"));
      Assert.AreEqual(text.Length, result.Length);
      Assert.IsFalse(result.Contains("fox"));
    }

    [TestMethod]
    public void IfDefUnknownDefine()
    {
      string text = "#ifdef UNK_DEFINE \r\n" +
                    " the lazy brown fox \r\n" +
                    "#else \r\n" +
                    " energizing bunny \r\n" +
                    "#endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("energizing bunny"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void MultipleIfDefs()
    {
      string text = "#ifdef UNK1\r\n" +
                    "#ifdef UNK2\r\n" +
                    "  the lazy brown fox \r\n" +
                    "#endif \r\n" +
                    "#endif \r\n" +
                    "noway";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("noway"));
      Assert.IsFalse(result.Contains("fox"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void MultipleIfDefsWithElse()
    {
      string text = "#ifdef 1\r\n" +
                    "#ifdef UNK2\r\n" +
                    "  the lazy brown fox \r\n" +
                    "#endif \r\n" +
                    "#endif \r\n" +
                    "yesway2";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("yesway2"));
      Assert.IsFalse(result.Contains("fox"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void MultiLineDefine()
    {
      string text = "#define MCR one \\ \r\n" +
                    "  two \\ \r\n" +
                    "  three \r\n" +
                    "included";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("included"));
      Assert.IsFalse(result.Contains("two"));
      Assert.IsFalse(result.Contains("one"));
      Assert.IsFalse(result.Contains("three"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void MultiLineDefineWithinIfdef()
    {
      string text = "#ifdef 1\r\n" +
                    "#define MCR one \\ \r\n" +
                    "  two \\ \r\n" +
                    "  three \r\n" +
                    "endif \r\n" +
                    "included";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("included"));
      Assert.IsFalse(result.Contains("one"));
      Assert.IsFalse(result.Contains("two"));
      Assert.IsFalse(result.Contains("three"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void IfNotDefined()
    {
      string text = "#ifndef UNK_DEFINE \r\n" +
                    "#ifndef UNK2 \r\n" +
                    " the lazy brown fox \r\n" +
                    "#endif\r\n" +
                    "#else \r\n" +
                    " energizing bunny \r\n" +
                    "#endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("the lazy brown fox"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void IfElseParanthesis()
    {
      string text = "#if(DEBUG)\r\n" +
                    " ddd \r\n" +
                    "#else \r\n" +
                    "uuuu\r\n" +
                    "#endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("uuuu"));
      Assert.IsFalse(result.Contains("ddd"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void MultipleIfDefsShouldExclude()
    {
      string text = "#ifdef _THE_DEFINE\r\n" +
                    "#ifdef _THE_DE2\r\n " +
                    "#ifdef ___DD\r\n" +
                    " the lazy brown fox \r\n" +
                    "#endif \r\n" +
                    "#endif \r\n" +
                    "#endif \r\n" +
                    "bunny";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("bunny"));
      Assert.IsFalse(result.Contains("fox"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void ConsumedCharsAreSpaced()
    {
      string text = "//j\r\n" +
                    "text";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      StreamReader result = preprocessor.Process();

      Assert.AreEqual("   \r\ntext", result.ReadToEnd());
    }

    [TestMethod]
    public void IfDefsWithWhiteSpaceIsHandled()
    {
      string text = "# ifdef _THE_DEFINE\r\n" +
                    " yada \r\n " +
                    "# else  ___DD\r\n" +
                    "//comment\r\n" +
                    "bunny\r\n" +
                    "# endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("bunny"));
      Assert.IsFalse(result.Contains("#"));
      Assert.IsFalse(result.Contains("/"));
      Assert.IsFalse(result.Contains("yada"));
      Assert.IsFalse(result.Contains("//"));
      Assert.IsFalse(result.Contains("comment"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void NestedIfDefsWithParentInclusive()
    {
      string text = "#if 1\r\n" +
                    "#if MY_MUTEX \r\n" +
                    "# define mutex() (1)\r\n" +
                    "#else\r\n" +
                    "static int mutex() {} \r\n" +
                    "#endif\r\n" +
                    "#endif\r\n";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("mutex()"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void IfnDefNestedInsideIfdef1()
    {
      string text = "#ifndef 1\r\n" +
                    "#ifndef MYS \r\n" +
                    "FINDERS KEEP\r\n" +
                    "#else\r\n" +
                    "static int mutex() {} \r\n" +
                    "#endif\r\n" +
                    "#endif\r\n" +
                    "bunny";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("bunny"));
      Assert.IsFalse(result.Contains("static int"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void NestedIfDefsWithParentExclusive()
    {
      string text = "#if 0\r\n" +
                    "#if MY_MUTEX \r\n" +
                    "# define mutex() (1)\r\n" +
                    "#else\r\n" +
                    "static int mutex() {} \r\n" +
                    "#endif\r\n" +
                    "#endif\r\n" +
                    "bunny";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("bunny"));
      Assert.IsFalse(result.Contains("mutex"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void DefinesAreRemoved()
    {
      string text = "#define ObjectIdFromObjectTypeAndInstanceId(cls,id)     (System::UInt32)((((System::UInt32)(cls)) << 22) | ((System::UInt32)(id)))\r\n" +
                    "text";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("text"));
      Assert.IsFalse(result.Contains("System"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void CommentBlockRemoved()
    {
      string text = "/* test */abc";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("abc"));
      Assert.IsTrue(result.StartsWith(" "));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void SingleLineCommentRemoved()
    {
      string text = "// test \r\n" +
                    "abc";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("abc"));
      Assert.IsTrue(result.StartsWith(" "));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void CommentBlockWithHalfCommentIsRemoved()
    {
      string text = "/*/ test \r\n" +
                    "*/abc";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.EndsWith("abc"));
      Assert.IsFalse(result.Contains("test"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void SingleLineCommentEmbeddedInBlock()
    {
      string text = "/*// don't care about this one...\r\n*/def";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("def"));
      Assert.IsFalse(result.Contains("care"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void UnclosedQuotedTextInComment()
    {
      string text = "// not properly ' closed \r\n" +
                    "/* don't care \r\n" +
                    "*/include";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("include"));
      Assert.IsFalse(result.Contains("/"));
      Assert.IsFalse(result.Contains("*"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void BlockCommentInsideBlockCommentIsConsumed()
    {
      string text = "/* don't care \r\n" +
                    "don not care \r\n" +
                    "*/include";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.Contains("include"));
      Assert.IsFalse(result.Contains("/"));
      Assert.IsFalse(result.Contains("*"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void DiffStyledStreamRemovesBlock()
    {
      string text = "152,153d153 \r\n " +
                    "< #if DEBUG \r\n " +
                    "< 155,167d154 \r\n " +
                    " public override string ToString() { return 1.ToString(); } \r\n " +
                    " < #endif \r\n" +
                    "188,22";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.StartsWith("152,153d153"));
      Assert.IsTrue(result.EndsWith("188,22"));
      Assert.IsFalse(result.Contains("ToString()"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void DiffStylePartialConditional()
    {
      string text = "152,153d153 \r\n " +
                    "< #endif \r\n " +
                    " public override string ToString() { return 1.ToString(); } \r\n " +
                    "188,22";

      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.IsTrue(result.StartsWith("152,153d153"));
      Assert.IsTrue(result.EndsWith("188,22"));
      Assert.AreEqual(text.Length, result.Length);
    }

    [TestMethod]
    public void TestMultilineCppCommentWithExtraStarOnlyRemovesComment()
    {
      string text = "/* comment \r\n more comment\r\n text **/text";
      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.AreEqual("text", result.Trim());
      Assert.AreEqual(text.Length, result.Length);

    }

    [TestMethod]
    public void TestTabAfterEndIfDoesntThrowParserOff()
    {
      string text = "#ifdef false \r\n" + 
                    "#ifdef false \r\n" +
                    " // something \r\n" +
                    "#else \r\n" +
                    " // something \r\n" +
                    "#endif\r\n" +
                    "#endif\t //comment\r\n" +
                    "text";
                    
      Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));
      string result = preprocessor.Process().ReadToEnd();

      Assert.AreEqual("text", result.Trim());
      Assert.AreEqual(text.Length, result.Length);
    }


    [TestMethod]
    public void Test()
    {
        string text = @"
//

function B()
{
}
";
        Preprocessor preprocessor = new Preprocessor(TestUtil.GetTextStream(text));

        StreamReader afterProcessing = TestUtil.GetTextStream(preprocessor.Process().ReadToEnd());
        StreamReader orignal = TestUtil.GetTextStream(text);

        Assert.AreEqual(TestUtil.GetNumLines(orignal), TestUtil.GetNumLines(afterProcessing));
    }
        

  }
}
