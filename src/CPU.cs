using static CHIP_8.Constants;

namespace CHIP_8
{
    internal class CPU
    {
        internal Display display = new Display();
        internal byte[] memory = new byte[MEMORY_SIZE]; //4KB memory
        internal byte[] registers = new byte[16];
        internal ushort PC = 0x200;
        internal ushort I;

        internal ushort FetchInstruction()
        {
            //fetch 2 bytes from memory at the current PC location, combine them into a single instruction,
            //and increment the PC by 2, 8bit big-endian shifting
            ushort instruction = (ushort)((memory[PC] << 8) | memory[PC + 1]);
            PC += 2;
            return instruction;
        }

        internal void DecodeExecute(ushort instruction)
        {
            //We are trying to get the instruction and decode it to then show it on the display
            //get all nibbles first and NN / NNN values
            byte firstNibble = (byte)((instruction & 0xF000) >> 12);
            byte x = (byte)((instruction & 0x0F00) >> 8);
            byte y = (byte)((instruction & 0x00F0) >> 4);
            byte n = (byte)(instruction & 0x000F);
            byte nn = (byte)(instruction & 0x00FF);
            ushort nnn = (ushort)(instruction & 0x0FFF);

            Execute(firstNibble, x, y, n, nn, nnn);

        }

        private void Execute(byte firstNibble, byte x, byte y, byte n, byte nn, ushort nnn)
        {
            switch (firstNibble)
            {
                case 0x0:
                    switch (nn)
                    {
                        case 0xE0: //00E0: Clear the display
                            for (int i = 0; i < display.display.Length; i++)
                            {
                                display.display[i] = false;
                            }
                            break;
                        case 0xEE: //00EE: Return from subroutine
                            //set PC to the address at the top of the stack, then decrement the stack pointer
                            //SP--;
                            //PC = stack[SP];
                            break;
                        default:
                            //0NNN: Calls RCA 1802 program at address NNN. Not necessary for most ROMs.
                            break;
                    }
                    break;
                case 0x1:
                    break;
                case 0x2:
                    break;
                case 0x3:
                    break;
                case 0x4:
                    break;
                case 0x5:
                    break;
                case 0x6:
                    break;
                case 0x7:
                    break;
                case 0x8:
                    break;
                case 0x9:
                    break;
                case 0xA:
                    break;
                case 0xB:
                    break;
                case 0xC:
                    break;
                case 0xD:
                    break;
                case 0xE:
                    break;
                case 0xF:
                    break;
            }
        }

        internal void LoadProgram(byte[] program)
        {
            //load program into memory starting at 0x200
            Array.Copy(program, 0, memory, PROGRAM_START, program.Length);
        }


    }
}


