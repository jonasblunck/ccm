
BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
                     )
{
    BOOL ok=FALSE;
    DWORD error=0;
    DWORD dwThreadId = 0;
    HANDLE hThread = NULL;

    switch(ul_reason_for_call)
    {
        //=====================================================================
        case DLL_PROCESS_ATTACH:
            {
                PVOID pReservedVM = NULL;
                BOOL bRetVal = TRUE;

                    do
                    {

            if ( ok )
            {

              LPCTSTR pp = _tcsstr( g_dctm_apps, g_exeName );
              if ( pp != NULL )
              {
                //make sure it matches full string
                g_dctm_capable = pp == g_dctm_apps ? TRUE : *(pp-1)==' ';
              }

            }
            else if (g_isDgPrompt)
            {
              //  DgPrompt only cares about the clipboard's low-level input hooks.
              ClipboardHooks_ProcessAttach(hModule);
            }
                    } while ( FALSE );
            }
            break;

    }

    //DebugTraceDirect( _F("LEAVING\n") );

    return TRUE;
}

void GetXMLStuff()
{
  CString strAddress = "http://www.blunck.se";
  _GetXmlNodeText(spXMLDOM, _bstr_t(_T("//prompt/pwd_retry_warning")), m_password_retry_warning_message);
}

void test::testing( CStringA strAddress, CStringA& strSMTPAddress, CStringA& strRcptName )
{
    strRcptName = strAddress.MakeLower();
 
//    int br = strRcptName.Find( '<' );                 
    if (br >= 0)
     {
        strSMTPAddress = ((LPCSTR)strRcptName) + br;
//        strSMTPAddress.Trim("< >");                   
 
        strRcptName = strRcptName.Left( br );         
        strRcptName.TrimRight();
        strRcptName.Trim('"');
    }
 
//    int at = strRcptName.Find( '@' );                 
    if (at >= 0)
     {
        if (strSMTPAddress.IsEmpty())
        {
            strSMTPAddress = strRcptName;             
        }
 
        strRcptName = strRcptName.Left( at );         
     }
}


static  PWCHAR blah[] = 
{
    L"test",
    L"test",
    L"test",
    L"test",
    L"test",
    L"test",
    L"test",
    L"test",
    /*L"test",
    /*L"test",
    /*L"test",
    /*L"test",
    /*L"test",*/
       NULL
};

BOOL AssertWinFile(PWindowFile pWinFile)
{
    BOOL RetVal = FALSE;
    if(pWinFile != NULL)
    {
        ASSERT_TAG(pWinFile->tag, WinFileTag);
        RetVal = TRUE;
    }
    return RetVal;
}
