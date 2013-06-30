#include "StdAfx.h"
#include "CChainHook.h"
#include "CAssmCopier.h"
#include <crtdbg.h>

// a instruction can be max 14 bytes, and the one before might have been 4 bytes and
// I need to copy both - allocate 32 bytes for the stub to be sure we have enough free mem
#define STUB_SIZE 32 

#pragma warning(disable: 4311)
#pragma warning(disable: 4312)

CChainHook::CChainHook(void)
{
}

CChainHook::~CChainHook(void)
{
} 

LPVOID CChainHook::AllocStub(HANDLE hProcess)
{
  DWORD dwMaxSize = STUB_SIZE; 

  DWORD* pdwStubCode = (DWORD*)VirtualAllocEx(hProcess, NULL, dwMaxSize, MEM_COMMIT|MEM_TOP_DOWN, PAGE_EXECUTE_READWRITE);

  return (LPVOID)pdwStubCode;
}

bool CChainHook::InjectCode(HANDLE hProcess, VOID* pAddress, BYTE* pbCode, int nCodeBytes)
{
  LPVOID pvStub = NULL;
  
  if (!CreateStub(hProcess, pAddress, pvStub, nCodeBytes))
    return false;

  // stub has been created, let's inject the code into the function
  BYTE OpJmpToStub[] = {0xE9, 0x00, 0x00, 0x00, 0x00};
  DWORD dwJumpAddressOffset = (DWORD)pvStub - (DWORD)pAddress - sizeof(OpJmpToStub) - nCodeBytes;
  memcpy(&OpJmpToStub[1], &dwJumpAddressOffset, 4); 

  // change the page-protection for the intercepted function
  DWORD dwOldProtect;

  if (!VirtualProtectEx(hProcess, pAddress, sizeof(OpJmpToStub) + nCodeBytes, PAGE_EXECUTE_READWRITE, &dwOldProtect))
    return NULL;

  // 
  // insert a jmp to the stub
  //
  DWORD dwWritten = 0;
  if (!WriteProcessMemory(hProcess, (LPVOID)(((DWORD)pAddress) + nCodeBytes), &OpJmpToStub, sizeof(OpJmpToStub), &dwWritten))
    return NULL;

  // insert the injection code
  if (NULL != pbCode)
  {
    if (!WriteProcessMemory(hProcess, pAddress, pbCode, nCodeBytes, &dwWritten))
      return NULL;
  }

  //
  // restore page protection
  //
  VirtualProtectEx(hProcess, pAddress, sizeof(OpJmpToStub) + nCodeBytes, dwOldProtect, &dwOldProtect);

  return true;
}

bool CChainHook::CreateStub(HANDLE hProcess, LPVOID pIntercept, LPVOID& rStub, int nCodeExtraSize)
{
  _ASSERTE(pIntercept);

  DWORD dwMaxSize = STUB_SIZE; 

  DWORD* pdwStubCode = (DWORD*)VirtualAllocEx(hProcess, NULL, dwMaxSize, MEM_COMMIT|MEM_TOP_DOWN, PAGE_EXECUTE_READWRITE);

  if (!pdwStubCode)
    return false;

  if (!CreateStubEx(hProcess, pIntercept, (LPVOID)pdwStubCode, nCodeExtraSize))
  {
    VirtualFree((LPVOID)pdwStubCode, dwMaxSize, MEM_RELEASE); 
    return false;
  }

  rStub = (LPVOID)pdwStubCode;

  return true;
}

