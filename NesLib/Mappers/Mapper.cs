using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Mappers
{
    public abstract class Mapper
    {
        protected int prgBanks, chrBanks;

        public Mapper(int prgBanks, int chrBanks)
        {
            this.prgBanks = prgBanks;
            this.chrBanks = chrBanks;
        }

        public abstract UInt16 MapCpuRead(UInt16 address);
        public abstract UInt16 MapCpuWrite(UInt16 address);


        public abstract UInt16 MapPpuRead(UInt16 address);
        public abstract UInt16 MapPpuWrite(UInt16 address);
    }
}
