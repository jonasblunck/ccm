#include "StdAfx.h"
#include "CAssmCopier.h"
#include "CDisAssm.h"

#pragma warning(disable: 4311)
CAssmCopier::CAssmCopier(BYTE* pSource, BYTE* pDestination)
{
  m_pSource = pSource;
  m_pDestination = pDestination;
}

CAssmCopier::~CAssmCopier(void)
{
}

// copy code from one place in memory to another, copy
// at least iMinLen byte - break after an instruction, not in the middle of one
// adjust addresses etc
bool CAssmCopier::CopyAssembler(int iMinLen, int& riNumSourceBytesCopied, int& riNumDestBytesCopied)
{
  _ASSERTE(m_pSource && m_pDestination);
  _ASSERTE(iMinLen);

  riNumSourceBytesCopied = 0; // keep track of how many source bytes copied
  riNumDestBytesCopied = 0; // keep track of how many dest bytes copied (since some addresses may be adjusted, it might not be the same as the number of source bytes)

  BYTE* pSourceStart = m_pSource;
  BYTE* pDestStart = m_pDestination;

  while ((m_pSource - pSourceStart) < iMinLen)
  {
    CDisAssm Dis;
    InstrInfo Info;
    int iSourceLen = 0;

    // get info about the instruction ahead
    if (!Dis.GetInstructionInfo(m_pSource, Info, iSourceLen))
      return false;

    // look up the copy-function
    _ASSERTE(m_aCopyInfo[*m_pSource].btOpCode == *m_pSource);

    pfnCopyFunc CopyFunc = m_aCopyInfo[*m_pSource].pCopyFunc;

    _ASSERTE(CopyFunc);

    // copy the instruction
    if (CopyFunc)
    {
      if (!CopyFunc(&m_pSource, &m_pDestination, Info, iSourceLen))
        return false;
    }
    else
      return false; // no copy func speced!

  }

  riNumSourceBytesCopied = (int)(m_pSource - pSourceStart);
  riNumDestBytesCopied = (int)(m_pDestination - pDestStart);

  return true;
}

bool CAssmCopier::CopyPrefixedInstruction(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen)
{
  _ASSERTE(ppSource && ppDestination);
  _ASSERTE(*ppSource && *ppDestination);
  _ASSERTE(1 == iInstrLen);

  // copy the prefix
  **ppDestination = **ppSource;
  
  *ppDestination += iInstrLen;
  *ppSource += iInstrLen;

  CAssmCopier CopyNext(*ppSource, *ppDestination);

  int iLenSource = 0;
  int iLenDest = 0;

  if (!CopyNext.CopyAssembler(1, iLenSource, iLenDest))
    return false;

  *ppDestination += iLenDest;
  *ppSource += iLenSource;

  return true;
}


// CAssmCopier::CopyShortJMP
bool CAssmCopier::CopyShortJMP(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen)
{
  _ASSERTE(ppSource && ppDestination);
  _ASSERTE(*ppSource && *ppDestination);
  _ASSERTE(en8BitDisplacement == rInfo.iFlags);
  _ASSERTE(0x70 == (**ppSource & 0xf0)); // must be a Jcc instruction

  BYTE* pSource = *ppSource;
  BYTE* pDest = *ppDestination;

  BYTE btDisplacement = 0;

  memcpy(&btDisplacement, (pSource + 1), sizeof(BYTE));

  // extend the JMP 8-bit displacement to JMP 32-bit displacement
  DWORD dwNewOffset = (DWORD)btDisplacement - (DWORD)pDest + (DWORD)pSource;
  
  // change the instruction to a JMP 32-bit displacment
  BYTE btInstruction[] = { 0x0F, 0x80, 0x00, 0x00, 0x00, 0x00 };

  // insert the 'tttn' pieces (condition) into the second byte
  btInstruction[1] |= (*pSource & 0x0f);

  // insert the address displacement
  memcpy((void*)&btInstruction[2], &dwNewOffset, sizeof(LPVOID));

  // copy instruction
  memcpy(pDest, (void*)&btInstruction[0], sizeof(btInstruction));

  *ppSource += iInstrLen;
  *ppDestination += sizeof(btInstruction);

  return true;
}

