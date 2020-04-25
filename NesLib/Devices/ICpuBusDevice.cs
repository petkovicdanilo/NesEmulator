using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices
{
    public interface ICpuBusDevice
    {
        byte CpuRead(UInt16 address);
        void CpuWrite(UInt16 address, byte data);
    }
}
