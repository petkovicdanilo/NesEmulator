using NesLib.Devices.Registers.Ppu;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices
{
    public class Ppu : IClockDevice, ICpuBusDevice, IPpuBusDevice
    {
        private StatusRegister statusRegister = new StatusRegister();
        private MaskRegister maskRegister = new MaskRegister();
        private ControlRegister controlRegister = new ControlRegister();

        private byte[,] nameTable = new byte[2, 1024];
        private byte[] palette = new byte[32];

        public byte CpuRead(UInt16 address)
        {
            return 1;
        }

        public void CpuWrite(UInt16 address, byte data)
        {

        }

        public byte PpuRead(UInt16 address)
        {
            return 1;
        }

        public void PpuWrite(UInt16 address, byte data)
        {

        }

        public void Clock()
        {

        }
    }
}