//
// CopyStdInstruction: copies an instruction where the correct number of bytes needs to be copied
//                     (no adjustments to addresses needs to be made - basically just memcpy)
//
bool CAssmCopier::CopyStdInstruction(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen)
{
  _ASSERTE(ppSource && ppDestination);
  _ASSERTE(*ppSource && *ppDestination);

  BYTE* pSource = *ppSource;
  BYTE* pDest = *ppDestination;

  for (int i = 0; i < iInstrLen; ++i)
    pDest[i] = pSource[i];

  // adjust the source and destination pointers to point after the copied data
  *ppDestination += iInstrLen;
  *ppSource += iInstrLen;

  return true;
}

bool CAssmCopier::CopyFullDisplacement(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen)
{
  _ASSERTE(ppSource && ppDestination);
  _ASSERTE(*ppSource && *ppDestination);
  _ASSERTE(enFullDisplacement == rInfo.iFlags);

  int iOffsetToAddress = rInfo.iOpSize;

  CDisAssm Dis;

  if (Dis.HasSibByte(rInfo, *ppSource))
    iOffsetToAddress += 1;

  BYTE* pSource = *ppSource;
  BYTE* pDest = *ppDestination;

  DWORD dwOrgOffset = NULL;
  memcpy(&dwOrgOffset, pSource + iOffsetToAddress, sizeof(LPVOID));

  // calculate the new offset
  DWORD dwNewOffset = dwOrgOffset - (DWORD)pDest + (DWORD)pSource;

  // copy the instruction
  CopyMem(pSource, pDest, iInstrLen);

  // exchange the address
  memcpy(pDest + iOffsetToAddress, &dwNewOffset, sizeof(LPVOID));

  // adjust the source and destination pointers to point after the copied data
  *ppDestination += iInstrLen;
  *ppSource += iInstrLen;

  return true;
}

bool CAssmCopier::CopyFFInstruction(BYTE** ppSource, BYTE** ppDestination, const InstrInfo& rInfo, int iInstrLen)
{
  _ASSERTE(ppSource && ppDestination);
  _ASSERTE(*ppSource && *ppDestination);

  BYTE* pInstruction = *ppSource;

	_ASSERTE(pInstruction);
	_ASSERTE(0xff == *pInstruction);

	int iSubDecode = ((*(pInstruction + 1)) & 0x38) >> 3;
	
  static CopyInfo s_CopyFF[] =
  { 
    { 0x00, CopyStdInstruction },  // INC
    { 0x01, NULL }, // illegal
    { 0x02, CopyStdInstruction }, // CALL - displacement is in registers
    { 0x03, NULL }, // illegal
    { 0x04, NULL }, // illegal
    { 0x05, CopyStdInstruction }, // JMP - displacement is in registers
    { 0x06, CopyStdInstruction }, // PUSH
    { 0x07, NULL }, // illegal
  };

  _ASSERTE(iSubDecode >= 0 && iSubDecode <= 7);
  _ASSERTE(iSubDecode == s_CopyFF[iSubDecode].btOpCode);

  pfnCopyFunc CopyFunc = s_CopyFF[iSubDecode].pCopyFunc;

  if (CopyFunc)
    return CopyFunc(ppSource, ppDestination, rInfo, iInstrLen);

  return false;
}

