# CHIP-8 Emulator

A CHIP-8 interpreter written in C# / .NET 10, using SDL2 (`ppy.SDL2-CS`) for rendering.

<img width="330" height="332" alt="image" src="https://github.com/user-attachments/assets/0485dfa2-ea78-46b9-9d9a-0b9850fc5b3f" /> <img width="450" height="375" alt="image" src="https://github.com/user-attachments/assets/18b1eebc-e5a5-423d-82f7-bdb45e2d9a34" />

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
