using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;
using System.Threading;
using BusPirateLibCS;
using BusPirateLibCS.Modes;

namespace BusPiratePICProgrammer
{
	public abstract class PicProgrammer
	{
		protected BusPirate bp;
		
		protected RawWire hw;
		
		bool lvp = false;
		
		public PicProgrammer(SerialPort sp, bool LVP)
		{
			bp = new BusPirate(sp);
			bp.Open();
			hw = new RawWire(bp);
			hw.EnterMode();

			var ActiveOut = true;


			hw.ConfigProtocol(activeOutput: ActiveOut, threeWire: false, LSBfirst: true);
			hw.ConfigPins(power: true, pullups: !ActiveOut, aux: false, cs: true);
			hw.SpeedMode = RawWire.Speed.s400khz;
			this.lvp = LVP;
		}

		bool program = false;
		public bool Program
		{
			set
			{
				//hw.Power = false;
				//hw.CS = false;
				BusPirate.Wait(50);

				hw.OutputPin = false;
				hw.ClockPin = false;

				if (lvp)
				{
					if (value)
					{
						hw.CS = value;
						BusPirate.Wait(10);
						hw.AUX = value;
					}
					else
					{
						hw.AUX = value;
						BusPirate.Wait(10);
						hw.CS = value;
					}
				}
				else {
					if (value)
					{
						hw.Power = true;
						BusPirate.Wait(10);
					}
					hw.AUX = value;
					if (!value)
					{
						BusPirate.Wait(10);
						hw.Power = false;
						BusPirate.Wait(10);
						hw.Power = true;
					}
				}

				BusPirate.Wait(50);
				//hw.Power = true;
				//hw.CS = true;

				program = value;
			}
			get
			{
				return program;
			}

		}

		public void DelayMs(int time)
		{
			Thread.Sleep(time);
		}

		public abstract void bulkErase(bool eraseEEPROM = true);

		public abstract void eraseData();

		public abstract void writeCode(int address, byte[] data, int offset, int length, ProgressReporter pr = null);

		public abstract void writeData(int address, byte[] data, int offset, int length, ProgressReporter pr = null);

		public delegate void ProgressReporter(int percentage);

		public abstract void writeConfig(int address, byte[] data, int offset, int length, ProgressReporter pr = null);

		public abstract void readData(int address, byte[] data, int offset, int length, ProgressReporter pr = null);

		public abstract void readCode(int address, byte[] data, int offset, int length, ProgressReporter pr = null);



		public void close()
		{
			hw.ExitMode();
			bp.Close();
		}


	}
}
