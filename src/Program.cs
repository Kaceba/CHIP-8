using SDL2;
using static CHIP_8.Constants;

namespace CHIP_8;

static class Program
{
    private static bool isRunning = true;
    private static nint _window;
    private static nint _renderer;
    private static int _scaleX;
    private static int _scaleY;
    private static SDL.SDL_Rect _rect;
    private static Cpu _cpu = new Cpu();

    static void Main(string[] args)
    {

        //Fontset loading
        for (int i = 0; i < FONTSET.Length; i++)
        {
            _cpu.memory[FONTSET_START + i] = FONTSET[i];
        }

        //~SDL2 startup
        SDL2Startup();

        _cpu.LoadProgram(File.ReadAllBytes(Path.Combine("..", "roms", "IBM Logo.ch8")));

        while (isRunning)
        {
            HandleTimerLogic();

            for (int i = 0; i < 10; i++)  // Execute ~10 instructions per frame
            {
                if (_cpu.PC < PROGRAM_START || _cpu.PC >= MEMORY_SIZE - 1) break;

                //1. Emulator cycle goes here (fetch, decode, execute)
                ushort FetchedInstruction = _cpu.FetchInstruction();
                _cpu.DecodeExecute(FetchedInstruction);
            }

            //2. Translate emulator information to SDL2 for rendering
            HandleEventsAndPrepareFrame();

            //3. Handle SDL2 events and Rendering
            SDL.SDL_RenderPresent(_renderer);
            SDL.SDL_RenderClear(_renderer);
            SDL.SDL_Delay(16);
        }

        SDL.SDL_DestroyRenderer(_renderer);
        SDL.SDL_DestroyWindow(_window);
        SDL.SDL_Quit();
    }

    private static void HandleEventsAndPrepareFrame()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    isRunning = false;
            }

            for (int y = 0; y < DISPLAY_HEIGHT; y++)
            {
                for (int x = 0; x < DISPLAY_WIDTH; x++)
                {
                    if (_cpu.display.getPixel(x, y))
                        SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255); // White for pixels that are on
                    else
                        SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255); // Black for pixels that are off
                    _rect.x = x * _scaleX;
                    _rect.y = y * _scaleY;
                    SDL.SDL_RenderFillRect(_renderer, ref _rect);
                }
            }
        }

    private static void SDL2Startup()
    {
        SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

        _window = SDL.SDL_CreateWindow(WINDOW_NAME, SDL_DRAW_START_X, SDL_DRAW_START_Y, SDL_WINDOW_WIDTH, SDL_WINDOW_HEIGHT, SDL_WINDOW_TYPE);
        _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);
        SDL.SDL_RenderDrawPoint(_renderer, 0, 0);

        _scaleX = SDL_WINDOW_WIDTH / DISPLAY_WIDTH;
        _scaleY = SDL_WINDOW_HEIGHT / DISPLAY_HEIGHT;
        _rect = new SDL.SDL_Rect
        {
            x = 0,
            y = 0,
            w = _scaleX,
            h = _scaleY
        };
    }

    private static void HandleTimerLogic()
    {
        if (_cpu.delayTimer > 0)
        {
            _cpu.delayTimer--;
        }

        if (_cpu.soundTimer > 0)
        {
            //Beep as long as above 0
            _cpu.soundTimer--;
        }
    }
}
