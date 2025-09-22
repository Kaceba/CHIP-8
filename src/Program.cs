using SDL2;
using static CHIP_8.Constants;

namespace CHIP_8;

class Program
{
    static bool isRunning = true;

    static void Main(string[] args)
    {
        //~CHIP-8 startup
        CPU CPU = new CPU();
        Display display = new Display();

        //Fontset loading
        for (int i = 0; i < FONTSET.Length; i++)
        {
            CPU.memory[FONTSET_START + i] = FONTSET[i];
        }

        //~SDL2 startup
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

        IntPtr window = SDL.SDL_CreateWindow(WINDOW_NAME, SDL_DRAW_START_X, SDL_DRAW_START_Y, SDL_WINDOW_WIDTH, SDL_WINDOW_HEIGHT, SDL_WINDOW_TYPE);
        IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
        SDL.SDL_RenderDrawPoint(renderer, 0, 0);

        int scaleX = SDL_WINDOW_WIDTH / DISPLAY_WIDTH;
        int scaleY = SDL_WINDOW_HEIGHT / DISPLAY_HEIGHT;

        SDL.SDL_Rect rect = new SDL.SDL_Rect
        {
            x = 0,
            y = 0,
            w = scaleX,
            h = scaleY
        };

        while (isRunning)
        {
            //1. Emulator cycle would go here (fetch, decode, execute)
            var nextstuff = CPU.FetchInstruction();

            //2. Translate emulator information to SDL2 for rendering
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    isRunning = false;
            }

            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    if (display.getPixel(x, y) == true)
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


}

