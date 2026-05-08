using static CHIP_8.Constants;

namespace CHIP_8
{
    internal class Cpu
    {
        internal Display Display = new Display();
        internal byte[] Memory = new byte[MEMORY_SIZE]; //4KB memory
        internal bool[] Keys = new bool[16];
        internal ushort PC = 0x200; //program counter
        internal byte DelayTimer = 0;
        internal byte SoundTimer = 0;

        private ushort[] Stack = new ushort[16];
        private byte[] Registers = new byte[16];
        private byte SP = 0; //stack pointer
        private ushort I;
        private readonly Random rand = new Random();


        internal ushort FetchInstruction()
        {
            //fetch 2 bytes from memory at the current PC location, combine them into a single instruction,
            //and increment the PC by 2, 8bit big-endian shifting
            ushort instruction = (ushort)((Memory[PC] << 8) | Memory[PC + 1]);
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
                            for (int i = 0; i < Display.display.Length; i++)
                            {
                                Display.display[i] = false;
                            }
                            break;
                        case 0xEE: //00EE: Return from subroutine
                            //set PC to the address at the top of the stack, then decrement the stack pointer
                            SP--;
                            PC = Stack[SP];
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
                    //2NNN: Call subroutine at NNN
                    Stack[SP] = PC;
                    SP++;
                    PC = nnn;
                    break;
                case 0x3:
                    if(Registers[x] == nn)
                        PC += 2; //skip next instruction if VX == NN
                    break;
                case 0x4:
                    if(Registers[x] != nn)
                        PC += 2; //skip next instruction if VX != NN
                    break;
                case 0x5:
                    if (Registers[x] == Registers[y])
                        PC += 2; //skip next instruction if VX == VY   
                    break;
                case 0x6:
                    Registers[x] = nn; //6XNN: Set VX to NN
                    break;
                case 0x7:
                    Registers[x] += nn; //7XNN: Add NN to VX
                    break;
                case 0x8:
                    switch (n)
                    {
                        case 0x0:
                            Registers[x] = Registers[y]; //mem copy of value
                            break;
                        case 0x1:
                            Registers[x] |= Registers[y]; //OR
                            break;
                        case 0x2:
                            Registers[x] &= Registers[y]; //AND
                            break;
                        case 0x3:
                            Registers[x] ^= Registers[y]; //XOR
                            break;
                        case 0x4:
                            int sum = Registers[x] + Registers[y]; //sum with carry
                            Registers[0xF] = (byte)(sum > 255 ? 1 : 0);
                            Registers[x] = (byte)(sum);
                            break;
                        case 0x5:
                            Registers[0xF] = (byte)(Registers[x] >= Registers[y] ? 1 : 0); //sub with borrow
                            Registers[x] = (byte)(Registers[x] - Registers[y]);
                            break;
                        case 0x6:
                            Registers[0xF] = (byte)(Registers[x] & 0x1);
                            Registers[x] >>= 1; //shift right
                            break;
                        case 0x7:
                            Registers[0xF] = (byte)(Registers[y] >= Registers[x] ? 1 : 0); //reverse sub with borrow (reverse means swap the operands)
                            Registers[x] = (byte)(Registers[y] - Registers[x]);
                            break;
                        case 0xE:
                            Registers[0xF] = (byte)((Registers[x] & 0x80) >> 7); //we shift the MSB to LSB position because 8 bits and 7 positions
                            Registers[x] <<= 1; //shift left
                            break;
                    }
                    break;
                case 0x9:
                    if(Registers[x] != Registers[y])
                        PC += 2; //skip next instruction if VX != VY
                    break;
                case 0xA:
                    I = nnn; //ANNN: Set I to the address NNN
                    break;
                case 0xB:
                    PC = (ushort)(nnn + Registers[0]); //BNNN: Jump to address NNN + V0
                    break;
                case 0xC:
                    byte randomByte = (byte)rand.Next(0, 256); //generate random byte
                    Registers[x] = (byte)(randomByte & nn); //CXNN: Set VX to result of random byte AND NN
                    break;
                case 0xD:
                    Registers[0xF] = 0; //reset VF
                    bool collision = Display.DrawSprite(Memory, I, Registers[x], Registers[y], n);
                    if (collision)
                    {
                        Registers[0xF] = 1; //set VF to 1 if there was a collision
                    }
                    break;
                case 0xE:
                    //Still missing EX9E, EXA1 needs input first
                    break;
                case 0xF:
                if (nn == 0x1E)
                    {
                        I += Registers[x]; //FX1E: Add VX to I
                    }
                    else if (nn == 0x29)
                    {
                        I = (ushort)(FONTSET_START + (Registers[x] * 5)); //FX29: Set I to the location of the sprite for the character in VX
                    }
                    else if (nn == 0x33)
                    {
                        //FX33: Store BCD representation of VX in memory locations I, I+1, and I+2
                        byte value = Registers[x];
                        Memory[I] = (byte)(value / 100); //hundreds
                        Memory[I + 1] = (byte)((value / 10) % 10); //tens
                        Memory[I + 2] = (byte)(value % 10); //ones
                    }
                    else if (nn == 0x55)
                    {
                        //FX55: Store registers V0 through VX in memory starting at location I
                        for (int i = 0; i <= x; i++)
                        {
                            Memory[I + i] = Registers[i];
                        }
                    }
                    else if (nn == 0x65)
                    {
                        //FX65: Read registers V0 through VX from memory starting at location I
                        for (int i = 0; i <= x; i++)
                        {
                            Registers[i] = Memory[I + i];
                        }
                    }
                    else if (nn == 0x07)
                    {
                        Registers[x] = DelayTimer; //FX07: Set VX to the value of the delay timer
                    }
                    else if (nn == 0x15)
                    {
                        DelayTimer = Registers[x]; //FX15: Set the delay timer to VX
                    }
                    else if (nn == 0x18)
                    {
                        SoundTimer = Registers[x]; //FX18: Set the sound timer to VX
                    }
                    else if (nn == 0x0A)
                    {
                        //Still missing FX0A needs input first
                        //FX0A: Wait for a key press, store the value of the key in VX
                        //Not implemented yet, needs input handling
                    }
                break;
            }
        }

        internal void LoadProgram(byte[] program)
        {
            //load program into memory starting at 0x200
            Array.Copy(program, 0, Memory, PROGRAM_START, program.Length);
        }


    }
}


