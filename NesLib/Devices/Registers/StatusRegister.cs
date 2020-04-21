namespace NesLib.Devices.Registers
{
    public class StatusRegister
    {
        public byte Register { get; set; }

        // C
        public bool CarryFlag
        {
            get
            {
                return getBit(0);
            }
            set
            {
                setBit(0, value);
            }
        }

        // Z
        public bool ZeroFlag
        {
            get
            {
                return getBit(1);
            }
            set
            {
                setBit(1, value);
            }
        }

        // I
        public bool InterruptDisableFlag
        {
            get
            {
                return getBit(2);
            }
            set
            {
                setBit(2, value);
            }
        }

        // D
        // unused in NES emulation
        public bool DecimalFlag
        {
            get
            {
                return getBit(3);
            }
            set
            {
                setBit(3, value);
            }
        }

        // B
        public bool BreakFlag
        {
            get
            {
                return getBit(4);
            }
            set
            {
                setBit(4, value);
            }
        }

        // U
        public bool UnusedFlag
        {
            get
            {
                return getBit(5);
            }
            set
            {
                setBit(5, value);
            }
        }

        // V
        public bool OverflowFlag
        {
            get
            {
                return getBit(6);
            }
            set
            {
                setBit(6, value);
            }
        }

        // N
        public bool NegativeFlag
        {
            get
            {
                return getBit(7);
            }
            set
            {
                setBit(7, value);
            }
        }

        public StatusRegister()
        {
            this.Register = 0x00;
        }

        private bool getBit(int n)
        {
            return (Register & (1 << n)) != 0;
        }

        private void setBit(int n, bool value)
        {
            if (value)
            {
                Register |= (byte)(1 << n);
            }
            else
            {
                Register &= (byte)~(1 << n);
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
