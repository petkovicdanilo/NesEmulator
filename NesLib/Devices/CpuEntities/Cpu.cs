using NesLib.Devices.CpuEntities.Registers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NesLib.Devices.CpuEntities
{
    [Serializable]
    public partial class Cpu : IClockDevice, ICpuBusDevice
    {
        // accumulator
        public byte A
        {
            get;
            private set;
        }

        public byte X
        {
            get;
            private set;
        }
        public byte Y
        {
            get;
            private set;
        }

        // program counter
        public UInt16 PC
        {
            get;
            private set;
        }

        // stack pointer
        public byte StackPointer
        {
            get;
            private set;
        }

        // processor status
        public StatusRegister Status
        {
            get;
            private set;
        }

        private const UInt16 StackBaseAddress = 0x0100;

        private int cycles;
        private byte currentOpcode;

        private UInt16 address;

        // only for branching
        // between -128 and 127
        private Int16 jumpOffset;

        private Dictionary<byte, Operation> operations;

        private Nes nes;

        public Cpu(Nes nes)
        {
            this.nes = nes;

            Status = new StatusRegister();

            InitInstructions();

            cycles = 0;

        }

        public void InitInstructions()
        {
            operations = new Dictionary<byte, Operation>();

            Dictionary<AddressingMode, Instruction> addressModes = new Dictionary<AddressingMode, Instruction>();

            // find all address methods
            GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .ToList()
                .ForEach(method =>
                {
                    foreach (var attr in method.GetCustomAttributes(typeof(AddressingModeMethod), false))
                    {
                        AddressingModeMethod addrMethodAtribute = (AddressingModeMethod)attr;
                        var del = (Instruction)Delegate.CreateDelegate(typeof(Instruction), this, method.Name);
                        addressModes.Add(addrMethodAtribute.AddressingMode, del);
                    }
                });

            // connect all instructions to address methods via opcodes
            GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .ToList()
                .ForEach(method =>
                {
                    foreach (var attr in method.GetCustomAttributes(typeof(InstructionMethod), false))
                    {
                        InstructionMethod instMethodAttribute = (InstructionMethod)attr;
                        var del = (Instruction)Delegate.CreateDelegate(typeof(Instruction), this, method.Name);
                        operations.Add(
                            instMethodAttribute.Opcode,
                            new Operation
                            {
                                AddressingModeInstruction = addressModes[instMethodAttribute.AddressingMode],
                                Mode = instMethodAttribute.AddressingMode,
                                Instruction = del,
                                InstructionName = method.Name,
                                AddressingModeName = addressModes[instMethodAttribute.AddressingMode]
                                    .Method.Name,
                                Cycles = instMethodAttribute.Cycles
                            }
                        );
                    }
                });
        }

        public void CpuWrite(UInt16 address, byte data)
        {
            nes.CpuWrite(address, data);
        }

        public byte CpuRead(UInt16 address, bool debugMode = false)
        {
            return nes.CpuRead(address, debugMode);
        }

        private string CpuStatus()
        {
            return $"A = {A:X}, X = {X:X}, Y = {Y:X}, PC = {PC:X}, SP = {StackPointer:X}, " +
                $"C = {Status.CarryFlag}, Z = {Status.ZeroFlag}, I = {Status.InterruptDisableFlag} " +
                $"B = {Status.BreakFlag}, U = {Status.UnusedFlag}, V = {Status.OverflowFlag} " +
                $"N = {Status.NegativeFlag}";
        }

        public void Clock()
        {
            if (cycles == 0)
            {
                currentOpcode = CpuRead(PC++);

                // illegal opcode
                if (!operations.ContainsKey(currentOpcode))
                {
                    return;
                }

                cycles = operations[currentOpcode].Cycles;

                bool add1 = operations[currentOpcode].AddressingModeInstruction();
                bool add2 = operations[currentOpcode].Instruction();

                cycles += (add1 && add2) ? 1 : 0;
                
                Status.UnusedFlag = true;
            }

            cycles--;
        }

        public void Reset()
        {
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            StackPointer = 0xFD;

            Status.Register = 0x00;
            Status.UnusedFlag = true;

            PC = ReadWord(0xFFFC);

            // clean up additional variables
            address = 0;
            jumpOffset = 0;

            cycles = 8;
        }

        // interrupt request signal
        public void IRQ()
        {
            if (!Status.InterruptDisableFlag)
            {
                StackPushWord(PC);

                // handle status register
                Status.BreakFlag = false;
                Status.UnusedFlag = true;
                Status.InterruptDisableFlag = true;
                StackPush(Status.Register);

                PC = ReadWord(0xFFFE);

                cycles = 7;
            }
        }

        // non-maskable interrupt signal
        public void NMI()
        {
            StackPushWord(PC);

            // handle status register
            Status.BreakFlag = false;
            Status.UnusedFlag = true;
            Status.InterruptDisableFlag = true;
            StackPush(Status.Register);

            PC = ReadWord(0xFFFA);

            cycles = 8;
        }
    }
}
