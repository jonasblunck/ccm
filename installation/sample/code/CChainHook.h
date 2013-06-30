#pragma once

/*
  Copyright Jonas Blunck, 2002-2006

  All rights reserved, no warranties extended. Use at your own risk!

*/

class CChainHook
{
	bool CreateStub(HANDLE hProcess, LPVOID pIntercept, LPVOID& rStub, int nCodeExtraSize = 0);
	bool CreateStubEx(HANDLE hProcess, LPVOID pIntercept, LPVOID pStubAddress, int nCodeExtraSize = 0);

public:
	CChainHook(void);
	~CChainHook(void);

	LPVOID HookProc(HANDLE hProcess, VOID* pFunctionToIntercept, VOID* pHook);
	bool   HookProcEx(HANDLE hProcess, VOID* pFunctionToIntercept, VOID* pHook, VOID* pStub);
	bool   RemoveHook(HANDLE hProcess, VOID* pInterceptedFunction, VOID* pStub);
  LPVOID AllocStub(HANDLE hProcess);

  bool   InjectCode(HANDLE hProcess, VOID* pAddress, BYTE* pbCode, int nCodeBytes);

};