bool CChainHook::CreateStubEx(HANDLE hProcess, LPVOID pIntercept, LPVOID pStubAddress, int nCodeExtraSize)
{
  //
  // the instruction to jump from the stub back into the intercepted function...
  //
  BYTE OpJmpFromStub[] = {0xE9, 0x00, 0x00, 0x00, 0x00};

  //
  // create the stub
  //
  LPVOID pStub = (LPVOID)pStubAddress;
  DWORD pdwRealCodeToCopy = (DWORD)pIntercept;

  int iSourceCopied = 0;
  int iDestCopied = 0;

  CAssmCopier AssmCopier((BYTE*)pIntercept, (BYTE*)pStub);

  if (!AssmCopier.CopyAssembler(sizeof(OpJmpFromStub) + nCodeExtraSize, iSourceCopied, iDestCopied))
    return false;

  // insert the JMP statement into the correct function!
  DWORD pReturnInFunc = (DWORD)pIntercept + (DWORD)iSourceCopied;
  DWORD pInsertJmpInStub = (DWORD)pStubAddress + (DWORD)iDestCopied;
  DWORD pReturnInFuncOffset = (DWORD)pReturnInFunc - (DWORD)pInsertJmpInStub - sizeof(OpJmpFromStub);

  memcpy(&OpJmpFromStub[1], &pReturnInFuncOffset, 4);

  memcpy((LPVOID)pInsertJmpInStub, &OpJmpFromStub, sizeof(OpJmpFromStub));

  return true;
}


LPVOID CChainHook::HookProc(HANDLE hProcess, VOID* pFunctionToIntercept, VOID* pHook)
{
  //
  // create a stub
  //
  LPVOID pStub = NULL;

  if (!CreateStub(hProcess, pFunctionToIntercept, pStub))
    return false;

  _ASSERTE(pStub);

  if (HookProcEx(hProcess, pFunctionToIntercept, pHook, pStub))
    return pStub;

  return NULL;
}   

bool CChainHook::HookProcEx(HANDLE hProcess, VOID* pFunctionToIntercept, VOID* pHook, VOID* pStub)
{
  _ASSERTE(pFunctionToIntercept && pHook && pStub);

  if (!CreateStubEx(hProcess, pFunctionToIntercept, pStub))
    return false;
  //
  // create instructions so the intercepted function jumps to the hook!
  //
  BYTE OpJmpToHook[] = {0xE9, 0x00, 0x00, 0x00, 0x00};
  DWORD dwJumpAddressOffset = (DWORD)pHook - (DWORD)pFunctionToIntercept - sizeof(OpJmpToHook);
  memcpy(&OpJmpToHook[1], &dwJumpAddressOffset, 4); 

  // change the page-protection for the intercepted function
  DWORD dwOldProtect;

  if (!VirtualProtectEx(hProcess, pFunctionToIntercept, sizeof(OpJmpToHook), PAGE_EXECUTE_READWRITE, &dwOldProtect))
    return NULL;

  // 
  // insert a jmp to the hook
  //
  DWORD dwWritten = 0;
  if (!WriteProcessMemory(hProcess, pFunctionToIntercept, &OpJmpToHook, sizeof(OpJmpToHook), &dwWritten))
    return NULL;

  //
  // restore page protection
  //
  VirtualProtectEx(hProcess, pFunctionToIntercept, sizeof(OpJmpToHook), dwOldProtect, &dwOldProtect);

  return true;
}
     

bool CChainHook::RemoveHook(HANDLE hProcess, VOID* pInterceptedFunction, VOID* pStub)
{
  _ASSERTE(pInterceptedFunction && pStub && hProcess);
  _ASSERTE(*((BYTE*)pInterceptedFunction) == 0xE9); // not intercepted?

  DWORD dwOldProtect;
  DWORD dwNum;
  const int iSizeOfFirstJmp = 5; // I always put a JMP statement into the intercepted function!

  if (!VirtualProtectEx(hProcess, pInterceptedFunction, iSizeOfFirstJmp, PAGE_EXECUTE_READWRITE, &dwOldProtect))
    return false;

  //
  // copy code from the stub into the intercepted function
  //
  if (!WriteProcessMemory(hProcess, pInterceptedFunction, pStub, iSizeOfFirstJmp, &dwNum))
    return false;

  //
  // delete the stub (the function is no longer intercepted
  //
  if (!VirtualFreeEx(hProcess, pStub, 0, MEM_DECOMMIT))
    return false;

  //
  // reset the protection
  //
  DWORD dwDummy;

  if (!VirtualProtectEx(hProcess, pInterceptedFunction, iSizeOfFirstJmp, dwOldProtect, &dwDummy))
    return false;

  return true;
}


#pragma warning(default: 4311)
#pragma warning(default: 4312)