using NesLib.Utils;
using System;

namespace NesLib.Devices.CpuEntities
{
    public partial class Cpu
    {
        // each functions returns true if additional cycle may be needed

        // ADC - Add with Carry
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0x69, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x65, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x75, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x6D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x7D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0x79, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0x61, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0x71, Cycles = 5)]
        public bool ADC()
        {
            byte data = Fetch();

            UInt16 result = (UInt16)(A + data + (Status.CarryFlag ? 1 : 0));

            Status.CarryFlag = (result >= (1 << 8));

            byte accSign = (byte)(A & (1 << 7));
            byte dataSign = (byte)(data & (1 << 7));
            byte resultSign = (byte)(result & (1 << 7));

            Status.OverflowFlag = (accSign == dataSign && resultSign != accSign);

            // write one byte result to accumulator
            A = (byte)(result & 0x00FF);
            UpdateNegativeFlag(A);
            UpdateZeroFlag(A);

            return true;
        }

        // AND - Logical and
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0x29, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x25, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x35, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x2D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x3D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0x39, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0x21, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0x31, Cycles = 5)]
        public bool AND()
        {
            byte data = Fetch();

            A &= data;

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return true;
        }

        // ASL - Arithmetic Shift Left
        [InstructionMethod(AddressingMode = AddressingMode.ACC, Opcode = 0x0A, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x06, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x16, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x0E, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x1E, Cycles = 7)]
        public bool ASL()
        {
            byte data = Fetch();

            Status.CarryFlag = BitMagic.IsBitSet(data, 7);

            data <<= 1;

            UpdateZeroFlag(data);
            UpdateNegativeFlag(data);

            if (operations[currentOpcode].Mode == AddressingMode.ACC)
            {
                A = data;
            }
            else
            {
                CpuWrite(address, data);
            }

            return false;
        }

        // BCC - Branch if Carry Clear
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0x90, Cycles = 2)]
        public bool BCC()
        {
            if (!Status.CarryFlag)
            {
                Branch();
            }

            return false;
        }

        // BCS - Branch if Carry Set
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0xB0, Cycles = 2)]
        public bool BCS()
        {
            if (Status.CarryFlag)
            {
                Branch();
            }

            return false;
        }

        // BEQ - Branch if Equal
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0xF0, Cycles = 2)]
        public bool BEQ()
        {
            if (Status.ZeroFlag)
            {
                Branch();
            }

            return false;
        }

        // BIT - Bit Test
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x24, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x2C, Cycles = 4)]
        public bool BIT()
        {
            byte data = Fetch();
            byte result = (byte)(A & data);

            UpdateZeroFlag(result);
            Status.OverflowFlag = (data & (1 << 6)) != 0;
            UpdateNegativeFlag(data);

            return false;
        }

        // BMI - Branch if Minus
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0x30, Cycles = 2)]
        public bool BMI()
        {
            if (Status.NegativeFlag)
            {
                Branch();
            }

            return false;
        }

        // BNE - Branch if Not Equal
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0xD0, Cycles = 2)]
        public bool BNE()   
        {
            if (!Status.ZeroFlag)
            {
                Branch();
            }

            return false;
        }

        // BPL - Branch if Positive
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0x10, Cycles = 2)]
        public bool BPL()
        {
            if (!Status.NegativeFlag)
            {
                Branch();
            }

            return false;
        }

        // BRK - Force Interrupt
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x00, Cycles = 7)]
        public bool BRK()
        {
            PC++;

            Status.InterruptDisableFlag = true;

            // push PC to stack
            StackPushWord(PC);

            Status.BreakFlag = true;
            StackPush(Status.Register);
            Status.BreakFlag = false;

            UInt16 newPCAddress = 0xFFFE;

            PC = ReadWord(newPCAddress);
            
            return false;
        }

        // BVC - Branch if Overflow Clear
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0x50, Cycles = 2)]
        public bool BVC()
        {
            if (!Status.OverflowFlag)
            {
                Branch();
            }

            return false;
        }

        // BVS - Branch if Overflow Set
        [InstructionMethod(AddressingMode = AddressingMode.REL, Opcode = 0x70, Cycles = 2)]
        public bool BVS()
        {
            if (Status.OverflowFlag)
            {
                Branch();
            }

            return false;
        }

        // CLC - Clear Carry Flag
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x18, Cycles = 2)]
        public bool CLC()
        {
            Status.CarryFlag = false;
            return false;
        }

        // CLD - Clear Decimal Mode
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xD8, Cycles = 2)]
        public bool CLD()
        {
            Status.DecimalFlag = false;
            return false;
        }

        // CLI - Clear Interrupt Disable
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x58, Cycles = 2)]
        public bool CLI()
        {
            Status.InterruptDisableFlag = false;
            return false;
        }

        // CLV - Clear Overflow Flag
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xB8, Cycles = 2)]
        public bool CLV()
        {
            Status.OverflowFlag = false;
            return false;
        }

        // CMP - Compare
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xC9, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xC5, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0xD5, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xCD, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0xDD, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0xD9, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0xC1, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0xD1, Cycles = 5)]
        public bool CMP()
        {
            byte data = Fetch();

            byte result = (byte)(A - data);

            Status.CarryFlag = A >= data;
            UpdateZeroFlag(result);
            UpdateNegativeFlag(result);

            return true;
        }

        // CPX - Compare X Register
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xE0, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xE4, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xEC, Cycles = 4)]
        public bool CPX()
        {
            byte data = Fetch();

            byte result = (byte)(X - data);

            Status.CarryFlag = X >= data;
            UpdateZeroFlag(result);
            UpdateNegativeFlag(result);

            return false;
        }

        // CPY - Compare Y Register
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xC0, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xC4, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xCC, Cycles = 4)]
        public bool CPY()
        {
            byte data = Fetch();

            byte result = (byte)(Y - data);

            Status.CarryFlag = Y >= data;
            UpdateZeroFlag(result);
            UpdateNegativeFlag(result);

            return false;
        }

        // DEC - Decrement Memory
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xC6, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0xD6, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xCE, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0xDE, Cycles = 7)]
        public bool DEC()
        {
            byte data = Fetch();

            data -= 1;

            CpuWrite(address, data);

            UpdateZeroFlag(data);
            UpdateNegativeFlag(data);

            return false;
        }

        // DEX - Decrement X Register
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xCA, Cycles = 2)]
        public bool DEX()
        {
            X -= 1;

            UpdateZeroFlag(X);
            UpdateNegativeFlag(X);

            return false;
        }

        // DEY - Decrement Y Register
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x88, Cycles = 2)]
        public bool DEY()
        {
            Y -= 1;

            UpdateZeroFlag(Y);
            UpdateNegativeFlag(Y);

            return false;
        }

        // EOR - Exclusive OR
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0x49, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x45, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x55, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x4D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x5D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0x59, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0x41, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0x51, Cycles = 5)]
        public bool EOR()
        {
            byte data = Fetch();

            A ^= data;

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return true;
        }

        // INC - Increment Memory
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xE6, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0xF6, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xEE, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0xFE, Cycles = 7)]
        public bool INC()
        {
            byte data = Fetch();

            data += 1;

            CpuWrite(address, data);

            UpdateZeroFlag(data);
            UpdateNegativeFlag(data);

            return false;
        }

        // INX - Increment X Register
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xE8, Cycles = 2)]
        public bool INX()
        {
            X += 1;

            UpdateZeroFlag(X);
            UpdateNegativeFlag(X);

            return false;
        }

        // INY - Increment Y Register
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xC8, Cycles = 2)]
        public bool INY()
        {
            Y += 1;

            UpdateZeroFlag(Y);
            UpdateNegativeFlag(Y);

            return false;
        }

        // JMP - Jump
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x4C, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.IND, Opcode = 0x6C, Cycles = 5)]
        public bool JMP()
        {
            PC = address;
            return false;
        }

        // JSR - Jump to Subroutine
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x20, Cycles = 6)]
        public bool JSR()
        {
            PC--;
            StackPushWord(PC);

            PC = address;

            return false;
        }

        // LDA - Load Accumulator
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xA9, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xA5, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0xB5, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xAD, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0xBD, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0xB9, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0xA1, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0xB1, Cycles = 5)]
        public bool LDA()
        {
            byte data = Fetch();

            A = data;

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return true;
        }

        // LDX - Load X Register
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xA2, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xA6, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPY, Opcode = 0xB6, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xAE, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0xBE, Cycles = 4)]
        public bool LDX()
        {
            X = Fetch();

            UpdateZeroFlag(X);
            UpdateNegativeFlag(X);

            return true;
        }

        // LDY - Load Y Register
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xA0, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xA4, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0xB4, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xAC, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0xBC, Cycles = 4)]
        public bool LDY()
        {
            Y = Fetch();

            UpdateZeroFlag(Y);
            UpdateNegativeFlag(Y);

            return true;
        }

        // LSR - Logical Shift Right
        [InstructionMethod(AddressingMode = AddressingMode.ACC, Opcode = 0x4A, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x46, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x56, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x4E, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x5E, Cycles = 7)]
        public bool LSR()
        {
            byte data = Fetch();

            Status.CarryFlag = (data & (1 << 0)) != 0;

            byte result = (byte)(data >> 1);

            UpdateZeroFlag(result);
            UpdateNegativeFlag(result);

            if (operations[currentOpcode].Mode == AddressingMode.ACC)
            {
                A = result;
            }
            else
            {
                CpuWrite(address, result);
            }

            return false;
        }

        // NOP - No Operation
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xEA, Cycles = 2)]
        public bool NOP()
        {
            return false;
        }

        // ORA - Logical Inclusive OR
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0x09, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x05, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x15, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x0D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x1D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0x19, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0x01, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0x11, Cycles = 5)]
        public bool ORA()
        {
            A |= Fetch();

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return true;
        }

        // PHA - Push Accumulator
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x48, Cycles = 3)]
        public bool PHA()
        {
            StackPush(A);

            return false;
        }

        // PHP - Push Processor Status
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x08, Cycles = 3)]
        public bool PHP()
        {
            Status.BreakFlag = true;
            Status.UnusedFlag = true;

            StackPush(Status.Register);

            Status.BreakFlag = false;
            Status.UnusedFlag = false;

            return false;
        }

        // PLA - Pull Accumulator
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x68, Cycles = 4)]
        public bool PLA()
        {
            A = StackPop();

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return false;
        }

        // PLP - Pull Processor Status
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x28, Cycles = 4)]
        public bool PLP()
        {
            Status.Register = StackPop();
            Status.UnusedFlag = true;

            return false;
        }

        // ROL - Rotate Left
        [InstructionMethod(AddressingMode = AddressingMode.ACC, Opcode = 0x2A, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x26, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x36, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x2E, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x3E, Cycles = 7)]
        public bool ROL()
        {
            byte data = Fetch();

            bool bit7Set = BitMagic.IsBitSet(data, 7);

            data <<= 1;
            BitMagic.SetBit(ref data, 0, Status.CarryFlag);
            Status.CarryFlag = bit7Set;

            UpdateZeroFlag(data);
            UpdateNegativeFlag(data);

            if(operations[currentOpcode].Mode == AddressingMode.ACC)
            {
                A = data;
            }
            else
            {
                CpuWrite(address, data);
            }

            return false;
        }

        // ROR - Rotate Right
        [InstructionMethod(AddressingMode = AddressingMode.ACC, Opcode = 0x6A, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x66, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x76, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x6E, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x7E, Cycles = 7)]
        public bool ROR()
        {
            byte data = Fetch();

            bool bit0Set = BitMagic.IsBitSet(data, 0);

            data >>= 1;
            BitMagic.SetBit(ref data, 7, Status.CarryFlag);
            Status.CarryFlag = bit0Set;

            UpdateZeroFlag(data);
            UpdateNegativeFlag(data);

            if (operations[currentOpcode].Mode == AddressingMode.ACC)
            {
                A = data;
            }
            else
            {
                CpuWrite(address, data);
            }

            return false;
        }

        // RTI - Return from Interrupt
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x40, Cycles = 6)]
        public bool RTI()
        {
            Status.Register = StackPop();
            Status.BreakFlag = false;
            Status.UnusedFlag = false;

            PC = StackPopWord();

            return false;
        }

        // RTS - Return from Subroutine
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x60, Cycles = 6)]
        public bool RTS()
        {
            PC = StackPopWord();
            PC++;

            return false;
        }

        // SBC - Subtract with Carry
        [InstructionMethod(AddressingMode = AddressingMode.IMM, Opcode = 0xE9, Cycles = 2)]
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0xE5, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0xF5, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0xED, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0xFD, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0xF9, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0xE1, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0xF1, Cycles = 5)]
        public bool SBC()
        {
            // invert bits
            byte data = (byte)(~Fetch());

            // carry flag is now borrow flag
            UInt16 result = (UInt16)(A + data + (Status.CarryFlag ? 1 : 0));

            Status.CarryFlag = (result & (1 << 8)) != 0;

            byte accSign = (byte)(A & (1 << 7));
            byte dataSign = (byte)(data & (1 << 7));
            byte resultSign = (byte)(result & (1 << 7));

            Status.OverflowFlag = (accSign == dataSign && resultSign != accSign);

            A = (byte)(result & 0x00FF);
            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return true;
        }

        // SEC - Set Carry Flag
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x38, Cycles = 2)]
        public bool SEC()
        {
            Status.CarryFlag = true;
            return false;
        }

        // SED - Set Decimal Flag
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xF8, Cycles = 2)]
        public bool SED()
        {
            Status.DecimalFlag = false;
            return false;
        }

        // SEI - Set Interrupt Disable
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x78, Cycles = 2)]
        public bool SEI()
        {
            Status.InterruptDisableFlag = true;
            return false;
        }

        // STA - Store Accumulator
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x85, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x95, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x8D, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABX, Opcode = 0x9D, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.ABY, Opcode = 0x99, Cycles = 5)]
        [InstructionMethod(AddressingMode = AddressingMode.IZX, Opcode = 0x81, Cycles = 6)]
        [InstructionMethod(AddressingMode = AddressingMode.IZY, Opcode = 0x91, Cycles = 6)]
        public bool STA()
        {
            CpuWrite(address, A);
            return false;
        }

        // STX - Store X Register
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x86, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPY, Opcode = 0x96, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x8E, Cycles = 4)]
        public bool STX()
        {
            CpuWrite(address, X);
            return false;
        }

        // STY - Store Y Register
        [InstructionMethod(AddressingMode = AddressingMode.ZP0, Opcode = 0x84, Cycles = 3)]
        [InstructionMethod(AddressingMode = AddressingMode.ZPX, Opcode = 0x94, Cycles = 4)]
        [InstructionMethod(AddressingMode = AddressingMode.ABS, Opcode = 0x8C, Cycles = 4)]
        public bool STY()
        {
            CpuWrite(address, Y);
            return false;
        }

        // TAX - Transfer Accumulator to X
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xAA, Cycles = 2)]
        public bool TAX()
        {
            X = A;

            UpdateZeroFlag(X);
            UpdateNegativeFlag(X);

            return false;
        }

        // TAY - Transfer Accumulator to Y
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xA8, Cycles = 2)]
        public bool TAY()
        {
            Y = A;

            UpdateZeroFlag(Y);
            UpdateNegativeFlag(Y);

            return false;
        }

        // TSX - Transfer Stack Pointer to X
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0xBA, Cycles = 2)]
        public bool TSX()
        {
            X = StackPointer;

            UpdateZeroFlag(X);
            UpdateNegativeFlag(X);

            return false;
        }

        // TXA - Transfer X to Accumulator
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x8A, Cycles = 2)]
        public bool TXA()
        {
            A = X;

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return false;
        }

        // TXS - Transfer X to Stack Pointer
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x9A, Cycles = 2)]
        public bool TXS()
        {
            StackPointer = X;
            return false;
        }

        // TYS - Transfer Y to Accumulator
        [InstructionMethod(AddressingMode = AddressingMode.IMP, Opcode = 0x98, Cycles = 2)]
        public bool TYA()
        {
            A = Y;

            UpdateZeroFlag(A);
            UpdateNegativeFlag(A);

            return false;
        }

        private void Branch()
        {
            UInt16 newAddress = (UInt16)(PC + jumpOffset);

            // +1 if branch succeeds
            cycles++;

            // +1 if page is crossed
            if (Page(newAddress) != Page(PC))
            {
                cycles++;
            }

            PC = newAddress;
        }
    }
}