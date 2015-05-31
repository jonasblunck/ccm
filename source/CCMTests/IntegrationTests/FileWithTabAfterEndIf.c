//-------------------------------------------------------------------------------------------------

#if (DebugLevel & Debug_MyMask)
#if (DebugLevel & DebugLevel_MyText)
//const byte abExample[]		= 	"12345678901234567890123456789012"; // =32 bytes
#else
//const byte abExample[]		= 	"";
#endif
#endif	// the TAB after the endif causes the problem

void Foo()
{
}