#include "StdAfx.h"
#include "CDisAssm.h"
#include <crtdbg.h>

CDisAssm::CDisAssm(void)
{
	m_b16BitAddress = false;
	m_b16BitOperand = false;
}

CDisAssm::~CDisAssm(void)
{
}

bool CDisAssm::GetInstructionInfo(const BYTE* pInstruction, InstrInfo& rInfo, int& riLen)
{
	_ASSERTE(pInstruction);
	_ASSERTE(m_aInstructions[*pInstruction].iOpCode == *pInstruction);

	rInfo = m_aInstructions[*pInstruction];

	// check the special ones
	if (0xff == *pInstruction)
	{
		if (!GetInstructionInfoFF(rInfo, pInstruction))
			return false;
	}
/*else if (0x66 == *pInstruction)
	{
		m_b16BitOperand = true;
	}
	else if (0x67 == *pInstruction)
	{
		m_b16BitAddress = true;
	}*/

	_ASSERTE(rInfo.iOpSize);  // undefined?

	if (!rInfo.iOpSize)
		return false;

  riLen = CalcSize(rInfo, pInstruction);

	return true;
}

bool CDisAssm::GetInstructionInfoFF(InstrInfo& rInfo, const BYTE* pInstruction)
{
	_ASSERTE(pInstruction);
	_ASSERTE(0xff == *pInstruction);

	int iSubDecode = ((*(pInstruction + 1)) & 0x38) >> 3;
	
	// map sub-coded instruction bytes (following the 2 mod bits)
	static InstrInfo s_InstrMapFF[] =
	{
		// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
		{ 0x00, 2, 0, 0, 1, enNone },			    // INC
		{ 0x01, 0, 0, 0, 0, enNone },			    // illegal
		{ 0x02, 2, 0, 0, 1, enNone },  // CALL
		{ 0x03, 0, 0, 0, 0, enNone },				// illegal
		{ 0x04, 0, 0, 0, 0, enNone },				// illegal
		{ 0x05, 2, 0, 0, 1, enNone },  // JMP
		{ 0x06, 2, 0, 0, 1, enNone },				// PUSH
		{ 0x07, 0, 0, 0, 0, enNone },				// illegal
	};

	if (s_InstrMapFF[iSubDecode].iOpSize)
	{
		rInfo = s_InstrMapFF[iSubDecode];

		return true;
	}

	return false;
}


bool CDisAssm::GetBit(int iBitCountingFromMSB, const BYTE* pInstrunction)
{
	//
	// example:
	//
	//  %0000 00sw 0000 0000 --> s = bit 6, w = bit 7 (zero index)
	//
	//  %0000 0000 w000 0000 --> w = bit 8 (zero index)
	//

	int iOffset = 0;

	while (iBitCountingFromMSB >= 8)
	{
		++iOffset;
		iBitCountingFromMSB -= 8;
	}

	_ASSERTE(iBitCountingFromMSB >= 0 && iBitCountingFromMSB <= 7);

	BYTE btToCheck = *(pInstrunction + iOffset);
	
	int iMask = 0x80;

	iMask = iMask >> iBitCountingFromMSB;

	if (btToCheck & (BYTE)iMask)
		return true;

	return false;
}

bool CDisAssm::HasSibByte(const InstrInfo& rInstrInfo, const BYTE* pInstruction) const
{
	if (rInstrInfo.bModRM)
	{
		if (!Is16BitAddress())
		{
			int iMod = (*(pInstruction + (rInstrInfo.iOpSize - 1)) & 0xc0) >> 6;
			int iRM = (*(pInstruction + (rInstrInfo.iOpSize - 1)) & 0x03);

			if (0%11 != iMod && 0%100 == iRM)
				return true;
		}

	}

	return false;
}

bool CDisAssm::Is16BitOperand() const
{
	return m_b16BitOperand;
}

bool CDisAssm::Is16BitAddress() const
{
	return m_b16BitAddress;
}

int CDisAssm::CalcSizeDisplacement(const InstrInfo& rInstrInfo, const BYTE* pInstruction)
{
  if (enFullDisplacement == rInstrInfo.iFlags)
    return Is16BitAddress() ? 2 : 4;
  else if (en16BitDisplacement == rInstrInfo.iFlags)
    return 2;
  else if (en8BitDisplacement == rInstrInfo.iFlags)
    return 1;

  return 0;
}

