using SDL2;

namespace CHIP_8
{
    internal static class Constants
    {
        //SDL Configuration
        internal const string WINDOW_NAME = "CHIP-8 Emulator";
        internal const int SDL_WINDOW_WIDTH = 640;
        internal const int SDL_WINDOW_HEIGHT = SDL_WINDOW_WIDTH / 2;
        internal const int SDL_DRAW_START_X = SDL.SDL_WINDOWPOS_CENTERED;
        internal const int SDL_DRAW_START_Y = SDL.SDL_WINDOWPOS_CENTERED;
        internal const SDL.SDL_WindowFlags SDL_WINDOW_TYPE = SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

        //Memory layout
        internal const ushort MEMORY_SIZE = 0x1000;           // 4KB total memory
        internal const ushort PROGRAM_START = 0x200;          // Programs start at 512
        internal const ushort FONTSET_START = 0x50;           

        // Display
        internal const int DISPLAY_WIDTH = 64;
        internal const int DISPLAY_HEIGHT = 32;

        // Registers
        internal const int REGISTER_COUNT = 16;               // V0-VF
        internal const int STACK_SIZE = 16;

        // Font
        internal const int FONT_HEIGHT = 5;                   
        internal const int FONT_WIDTH = 4;                    

        //Font allocation
        internal static readonly byte[] FONTSET = new byte[]
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        // Timers
        internal const int TIMER_FREQUENCY = 60;              // 60Hz timer decrement
    }
}
