using NesLib.Utils;

namespace NesLib.Devices.Registers.Cpu
{
    public class StatusRegister : AbstractRegister
    {
        // C
        public bool CarryFlag
        {
            get
            {
                return BitMagic.IsBitSet(Register, 0);
            }
            set
            {
                BitMagic.SetBit(ref _register, 0, value);
            }
        }

        // Z
        public bool ZeroFlag
        {
            get
            {
                return BitMagic.IsBitSet(Register, 1);
            }
            set
            {
                BitMagic.SetBit(ref _register, 1, value);
            }
        }

        // I
        public bool InterruptDisableFlag
        {
            get
            {
                return BitMagic.IsBitSet(Register, 2);
            }
            set
            {
                BitMagic.SetBit(ref _register, 2, value);
            }
        }

        // D
        // unused in NES emulation
        public bool DecimalFlag
        {
            get
            {
                return BitMagic.IsBitSet(Register, 3);
            }
            set
            {
                BitMagic.SetBit(ref _register, 3, value);
            }
        }

        // B
        public bool BreakFlag
        {
            get
            {
                return BitMagic.IsBitSet(Register, 4);
            }
            set
            {
                BitMagic.SetBit(ref _register, 4, value);
            }
        }

        // U
        public bool UnusedFlag
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

        // V
        public bool OverflowFlag
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

        // N
        public bool NegativeFlag
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

        public override string ToString()
        {
            return $"Carry={CarryFlag}, Zero={ZeroFlag}, Interrupt={InterruptDisableFlag}, " +
                   $"Decimal={DecimalFlag}, Break={BreakFlag}, Unused={UnusedFlag}, " +
                   $"Overflow={OverflowFlag}, Negative={NegativeFlag}";
        }
    }
}
