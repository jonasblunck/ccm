using System;
using System.Collections.Generic;
using System.Text;
using CCMEngine;
using System.IO;
using NUnit.Framework;

namespace CCMTests
{
  [TestFixture]
  public class LookAheadTokenParserBaseTests
  {
    [Test]
    public void NoTokenSpecified()
    {
      string text = "the lazy brown dog";

      LookAheadTokenParserBase parser = new LookAheadTokenParserBase(
        TestUtil.GetTextStream(text), new string[] { });

      Assert.AreEqual(text, parser.PeekNextToken());
      Assert.AreEqual(text, parser.NextToken());
    }

    [Test]
    public void NoTokenPeekRepeatable()
    {
      string text = "the lazy brown dog";

      LookAheadTokenParserBase parser = new LookAheadTokenParserBase(
        TestUtil.GetTextStream(text), new string[] { });

      Assert.AreEqual(text, parser.PeekNextToken());
      Assert.AreEqual(text, parser.PeekNextToken());
    }

    [Test]
    public void TokenReturned()
    {
      string text = "lazy dog";

      LookAheadTokenParserBase parser = new LookAheadTokenParserBase(
        TestUtil.GetTextStream(text), new string[] { " " });

      Assert.AreEqual("lazy", parser.PeekNextToken());
      Assert.AreEqual("lazy", parser.NextToken());
      Assert.AreEqual(" ", parser.PeekNextToken());
      Assert.AreEqual(" ", parser.NextToken());
      Assert.AreEqual("dog", parser.PeekNextToken());
      Assert.AreEqual("dog", parser.NextToken());
    }

    [Test]
    public void LongNonExistentToken()
    {
      string text = "#ifdef x #endif";

      LookAheadTokenParserBase parser = new LookAheadTokenParserBase(
        TestUtil.GetTextStream(text), new string[] { "NotToBeFoundTokenButIsIsLongEnoughEh?", "#", "ifdef", "endif", " " });

      Assert.AreEqual("#", parser.NextToken());
      Assert.AreEqual("ifdef", parser.NextToken());
      Assert.AreEqual(" ", parser.NextToken());
      Assert.AreEqual("x", parser.NextToken());
      Assert.AreEqual(" ", parser.NextToken());
      Assert.AreEqual("#", parser.NextToken());
      Assert.AreEqual("endif", parser.NextToken());
    }

    [Test]
    [ExpectedException(typeof(EndOfStreamException))]
    public void ExhaustedThrows()
    {
      string text = "lazy";

      LookAheadTokenParserBase parser = new LookAheadTokenParserBase(
        TestUtil.GetTextStream(text), new string[] { });

      Assert.AreEqual("lazy", parser.PeekNextToken());
      Assert.AreEqual("lazy", parser.NextToken());

      parser.PeekNextToken();
    }

    [Test]
    public void SingeLineCommentIsConsumed()
    {
      string text = "lazy// some text \r\n" +
                    "dog";

      LookAheadTokenParserBase parser = new LookAheadTokenParserBase(
        TestUtil.GetTextStream(text), new string[] { });

      Assert.AreEqual("lazy", parser.NextToken());
      Assert.AreEqual("dog", parser.NextToken());


    }
  }
}
