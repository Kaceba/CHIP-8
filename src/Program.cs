using SDL2;

namespace CHIP_8;

class Program
{

    //SDL Configuration
    static string WINDOW_NAME = "CHIP-8 Emulator";
    static int WINDOW_WIDTH = 640;
    static int WINDOW_HEIGHT = WINDOW_WIDTH / 2;
    static int DRAW_START_X = 720;
    static int DRAW_START_Y = 1440;
    static SDL.SDL_WindowFlags WINDOW_TYPE = SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

    //CHIP-8 Configuration
    static byte[] memory = new byte[0x1000];
    static bool[] display = new bool[64 * 32];
    static byte[] registers = new byte[16];
    static ushort PC = 0x200; 

    void testprogram()
    {
        //set memory for testing, basically loading the program into memory
        memory[0x200] = 0x00;
        memory[0x201] = 0xE0; 
        memory[0x202] = 0x12;
        memory[0x203] = 0x00;
    }

    static ushort FetchInstruction()
    {
        //fetch 2 bytes from memory at the current PC location, combine them into a single instruction,
        //and increment the PC by 2, 8bit big-endian shifting
        ushort instruction = (ushort)((memory[PC] << 8) | memory[PC + 1]);
        PC += 2;
        return instruction;
    }

    static bool isRunning = false;

    //font allocation
    static readonly byte[] FontSet = new byte[]
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

    static void Main(string[] args)
    {
        //~CHIP-8 startup
        //Fontset loading
        for (int i = 0; i < FontSet.Length; i++)
        {
            memory[0x50 + i] = FontSet[i];
        }

        //~SDL2 startup
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
        IntPtr window = SDL.SDL_CreateWindow(WINDOW_NAME, DRAW_START_X, DRAW_START_Y, WINDOW_WIDTH, WINDOW_HEIGHT, WINDOW_TYPE);
        IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
        SDL.SDL_RenderDrawPoint(renderer, 0, 0);

        int scaleX = WINDOW_WIDTH / 64;
        int scaleY = WINDOW_HEIGHT / 32;

        SDL.SDL_Rect rect = new SDL.SDL_Rect
        {
            x = 0,
            y = 0,
            w = scaleX,
            h = scaleY
        };

        isRunning = true;

        while (isRunning)
        {
            //1. Emulator cycle would go here (fetch, decode, execute)




            display = new bool[64 * 32]; // Clear display each frame for testing

            //DrawCharacter(0x50, 0, 0);
            //DrawCharacter(0x55, 4, 0);
            //DrawCharacter(0x5A, 1 + 8, 0);
            //DrawCharacter(0x5F, 2 + 12, 0);

            //2. Translate emulator information to SDL2 for rendering
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    isRunning = false;
            }

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (display[y * 64 + x] == true)
                        SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255); // White for pixels that are on
                    else
                        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255); // Black for pixels that are off
                    rect.x = x * scaleX;
                    rect.y = y * scaleY;
                    SDL.SDL_RenderFillRect(renderer, ref rect);
                }
            }

            //3. Handle SDL2 events and Rendering
            SDL.SDL_RenderPresent(renderer);
            SDL.SDL_RenderClear(renderer);
            SDL.SDL_Delay(16);
        }

        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }

    private static void DrawCharacter(byte character, int startX, int startY)
    {
        //this puts 0 to display for testing
        for (int row = 0; row < 5; row++)
        {
            byte CharacterByte = memory[character + row];
            for (int col = 0; col < 4; col++)
            {
                if ((CharacterByte & (0x80 >> col)) != 0)
                {
                    int x = startX + col;
                    int y = startY + row;
                    if (x < 64 && y < 32)
                    {
                        display[y * 64 + x] = true;
                    }
                }
            }
        }
    }
}
