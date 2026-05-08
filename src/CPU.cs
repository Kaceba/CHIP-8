using static CHIP_8.Constants;

namespace CHIP_8
{
    internal class Cpu
    {
        internal readonly Display Display = new Display();
        internal readonly byte[] Memory = new byte[MEMORY_SIZE]; //4KB memory
        internal bool[] Keys = new bool[16];
        internal ushort Pc = 0x200; //program counter
        internal byte DelayTimer;
        internal byte SoundTimer;

        private readonly ushort[] _stack = new ushort[16];
        private readonly byte[] _registers = new byte[16];
        private byte _sp; //stack pointer
        private ushort _index;
        private readonly Random _rand = new Random();


        internal ushort FetchInstruction()
        {
            //fetch 2 bytes from memory at the current PC location, combine them into a single instruction,
            //and increment the PC by 2, 8bit big-endian shifting
            ushort instruction = (ushort)((Memory[Pc] << 8) | Memory[Pc + 1]);
            Pc += 2;
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
                            _sp--;
                            Pc = _stack[_sp];
                            break;
                        
                        //0NNN: Calls RCA 1802 assembly programs, out of scope so not implemented
                    }
                    break;
                case 0x1:
                    Pc = nnn; //1NNN: Jump to address NNN
                    break;
                case 0x2:
                    //2NNN: Call subroutine at NNN
                    _stack[_sp] = Pc;
                    _sp++;
                    Pc = nnn;
                    break;
                case 0x3:
                    if(_registers[x] == nn)
                        Pc += 2; //skip next instruction if VX == NN
                    break;
                case 0x4:
                    if(_registers[x] != nn)
                        Pc += 2; //skip next instruction if VX != NN
                    break;
                case 0x5:
                    if (_registers[x] == _registers[y])
                        Pc += 2; //skip next instruction if VX == VY   
                    break;
                case 0x6:
                    _registers[x] = nn; //6XNN: Set VX to NN
                    break;
                case 0x7:
                    _registers[x] += nn; //7XNN: Add NN to VX
                    break;
                case 0x8:
                    switch (n)
                    {
                        case 0x0:
                            _registers[x] = _registers[y]; //mem copy of value
                            break;
                        case 0x1:
                            _registers[x] |= _registers[y]; //OR
                            break;
                        case 0x2:
                            _registers[x] &= _registers[y]; //AND
                            break;
                        case 0x3:
                            _registers[x] ^= _registers[y]; //XOR
                            break;
                        case 0x4:
                            int sum = _registers[x] + _registers[y]; //sum with carry
                            _registers[0xF] = (byte)(sum > 255 ? 1 : 0);
                            _registers[x] = (byte)(sum);
                            break;
                        case 0x5:
                            _registers[0xF] = (byte)(_registers[x] >= _registers[y] ? 1 : 0); //sub with borrow
                            _registers[x] = (byte)(_registers[x] - _registers[y]);
                            break;
                        case 0x6:
                            _registers[0xF] = (byte)(_registers[x] & 0x1);
                            _registers[x] >>= 1; //shift right
                            break;
                        case 0x7:
                            _registers[0xF] = (byte)(_registers[y] >= _registers[x] ? 1 : 0); //reverse sub with borrow (reverse means swap the operands)
                            _registers[x] = (byte)(_registers[y] - _registers[x]);
                            break;
                        case 0xE:
                            _registers[0xF] = (byte)((_registers[x] & 0x80) >> 7); //we shift the MSB to LSB position because 8 bits and 7 positions
                            _registers[x] <<= 1; //shift left
                            break;
                    }
                    break;
                case 0x9:
                    if(_registers[x] != _registers[y])
                        Pc += 2; //skip next instruction if VX != VY
                    break;
                case 0xA:
                    _index = nnn; //ANNN: Set I to the address NNN
                    break;
                case 0xB:
                    Pc = (ushort)(nnn + _registers[0]); //BNNN: Jump to address NNN + V0
                    break;
                case 0xC:
                    byte randomByte = (byte)_rand.Next(0, 256); //generate random byte
                    _registers[x] = (byte)(randomByte & nn); //CXNN: Set VX to result of random byte AND NN
                    break;
                case 0xD:
                    _registers[0xF] = 0; //reset VF
                    bool collision = Display.DrawSprite(Memory, _index, _registers[x], _registers[y], n);
                    if (collision)
                    {
                        _registers[0xF] = 1; //set VF to 1 if there was a collision
                    }
                    break;
                case 0xE:
                    //Still missing EX9E, EXA1 needs input first
                    break;
                case 0xF:
                switch (nn)
                {
                    case 0x1E:
                        _index += _registers[x]; //FX1E: Add VX to I
                        break;
                    case 0x29:
                        _index = (ushort)(FONTSET_START + (_registers[x] * 5)); //FX29: Set I to the location of the sprite for the character in VX
                        break;
                    case 0x33:
                    {
                        //FX33: Store BCD representation of VX in memory locations I, I+1, and I+2
                        byte value = _registers[x];
                        Memory[_index] = (byte)(value / 100); //hundreds
                        Memory[_index + 1] = (byte)((value / 10) % 10); //tens
                        Memory[_index + 2] = (byte)(value % 10); //ones
                        break;
                    }
                    case 0x55:
                    {
                        //FX55: Store registers V0 through VX in memory starting at location I
                        for (int i = 0; i <= x; i++)
                        {
                            Memory[_index + i] = _registers[i];
                        }

                        break;
                    }
                    case 0x65:
                    {
                        //FX65: Read registers V0 through VX from memory starting at location I
                        for (int i = 0; i <= x; i++)
                        {
                            _registers[i] = Memory[_index + i];
                        }

                        break;
                    }
                    case 0x07:
                        _registers[x] = DelayTimer; //FX07: Set VX to the value of the delay timer
                        break;
                    case 0x15:
                        DelayTimer = _registers[x]; //FX15: Set the delay timer to VX
                        break;
                    case 0x18:
                        SoundTimer = _registers[x]; //FX18: Set the sound timer to VX
                        break;
                    case 0x0A:
                        //Still missing FX0A needs input first
                        //FX0A: Wait for a key press, store the value of the key in VX
                        //Not implemented yet, needs input handling
                        break;
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


