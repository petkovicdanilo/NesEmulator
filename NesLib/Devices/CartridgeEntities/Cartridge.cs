using NesLib.Devices.CartridgeEntities.Exceptions;
using NesLib.Devices.CartridgeEntities.Mappers;
using NesLib.Utils;
using System;
using System.IO;

namespace NesLib.Devices.CartridgeEntities
{
    [Serializable]
    public class Cartridge : ICpuBusDevice, IPpuBusDevice
    {
        private Mapper mapper;

        private byte[] prgMemory;
        private byte[] chrMemory;

        public MirrorMode Mode { get; private set; }

        public enum MirrorMode
        {
            HORIZONTAL,
            VERTICAL
        };

        public string GameName { get; private set; }

        private bool usingChrRam = false;

        public Cartridge(string filePath)
        {
            GameName = "";
            try
            {
                using (FileStream fs = new FileStream(filePath, 
                        FileMode.Open, FileAccess.Read))
                {
                    ReadHeader(fs);

                    fs.Read(prgMemory, 0, prgMemory.Length);
                    fs.Read(chrMemory, 0, chrMemory.Length);
                }

                GameName = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (FileNotFoundException ioEx)
            {
                Console.WriteLine(ioEx.Message);
            }
        }

        public byte CpuRead(ushort address, bool debugMode = false)
        {
            UInt16 mappedAddress = mapper.MapCpuRead(address);
            return prgMemory[mappedAddress];
        }

        public void CpuWrite(ushort address, byte data)
        {
            UInt16 mappedAddress = mapper.MapCpuWrite(address);
            prgMemory[mappedAddress] = data;
        }

        public byte PpuRead(ushort address)
        {
            UInt16 mappedAddress = mapper.MapPpuRead(address);
            return chrMemory[mappedAddress];
        }

        public void PpuWrite(ushort address, byte data)
        {
            UInt16 mappedAddress = mapper.MapPpuWrite(address);
            chrMemory[mappedAddress] = data;
        }

        // TODO: 
        // - handle different file formats
        private void ReadHeader(FileStream fs)
        {
            // discard name
            fs.Seek(4, SeekOrigin.Begin);

            int prgRomSize = fs.ReadByte(); // num of 16kb units
            prgMemory = new byte[prgRomSize * 16 * 1024];

            int chrRomSize = fs.ReadByte(); // num of 8kb units
            if(chrRomSize == 0)
            {
                usingChrRam = true;
                chrMemory = new byte[8 * 1024];
            }
            else
            {
                chrMemory = new byte[chrRomSize * 8 * 1024];
            }

            int flags6 = (byte)fs.ReadByte();
            int flags7 = (byte)fs.ReadByte();
            int flags8 = (byte)fs.ReadByte();
            int flags9 = (byte)fs.ReadByte();
            int flags10 = (byte)fs.ReadByte();
            // padding
            fs.Seek(5, SeekOrigin.Current);

            int mapperId = (flags7 & 0xF0) | ((flags6 & 0xF0) >> 4);
            AddMapperFromId(mapperId);

            Mode = BitMagic.IsBitSet((byte) flags6, 0) ? MirrorMode.VERTICAL : MirrorMode.HORIZONTAL;

            bool hasTrainer = BitMagic.IsBitSet((byte) flags6, 2);

            if(hasTrainer)
            {
                fs.Seek(512, SeekOrigin.Current);
            }
        }

        private void AddMapperFromId(int mapperId)
        {
            string mapperIdString = mapperId.ToString().PadLeft(3, '0');

            var objectType = 
                Type.GetType($"NesLib.Devices.CartridgeEntities.Mappers.Mapper{mapperIdString}");
            try
            {
                int prgBanks = prgMemory.Length / (16 * 1024);
                int chrBanks = usingChrRam ? 0 : chrMemory.Length / (8 * 1024);

                mapper = Activator.CreateInstance(objectType, prgBanks, chrBanks) as Mapper;
            }
            catch(ArgumentNullException)
            {
                throw new MapperNotSupportedException(mapperId);
            }
        }
    }
}
