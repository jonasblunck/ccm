#pragma once

/*
  Copyright Jonas Blunck, 2002-2006

  All rights reserved, no warranties extended. Use at your own risk!

*/

typedef enum InstrInfoFlags
{
	enNone = 0,
	enImmediateData,
	en8BitImmediateData,
	en16BitImmediateData,
	enFullDisplacement,
	en8BitDisplacement,
	en16BitDisplacement,
  enPrefix,
} InstrInfoFlags;

typedef struct InstrInfo
{
	BYTE iOpCode : 8;			// byte code for instruction, 0xe8 for CALL, etc..
	int  iOpSize : 4;			// size of instruction, excluding sib, displacement and immediate data
	int  iOffset_s : 4;			// offset to s-bit (how many bits counting from msb)
	int  iOffset_w : 4;			// offset to w-bit
	bool bModRM : 1;		// has ModRM-bit
	InstrInfoFlags  iFlags : 8;
} InstrInfo;

class CDisAssm
{
private:
	bool m_b16BitAddress;
	bool m_b16BitOperand;

	static InstrInfo m_aInstructions[256];

	bool GetBit(int iBitCountingFromMSB, const BYTE* pInstruction);
	bool GetInstructionInfoFF(InstrInfo& rInfo, const BYTE* pInstruction);

public:
	CDisAssm(void);
	~CDisAssm(void);

	int  CalcSize(const InstrInfo& rInstrInfo, const BYTE* pInstruction);
  int  CalcSizeDisplacement(const InstrInfo& rInstrInfo, const BYTE* pInstruction);
	int  CalcSizeForModRM(const InstrInfo& rInstrInfo, const BYTE* pInstruction);
	int  CalcSizeImmediateData(const InstrInfo& rInstrInfo, const BYTE* pInstruction);

  bool GetInstructionInfo(const BYTE* pInstruction, InstrInfo& rInfo, int& riLen);
	bool HasSibByte(const InstrInfo& rInstrInfo, const BYTE* pInstruction) const;

  bool Is16BitAddress() const;
	bool Is16BitOperand() const;


};
