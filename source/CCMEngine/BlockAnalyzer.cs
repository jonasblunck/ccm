using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CCMEngine
{
  public delegate void OnLocalFunctionDelegate(IFunctionStream functionStream);
  public enum ParserSwitchBehavior { TraditionalInclude, IgnoreCases };

  // BlockAnalyzer analyzes a block of code beginning with "{" and ending with "}"
  //  the block of code is consumed from the parser and the CC metric is returned
  public class BlockAnalyzer
  {
    private LookAheadLangParser parser;
    private List<string> conditionalsWithExpressions;
    private List<string> branchPointKeywords;
    private IFunctionStream functionStream;
    private OnLocalFunctionDelegate onLocalFunctionDelegate;

    public BlockAnalyzer(LookAheadLangParser parser, IFunctionStream functionStream = null, OnLocalFunctionDelegate onLocalFunctionDelegate = null,
      ParserSwitchBehavior switchBehavior = ParserSwitchBehavior.TraditionalInclude)
    {
      this.parser = parser;
      this.conditionalsWithExpressions = new List<string>(
        new string[] { "if", "while", "foreach", "for", "else if", "elseif" });

      this.branchPointKeywords = new List<string>(new string[] { "catch" });

      if (switchBehavior == ParserSwitchBehavior.TraditionalInclude)
        this.branchPointKeywords.Add("case");

      this.functionStream = functionStream;
      this.onLocalFunctionDelegate = onLocalFunctionDelegate;
    }

    public int CalcNumExpressionConditions()
    {
      int expressions = 0;

      this.parser.NextKeyword();

      while (")" != this.parser.PeekNextKeyword())
      {
        if (this.functionStream.NextIsLogicOperand())
        {
           // these should come from the function-stream?
          ++expressions;
          this.parser.NextKeyword();
        }
        else if ("(" == this.parser.PeekNextKeyword())
        {
          expressions += CalcNumExpressionConditions();
          continue; // dont' consume more tokens without analyzing them!
        }
        else
        {
          this.parser.NextKeyword();
        }
      }

      this.parser.NextKeyword();

      return expressions;
    }

    public int CalcCCOnConditionalBranch()
    {
      int ccm = 1;

      while ("(" != this.parser.PeekNextKeyword())
        this.parser.NextKeyword();

      ccm += CalcNumExpressionConditions();

      if ("{" == this.parser.PeekNextKeyword())
        ccm += ConsumeBlockCalculateAdditionalComplexity();

      return ccm;
    }

    public int ConsumeBlockCalculateAdditionalComplexity()
    {
      int ccm = 0;

      if ("{" == this.parser.PeekNextKeyword())
      {
        this.parser.NextKeyword();

        while ("}" != this.parser.PeekNextKeyword())
        {
          if (this.parser.PeekNextKeyword().Equals("{"))
            ccm += ConsumeBlockCalculateAdditionalComplexity();
          else if (this.parser.PeekNextKeyword().Equals("switch"))
          {
            while (!this.parser.PeekNextKeyword().Equals("{"))
              this.parser.NextKeyword();

            ccm += ConsumeBlockCalculateAdditionalComplexity();
          }
          else if (this.branchPointKeywords.Contains(this.parser.PeekNextKeyword()))
          {
            this.parser.NextKeyword();

            ++ccm;
          }
          else if (this.conditionalsWithExpressions.Contains(this.parser.PeekNextKeyword()))
            ccm += CalcCCOnConditionalBranch();
          else if (this.parser.PeekNextKeyword().Equals("#"))
            this.parser.MoveToNextLine();
          else if (this.functionStream != null && null != this.onLocalFunctionDelegate && this.functionStream.NextIsLocalFunction())
          {
            this.onLocalFunctionDelegate(this.functionStream);
          }
          else
            this.parser.NextKeyword();
        }

        this.parser.NextKeyword();
      }

      return ccm;
    }

  }
}