int CDisAssm::CalcSizeForModRM(const InstrInfo& rInstrInfo, const BYTE* pInstruction)
{
	_ASSERTE(rInstrInfo.iOpSize); // undefined?

	typedef struct 
	{
		int iMod;
		int iRMSize[8];
	} ModRMSizeEntry;
	
	static ModRMSizeEntry s_ModRMSize32Bit[4] =
	{
		{ 
			00, { 0, 0, 0, 0, 1, 4, 0, 0},		// Mod: 00
		},
		{
			01, { 1, 1, 1, 1, 2, 1, 1, 1},		// Mod: 01, disp 8
		},
		{
			02, { 4, 4, 4, 4, 5, 4, 4, 4},		// Mod: 02, disp 32
		},
		{
			03, { 0, 0, 0, 0, 0, 0, 0, 0},		// Mod: 03, /r
		},
	};

	static ModRMSizeEntry s_ModRMSize16Bit[4] =
	{
		{ 
			00, { 0, 0, 0, 0, 0, 0, 1, 0},		// Mod: 00
		},
		{
			01, { 1, 1, 1, 1, 1, 1, 1, 1},		// Mod: 01, disp 8
		},
		{
			02, { 2, 2, 2, 2, 2, 2, 2, 2},		// Mod: 02, disp 16
		},
		{
			03, { 0, 0, 0, 0, 0, 0, 0, 0},		// Mod: 03, /r
		},
	
	};


	if (rInstrInfo.bModRM)
	{
		int iMod = (*(pInstruction + 1) & 0xc0) >> 6;
		int iRM = (*(pInstruction + 1) & 0x07);

		if (Is16BitAddress())
		{
			_ASSERTE(s_ModRMSize16Bit[iMod].iMod == iMod);		
			return s_ModRMSize16Bit[iMod].iRMSize[iRM];
		}
		else
		{
			_ASSERTE(s_ModRMSize32Bit[iMod].iMod == iMod);
			return s_ModRMSize32Bit[iMod].iRMSize[iRM];
		}
	}


	return 0;
}

int CDisAssm::CalcSizeImmediateData(const InstrInfo& rInstrInfo, const BYTE* pInstruction)
{
	_ASSERTE(rInstrInfo.iOpSize); // undefined?

	if (en8BitImmediateData == rInstrInfo.iFlags)
		return 1;
	else if (en16BitImmediateData == rInstrInfo.iFlags)
		return 2;
	else if (enImmediateData == rInstrInfo.iFlags)
	{
		_ASSERTE(rInstrInfo.iOffset_s || rInstrInfo.iOffset_w);
		
		if (rInstrInfo.iOffset_s && GetBit(rInstrInfo.iOffset_s, pInstruction))
			return 1; // always 1 byte when s is set
		else if (rInstrInfo.iOffset_w && GetBit(rInstrInfo.iOffset_w, pInstruction))
			return Is16BitOperand() ? 2 : 4;
		else if (rInstrInfo.iOffset_w && !GetBit(rInstrInfo.iOffset_w, pInstruction))
			return 1; // 1 byte immediate data
		else
		{	// immediate data specified, but no w-qualifier
			// must be either 2 or 4 bytes depending on current operand size attribute
			return Is16BitOperand() ? 2 : 4;
		}
	}

	return 0;
}


int CDisAssm::CalcSize(const InstrInfo& rInstrInfo, const BYTE* pInstruction)
{
	// get the size of the instruction
	int iSize = rInstrInfo.iOpSize;

	_ASSERTE(iSize);

	iSize += CalcSizeForModRM(rInstrInfo, pInstruction);
  iSize += CalcSizeDisplacement(rInstrInfo, pInstruction);
	iSize += CalcSizeImmediateData(rInstrInfo, pInstruction);

	return iSize;
}

