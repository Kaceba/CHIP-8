# CHIP-8 Emulator

A CHIP-8 interpreter written in C# / .NET 10, using SDL2 (`ppy.SDL2-CS`) for rendering.

## Status

Early / in progress. Core fetch-decode-execute loop, display (XOR-drawing with collision flag), and most opcodes are implemented. Input, sound, and a few opcodes are still missing — see `TODO.md` for the current punch list.

## Requirements

- .NET 10 SDK
- SDL2 (pulled in automatically via the `ppy.SDL2-CS` NuGet package)

## Building & running

```bash
cd src
dotnet build
dotnet run
```

The emulator currently loads `roms/IBM Logo.ch8` on startup.

## Project layout

```
src/
  Program.cs     # SDL2 setup, main loop, rendering
  CPU.cs         # Fetch/decode/execute, registers, memory, stack, timers
  Display.cs     # 64x32 framebuffer and sprite drawing
  Constants.cs   # Memory layout, display size, fontset
roms/            # Test ROMs
```

## Reference

Implementation follows Tobias V. Langhoff's [Guide to making a CHIP-8 emulator](https://tobiasvl.github.io/blog/write-a-chip-8-emulator/), used as the source of truth for opcode semantics and quirk handling.
