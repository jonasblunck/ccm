#include <windows.h>

static SELECTION ExtraEventSelect9065[] =
{
    { METHF_METHODOPCODE, EQ, (LONG)M_CRT_WIDTH },
    { METHF_METHODOPCODE, EQ, (LONG)M_SCAN_FREQ },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_AUTO_PRINT },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_A_TYPE },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_A_BANDWIDTH },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_A_TC },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_A_OFFSET },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_B_TYPE },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_B_BANDWIDTH },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_B_TC },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_B_OFFSET },
    { 0, OR, 0L },
    { METHF_METHODTIME, EQ, (LONG)0 },
    { 0, AND, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_CHROMATO },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_SPECTRUM },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_RATIO_A },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_RATIO_B },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_IMPUR_A },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_IMPUR_B },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_CHROM_A },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_CHROM_B },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_PKSENS },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_STORAGE },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_AZ },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_STRTPKSENS },
    { 0, OR, 0L },
    { METHF_METHODOPCODE, EQ, (LONG)M_END_METHOD },
    { 0, OR, 0L },
    { 0, NOT, 0L },
    { 0, LAST, 0L },
};

LRESULT WINAPI SmpEditFrameProc
(
	HWND     hWnd,
	UINT	 message,
	WPARAM   wParam,
	LPARAM   lParam
)
{ // 1
    switch (message) 
	{
		case WM_ERASEBKGND: //2
		{
			HDC hDC = (HDC)wParam;
			HBRUSH hFill;
			RECT rect = {0, 0, 0, 0};
			     
			GetClientRect(hWnd, &rect);
			
			if (hFill = CreateSolidBrush(GetSysColor(COLOR_WINDOW))) //3
			{
				FillRect(hDC, (LPRECT)&rect, hFill);
				DeleteObject(hFill);
			}
			
			break;
		}

		// Nonsense code added for test
		case 0: //4
			if (hWnd) //5 
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			else
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			break;

		// Nonsense code added for test
		case 1: //6
			if (hWnd) //7
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			else
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			break;

		// Nonsense code added for test
		case 2: //8
			if (hWnd) //9
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			else
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			break;

		// Nonsense code added for test
		default:
			if (hWnd) //10
				if (hWnd)
					if (hWnd)
						if (hWnd)
							if (hWnd)
								if (hWnd)
									if (hWnd)
										if (hWnd)
											if (hWnd)
												if (hWnd)
													if (hWnd) // 20
														if (hWnd) 
															if (hWnd) //22
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			else
			{
				MessageBox (hWnd,"","",MB_OK);
			}
			break;
	}
	
	return DefWindowProc(hWnd, message, wParam, lParam);
}

LRESULT CALLBACK InstrumentWndProc (WPARAM wParam)
{
	switch( wParam )
	{
		case 3:
		{
			break;
		}
	}

	switch( wParam )
	{
		case 3:
		{
			break;
		}
	}

	return ( 0L ) ; 
}

LRESULT CALLBACK InstrumentWndProc2 (WPARAM wParam)
{
	switch( wParam )
	{
		case 1:
		{
			return( 0L );
		}
		case 2:
		{
			AnyFunctionCall(); 
			break;
		}
		case 3:
		{
			if( wParam )
			{
				return( 0L );
			}
			return( 0L );
		}
	}
	return ( 0L ) ; 
}

// the c-style functions below are not supported

//BOOL cstylea ( a, b )
//int a;
//int b;
//{
//	if ( a > b )
//		return TRUE;
//	return FALSE;
//}
//
//BOOL cstyleb ( a, b )
//int a, b;
//{
//	if ( a > b )
//		return TRUE;
//	return FALSE;
//}