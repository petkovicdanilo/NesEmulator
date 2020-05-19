using NesLib.Devices;
using NesLib.Devices.CartridgeEntities;
using NesLib.Devices.CpuEntities;
using NesLib.Devices.PpuEntities;
using NesLib.Utils;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace NesLib
{
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

        private const UInt16 CPU_RAM_LEFT = 0x0000, CPU_RAM_RIGHT = 0x1FFF;
        private const UInt16 PPU_LEFT = 0x2000, PPU_RIGHT = 0x3FFF;

        private const UInt16 CPU_RAM_MASK = 0x07FF;
        private const UInt16 PPU_MASK = 0x0007;

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

        public Nes()
        {
            this.cpu = new Cpu(this);
            this.ppu = new Ppu(this);

            InsertCartridge(@"C:\Users\Danilo\Desktop\NES games\Donkey Kong (World) (Rev A).nes");
            Reset();

        }

        public void InsertCartridge(string filePath)
        {
            cartridge = new Cartridge(filePath);
            ppu.ConnectCartridge(cartridge);
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
            else if (address >= 0x4016 && address <= 0x4017)
            {
                int index = address & 0x0001;
                byte data = (byte)(BitMagic.IsBitSet(capturedController[index], 7) ? 0x01 : 0x00);

                capturedController[index] <<= 1;

                return data;
            }
            else if(address >= 0x4020 && address <= 0xFFFF)
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
            else if (address >= 0x4016 && address <= 0x4017)
            {
                int index = address & 0x0001;
                capturedController[index] = controllers[index].State;
            }
            else if (address >= 0x4020 && address <= 0xFFFF)
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

        public void Reset()
        {
            cpu.Reset();
            ppu.Reset();
            clockCounter = 0;
        }

        private bool InCpuRamRange(UInt16 address)
        {
            return (address >= CPU_RAM_LEFT && address <= CPU_RAM_RIGHT);
        }

        private bool InPpuRange(UInt16 address)
        {
            return (address >= PPU_LEFT && address <= PPU_RIGHT);
        }

        private UInt16 MaskCpuRam(UInt16 address)
        {
            return (UInt16)(address & CPU_RAM_MASK);
        }

        private UInt16 MaskPpu(UInt16 address)
        {
            return (UInt16)(address & PPU_MASK);
        }

        public WriteableBitmap Screen()
        {
            return ppu.Screen;
        }
     }
}
