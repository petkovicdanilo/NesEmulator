﻿using NesLib.Devices;
using NesLib.Devices.CartridgeEntities;
using NesLib.Devices.CpuEntities;
using NesLib.Devices.PpuEntities;
using NesLib.Utils;
using System;

namespace NesLib
{
    [Serializable]
    public class Nes
    {
        private Cpu cpu;
        private Ppu ppu;
        private Cartridge cartridge;

        public Controller[] controllers = new Controller[2] 
        {
            new Controller(),
            new Controller()
        };

        private int clockCounter = 0;

        private byte[] cpuRam = new byte[2048];

        private byte[] capturedController = new byte[2];

        private byte dmaPage, dmaAddress, dmaData;
        private bool dmaStarted = false, dmaDummy = true;

        public bool FrameComplete
        {
            get
            {
                return ppu.FrameComplete;
            }
            set
            {
                ppu.FrameComplete = value;
            }
        }

        public int[] PixelBuffer
        {
            get
            {
                return ppu.PixelBuffer;
            }
        }

        public string GameName
        {
            get
            {
                return cartridge.GameName;
            }
        }

        public Nes()
        {
            this.cpu = new Cpu(this);
            this.ppu = new Ppu();
        }

        public void InsertCartridge(string filePath)
        {
            cartridge = new Cartridge(filePath);
            ppu.ConnectCartridge(cartridge);
            Reset();
        }

        public byte CpuRead(UInt16 address, bool debugMode = false)
        {
            if (InCpuRamRange(address))
            {
                return cpuRam[MaskCpuRam(address)];
            }
            else if(InPpuRange(address))
            {
                return ppu.CpuRead(MaskPpu(address), debugMode);
            }
            else if (InControllerRange(address))
            {
                int index = address & 0x0001;
                byte data = (byte)(BitMagic.IsBitSet(capturedController[index], 7) ? 0x01 : 0x00);

                capturedController[index] <<= 1;

                return data;
            }
            else if(InCartridgeRange(address))
            {
                return cartridge.CpuRead(address);
            }

            return 0x00;
        }

        public void CpuWrite(UInt16 address, byte data)
        {
            if (InCpuRamRange(address))
            {
                cpuRam[MaskCpuRam(address)] = data;
            }
            else if (InPpuRange(address))
            {
                ppu.CpuWrite(MaskPpu(address), data);
            }
            else if (address == 0x4014)
            {
                dmaPage = data;
                dmaAddress = 0x00;
                dmaStarted = true;
            }
            else if (InControllerRange(address))
            {
                int index = address & 0x0001;
                capturedController[index] = controllers[index].State;
            }
            else if (InCartridgeRange(address))
            {
                cartridge.CpuWrite(address, data);
            }
        }

        // global system clock
        public void Clock()
        {
            ppu.Clock();

            if(clockCounter % 3 == 0)
            {
                if(dmaStarted)
                {
                    if(dmaDummy)
                    {
                        if(clockCounter % 2 == 1)
                        {
                            dmaDummy = false;
                        }
                    }
                    else 
                    {
                        if (clockCounter % 2 == 0)
                        {
                            UInt16 address = BitMagic.Combine(dmaPage, dmaAddress);
                            dmaData = CpuRead(address);
                        }
                        else
                        {
                            ppu.Oam[dmaAddress++] = dmaData;

                            // after 256 writes dmaAddress loops back to zero
                            if(dmaAddress == 0)
                            {
                                dmaStarted = false;
                                dmaDummy = true;
                            }
                        }
                    }
                }
                else
                {
                    cpu.Clock();
                }
            }

            if(ppu.EmitNmi)
            {
                cpu.NMI();
            }

            clockCounter++;
        }

        public void DoOneFrame()
        {
            do
            {
                Clock();
            }
            while (!FrameComplete);
            FrameComplete = false;
        }

        public void Reset()
        {
            cpu.Reset();
            ppu.Reset();
            cartridge.Reset();
            clockCounter = 0;
        }

        private bool InCpuRamRange(UInt16 address)
        {
            return (address >= 0x0000 && address <= 0x1FFF);
        }

        private bool InPpuRange(UInt16 address)
        {
            return (address >= 0x2000 && address <= 0x3FFF);
        }

        private bool InControllerRange(UInt16 address)
        {
            return (address >= 0x4016 && address <= 0x4017);
        }

        private bool InCartridgeRange(UInt16 address)
        {
            return (address >= 0x4020 && address <= 0xFFFF);
        }

        private UInt16 MaskCpuRam(UInt16 address)
        {
            return (UInt16)(address & 0x07FF);
        }

        private UInt16 MaskPpu(UInt16 address)
        {
            return (UInt16)(address & 0x0007);
        }

     }
}
