using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;

namespace BusPiratePICProgrammer
{
	public partial class PIC18Programmer : PicProgrammer
	{

		public PIC18Programmer(SerialPort sp, bool LVP = true)
			: base(sp, LVP)
		{

		}

		public override void bulkErase(bool eraseEEPROM = true)
		{
			throw new NotImplementedException();
		}

		public override void eraseData()
		{
			throw new NotImplementedException();
		}

		public override void writeCode(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			throw new NotImplementedException();
		}

		public override void writeData(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			throw new NotImplementedException();
		}

		public override void writeConfig(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			throw new NotImplementedException();
		}

		public override void readData(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			throw new NotImplementedException();
		}

		public override void readCode(int address, byte[] data, int offset, int length, PicProgrammer.ProgressReporter pr = null)
		{
			Program = true;

			setAddress(address);
			for (int i = 0; i < length; i++)
			{
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
	}
}
