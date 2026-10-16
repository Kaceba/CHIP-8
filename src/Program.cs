using SDL2;
using System.Runtime.InteropServices;
using static CHIP_8.Constants;

namespace CHIP_8;

internal static class Program
{

    private static byte[]? _keyState;
    private static bool _isRunning = true;
    private static nint _window;
    private static nint _renderer;
    private static int _scaleX;
    private static int _scaleY;
    private static SDL.SDL_Rect _rect;
    private static readonly Cpu Cpu = new Cpu();

    private static void Main()
    {

        //Font set loading
        Cpu.LoadFontset();

        //~SDL2 startup
        Sdl2Starter();

        //Load ROM into memory and check if it fits
        byte[] rom = File.ReadAllBytes(Path.Combine("..", "roms", "6-keypad.ch8"));

        if (rom.Length > MEMORY_SIZE - PROGRAM_START)
        {
            //TODO: ConsoleWriteline is not great, a popup message box would be better
            Console.WriteLine("Error: ROM is too large to fit in memory.");
            Sdl2Killer();

            return;
        }

        Cpu.LoadProgram(rom);

        while (_isRunning)
        {

            //2. Translate emulator information to SDL2 for rendering
            HandleSDLEvents();
            HandleTimerLogic();
            ReadInput();

            for (int i = 0; i < 1000; i++)  // Execute ~10 instructions per frame
            {
                if (Cpu.PCOutOfBounds()) break;

                //1. Emulator cycle goes here (fetch, decode, execute)
                ushort fetchedInstruction = Cpu.FetchInstruction();
                Cpu.DecodeExecute(fetchedInstruction);

                Console.WriteLine("Executed Instruction: 0x" + fetchedInstruction.ToString("X4"));
            }

            //3. Present frame
            PrepareFrame();
            SDL.SDL_RenderPresent(_renderer);

            //todo: implement a good error handler
            int something = SDL.SDL_RenderClear(_renderer);
            SDL.SDL_Delay(16);
        }

        //Cleanup SDL2 resources
        Sdl2Killer();
    }

    private static void PrepareFrame()
    {
        for (int y = 0; y < DISPLAY_HEIGHT; y++)
        {
            for (int x = 0; x < DISPLAY_WIDTH; x++)
            {
                if (Cpu.Display.GetPixel(x, y))
                    SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255); // White for pixels that are on
                else
                    SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255); // Black for pixels that are off
                _rect.x = x * _scaleX;
                _rect.y = y * _scaleY;
                SDL.SDL_RenderFillRect(_renderer, ref _rect);
            }
        }
    }

    private static void ReadInput()
    {
        IntPtr state = SDL.SDL_GetKeyboardState(out int numKeys);
        _keyState ??= new byte[numKeys];
        Marshal.Copy(state, _keyState, 0, numKeys);

        for (int i = 0; i < 16; i++)
            Cpu.Keys[i] = _keyState[(int)KeyMap[i]] == 1;
    }

    private static void HandleSDLEvents()
    {
        while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
        {
            if (e.type == SDL.SDL_EventType.SDL_QUIT)
                _isRunning = false;
        }
    }

    private static void Sdl2Starter()
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

    private static void Sdl2Killer()
    {
        SDL.SDL_DestroyRenderer(_renderer);
        SDL.SDL_DestroyWindow(_window);
        SDL.SDL_Quit();
    }

    private static void HandleTimerLogic()
    {
        if (Cpu.DelayTimer > 0)
        {
            Cpu.DelayTimer--;
        }

        if (Cpu.SoundTimer > 0)
        {
            //Beep as long as above 0
            Cpu.SoundTimer--;
        }
    }
}
