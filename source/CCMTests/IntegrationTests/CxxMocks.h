#pragma once

#include "MockBase.h"
#include "CxxDetour.h"

namespace CxxDM
{
  //
  // everything below here could be generated
  //
  template <typename _Ret>
  class Mock0 : public MockBase
  {
    _Ret m_ret;
  public:

    Mock0& AndReturn(_Ret r)
    {
      m_ret = r;
      return *this;
    }

    _Ret Func()
    {
      SetActive(false);
      return m_ret;
    }
  };

  template <>
  class Mock0<void> : public MockBase
  {
  public:

    void Func()
    {
    }
  };

  template <typename _Ret, typename _Arg1>
  class Mock1 : public MockBase
  {
    _Ret m_ret;
    _Arg1 m_arg1;
  public:
    Mock1& Expect(_Arg1 a1)
    {
      m_arg1 = a1;
      return *this;
    }

    Mock1& AndReturn(_Ret r)
    {
      m_ret = r;
      return *this;
    }

    _Ret Func(_Arg1 a1)
    {
      MockBase::VerifyExpects(m_arg1, a1);

      SetActive(false);
      return m_ret;
    }

  };

  template <typename _Arg1>
  class Mock1<void, _Arg1> : public MockBase
  {
    _Arg1 m_arg1;
  public:
    Mock1& Expect(_Arg1 a1)
    {
      m_arg1 = a1;
      return *this;
    }

    void Func(_Arg1 a1)
    {
      MockBase::VerifyExpects(m_arg1, a1);

      SetActive(false);
    }
  };

  template <class retType>
  CxxDM::Mock0<retType>& MockFunction(DWORD addr)
  {
    CxxDM::Mock0<retType>* pMock = new CxxDM::Mock0<retType>();

    __asm
    {
      push CxxDM::Mock0<retType>::Func
      push pMock
      push addr
      call CreateDetour
    }
   
    return *pMock; // TODO; fix deletion upon termination
  }

};