InstrInfo CDisAssm::m_aInstructions[] =
{
	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0x00, 2, 0, 0, 1, enNone },		// ADD /r
	{ 0x01, 2, 0, 0, 1, enNone },		// ADD /r
	{ 0x02, 2, 0, 0, 1, enNone },		// ADD /m /r
	{ 0x03, 2, 0, 0, 1, enNone },		// ADD /m /r
	{ 0x04, 1, 0, 7, 0, enImmediateData },		// ADD /r (eax)
	{ 0x05, 1, 0, 7, 0, enImmediateData },		// ADD /r (eax)
	{ 0x06, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x07, 1, 0, 0, 0, enNone },		// POP
	{ 0x08, 2, 0, 0, 1, enNone },		// OR
	{ 0x09, 2, 0, 0, 1, enNone },		// OR
	{ 0x0a, 2, 0, 0, 1, enNone },		// OR
	{ 0x0b, 2, 0, 0, 1, enNone },		// OR
	{ 0x0c, 1, 0, 7, 0, enImmediateData },		// OR immediate
	{ 0x0d, 1, 0, 7, 0, enImmediateData },		// OR immediate
	{ 0x0e, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x0f, 0, 0, 0, 0, enNone },		// undefined extension ops

	{ 0x10, 2, 0, 0, 1, enNone },		// ADC /r
	{ 0x11, 2, 0, 0, 1, enNone },		// ADC /r
	{ 0x12, 2, 0, 0, 1, enNone },		// ADC /r
	{ 0x13, 2, 0, 0, 1, enNone },		// ADC /r
	{ 0x14, 1, 0, 7, 0, enImmediateData },		// ADC immediate
	{ 0x15, 1, 0, 7, 0, enImmediateData },		// ADC immediate
	{ 0x16, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x17, 1, 0, 0, 0, enNone },		// POP
	{ 0x18, 2, 0, 0, 1, enNone },		// SBB /r
	{ 0x19, 2, 0, 0, 1, enNone },		// SBB /r
	{ 0x1a, 2, 0, 0, 1, enNone },		// SBB /r
	{ 0x1b, 2, 0, 0, 1, enNone },		// SBB /r
	{ 0x1c, 1, 0, 7, 0, enImmediateData },		// SBB /immediate
	{ 0x1d, 1, 0, 7, 0, enImmediateData },		// SBB /immediate
	{ 0x1e, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x1f, 1, 0, 0, 0, enNone },		// POP

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0x20, 2, 0, 0, 1, enNone },		// AND
	{ 0x21, 2, 0, 0, 1, enNone },		// AND
	{ 0x22, 2, 0, 0, 1, enNone },		// AND
	{ 0x23, 2, 0, 0, 1, enNone },		// AND
	{ 0x24, 2, 0, 7, 0, enImmediateData },		// AND
	{ 0x25, 2, 0, 7, 0, enImmediateData },		// AND
	{ 0x26, 0, 0, 0, 0, enNone },		// undefined
	{ 0x27, 1, 0, 0, 0, enNone },		// DAA
	{ 0x28, 2, 0, 0, 1, enNone },		// SUB /r
	{ 0x29, 2, 0, 0, 1, enNone },		// SUB /r
	{ 0x2a, 2, 0, 0, 1, enNone },		// SUB /r
	{ 0x2b, 2, 0, 0, 1, enNone },		// SUB /r
	{ 0x2c, 1, 0, 7, 0, enImmediateData },		// SUB /immediate
	{ 0x2d, 1, 0, 7, 0, enImmediateData },		// SUB /immediate
	{ 0x2e, 0, 0, 0, 0, enNone },		// undefined
	{ 0x2f, 1, 0, 0, 0, enNone },		// DAS

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0x30, 2, 0, 0, 1, enNone },		// XOR
	{ 0x31, 2, 0, 0, 1, enNone },		// XOR
	{ 0x32, 2, 0, 0, 1, enNone },		// XOR
	{ 0x33, 2, 0, 0, 1, enNone },		// XOR
	{ 0x34, 1, 0, 7, 0, enImmediateData },		// XOR /immediate
	{ 0x35, 1, 0, 7, 0, enImmediateData },		// XOR /immediate
	{ 0x36, 0, 0, 0, 0, enNone },		// undefined
	{ 0x37, 1, 0, 0, 0, enNone },		// AAA
	{ 0x38, 2, 0, 0, 1, enNone },		// CMP /r
	{ 0x39, 2, 0, 0, 1, enNone },		// CMP /r
	{ 0x3a, 2, 0, 0, 1, enNone },		// CMP /r
	{ 0x3b, 2, 0, 0, 1, enNone },		// CMP /r
	{ 0x3c, 1, 0, 7, 0, enImmediateData },		// CMP /immediate
	{ 0x3d, 1, 0, 7, 0, enImmediateData},		// CMP /immediate
	{ 0x3e, 0, 0, 0, 0, enNone },		// undefined
	{ 0x3f, 1, 0, 0, 0, enNone },		// AAS

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0x40, 1, 0, 0, 0, enNone },		// INC
	{ 0x41, 1, 0, 0, 0, enNone },		// INC
	{ 0x42, 1, 0, 0, 0, enNone },		// INC
	{ 0x43, 1, 0, 0, 0, enNone },		// INC
	{ 0x44, 1, 0, 0, 0, enNone },		// INC
	{ 0x45, 1, 0, 0, 0, enNone },		// INC
	{ 0x46, 1, 0, 0, 0, enNone },		// INC
	{ 0x47, 1, 0, 0, 0, enNone },		// INC
	{ 0x48, 1, 0, 0, 0, enNone },		// DEC
	{ 0x49, 1, 0, 0, 0, enNone },		// DEC
	{ 0x4a, 1, 0, 0, 0, enNone },		// DEC
	{ 0x4b, 1, 0, 0, 0, enNone },		// DEC
	{ 0x4c, 1, 0, 0, 0, enNone },		// DEC
	{ 0x4d, 1, 0, 0, 0, enNone },		// DEC
	{ 0x4e, 1, 0, 0, 0, enNone },		// DEC
	{ 0x4f, 1, 0, 0, 0, enNone },		// DEC

	{ 0x50, 1, 0, 0, 0, enNone },		// PUSH 
	{ 0x51, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x52, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x53, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x54, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x55, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x56, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x57, 1, 0, 0, 0, enNone },		// PUSH
	{ 0x58, 1, 0, 0, 0, enNone },		// POP
	{ 0x59, 1, 0, 0, 0, enNone },		// POP
	{ 0x5a, 1, 0, 0, 0, enNone },		// POP
	{ 0x5b, 1, 0, 0, 0, enNone },		// POP
	{ 0x5c, 1, 0, 0, 0, enNone },		// POP
	{ 0x5d, 1, 0, 0, 0, enNone },		// POP
	{ 0x5e, 1, 0, 0, 0, enNone },		// POP
	{ 0x5f, 1, 0, 0, 0, enNone },		// POP

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0x60, 1, 0, 0, 0, enNone },		// PUSHAD
	{ 0x61, 1, 0, 0, 0, enNone },		// POPAD
	{ 0x62, 0, 0, 0, 0, enNone },		// undefined
	{ 0x63, 0, 0, 0, 0, enNone },		// undefined
	{ 0x64, 1, 0, 0, 0, enNone },		// FS prefix
	{ 0x65, 1, 0, 0, 0, enNone },		// GS prefix
	{ 0x66, 1, 0, 0, 0, enPrefix },		// operand size attribute
	{ 0x67, 1, 0, 0, 0, enPrefix },		// address size attribute 
	{ 0x68, 1, 6, 0, 0, enImmediateData },		// PUSH immediate data
	{ 0x69, 0, 0, 0, 0, enNone },		// undefined
	{ 0x6a, 1, 6, 0, 0, enImmediateData },		// PUSH immediate data
	{ 0x6b, 0, 0, 0, 0, enNone },		// undefined
	{ 0x6c, 1, 0, 0, 0, enNone },		// INS
	{ 0x6d, 1, 0, 0, 0, enNone },		// INS
	{ 0x6e, 1, 0, 0, 0, enNone },		// OUTS
	{ 0x6f, 1, 0, 0, 0, enNone },		// OUTS

	{ 0x70, 1, 0, 0, 0, en8BitDisplacement },		// Jcc - JMP if condition met
	{ 0x71, 1, 0, 0, 0, en8BitDisplacement },		//
	{ 0x72, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x73, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x74, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x75, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x76, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x77, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x78, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x79, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x7a, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x7b, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x7c, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x7d, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x7e, 1, 0, 0, 0, en8BitDisplacement },		// 
	{ 0x7f, 1, 0, 0, 0, en8BitDisplacement },		// Jcc - JMP if condition met

 	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0x80, 2, 6, 7, 1, enImmediateData },	// add /r /m immediate
	{ 0x81, 2, 6, 7, 1, enImmediateData },	// add /r /m immediate
	{ 0x82, 2, 6, 7, 1, enImmediateData },	// add /r /m immediate
	{ 0x83, 2, 6, 7, 1, enImmediateData },	// add /r /m immediate
	{ 0x84, 2, 0, 0, 1, enNone },		// TEST /r
	{ 0x85, 2, 0, 0, 1, enNone },		// TEST /r
	{ 0x86, 2, 0, 0, 1, enNone },		// XCHG
	{ 0x87, 2, 0, 0, 1, enNone },		// XCHG
	{ 0x88, 2, 0, 0, 1, enNone },		// MOV /r1 /r2
	{ 0x89, 2, 0, 0, 1, enNone },		// MOV /r1 /r2
	{ 0x8a, 2, 0, 0, 1, enNone },		// MOV /r1 /r2
	{ 0x8b, 2, 0, 0, 1, enNone },		// MOV /r1 /r2
	{ 0x8c, 2, 0, 0, 1, enNone },		// MOV /r
	{ 0x8d, 2, 0, 0, 1, enNone },		// LEA
	{ 0x8e, 2, 0, 0, 1, enNone },		// MOV /r
	{ 0x8f, 2, 0, 0, 1, enNone },		// POP register

	{ 0x90, 1, 0, 0, 0, enNone },		// NOP
	{ 0x91, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x92, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x93, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x94, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x95, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x96, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x97, 1, 0, 0, 0, enNone },		// XCHG
	{ 0x98, 1, 0, 0, 0, enNone },		// CWDE
	{ 0x99, 1, 0, 0, 0, enNone },		// CDQ
	{ 0x9a, 1, 0, 0, 0, enFullDisplacement},		// CALL
	{ 0x9b, 1, 0, 0, 0, enNone },		// WAIT/FWAIT
	{ 0x9c, 1, 0, 0, 0, enNone },		// PUSHFD
	{ 0x9d, 1, 0, 0, 0, enNone },		// POPFD
	{ 0x9e, 1, 0, 0, 0, enNone },		// SAHF
	{ 0x9f, 1, 0, 0, 0, enNone },		// LAHF

	{ 0xa0, 1, 0, 7, 0, enFullDisplacement },		// MOV
	{ 0xa1, 1, 0, 7, 0, enFullDisplacement },		// MOV
	{ 0xa2, 1, 0, 7, 0, enFullDisplacement },		// MOV
	{ 0xa3, 1, 0, 7, 0, enFullDisplacement },		// MOV
	{ 0xa4, 1, 0, 0, 0, enNone },		// MOVS
	{ 0xa5, 1, 0, 0, 0, enNone },		// MOVS
	{ 0xa6, 1, 0, 0, 0, enNone },		// CMPS
	{ 0xa7, 1, 0, 0, 0, enNone },		// CMPS
	{ 0xa8, 1, 0, 7, 0, enImmediateData },		// TEST
	{ 0xa9, 1, 0, 7, 0, enImmediateData },		// TEST
	{ 0xaa, 1, 0, 0, 0, enNone },		// STOS
	{ 0xab, 1, 0, 0, 0, enNone },		// STOS
	{ 0xac, 1, 0, 0, 0, enNone },		// LODS
	{ 0xad, 1, 0, 0, 0, enNone },		// LODS
	{ 0xae, 1, 0, 0, 0, enNone },		// SCAS
	{ 0xaf, 1, 0, 0, 0, enNone },		// SCAS

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0xb0, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb1, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb2, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb3, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb4, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb5, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb6, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb7, 0, 0, 0, 0, enNone },		// undefined
	{ 0xb8, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xb9, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xba, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xbb, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xbc, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xbd, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xbe, 1, 0, 4, 0, enImmediateData },		// MOV
	{ 0xbf, 1, 0, 4, 0, enImmediateData },		// MOV

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0xc0, 0, 0, 0, 0, enNone },		// undefined
	{ 0xc1, 0, 0, 0, 0, enNone },		// undefined
	{ 0xc2, 1, 0, 0, 0, en16BitDisplacement },		// RET
	{ 0xc3, 1, 0, 0, 0, enNone },		// RET
	{ 0xc4, 0, 0, 0, 0, enNone },		// undefined
	{ 0xc5, 0, 0, 0, 0, enNone },		// undefined
	{ 0xc6, 0, 0, 0, 0, enNone },		// undefined
	{ 0xc7, 2, 0, 7, 1, enImmediateData },		// MOV /r /m /immediate
	{ 0xc8, 0, 0, 0, 0, enNone },		// undefined
	{ 0xc9, 1, 0, 0, 0, enNone },		// LEAVE
	{ 0xca, 0, 0, 0, 0, enNone },		// undefined
	{ 0xcb, 0, 0, 0, 0, enNone },		// undefined
	{ 0xcc, 0, 0, 0, 0, enNone },		// undefined
	{ 0xcd, 0, 0, 0, 0, enNone },		// undefined
	{ 0xce, 0, 0, 0, 0, enNone },		// undefined
	{ 0xcf, 0, 0, 0, 0, enNone },		// undefined

	{ 0xd0, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd1, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd2, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd3, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd4, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd5, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd6, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd7, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd8, 0, 0, 0, 0, enNone },		// undefined
	{ 0xd9, 0, 0, 0, 0, enNone },		// undefined
	{ 0xda, 0, 0, 0, 0, enNone },		// undefined
	{ 0xdb, 0, 0, 0, 0, enNone },		// undefined
	{ 0xdc, 0, 0, 0, 0, enNone },		// undefined
	{ 0xdd, 0, 0, 0, 0, enNone },		// undefined
	{ 0xde, 0, 0, 0, 0, enNone },		// undefined
	{ 0xdf, 0, 0, 0, 0, enNone },		// undefined

	{ 0xe0, 0, 0, 0, 0, enNone },		// undefined
	{ 0xe1, 0, 0, 0, 0, enNone },		// undefined
	{ 0xe2, 0, 0, 0, 0, enNone },		// undefined
	{ 0xe3, 1, 0, 0, 0, en8BitDisplacement },		// JCXZ/JECXZ
	{ 0xe4, 2, 0, 0, 0, enNone },		// IN ib
	{ 0xe5, 2, 0, 0, 0, enNone },		// IN ib
	{ 0xe6, 2, 0, 0, 0, enNone },		// OUT ib
	{ 0xe7, 2, 0, 0, 0, enNone },		// OUT ib
	{ 0xe8, 1, 0, 0, 0, enFullDisplacement }, // call full displacement
	{ 0xe9, 1, 0, 0, 0, enFullDisplacement }, // JMP full displacement
	{ 0xea, 1, 0, 0, 0, enFullDisplacement },		// JMP full displacement
	{ 0xeb, 1, 0, 0, 0, en8BitDisplacement },		// JMP short -  en8BitDisplacement
	{ 0xec, 1, 0, 0, 0, enNone },		// IN
	{ 0xed, 1, 0, 0, 0, enNone },		// IN
	{ 0xee, 1, 0, 0, 0, enNone },		// OUT
	{ 0xef, 1, 0, 0, 0, enNone },		// OUT

	// OpCode, OpSize, Offset_s, Offset_w, bModRM, Flags
	{ 0xf0, 1, 0, 0, 0, enNone },		// LOCK prefix
	{ 0xf1, 0, 0, 0, 0, enNone },		// undefined
	{ 0xf2, 1, 0, 0, 0, enNone },		// REPNE prefix
	{ 0xf3, 1, 0, 0, 0, enNone },		// REPE prefix
	{ 0xf4, 1, 0, 0, 0, enNone },		// HLT
	{ 0xf5, 1, 0, 0, 0, enNone },		// CMC
	{ 0xf6, 0, 0, 0, 0, enNone },		// undefined - handle seperate, TEST and DIV
	{ 0xf7, 0, 0, 0, 0, enNone },		// undefined - handle seperate, TEST and DIV
	{ 0xf8, 1, 0, 0, 0, enNone },		// CLC
	{ 0xf9, 1, 0, 0, 0, enNone },		// STC
	{ 0xfa, 1, 0, 0, 0, enNone },		// CLI
	{ 0xfb, 1, 0, 0, 0, enNone },		// STI
	{ 0xfc, 1, 0, 0, 0, enNone },		// CLD
	{ 0xfd, 1, 0, 0, 0, enNone },		// STD
	{ 0xfe, 2, 0, 7, 1, enNone },		// DEC
	{ 0xff, 0, 0, 0, 0, enNone },		// Copy FF
};


