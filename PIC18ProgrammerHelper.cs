using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusPirateLibCS;

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


		const byte MOVLW = 0x0e;
		const byte MOVWF = 0x6e;

		const byte TBLPTRU = 0xf8;
		const byte TBLPTRH = 0xf7;
		const byte TBLPTRL = 0xf6;

		private void loadTBLPTR(int address)
		{

			CoreInstruction(MOVLW, (byte)((address >> 16) & 0xff));	// MOVLW Addr[21:16]
			CoreInstruction(MOVWF, TBLPTRU);							// MOVWF TBLRPTRU
			CoreInstruction(MOVLW, (byte)((address >> 8) & 0xff));	// MOVLW Addr[15:8]
			CoreInstruction(MOVWF, TBLPTRH);							// MOVWF TBLRPTRH
			CoreInstruction(MOVLW, (byte)((address) & 0xff));	// MOVLW Addr[7:0]
			CoreInstruction(MOVWF, TBLPTRL);							// MOVWF TBLRPTR
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
			BusPirate.Wait(1);
			hw.WriteByte(0);
			BusPirate.Wait(1);
			var read = hw.ReadByte();
			BusPirate.Wait(1);
			return read;
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

		private void TableWriteInc2(byte data1, byte data2)
		{
			icspInstruction(0xd, data1, data2);
		}

		private void TableWriteProg(byte data1, byte data2) {
			icspInstruction(0xf, data1, data2);

			hw.WriteBits(0, 3);
			hw.OutputPin = false;
			hw.ClockPin = true;
			BusPirate.Wait(10);
			hw.ClockPin = false;
			hw.WriteBulk(new byte[] { 0, 0 });

		}


		private void BulkErase()
		{


			CoreInstruction(0, 0);

			hw.WriteBits(0, 4);
			//hw.OutputPin = false;
			hw.ClockPin = false;
			BusPirate.Wait(10);
			//hw.ClockPin = false;
			
			hw.WriteBulk(new byte[] { 0, 0 });
		}

	}
}
