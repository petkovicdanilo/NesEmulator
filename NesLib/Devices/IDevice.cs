using System;

namespace NesLib.Devices
{
    public interface IDevice
    {
        void Write(UInt16 address, byte data);
        byte Read(UInt16 address);
    }
}
