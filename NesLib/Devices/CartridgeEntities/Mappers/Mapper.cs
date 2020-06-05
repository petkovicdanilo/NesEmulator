using System;

namespace NesLib.Devices.CartridgeEntities.Mappers
{
    [Serializable]
    public abstract class Mapper
    {
        protected int prgBanks, chrBanks;

        public Mapper(int prgBanks, int chrBanks)
        {
            this.prgBanks = prgBanks;
            this.chrBanks = chrBanks;
        }

        // Read/Write methods return boolean to check if they populated mappedAddress
        public abstract bool MapCpuRead(UInt16 address, ref UInt16 mappedAddress);
        public abstract bool MapCpuWrite(UInt16 address, ref UInt16 mappedAddress, byte data);

        public abstract bool MapPpuRead(UInt16 address, ref UInt16 mappedAddress);
        public abstract bool MapPpuWrite(UInt16 address, ref UInt16 mappedAddress);

        public abstract void Reset();
    }
}
