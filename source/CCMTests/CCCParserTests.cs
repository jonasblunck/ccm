using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CCMEngine;

namespace CCMTests
{
  [TestClass]
  public class CCCParserTests
  {
    private struct TestContext
    {
      public LookAheadLangParser parser;
      public CCCParser ccc;

      public TestContext(LookAheadLangParser parser, CCCParser ccc)
      {
        this.parser = parser;
        this.ccc = ccc;
      }

      public string NextFunction()
      {
        try
        {
          this.ccc.AdvanceToNextFunction();
        }
        catch (CCCParserSuccessException info)
        {
          return info.Function;
        }

        return String.Empty;
      }

      public bool NextIsClass()
      {
        return this.ccc.NextIsClass();
      }

      public bool NextIsFunction()
      {
        return this.ccc.NextIsFunction();
      }

    }

    private static TestContext SetupContext(string code)
    {
      return SetupContext(code, false);
    }

    private static TestContext SetupContext(string code, bool suppressMethodSignature)
    {
      LookAheadLangParser parser = LookAheadLangParser.CreateCppParser(TestUtil.GetTextStream(code));
      CCCParser ccc = new CCCParser(parser, suppressMethodSignature);

      TestContext context = new TestContext(parser, ccc);

      return context;
    }

    [TestMethod]
    public void CStyleFunction()
    {
      string code = "int Foo(int x, int Y) {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("Foo(int x,int Y)", function);
    }

    [TestMethod]
    public void CppStyleFunction()
    {
      string code = "int X::Foo() {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("X::Foo()", function);
    }

    [TestMethod]
    public void CppConstStyleFunction()
    {
      string code = "int Y::Foo() const {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("Y::Foo()", function);
    }

    [TestMethod]
    [ExpectedException(typeof(EndOfStreamException))]
    public void CStyleForwardDeclared()
    {
      string code = "int Deliver(int y, int x);";

      TestContext context = CCCParserTests.SetupContext(code);

      context.NextFunction();
    }

    [TestMethod]
    [ExpectedException(typeof(EndOfStreamException))]
    public void CppStyleForwardDeclared()
    {
      string code = "int X::Deliver() const;";

      TestContext context = CCCParserTests.SetupContext(code);

      context.NextFunction();
    }

    [TestMethod]
    [ExpectedException(typeof(EndOfStreamException))]
    public void PureVirtual()
    {
      string code = "virtual int XYZ() = 0;";

      TestContext context = CCCParserTests.SetupContext(code);

      context.NextFunction();
    }

    [TestMethod]
    public void CStyleDeclspecNaked()
    {
      string code = "__declspec(naked) int XYZ() {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("XYZ()", function);
    }

    [TestMethod]
    public void CppThrows()
    {
      string code = "MACRO(x) int MyFunc() throws(...) {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("MyFunc()", function);
    }

    [TestMethod]
    public void CppStyleConst()
    {
      string code = "class C {   " +
                    "  void Function() const {} " +
                    "};";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C::Function()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }


