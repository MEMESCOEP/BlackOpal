using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using BlackOpal.Utilities.Converters;
using IO.CMD;

namespace BlackOpal.Utilities.Viewers
{
    public class ELFInfo
    {
        /* VARIABLES */
        public static Dictionary<int, string> ISAs = new Dictionary<int, string>();
        public static Dictionary<int, string> TargetOSABIs = new Dictionary<int, string>();
        public static readonly byte[] MagicNumber = new byte[4] { 0x7F, 0x45, 0x4C, 0x46 };
        public static readonly int TargetOSABIOffset = 0x07;
        public static readonly int EndiannessOffset = 0x05;
        public static readonly int EntryPointOffset = 0x18;
        public static readonly int BitFormatOffset = 0x04;
        public static readonly int VersionOffset = 0x06;
        public static readonly int HeaderSize64 = 64;
        public static readonly int HeaderSize32 = 52;
        public static readonly int ISAOffset = 0x12;
        public string ExecutablePath = "";
        public FileStream ELFStream;

        /* CONSTRUCTOR */
        public ELFInfo(string ExecutablePath)
        {
            try
            {
                // Open the file
                ELFStream = File.OpenRead(ExecutablePath);
                this.ExecutablePath = ExecutablePath;

                // ABIs
                if (TargetOSABIs.Count == 0)
                {
                    TargetOSABIs.Add(0x00, "System V");
                    TargetOSABIs.Add(0x01, "HP-UX");
                    TargetOSABIs.Add(0x02, "NetBSD");
                    TargetOSABIs.Add(0x03, "Linux");
                    TargetOSABIs.Add(0x04, "GNU Hurd");
                    TargetOSABIs.Add(0x06, "Solaris");
                    TargetOSABIs.Add(0x07, "AIX (Monterey)");
                    TargetOSABIs.Add(0x08, "IRIX");
                    TargetOSABIs.Add(0x09, "FreeBSD");
                    TargetOSABIs.Add(0x0A, "Tru64");
                    TargetOSABIs.Add(0x0B, "Novell Modesto");
                    TargetOSABIs.Add(0x0C, "OpenBSD");
                    TargetOSABIs.Add(0x0D, "OpenVMS");
                    TargetOSABIs.Add(0x0E, "NonStop Kernel V");
                    TargetOSABIs.Add(0x0F, "AROS");
                    TargetOSABIs.Add(0x10, "FenixOS");
                    TargetOSABIs.Add(0x11, "Nuxi CloudABI");
                    TargetOSABIs.Add(0x12, "Stratus Technologies OpenVOS");
                    TargetOSABIs.Add(0x13, "Black Opal");
                }

                // ISAs
                if (ISAs.Count == 0)
                {
                    ISAs.Add(0x00, "Not specified");
                    ISAs.Add(0x01, "AT&T WE 32100");
                    ISAs.Add(0x02, "SPARC");
                    ISAs.Add(0x03, "x86");
                    ISAs.Add(0x04, "Motorola 68000 (M68k)");
                    ISAs.Add(0x05, "Motorola 88000 (M88k)");
                    ISAs.Add(0x06, "Intel MCU");
                    ISAs.Add(0x07, "Intel 80860");
                    ISAs.Add(0x08, "MIPS");
                    ISAs.Add(0x09, "IBM System/370");
                    ISAs.Add(0x0A, "MIPS RS3000 Little-endian");
                    ISAs.Add(0x0B, "Reserved for future use");
                    ISAs.Add(0x0C, "Sparc V8");
                    ISAs.Add(0x0D, "Reserved for future use");
                    ISAs.Add(0x0E, "Reserved for future use");
                    ISAs.Add(0x0F, "Hewlett-Packard PA-RISC");
                    ISAs.Add(0x13, "Intel 80960");
                    ISAs.Add(0x14, "PowerPC");
                    ISAs.Add(0x15, "PowerPC (64-bit)");
                    ISAs.Add(0x16, "S390 / S390x");
                    ISAs.Add(0x17, "IBM SPU/SPC");
                    ISAs.Add(0x18, "Reserved for future use");
                    ISAs.Add(0x19, "Reserved for future use");
                    ISAs.Add(0x20, "Reserved for future use");
                    ISAs.Add(0x21, "Reserved for future use");
                    ISAs.Add(0x22, "Reserved for future use");
                    ISAs.Add(0x23, "Reserved for future use");
                    ISAs.Add(0x24, "NEC V800");
                    ISAs.Add(0x25, "Fujitsu FR20");
                    ISAs.Add(0x26, "TRW RH-32");
                    ISAs.Add(0x27, "Motorola RCE");
                    ISAs.Add(0x28, "ARMv7 / AArch32");
                    ISAs.Add(0x29, "Digital Alpha");
                    ISAs.Add(0x2A, "SuperH");
                    ISAs.Add(0x2B, "SPARC v9");
                    ISAs.Add(0x2C, "Siemens TriCore embedded processor");
                    ISAs.Add(0x2D, "Argonaut RISC Core");
                    ISAs.Add(0x2E, "Hitachi H8/300");
                    ISAs.Add(0x2F, "Hitachi H8/300H");
                    ISAs.Add(0x30, "Hitachi H8S");
                    ISAs.Add(0x31, "Hitachi H8/500");
                    ISAs.Add(0x32, "IA-64");
                    ISAs.Add(0x33, "Stanford MIPS-X");
                    ISAs.Add(0x34, "Motorola ColdFire");
                    ISAs.Add(0x35, "Motorola M68HC12");
                    ISAs.Add(0x36, "Fujitsu MMA Multimedia Accelerator");
                    ISAs.Add(0x37, "Siemens PCP");
                    ISAs.Add(0x38, "Sony nCPU embedded RISC processor");
                    ISAs.Add(0x39, "Denso NDR1 microprocessor");
                    ISAs.Add(0x3A, "Motorola Star*Core processor");
                    ISAs.Add(0x3B, "Toyota ME16 processor");
                    ISAs.Add(0x3C, "STMicroelectronics ST100 processor");
                    ISAs.Add(0x3D, "Advanced Logic Corp. TinyJ embedded processor family");
                    ISAs.Add(0x3E, "AMD x86-64");
                    ISAs.Add(0x3F, "Sony DSP Processor");
                    ISAs.Add(0x40, "Digital Equipment Corp. PDP-10");
                    ISAs.Add(0x41, "Digital Equipment Corp. PDP-11");
                    ISAs.Add(0x42, "Siemens FX66 microcontroller");
                    ISAs.Add(0x43, "STMicroelectronics ST9+ 8/16 bit microcontroller");
                    ISAs.Add(0x44, "STMicroelectronics ST7 8-bit microcontroller");
                    ISAs.Add(0x45, "Motorola MC68HC16 Microcontroller");
                    ISAs.Add(0x46, "Motorola MC68HC11 Microcontroller");
                    ISAs.Add(0x47, "Motorola MC68HC08 Microcontroller");
                    ISAs.Add(0x48, "Motorola MC68HC05 Microcontroller");
                    ISAs.Add(0x49, "Silicon Graphics SVx");
                    ISAs.Add(0x4A, "STMicroelectronics ST19 8-bit microcontroller");
                    ISAs.Add(0x4B, "Digital VAX");
                    ISAs.Add(0x4C, "Axis Communications 32-bit embedded processor");
                    ISAs.Add(0x4D, "Infineon Technologies 32-bit embedded processor");
                    ISAs.Add(0x4E, "Element 14 64-bit DSP Processor");
                    ISAs.Add(0x4F, "LSI Logic 16-bit DSP Processor");
                    ISAs.Add(0x8C, "TMS320C6000 Family");
                    ISAs.Add(0xAF, "MCST Elbrus e2k");
                    ISAs.Add(0xB7, "Armv8 / AArch64");
                    ISAs.Add(0xDC, "Zilog Z80");
                    ISAs.Add(0xF3, "RISC-V");
                    ISAs.Add(0xF7, "Berkeley Packet Filter");
                    ISAs.Add(0x101, "WDC 65C816");
                }
            }
            catch (Exception EX)
            {
                ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
            }
        }

