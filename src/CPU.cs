using System.Globalization;
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
                    PC = nnn; //1NNN: Jump to address NNN
                    break;
                case 0x2:
                    break;
                case 0x3:
                    if(registers[x] == nn)
                        PC += 2; //skip next instruction if VX == NN
                    break;
                case 0x4:
                    if(registers[x] != nn)
                        PC += 2; //skip next instruction if VX != NN
                    break;
                case 0x5:
                    if (registers[x] == registers[y])
                        PC += 2; //skip next instruction if VX == VY   
                    break;
                case 0x6:
                    registers[x] = nn; //6XNN: Set VX to NN
                    break;
                case 0x7:
                    registers[x] += nn; //7XNN: Add NN to VX
                    break;
                case 0x8:
                    switch (n)
                    {
                        case 0x0:
                            registers[x] = registers[y]; //mem copy of value
                            break;
                        case 0x1:
                            registers[x] |= registers[y]; //OR
                            break;
                        case 0x2:
                            registers[x] &= registers[y]; //AND
                            break;
                        case 0x3:
                            registers[x] ^= registers[y]; //XOR
                            break;
                        case 0x4:
                            int sum = registers[x] + registers[y]; //sum with carry
                            registers[0xF] = (byte)(sum > 255 ? 1 : 0);
                            registers[x] = (byte)(sum);
                            break;
                        case 0x5:
                            registers[0xF] = (byte)(registers[x] >= registers[y] ? 1 : 0); //sub with borrow
                            registers[x] = (byte)(registers[x] - registers[y]);
                            break;
                        case 0x6:
                            registers[0xF] = (byte)(registers[x] & 0x1);
                            registers[x] >>= 1; //shift right
                            break;
                        case 0x7:
                            registers[0xF] = (byte)(registers[y] >= registers[x] ? 1 : 0); //reverse sub with borrow (reverse means swap the operands)
                            registers[x] = (byte)(registers[y] - registers[x]);
                            break;
                        case 0xE:
                            registers[0xF] = (byte)((registers[x] & 0x80) >> 7); //we shift the MSB to LSB position because 8 bits and 7 positions
                            registers[x] <<= 1; //shift left
                            break;
                    }
                    break;
                case 0x9:
                    if(registers[x] != registers[y])
                        PC += 2; //skip next instruction if VX != VY
                    break;
                case 0xA:
                    I = nnn; //ANNN: Set I to the address NNN
                    break;
                case 0xB:
                    break;
                case 0xC:
                    break;
                case 0xD:
                    registers[0xF] = 0; //reset VF
                    bool collision = display.DrawSprite(memory, I, registers[x], registers[y], n);
                    if (collision)
                    {
                        registers[0xF] = 1; //set VF to 1 if there was a collision
                    }
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