    [TestMethod]
    public void CppStyleStaticH()
    {
      string code = "class C {   " +
                    "  static int StaticFunction() {} " +
                    "};";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C::StaticFunction()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CppStyleStaticCPP()
    {
      string code = "int C::StaticFunction() { }";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C::StaticFunction()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CSharpAttribute()
    {
      string code = "[Attribute(a)] public int Foo() {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("Foo()", function);
    }

    [TestMethod]
    public void ConstructorWithInheritence()
    {
      string code = "X::X() : Y() {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("X::X()", function);
    }

    [TestMethod]
    public void TemplatebasedReturn()
    {
      string code = "private List<string> GetList(int j) {}";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("GetList(int j)", function);
    }

    [TestMethod]
    public void ClassInline()
    {
      string code = "class A {  " +
                    " void FooInline() {} };";

      TestContext context = CCCParserTests.SetupContext(code);

      string function = context.NextFunction();

      Assert.AreEqual("{", context.parser.PeekNextKeyword());
      Assert.AreEqual("A::FooInline()", function);
    }

    [TestMethod]
    public void ClassMultipleInlines()
    {
      string code = "class A {  " +
                    " void FooInline() {} " +
                    " void Outline() {} };";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("A::FooInline()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());

      Assert.AreEqual("{", context.parser.NextKeyword());
      Assert.AreEqual("}", context.parser.NextKeyword());

      Assert.AreEqual("A::Outline()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CSharpStyleClassWithMembersAndFunctions()
    {
      string code = "" +
           "  public class MyClass {                              " +
           "    private List<int, int> l = new List<int, int>();  " +
           "    public int Foo(double d, X x) { }";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("MyClass::Foo(double d,X x)", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CppEqualityOperator()
    {
      string code = "bool X::operator == (const X& r) {} ";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("X::operator ==(const X& r)", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CppyUnaryOperator()
    {
      string code = "bool X::operator != (const X& r) {} ";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("X::operator !=(const X& r)", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CEqualityOperator()
    {
      string code = "bool operator == (GUID& r1, GUID& r2) {}";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("operator ==(GUID& r1,GUID& r2)", context.NextFunction()); // global context
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CSharpEqualityOperator()
    {
      string code = "public class CO { " +
                    "  public static bool operator ==(CO c1, CO c2) {} }";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("CO::operator ==(CO c1,CO c2)", context.NextFunction()); // global context
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void LocalClass()
    {
      string code = "public class C1 { " +
                    "  public class C2 {      " +
                    "     void Foo() {} " +
                    "  } " +
                    "}";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C1::C2::Foo()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CppUsingStatements()
    {
      string code = "using xxx::YYY::cf::x; " +
                    "void CMyClass::Foo() {} ";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("CMyClass::Foo()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CSharpFieldInitialization()
    {
      string code = " public class C0 { " +
                    " public class C1 { " +
                    "  private List<string> list = new List<string>(); " +
                    "  private List<string> items = new List<string>(new string[] { \"k\", \"l\", \"o\" }); " +
                    "  public void Foo() {} " +
                    " } " +
                    "}";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C0::C1::Foo()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void MultiLocalClass()
    {
      string code = "public class C1 { " +
                    "  class C2 {      " +
                    "     void Foo() {} " +
                    "  } " +
                    "  class C3 {      " +
                    "     void Bar() {} " +
                    "  } " +
                    "}";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C1::C2::Foo()", context.NextFunction());

      // fake function-parser will parse the function
      Assert.AreEqual("{", context.parser.NextKeyword());
      Assert.AreEqual("}", context.parser.NextKeyword());

      Assert.AreEqual("C1::C3::Bar()", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CSharpGetProperty()
    {
      string code = "public class C { " +
                    " public int Size { " +
                    "    set {} " +
                    "    get {} " +
                    " }" +
                    "}";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C::Size::set", context.NextFunction());
      Assert.AreEqual("{", context.parser.NextKeyword());
      Assert.AreEqual("}", context.parser.NextKeyword());

      Assert.AreEqual("C::Size::get", context.NextFunction());
      Assert.AreEqual("{", context.parser.NextKeyword());
    }

    [TestMethod]
    public void CppMultiFunctions()
    {
      string code = "void C::Foo() const {} " +
                    "void B::Bar() const {} ";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual("C::Foo()", context.NextFunction());
      Assert.AreEqual("B::Bar()", context.NextFunction());
    }

    /*  
     * 
     * 
     */
    [TestMethod]
    public void TestNextIsClassForwardDeclared()
    {
      string code = "class CMyClass;"; //forward declaration

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual(false, context.NextIsClass());
    }

    [TestMethod]
    public void TestNextIsClass()
    {
      string code = "class CMyClass {"; //forward declaration

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual(true, context.NextIsClass());
    }

    [TestMethod]
    public void TestFunctionNotRecognizedAsClass()
    {
      string code = "void ClassFunc();";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.AreEqual(false, context.NextIsClass());
    }

    [TestMethod]
    public void NextOperatorIsFunction()
    {
      string code = "CMy::operator == () {}" +
                    "CMy2::operator != ();";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual(true, context.NextIsFunction());

      // forward parser to next operator
      while (!context.parser.PeekNextKeyword().Equals("CMy2"))
        context.parser.NextKeyword();

      // parser positioned on second, forward declared, operator
      Assert.AreEqual(false, context.NextIsFunction()); // forward declared
    }

    [TestMethod]
    public void NextIsFunction()
    {
      string code = "Bar() const {}" +
                    "MyClass::Bar() const {}";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual(true, context.NextIsFunction());

      // forward parser to "MyClass" 
      while (!context.parser.PeekNextKeyword().Equals("MyClass"))
        context.parser.NextKeyword();

      Assert.AreEqual(true, context.NextIsFunction());
    }

    [TestMethod]
    public void NextIsNotFunction()
    {
      string code = "public class X;" +
                    "MyClass::Bar() const;";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual(false, context.NextIsFunction());

      // forward parser to "MyClass" 
      while (!context.parser.PeekNextKeyword().Equals("MyClass"))
        context.parser.NextKeyword();

      Assert.AreEqual(false, context.NextIsFunction()); // forward declared
    }

    [TestMethod]
    public void FullNamespaceQualifiedCppFunctionIsFound()
    {
      string code = "void xxx::yyy::zzz::MyFoo::Bar(void) { }";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual("xxx::yyy::zzz::MyFoo::Bar(void)", context.NextFunction());
    }

    [TestMethod]
    public void TestCSharpAttributesWithNamedParam()
    {
      string code = "[SuppressMessage(\"Rule\", Justification = \"Yeah\"]\r\n" +
                    "protected static Func(A b,B b,C c) {}";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual("Func(A b,B b,C c)", context.NextFunction());
    }

    [TestMethod]
    public void TestTemplateFunctionMultiParams()
    {
      string code = "template <typename X, typename Z>\r\n" +
                    "static int FuncB(X x) { return 1; } \r\n";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual("FuncB(X x)", context.NextFunction());
    }

    [TestMethod]
    public void TemplateStatementIsConsumed()
    {
      string code = "template <typename X, class Y>NEXT";
      TestContext context = CCCParserTests.SetupContext(code);

      context.ccc.ConsumeTemplate();
      Assert.AreEqual("NEXT", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void TestTemplatedClassName()
    {
      string code = "template <typename X, class B>\r\n" +
                    "class Bar<X, B> { \r\n " +
                    "  void Foo(void) { } \r\n " +
                    "}";


      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual("Bar<X,B>::Foo(void)", context.NextFunction());
    }

    [TestMethod]
    public void TestTemplatedClassWithInheritance()
    {
      string code = "template <typename X, typename B>\r\n" +
                    "class Bar<X, B> : public Base { \r\n " +
                    "  void Foo(void) { } \r\n "
                  + "}";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual("Bar<X,B>::Foo(void)", context.NextFunction());
    }

    [TestMethod]
    public void SpecializedTemplate()
    {
      string code = "template <>EXTRA";

      TestContext context = CCCParserTests.SetupContext(code);
      context.ccc.ConsumeTemplate();

      Assert.AreEqual("EXTRA", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    [Ignore]
    public void ArgsWithParenthesis()
    {
      string code = "GetMimeData(__inout CStringA &str, __in_ecount(dwFieldLen) LPCSTR szField, __in DWORD dwFieldLen, __out LPBOOL pbFound, __in BOOL bIgnoreCase = FALSE) throw() {}";

      TestContext context = CCCParserTests.SetupContext(code);

      Assert.IsTrue(context.NextIsFunction());
      Assert.AreEqual("GetMimeData", context.NextFunction());
    }

    [TestMethod]
    [Ignore]
    public void TestMethodNameWithGenericsAndParameterWithGenericsIsReturnedAsFunction()
    {
      string code = "TypeX<T>(TypeY<T> sourceArray){}";
      TestContext context = CCCParserTests.SetupContext(code);

      Assert.IsTrue(context.NextIsFunction());
      Assert.AreEqual("TypeX<T>(TypeY<T> sourceArray)", context.NextFunction()); 
    }

    [TestMethod]
    public void FunctionNameWithGenericsIsRecognizedAsFunction()
    {
      string code = "CreateVariable<T>(string name) {}";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.IsTrue(context.NextIsFunction());
    }

    [TestMethod]
    public void FunctionWithGeneric()
    {
      string code = "CreateVariable<T>(string name) {}";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.AreEqual("CreateVariable<T>(string name)", context.NextFunction());
    }

    [TestMethod]
    public void TestClassWithGeneric()
    {
      string code = "" +
        "class SingletonProvider<T> where T : new()\r\n" +
        "{ void Foo() {} }";

      TestContext context = CCCParserTests.SetupContext(code);
      Assert.IsTrue(context.NextIsClass());
      Assert.AreEqual("SingletonProvider<T>::Foo()", context.NextFunction());
    }

    [TestMethod]
    public void TestOnlyReturnNameWhenAskedToSkipSignature()
    {
      string code = "void CMyClass::Function(int j, double d, const X& x){}";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.AreEqual("CMyClass::Function", context.NextFunction());
    }

    [TestMethod]
    public void TestCppStyleLessThanOperatorIsFoundAsFunction()
    {
      string code = "CSHA1::operator<< (const char *message_array){}";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.IsTrue(context.NextIsFunction());
      Assert.AreEqual("CSHA1::operator<<", context.NextFunction());
    }

    [TestMethod]
    public void TestCppStylePointerOperator()
    {
      string code = "CItemValue::operator->(){}";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.IsTrue(context.NextIsFunction());
      Assert.AreEqual("CItemValue::operator ->", context.NextFunction());
    }

    [TestMethod]
    public void TestVoidOperator()
    {
      string code = "operator()(void const*) const {}";

      TestContext context = CCCParserTests.SetupContext(code, false);
      Assert.AreEqual("operator()", context.NextFunction());

    }

    [TestMethod]
    [Ignore]
    public void TestCShardFieldsDoesNotThrowParserOff()
    {
      string code = "public class X { " +
                    " public int Age { get; private set; } " +
                    " public Foo() {}";


      TestContext context = CCCParserTests.SetupContext(code, false);
      Assert.AreEqual("X::Foo()", context.NextFunction());

    }

    [TestMethod]
    public void FunctionWithOperatorNameIsNotAnOperator()
    {
      string code = "bool isoperator(int ch) { return false; }";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.AreEqual("isoperator", context.NextFunction());
    }

    [TestMethod]
    public void MethodPreceededByTabIsRecognized()
    {
      string code = "int\tFoo() { ";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.AreEqual("Foo", context.NextFunction());
    }

    [TestMethod]
    public void CStyleFunctionDeclaration()
    {
      string code = "void " +
                    "error (message,a1,a2) " +
                    "char message, a1; " +
                    "char a2; " +
                    "{ printf(..) }";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.AreEqual("error", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CStyleFunctionDeclarationWithPointers()
    {
      string code = "void " +
                    "func (message,a1,a2) " +
                    "char *message; " +
                    "char* a1,*a2; " +
                    "{ printf(..) }";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.AreEqual("func", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }

    [TestMethod]
    public void CStyleFunctionDeclarationWithArrays()
    {
      string code = "void " +
                    "funcis (toggle) " +
                    "type toggle[PROCS][MAX_PROCESS]; " +
                    "{ printf(..) }";

      TestContext context = CCCParserTests.SetupContext(code, true);
      Assert.AreEqual("funcis", context.NextFunction());
      Assert.AreEqual("{", context.parser.PeekNextKeyword());
    }
  }  

}
