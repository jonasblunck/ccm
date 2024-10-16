using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;
using System.Text;

namespace CCMTests
{
  [TestClass]
  public class LookAheadTokenParserTests
  {
    [TestMethod]
    [ExpectedException(typeof(EndOfStreamException))]
    public void EmptyStringThrows()
    {
      string text = "";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
             new string[] { }, true);

      parser.PeekNextToken();
    }

    [TestMethod]
    public void OneTokenOnly()
    {
      string text = "text";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
             new string[] { }, true);

      Assert.AreEqual("text", parser.PeekNextToken());
      Assert.AreEqual("text", parser.NextToken());
    }

    [TestMethod]
    public void WhiteSpacesAreConsumed()
    {
      string text = "&& \r\n text ::{";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
             new string[] { "&&", "::", "{", " " }, true);

      Assert.AreEqual("&&", parser.PeekNextToken());
      Assert.AreEqual("&&", parser.NextToken());
      Assert.AreEqual("text", parser.PeekNextToken());
      Assert.AreEqual("text", parser.NextToken());
      Assert.AreEqual("::", parser.PeekNextToken());
      Assert.AreEqual("::", parser.NextToken());
      Assert.AreEqual("{", parser.PeekNextToken());
      Assert.AreEqual("{", parser.NextToken());
    }

    [TestMethod]
    public void PeekIsRepeatable()
    {
      string text = "&& ||";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { "&&", "||" }, true);

      Assert.AreEqual("&&", parser.PeekNextToken());
      Assert.AreEqual("&&", parser.PeekNextToken());
      Assert.AreEqual("&&", parser.PeekNextToken());
    }

    [TestMethod]
    public void KeywordsWithoutSpacing()
    {
      string text = "Foo::Bar()";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { "::", "(", ")" }, true);

      Assert.AreEqual("Foo", parser.NextToken());
      Assert.AreEqual("::", parser.NextToken());
      Assert.AreEqual("Bar", parser.NextToken());
      Assert.AreEqual("(", parser.NextToken());
      Assert.AreEqual(")", parser.NextToken());
    }


    [TestMethod]
    public void NextConsumesToken()
    {
      string text = "&& ||";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { "&&", "||" }, true);

      Assert.AreEqual("&&", parser.NextToken());
      Assert.AreEqual("||", parser.NextToken());
    }

    [TestMethod]
    public void MoveToNextLine()
    {
      string text = "&& || somecrap \r\n" +
                    "text";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { "&&", "||" }, true);

      Assert.AreEqual("&&", parser.NextToken());
      Assert.AreEqual("||", parser.NextToken());

      parser.MoveToNextLine();

      Assert.AreEqual("text", parser.PeekNextToken());
    }

    [TestMethod]
    public void MoveNextLineReturnsLine()
    {
      string text = "This is a line \\ \r\nAnd this is next";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text), new string[] { }, false);

      Assert.AreEqual("This", parser.PeekNextToken());
      Assert.AreEqual("This is a line \\ \r\n", parser.MoveToNextLine());
      Assert.AreEqual("And", parser.PeekNextToken());
    }

    [TestMethod]
    public void NewlineIsConsumed()
    {
      string text = "else\r\n" +
                    "}\r\n}";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { }, true);

      Assert.AreEqual("else", parser.PeekNextToken());
      Assert.AreEqual("else", parser.NextToken());
      Assert.AreEqual("}", parser.PeekNextToken());
      Assert.AreEqual("}", parser.NextToken());
      Assert.AreEqual("}", parser.PeekNextToken());
      Assert.AreEqual("}", parser.NextToken());
    }

    [TestMethod]
    public void SingleQuotedText()
    {
      string text = "text 'c' more";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { }, true);

      Assert.AreEqual("text", parser.NextToken());
      Assert.AreEqual("'c'", parser.NextToken());
      Assert.AreEqual("more", parser.NextToken());
    }

    [TestMethod]
    public void CSharpStyleQuotes()
    {
      string text = "@\"\\\"TEXT"; // this is the string @"\"

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { }, true);

      Assert.IsTrue(parser.NextIsQuotedText());
      Assert.AreEqual("@\"\\\"", parser.NextToken());
      Assert.AreEqual("TEXT", parser.NextToken());

    }

    [TestMethod]
    public void SingleQuotedToken()
    {
      string text = "J' ' TEXT";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { }, true);

      Assert.AreEqual("J", parser.NextToken());
      Assert.AreEqual("' '", parser.NextToken());
      Assert.AreEqual("TEXT", parser.NextToken());;
    }

    [TestMethod]
    public void DoubleQuoteCanBeInSingleQuotes()
    {
      string text = "'\"' TEXT";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
        new string[] { }, true);

      Assert.AreEqual("'\"'", parser.NextToken());
      Assert.AreEqual("TEXT", parser.NextToken());
    }

    [TestMethod]
    public void QuotedString()
    {
      string text = "text \"can be\" more";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { }, true);

      Assert.AreEqual("text", parser.NextToken());
      Assert.AreEqual("\"can be\"", parser.NextToken());
      Assert.AreEqual("more", parser.NextToken());
    }

    [TestMethod]
    public void SpaceAreNotConsumed()
    {
      string text = "text text2 text3";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { " " }, false);

      Assert.AreEqual("text", parser.NextToken());
      Assert.AreEqual(" ", parser.NextToken());
      Assert.AreEqual("text2", parser.NextToken());
      Assert.AreEqual(" ", parser.NextToken());
      Assert.AreEqual("text3", parser.NextToken());
    }

    [TestMethod]
    public void QuotedTextWithKeywords()
    {
      string text = "\"\\\\\";"; // this is "\\";

      LookAheadTokenParser parser = new LookAheadTokenParser(TestUtil.GetTextStream(text),
          new string[] { " " }, false);

      Assert.AreEqual("\"\\\\\"", parser.NextToken());
    }

  }
}
