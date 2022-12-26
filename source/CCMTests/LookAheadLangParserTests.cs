using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;
using System.IO;

namespace CCMTests
{
  [TestClass]
  public class LookAheadLangParserTests
  {
    [TestMethod]
    public void MultiPeek()
    {
      string code = "the lazy brown dog";

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(code));

      Assert.AreEqual("the", parser.PeekNextKeyword());
      Assert.AreEqual("lazy", parser.PeekNextKeyword(1));
      Assert.AreEqual("brown", parser.PeekNextKeyword(2));
      Assert.AreEqual("dog", parser.PeekNextKeyword(3));
    }

    [TestMethod]
    public void MultiPeekWithNext()
    {
      string code = "the lazy brown dog";

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(code));

      Assert.AreEqual("the", parser.PeekNextKeyword());
      Assert.AreEqual("lazy", parser.PeekNextKeyword(1));
      Assert.AreEqual("brown", parser.PeekNextKeyword(2));
      Assert.AreEqual("dog", parser.PeekNextKeyword(3));

      Assert.AreEqual("the", parser.NextKeyword());
      Assert.AreEqual("lazy", parser.NextKeyword());
      Assert.AreEqual("brown", parser.PeekNextKeyword());
      Assert.AreEqual("dog", parser.PeekNextKeyword(1));
      Assert.AreEqual("brown", parser.NextKeyword());
      Assert.AreEqual("dog", parser.NextKeyword());
    }

    [TestMethod]
    public void QuotedText()
    {
      string code = "std::list c = (\"kkl \"); \r\n";
      string[] tokens = new string[] { "#", " ", "\r", "\n", "\"" };

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(code));

      Assert.AreEqual("std", parser.NextKeyword());
      Assert.AreEqual("::", parser.NextKeyword());
      Assert.AreEqual("list", parser.NextKeyword());
      Assert.AreEqual("c", parser.NextKeyword());
      Assert.AreEqual("=", parser.NextKeyword());
      Assert.AreEqual("(", parser.NextKeyword());
      Assert.AreEqual("\"kkl \"", parser.NextKeyword());
      Assert.AreEqual(")", parser.NextKeyword());
      Assert.AreEqual(";", parser.NextKeyword());
    }

    [TestMethod]
    public void CanConsumeComment ()
    {
      string text = "/* this is comment ' */TEXT";

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text));
      
      Assert.AreEqual ("/* this is comment ' */", parser.ConsumeBlockComment());
      Assert.AreEqual ("TEXT", parser.NextKeyword());
    }

    [TestMethod]
    [ExpectedException(typeof(UnknownStructureException))]
    public void TestConsumeIllegalCommentThrows()
    {
      string text = "ss/*ent ' */TEXT";

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text));

      parser.ConsumeBlockComment();
    }

    [TestMethod]
    public void TestErrorMessageInExceptionSaysStructureIsUnknown()
    {
      string text = "ss/*ent ' */TEXT";

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(text));

      try
      {
        parser.ConsumeBlockComment();
        Assert.Fail();
      }
      catch (UnknownStructureException e)
      {
        Assert.AreEqual("Expected beginning of comment.", e.Message);
      }
    }

    [TestMethod]
    public void TestWordwithEmbeddedKeyworkNotSplitIntoTwoWords()
    {
      string code = "isoperator";

      LookAheadLangParser parser = LookAheadLangParserFactory.CreateCppParser(TestUtil.GetTextStream(code));

      Assert.AreEqual("isoperator", parser.PeekNextKeyword());
    }
  }
}
