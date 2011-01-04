using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using BusPirateLibCS;

namespace BusPiratePICProgrammer
{
	public partial class PIC18Programmer : PicProgrammer
	{

		public PIC18Programmer(SerialPort sp, bool LVP = true)
			: base(sp, LVP)
		{
			hw.PicMode = BusPirateLibCS.Modes.RawWire.PICMode.PIC416;

		}

		

		private int WriteBlockSize { 
			get {
				return 64;
			}
		}

		private int EraseBlockSize
		{
			get
			{
				return 64;
			}
		}

		public override void bulkErase(bool eraseEEPROM = true)
		{

			Program = true;

			if (eraseEEPROM == false)
				throw new NotImplementedException("erase without eeprom erase not implemented");

			setAddress(0x3c0005);

			byte ercode1 = 0x3f;// 0x3f;
			byte ercode2 = 0x8f; // 0x8f;
			icspInstruction(0xc, ercode1, ercode1);

			setAddress(0x3c0004);

			icspInstruction(0xc, ercode2, ercode2);

			BulkErase();


			BusPirate.Wait(10);

			Program = false;

		}



		public override void eraseData()
		{
			throw new NotImplementedException();
		}

		public override void writeCode(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{

			Program = true;

			CoreInstruction(0x8e, 0xa6); // BSF EECON1, EEPGD
			CoreInstruction(0x9c, 0xa6); // BCF EECON1, CFGS
			//CoreInstruction(0x84, 0xa6); // BSF EECON1, WREN


			var initialOffset = address % WriteBlockSize;
			var blockAddress = address - initialOffset;

			var shiftedData = new byte[length + initialOffset];

			Array.ConstrainedCopy(data, offset, shiftedData, initialOffset, length);

			for (int i = 0; i < initialOffset; i++)
			{
				shiftedData[i] = 0xff;
			}

			length = length + initialOffset;
			address = (address  - initialOffset);
			data = shiftedData;

			var leftOver = length % WriteBlockSize;
			var paddedLength = length + (WriteBlockSize - leftOver) % WriteBlockSize;
			byte[] paddedData = new byte[paddedLength];
			Array.ConstrainedCopy(data, offset, paddedData, 0, length);
			
			for (int i = length; i < paddedData.Length; i++ ) {
				paddedData[i] = 0xff;
			}

			for (int block = 0; block < paddedLength / WriteBlockSize; block++)
			{
				setAddress(address + block * WriteBlockSize);

				for (int i = 0; i < WriteBlockSize && (block * WriteBlockSize + i < paddedData.Length); i += 2)
				{
					var arrayIndex = block * WriteBlockSize + i;

					if (i == WriteBlockSize - 2 || arrayIndex + 2 == paddedData.Length)
					{
						//TableWriteProg(paddedData[arrayIndex], paddedData[arrayIndex + 1]);
						TableWriteProg(paddedData[arrayIndex + 1], paddedData[arrayIndex]);
					} else {
						//TableWriteInc2(paddedData[arrayIndex], paddedData[arrayIndex + 1]);
						TableWriteInc2(paddedData[arrayIndex + 1], paddedData[arrayIndex]);
					}
				}
			}
			Program = false;
		}

		public override void writeData(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			throw new NotImplementedException();
		}

		public override void writeConfig(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			Program = true;

			CoreInstruction(0x8e, 0xa6); // BSF EECON1, EEPGD
			CoreInstruction(0x8c, 0xa6); // BSF EECON1, CFGS
			CoreInstruction(0x84, 0xa6); // BSF EECON1, WREN

			for (int i = 0; i < length; i+=2 ) {
				setAddress(address + i);

				byte data1 = data[offset + i];
				byte data2 = 0;
				if (i+1 < length)  {
					data2 = data[offset + i + 1];
				}

				TableWriteProg(0, data1);

				setAddress(address + i + 1);

				TableWriteProg(data2, 0);

			}

			
			Program = false;


		}

		public override void readData(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			Program = true;

			CoreInstruction(0x0E, 0);
			CoreInstruction(0x6E, 0x92);

			CoreInstruction(0x0E, (byte)address);
			CoreInstruction(0x6E,0x80);
			BusPirate.Wait(1);
			Program = false;
		}

		public override void readCode(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			Program = true;

			setAddress(address);
			for (int i = 0; i < length; i++)
			{
				setAddress(address + i);
				data[offset + i] = readCodeByteInc();
				if (data[offset + i] != 0xff)
				{
					//Debugger.Break();
				}
				if (pr != null)
				{
					pr((i+1) * 100 / length);
				}
			}

			Program = false;
		}

		private byte readCodeByte()
		{
			return TableRead();
		}
	}
}
