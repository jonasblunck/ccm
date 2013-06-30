#pragma once

/*
  Copyright Jonas Blunck, 2002-2006

  All rights reserved, no warranties extended. Use at your own risk!

*/


#include "CDisAssm.h"

// define prototype for copy-function
typedef bool (__stdcall* pfnCopyFunc)(BYTE**, BYTE**, const InstrInfo&, int);

// define the info I need to call the copy-func
typedef struct CopyInfo
{
  BYTE  btOpCode;
  pfnCopyFunc pCopyFunc;
} CopyInfo;


class CAssmCopier
{
  BYTE* m_pSource;          // the instruction to be copied
  BYTE* m_pDestination;     // where the copy goes...

  static CopyInfo m_aCopyInfo[];

  static bool __stdcall CopyStdInstruction(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen);
  static bool __stdcall CopyFFInstruction(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen);
  static bool __stdcall CopyFullDisplacement(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen);
  static bool __stdcall CopyShortJMP(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen);
  static bool __stdcall CopyPrefixedInstruction(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen);

  static void CopyMem(BYTE* pFrom, BYTE* pTo, int iLen)
  {
    for (int i = 0; i < iLen; ++i)
      pTo[i] = pFrom[i];
  }


public:
  CAssmCopier(BYTE* pSource, BYTE* pDestination);
  ~CAssmCopier(void);

  bool CopyAssembler(int iMinLen, int& riNumSourceBytesCopied, int& riNumDestBytesCopied);

};

