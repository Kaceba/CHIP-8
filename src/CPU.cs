using static CHIP_8.Constants;

namespace CHIP_8
{
    internal class CPU
    {
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

        internal void LoadProgram(byte[] program)
        {
            //load program into memory starting at 0x200
            Array.Copy(program, 0, memory, PROGRAM_START, program.Length);
        }

        
    }
}