CopyInfo CAssmCopier::m_aCopyInfo [] =
{
	{ 0x00, CopyStdInstruction },
	{ 0x01, CopyStdInstruction },
	{ 0x02, CopyStdInstruction },
	{ 0x03, CopyStdInstruction },
	{ 0x04, CopyStdInstruction },
	{ 0x05, CopyStdInstruction },
	{ 0x06, CopyStdInstruction },
	{ 0x07, CopyStdInstruction },
	{ 0x08, CopyStdInstruction },
	{ 0x09, CopyStdInstruction },
	{ 0x0a, CopyStdInstruction },
	{ 0x0b, CopyStdInstruction },
	{ 0x0c, CopyStdInstruction },
	{ 0x0d, CopyStdInstruction },
	{ 0x0e, CopyStdInstruction },
	{ 0x0f, NULL }, // undefined extension ops

	{ 0x10, CopyStdInstruction },
	{ 0x11, CopyStdInstruction },
	{ 0x12, CopyStdInstruction },
	{ 0x13, CopyStdInstruction },
	{ 0x14, CopyStdInstruction },
	{ 0x15, CopyStdInstruction },
	{ 0x16, CopyStdInstruction },
	{ 0x17, CopyStdInstruction },
	{ 0x18, CopyStdInstruction },
	{ 0x19, CopyStdInstruction },
	{ 0x1a, CopyStdInstruction },
	{ 0x1b, CopyStdInstruction },
	{ 0x1c, CopyStdInstruction },
	{ 0x1d, CopyStdInstruction },
	{ 0x1e, CopyStdInstruction },
	{ 0x1f, CopyStdInstruction },

	{ 0x20, CopyStdInstruction },
	{ 0x21, CopyStdInstruction },
	{ 0x22, CopyStdInstruction },
	{ 0x23, CopyStdInstruction },
	{ 0x24, CopyStdInstruction },
	{ 0x25, CopyStdInstruction },
	{ 0x26, NULL },
	{ 0x27, CopyStdInstruction },
	{ 0x28, CopyStdInstruction },
	{ 0x29, CopyStdInstruction },
	{ 0x2a, CopyStdInstruction },
	{ 0x2b, CopyStdInstruction },
	{ 0x2c, CopyStdInstruction },
	{ 0x2d, CopyStdInstruction },
	{ 0x2e, NULL },
	{ 0x2f, CopyStdInstruction },

	{ 0x30, CopyStdInstruction },
	{ 0x31, CopyStdInstruction },
	{ 0x32, CopyStdInstruction },
	{ 0x33, CopyStdInstruction },
	{ 0x34, CopyStdInstruction },
	{ 0x35, CopyStdInstruction },
	{ 0x36, NULL },
	{ 0x37, CopyStdInstruction },
	{ 0x38, CopyStdInstruction },
	{ 0x39, CopyStdInstruction },
	{ 0x3a, CopyStdInstruction },
	{ 0x3b, CopyStdInstruction },
	{ 0x3c, CopyStdInstruction },
	{ 0x3d, CopyStdInstruction },
	{ 0x3e, NULL },
	{ 0x3f, CopyStdInstruction },

	// OpCod
	{ 0x40, CopyStdInstruction },
	{ 0x41, CopyStdInstruction },
	{ 0x42, CopyStdInstruction },
	{ 0x43, CopyStdInstruction },
	{ 0x44, CopyStdInstruction },
	{ 0x45, CopyStdInstruction },
	{ 0x46, CopyStdInstruction },
	{ 0x47, CopyStdInstruction },
	{ 0x48, CopyStdInstruction },
	{ 0x49, CopyStdInstruction },
	{ 0x4a, CopyStdInstruction },
	{ 0x4b, CopyStdInstruction },
	{ 0x4c, CopyStdInstruction },
	{ 0x4d, CopyStdInstruction },
	{ 0x4e, CopyStdInstruction },
	{ 0x4f, CopyStdInstruction },

	{ 0x50, CopyStdInstruction },
	{ 0x51, CopyStdInstruction },
	{ 0x52, CopyStdInstruction },
	{ 0x53, CopyStdInstruction },
	{ 0x54, CopyStdInstruction },
	{ 0x55, CopyStdInstruction },
	{ 0x56, CopyStdInstruction },
	{ 0x57, CopyStdInstruction },
	{ 0x58, CopyStdInstruction },
	{ 0x59, CopyStdInstruction },
	{ 0x5a, CopyStdInstruction },
	{ 0x5b, CopyStdInstruction },
	{ 0x5c, CopyStdInstruction },
	{ 0x5d, CopyStdInstruction },
	{ 0x5e, CopyStdInstruction },
	{ 0x5f, CopyStdInstruction },

	// OpCod
	{ 0x60, CopyStdInstruction },
	{ 0x61, CopyStdInstruction },
	{ 0x62, NULL },
	{ 0x63, NULL },
	{ 0x64, CopyPrefixedInstruction },
	{ 0x65, CopyPrefixedInstruction },
	{ 0x66, NULL },
	{ 0x67, NULL },
	{ 0x68, CopyStdInstruction },
	{ 0x69, NULL },
	{ 0x6a, CopyStdInstruction },
	{ 0x6b, NULL },
	{ 0x6c, CopyStdInstruction },
	{ 0x6d, CopyStdInstruction },
	{ 0x6e, CopyStdInstruction },
	{ 0x6f, CopyStdInstruction },

	{ 0x70, CopyShortJMP },
	{ 0x71, CopyShortJMP },
	{ 0x72, CopyShortJMP },
	{ 0x73, CopyShortJMP },
	{ 0x74, CopyShortJMP },
	{ 0x75, CopyShortJMP },
	{ 0x76, CopyShortJMP },
	{ 0x77, CopyShortJMP },
	{ 0x78, CopyShortJMP },
	{ 0x79, CopyShortJMP },
	{ 0x7a, CopyShortJMP },
	{ 0x7b, CopyShortJMP },
	{ 0x7c, CopyShortJMP },
	{ 0x7d, CopyShortJMP },
	{ 0x7e, CopyShortJMP },
	{ 0x7f, CopyShortJMP },

	{ 0x80, CopyStdInstruction },
	{ 0x81, CopyStdInstruction },
	{ 0x82, CopyStdInstruction },
	{ 0x83, CopyStdInstruction },
	{ 0x84, CopyStdInstruction },
	{ 0x85, CopyStdInstruction },
	{ 0x86, CopyStdInstruction },
	{ 0x87, CopyStdInstruction },
	{ 0x88, CopyStdInstruction },
	{ 0x89, CopyStdInstruction },
	{ 0x8a, CopyStdInstruction },
	{ 0x8b, CopyStdInstruction },
	{ 0x8c, CopyStdInstruction },
	{ 0x8d, CopyStdInstruction },
	{ 0x8e, CopyStdInstruction },
	{ 0x8f, CopyStdInstruction },

	{ 0x90, CopyStdInstruction },
	{ 0x91, CopyStdInstruction },
	{ 0x92, CopyStdInstruction },
	{ 0x93, CopyStdInstruction },
	{ 0x94, CopyStdInstruction },
	{ 0x95, CopyStdInstruction },
	{ 0x96, CopyStdInstruction },
	{ 0x97, CopyStdInstruction },
	{ 0x98, CopyStdInstruction },
	{ 0x99, CopyStdInstruction },
	{ 0x9a, CopyFullDisplacement },  // displacement!!! address needs to be adjusted!
	{ 0x9b, CopyStdInstruction },
	{ 0x9c, CopyStdInstruction },
	{ 0x9d, CopyStdInstruction },
	{ 0x9e, CopyStdInstruction },
	{ 0x9f, CopyStdInstruction },

	{ 0xa0, CopyStdInstruction },
	{ 0xa1, CopyStdInstruction },
	{ 0xa2, CopyStdInstruction },
	{ 0xa3, CopyStdInstruction },
	{ 0xa4, CopyStdInstruction },
	{ 0xa5, CopyStdInstruction },
	{ 0xa6, CopyStdInstruction },
	{ 0xa7, CopyStdInstruction },
	{ 0xa8, CopyStdInstruction },
	{ 0xa9, CopyStdInstruction },
	{ 0xaa, CopyStdInstruction },
	{ 0xab, CopyStdInstruction },
	{ 0xac, CopyStdInstruction },
	{ 0xad, CopyStdInstruction },
	{ 0xae, CopyStdInstruction },
	{ 0xaf, CopyStdInstruction },

	{ 0xb0, NULL },
	{ 0xb1, NULL },
	{ 0xb2, NULL },
	{ 0xb3, NULL },
	{ 0xb4, NULL },
	{ 0xb5, NULL },
	{ 0xb6, NULL },
	{ 0xb7, NULL },
	{ 0xb8, CopyStdInstruction },
	{ 0xb9, CopyStdInstruction },
	{ 0xba, CopyStdInstruction },
	{ 0xbb, CopyStdInstruction },
	{ 0xbc, CopyStdInstruction },
	{ 0xbd, CopyStdInstruction },
	{ 0xbe, CopyStdInstruction },
	{ 0xbf, CopyStdInstruction },

	{ 0xc0, NULL },
	{ 0xc1, NULL },
	{ 0xc2, NULL },
	{ 0xc3, NULL }, // RET
	{ 0xc4, NULL },
	{ 0xc5, NULL },
	{ 0xc6, NULL },
	{ 0xc7, CopyStdInstruction },
	{ 0xc8, NULL },
	{ 0xc9, NULL },
	{ 0xca, NULL },
	{ 0xcb, NULL },
	{ 0xcc, NULL },
	{ 0xcd, NULL },
	{ 0xce, NULL },
	{ 0xcf, NULL },

	{ 0xd0, NULL },
	{ 0xd1, NULL },
	{ 0xd2, NULL },
	{ 0xd3, NULL },
	{ 0xd4, NULL },
	{ 0xd5, NULL },
	{ 0xd6, NULL },
	{ 0xd7, NULL },
	{ 0xd8, NULL },
	{ 0xd9, NULL },
	{ 0xda, NULL },
	{ 0xdb, NULL },
	{ 0xdc, NULL },
	{ 0xdd, NULL },
	{ 0xde, NULL },
	{ 0xdf, NULL },

	{ 0xe0, NULL },
	{ 0xe1, NULL },
	{ 0xe2, NULL },
	{ 0xe3, NULL },
	{ 0xe4, NULL },
	{ 0xe5, NULL },
	{ 0xe6, NULL },
	{ 0xe7, NULL },
	{ 0xe8, CopyFullDisplacement }, // call displacement!!! address needs to be adjusted!
	{ 0xe9, CopyFullDisplacement }, // jmp displacement!!! address needs to be adjusted!
	{ 0xea, CopyFullDisplacement }, // jmp full displacement
	{ 0xeb, CopyShortJMP }, // jmp short displacement - address needs to be expanded!
	{ 0xec, CopyStdInstruction },
	{ 0xed, CopyStdInstruction },
	{ 0xee, CopyStdInstruction },
	{ 0xef, CopyStdInstruction },

	{ 0xf0, CopyPrefixedInstruction },
	{ 0xf1, NULL },
	{ 0xf2, CopyPrefixedInstruction },
	{ 0xf3, CopyPrefixedInstruction },
	{ 0xf4, CopyStdInstruction },
	{ 0xf5, CopyStdInstruction },
	{ 0xf6, NULL },
	{ 0xf7, NULL },
	{ 0xf8, CopyStdInstruction },
	{ 0xf9, CopyStdInstruction },
	{ 0xfa, CopyStdInstruction },
	{ 0xfb, CopyStdInstruction },
	{ 0xfc, CopyStdInstruction },
	{ 0xfd, CopyStdInstruction },
	{ 0xfe, CopyStdInstruction },
	{ 0xff, CopyFFInstruction },
};

#pragma warning(default: 4311)