using NesLib.Utils;
using System;

namespace NesLib.Devices.PpuEntities.Registers
{
    [Serializable]
    public class StatusRegister : AbstractRegister
    {
        public bool VerticalBlank
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

        public bool SpriteZeroHit
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

        public bool SpriteOverflow
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
    }
}
