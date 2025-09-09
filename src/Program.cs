using SDL2;

namespace CHIP_8;


class Program
{

    //SDL Configuration
    static string WINDOW_NAME = "CHIP-8 Emulator";
    static int WINDOW_WIDTH = 640;
    static int WINDOW_HEIGHT = WINDOW_WIDTH/2;
    static int DRAW_START_X = 720;
    static int DRAW_START_Y = 1440;
    static SDL.SDL_WindowFlags WINDOW_TYPE = SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

    //CHIP-8 Configuration
    static byte[] memory = new byte[0x1000];
    static bool[] display = new bool[64 * 32];
    static byte[] registers = new byte[16];

    static bool isRunning = false;

    //font allocation


    static void Main(string[] args)
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
        IntPtr window = SDL.SDL_CreateWindow(WINDOW_NAME, DRAW_START_X, DRAW_START_Y, WINDOW_WIDTH, WINDOW_HEIGHT, WINDOW_TYPE);
        IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        isRunning = true;

        while (isRunning)
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    isRunning = false;
            }

            SDL.SDL_RenderClear(renderer);
            SDL.SDL_RenderPresent(renderer);
        }

        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();

    }
}