        /* FUNCTIONS */
        // Check if a file is an executable in ELF format
        public bool IsElfExecutable()
        {
            return VerifyExecutableSize() && VerifyMagicNumber();
        }

        // Compare the magic number of a file against the ELF magic number
        public bool VerifyMagicNumber()
        {
            var ExecutableHeader = new byte[4];

            if (ELFStream.Length < HeaderSize32)
            {
                return false;
            }

            ELFStream.Read(ExecutableHeader, 0, 4);
            return ExecutableHeader.SequenceEqual(MagicNumber);
        }

        // Read the magic number of the ELF file
        public byte[] GetMagicNumber()
        {
            var ExecutableMagicNumber = new byte[4];

            ELFStream.Read(ExecutableMagicNumber, 0, 4);
            return ExecutableMagicNumber;
        }

        // Make sure the file size is greater than or equal to the 32-bit ELF header size
        public bool VerifyExecutableSize()
        {
            return new FileInfo(ExecutablePath).Length >= HeaderSize32;
        }

        // Check the bit format
        public int GetBitFormat()
        {
            ELFStream.Position = BitFormatOffset;            
            return ELFStream.ReadByte();
        }

        /* Get the executable properties in the folowing order:
            1. Magic number
            2. Bit format
            3. Endianness
            4. Target OS
         */
        public List<string> GetExecutableProperties()
        {
            var PropertyList = new List<string>();
            var ExecutableMagicNumber = new byte[4];

            // Get the magic number
            ELFStream.Position = 0;
            ELFStream.Read(ExecutableMagicNumber, 0, 4);
            PropertyList.Add(ByteConverters.ByteArrayToString(ExecutableMagicNumber));

            // Get the bit format
            ELFStream.Position = BitFormatOffset;
            var BitFormat = ELFStream.ReadByte();

            if (BitFormat != 1 && BitFormat != 2)
            {
                PropertyList.Add($"0x{BitFormat} (Unknown)");
            }
            else
            {
                PropertyList.Add($"{BitFormat * 32}-bit");
            }

            // Get the endianness
            ELFStream.Position = EndiannessOffset;
            var Endianness = ELFStream.ReadByte();

            if (Endianness == 1)
            {
                PropertyList.Add("Little endian");
            }
            else if (Endianness == 2)
            {
                PropertyList.Add("Big endian");
            }
            else
            {
                PropertyList.Add($"0x{Endianness} (Unknown)");
            }

            // Get the target OS ABI
            ELFStream.Position = TargetOSABIOffset;
            var TargetOSABI = ELFStream.ReadByte();

            if (TargetOSABIs.ContainsKey(TargetOSABI))
            {
                PropertyList.Add(TargetOSABIs[TargetOSABI]);
            }
            else
            {
                PropertyList.Add("Unknown");
            }

            PropertyList.Add($"0x{TargetOSABI}");

            // Get the ISA
            var ISA = new byte[2];
            ELFStream.Position = ISAOffset;
            ELFStream.Read(ISA, 0, 2);     
            
            // Reverse the byte array based on endianness
            if (Endianness == 2)
            {
                Array.Reverse(ISA);
            }

            var ISAKey = Convert.ToInt32(ByteConverters.ByteArrayToString(ISA), 16);

            if (ISAs.ContainsKey(ISAKey))
            {
                PropertyList.Add(ISAs[ISAKey]);
            }
            else
            {
                PropertyList.Add("Unknown");
            }

            PropertyList.Add($"0x{ByteConverters.ByteArrayToString(ISA)}");

            // Get the entry point of the ELF file
            var EntryPoint = new byte[4];

            if (BitFormat == 2)
            {
                EntryPoint = new byte[8];
            }

            ELFStream.Position = EntryPointOffset;
            ELFStream.Read(EntryPoint, 0, BitFormat * 4);

            if (Endianness == 2)
            {
                Array.Reverse(EntryPoint);
            }

            PropertyList.Add($"0x{ByteConverters.ByteArrayToString(EntryPoint)}");

            // Get the version of the ELF file
            ELFStream.Position = VersionOffset;
            var ELFVersion = ELFStream.ReadByte();

            PropertyList.Add($"{ELFVersion}");
            return PropertyList;
        }
    }
}
