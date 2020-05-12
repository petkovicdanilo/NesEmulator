using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesLib.Devices.PpuEntities.OAM
{
    public class Attributes
    {
        private byte _register;

        public byte Register
        {
            get
            {
                return _register;
            }
            set
            {
                _register = value;
            }
        }

        public int Palette
        {
            get
            {
                return Register & 0x03;
            }
            set
            {
                Register &= (byte)(0xFC);

                // we only need bottom 2 bits
                value &= 0x03;
                Register |= (byte)(value);
            }
        }

        public bool Priority
        {
            get
            {
                return BitMagic.IsBitSet(Register, 5);
            }
            set
            {
                BitMagic.SetBit(ref _register, 5, value);
            }
        }

        public bool FlipHorizontally
        {
            get
            {
                return BitMagic.IsBitSet(Register, 6);
            }
            set
            {
                BitMagic.SetBit(ref _register, 6, value);
            }
        }

        public bool FlipVertically
        {
            get
            {
                return BitMagic.IsBitSet(Register, 7);
            }
            set
            {
                BitMagic.SetBit(ref _register, 7, value);
            }
        }
    }
}
