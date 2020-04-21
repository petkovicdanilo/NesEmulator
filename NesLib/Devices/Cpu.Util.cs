using System;

namespace NesLib.Devices
{
    public partial class Cpu
    {
        private enum AddressingMode
        {
            ACC, // accumulator
            IMP, // implied
            IMM, // immediate
            ZP0, // zero page
            ZPX, // zero page with X offset
            ZPY, // zero page with Y offset
            REL, // relative
            ABS, // absolute
            ABX, // absolute with X offset
            ABY, // absolute with Y offset
            IND, // indirect
            IZX, // indirect X
            IZY, // indirect Y
        }


        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        private class AddressingModeMethod : Attribute
        {
            public AddressingMode AddressingMode;
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private class InstructionMethod : Attribute
        {
            public byte Opcode;
            public AddressingMode AddressingMode;
            public byte Cycles;
        }

        private delegate bool Instruction();

        private class Operation
        {
            public Instruction Instruction;
            public Instruction AddressingModeInstruction;
            public AddressingMode Mode;
            public String InstructionName;
            public int Cycles;
        }

        private UInt16 Page(UInt16 memAddr)
        {
            return (UInt16)(memAddr & 0xFF00);
        }

        private byte Fetch()
        {
            if (operations[currentOpcode].Mode == AddressingMode.ACC)
            {
                return A;
            }

            return Read(address);
        }

        private void StackPush(byte data)
        {
            Write((UInt16)(StackBaseAddress + StackPointer), data);
            StackPointer--;
        }

        private byte StackPop()
        {
            byte res = Read((UInt16)(StackBaseAddress + StackPointer));
            StackPointer++;
            return res;
        }
    }
}