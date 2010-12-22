using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusPiratePICProgrammer
{
	public partial class PIC18Programmer
	{
		private byte readCodeByte(int address)
		{
			loadTBLPTR(address);
			return TableRead();
		}

		private void setAddress(int address)
		{
			loadTBLPTR(address);
		}

		private byte readCodeByteInc()
		{
			return TableReadInc();
		}

		private void loadTBLPTR(int address)
		{
			CoreInstruction(0x0e, (byte)((address >> 16) & 0xff));	// MOVLW Addr[21:16]
			CoreInstruction(0x6e, 0xf8);							// MOVWF TBLRPTRU
			CoreInstruction(0x0e, (byte)((address >>  8) & 0xff));	// MOVLW Addr[15:8]
			CoreInstruction(0x6e, 0xf7);							// MOVWF TBLRPTRH
			CoreInstruction(0x0e, (byte)((address      ) & 0xff));	// MOVLW Addr[7:0]
			CoreInstruction(0x6e, 0xf6);							// MOVWF TBLRPTR
		}


		private void icspInstruction(int code, byte data1, byte data2)
		{
			hw.WriteBits(code, 4);
			hw.WriteBulk(new byte[] { data2, data1 });
		}


		private byte TableRead()
		{
			hw.WriteBits(0x8, 4);
			hw.WriteByte(0);
			return hw.ReadByte();
		}

		private byte TableReadInc()
		{
			hw.WriteBits(0x9, 4);
			hw.WriteByte(0);
			return hw.ReadByte();
		}

		private byte TableReadDec()
		{
			hw.WriteBits(0x10, 4);
			hw.WriteByte(0);
			return hw.ReadByte();
		}

		private void CoreInstruction(byte data1, byte data2) {
			icspInstruction(0, data1, data2);
		}


	}
}
