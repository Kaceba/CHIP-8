# CHIP-8 TODO

Compared feature by feature against the [Tobias Langhoff guide](https://tobiasvl.github.io/blog/write-a-chip-8-emulator/) on 2026-10-11.
Line numbers are approximate and will drift.

Legend: ✅ done · 🟡 partly done / has a bug · ❌ missing · ➖ optional

## Overview

| Feature | Status | What's left |
|---|---|---|
| Memory | 🟡 | 12-bit address masking, ROM size check off-by-one |
| Font | ✅ | nothing |
| Display | 🟡 | sprite clipping, window scaling |
| Stack | 🟡 | overflow/underflow guards |
| Timers | 🟡 | real 60 Hz timing, no beep |
| Keypad | ❌ | keyboard input not wired up |
| Fetch/decode/execute loop | 🟡 | configurable speed, unknown-opcode error |
| Instruction set | 🟡 | 4 opcodes missing, several with bugs or no quirk option |
| Quirks / configuration | ❌ | no config at all |
| Debugging tools | ❌ | none |
| Test ROMs | 🟡 | Timendus ROMs are in `roms/`, but none have been verified yet |

## Memory

Spec: 4 KB, writable. I and PC are 12-bit. Reserve `000-1FF` for the interpreter and font; programs load at `0x200`.

- ✅ 4096-byte array, programs load at `0x200` (`src/CPU.cs`, `src/Constants.cs`)
- [ ] 🟡 **Mask `I` to 12 bits.** `FX1E` can push `_index` past 4095 (`src/CPU.cs:160`), so `DXYN`, `FX33`, `FX55` and `FX65` throw `IndexOutOfRangeException`.
- [ ] 🟡 **Fix the ROM size check off-by-one** (`src/Program.cs:32`). The check now lives in `Program.cs` and cleans up through `Sdl2Killer()`, but `rom.Length >= MEMORY_SIZE - PROGRAM_START` rejects a ROM of exactly 3584 bytes, which fits. Use `>`. `LoadProgram` itself (`src/CPU.cs:216`) still has no guard of its own.
- [ ] 🟡 **ROM path is hardcoded and relative to the working directory** (`src/Program.cs:30`, currently `1-chip8-logo.ch8`). Replaced by drag-and-drop (see UI); optionally also accept a command-line path.
- ✅ SDL teardown is now a single `Sdl2Killer()` helper (`src/Program.cs:118`), used on normal exit and on the ROM-too-large path.

## Font

Spec: 16 hex-digit sprites (4×5 px, 5 bytes each) stored anywhere in `000-1FF`; `050-09F` is the usual place.

- ✅ Font data matches the guide byte for byte, stored at `0x50` (`src/Constants.cs`)
- ➖ Move font loading from `Program.Main` into the `Cpu` constructor (`src/Program.cs:19-23`).

## Display

Spec: 64×32 monochrome. `DXYN` XORs sprites, sets VF on collision, wraps the starting position, and **clips** at the edges.

- ✅ 64×32 framebuffer, XOR drawing, collision flag, `00E0` clear
- [ ] 🟡 **Clip sprites at the edges** (`src/Display.cs:31-32`). The starting position wraps (mod 64/32), but pixels past the right/bottom edge must be dropped. The code wraps every pixel.
- [ ] 🟡 **Scale with the window** (`src/Program.cs:107-108`). The window is resizable but the scale is fixed at startup. Use `SDL_RenderSetLogicalSize(64, 32)` instead of drawing 2048 rects by hand.
- [ ] 🟡 **SDL error handling.** `SDL_Init`, `SDL_CreateWindow` and `SDL_CreateRenderer` results are never checked (`src/Program.cs:100-103`). Remove the stray `SDL_RenderDrawPoint(0, 0)` (`:105`) and the placeholder `int something = SDL_RenderClear(...)` (`:68`).
- ➖ Redraw only when `00E0` or `DXYN` ran.
- ➖ Fade pixels to reduce flicker.

## Stack

Spec: LIFO of 16-bit return addresses, at least 16 deep.

- ✅ 16-entry stack, used by `2NNN` / `00EE`
- [ ] 🟡 **Guards.** `00EE` underflows the `byte` stack pointer to 255 (`src/CPU.cs:60`) and `2NNN` overflows `_stack[16]` (`:72`); both crash with `IndexOutOfRangeException`. Raise a proper error instead.

## Timers

Spec: delay and sound timers are 8-bit, tick down at 60 Hz independently of CPU speed. The emulator beeps while the sound timer is above 0.

- ✅ Both timers exist and decrement (`src/Program.cs:125-137`)
- [ ] 🟡 **Not really 60 Hz.** They tick once per frame, and a frame is `SDL_Delay(16)` plus render time (`src/Program.cs:45`, `:69`). Tick from a `Stopwatch`, or use `SDL_RENDERER_PRESENTVSYNC`. `TIMER_FREQUENCY` is defined but unused.
- [ ] ❌ **Sound.** Nothing plays when `SoundTimer > 0` (`src/Program.cs:132-136`). Needs `SDL_INIT_AUDIO` and a square wave.

## Keypad

Spec: 16 keys (0-F) in a 4×4 grid, mapped on a PC to the left side of QWERTY, ideally via scancodes so AZERTY works:

```
1 2 3 4        1 2 3 C
Q W E R   →    4 5 6 D
A S D F        7 8 9 E
Z X C V        A 0 B F
```

- 🟡 `Cpu.Keys` (`bool[16]`) exists, but nothing ever sets it.
- [ ] ❌ **Handle `SDL_KEYDOWN` / `SDL_KEYUP`** in the event loop (`src/Program.cs:77-81`) and map scancodes to `Keys[]` using the layout above.
- ➖ Alternative layouts (DREAM 6800 / ETI-660: 0-F in order).

## Fetch / decode / execute loop

Spec: fetch two bytes at PC, advance PC by 2, decode on the first nibble after extracting X, Y, N, NN, NNN, execute. Default speed about 700 instructions/second, configurable.

- ✅ Fetch (big-endian, PC += 2), decode and extract (`src/CPU.cs:21-43`)
- [ ] 🟡 **Speed is hardcoded at 10 instructions per frame (about 600/s)** (`src/Program.cs:47`). Make it configurable, with a default near 700/s.
- [ ] 🟡 **Unknown opcodes are silently ignored**, including inside the `0x0` and `8XY?` sub-switches (`src/CPU.cs`). The guide recommends raising an error.
- [ ] 🟡 **PC bounds check lives in the frame loop** and silently `break`s (`src/Program.cs:51`). Move it into `Cpu` and halt with a message.
- [ ] 🟡 **`Console.WriteLine` per instruction** (`src/Program.cs:44`) floods the terminal (`src/Program.cs:57`). Gate it behind a debug flag.

## Instruction set

### Display, flow and calls
| Opcode | Meaning | Status |
|---|---|---|
| `0NNN` | machine-code call | ✅ intentionally not implemented |
| `00E0` | clear screen | ✅ |
| `1NNN` | jump | ✅ |
| `00EE` | return | 🟡 no underflow guard |
| `2NNN` | call | 🟡 no overflow guard |

### Skips
| Opcode | Meaning | Status |
|---|---|---|
| `3XNN` | skip if VX == NN | ✅ |
| `4XNN` | skip if VX != NN | ✅ |
| `5XY0` | skip if VX == VY | ✅ |
| `9XY0` | skip if VX != VY | ✅ |

### Registers and arithmetic
| Opcode | Meaning | Status |
|---|---|---|
| `6XNN` | VX = NN | ✅ |
| `7XNN` | VX += NN (VF untouched) | ✅ |
| `8XY0` | VX = VY | ✅ |
| `8XY1` `8XY2` `8XY3` | OR / AND / XOR | ✅ (VF-reset quirk missing, see Quirks) |
| `8XY4` | add with carry | 🟡 VF write ordering bug |
| `8XY5` | VX - VY, VF = no borrow | 🟡 VF write ordering bug |
| `8XY7` | VY - VX, VF = no borrow | 🟡 VF write ordering bug |
| `8XY6` / `8XYE` | shift | 🟡 VF write ordering bug; shift quirk missing |
| `ANNN` | I = NNN | ✅ |
| `BNNN` | jump with offset | 🟡 `NNN + V0` only; no quirk |
| `CXNN` | VX = random & NN | ✅ |

The VF ordering bug (`src/CPU.cs:109-129`): the flag is written first and the result second, so when `x == 0xF` the result overwrites the flag. Compute the result, write `VX`, then write `VF`.

### Drawing
| Opcode | Meaning | Status |
|---|---|---|
| `DXYN` | draw sprite | 🟡 wraps pixels instead of clipping (see Display) |

### Keyboard
| Opcode | Meaning | Status |
|---|---|---|
| `EX9E` | skip if key VX pressed | ❌ empty case (`src/CPU.cs:154-156`) |
| `EXA1` | skip if key VX not pressed | ❌ empty case |
| `FX0A` | wait for key, store in VX | ❌ empty case (`src/CPU.cs:204-207`). It must hold PC (`Pc -= 2`) until a key is pressed; the original VIP waited for release. |

### Timers, I, font and memory
| Opcode | Meaning | Status |
|---|---|---|
| `FX07` | VX = delay timer | ✅ |
| `FX15` | delay timer = VX | ✅ |
| `FX18` | sound timer = VX | ✅ |
| `FX1E` | I += VX | 🟡 no 12-bit mask; no VF overflow quirk |
| `FX29` | I = font address for VX | 🟡 should use `VX & 0xF`; replace the `* 5` with `FONT_HEIGHT` |
| `FX33` | BCD | ✅ (needs the `I` bounds fix) |
| `FX55` | store V0..VX | 🟡 modern behavior only, undocumented; no quirk |
| `FX65` | load V0..VX | 🟡 modern behavior only, undocumented; no quirk |

## Quirks / configuration

Spec: make the ambiguous behaviors configurable. The test ROMs expect the modern defaults.

- [ ] ❌ Add a `Quirks` config class (or use CLI flags) and wire the options below to it.

| Quirk | Original (VIP) | Modern (what you have) |
|---|---|---|
| `8XY6` / `8XYE` shift | copy VY into VX, then shift | shift VX in place, ignore Y ✅ |
| `BNNN` | `PC = NNN + V0` ✅ | `BXNN`: `PC = XNN + VX` |
| `FX55` / `FX65` | `I` ends at `I + X + 1` | `I` unchanged ✅ |
| `FX1E` | no flag | VF = 1 on overflow past `0xFFF` (Amiga; Spacefight 2091!) |
| `8XY1/2/3` | VF reset to 0 | VF untouched ✅ |
| `DXYN` | waits for vblank | no wait ✅ |

## UI (SDL2 only: drag-and-drop, popups, hotkeys)

Goal: drop ROMs onto the window, tweak small settings, and get a popup that stops emulation on error.

- [ ] ❌ **Emulator state** (`NoRom` / `Running` / `Paused` / `Halted`). Start in `NoRom` with a blank screen instead of loading the hardcoded ROM.
- [ ] ❌ **`Cpu.Reset()`**: clear memory, registers, stack, display and timers, reload the font, set `PC = 0x200`. Called on every ROM load.
- [ ] ❌ **Drag-and-drop ROMs.** Handle `SDL_DROPFILE` in the event loop (`src/Program.cs`); read the path with `SDL.UTF8_ToManaged(e.drop.file, true)`, which also frees the native string. Then size-check, `Reset()`, load.
- [ ] ❌ **`Chip8Exception`** thrown by the CPU for unknown opcodes, stack over/underflow and out-of-range memory access (this replaces the silent ignores and crashes listed above).
- [ ] ❌ **Error popup.** The main loop catches `Chip8Exception`, calls `SDL.SDL_ShowSimpleMessageBox` (modal, with the window as parent) and switches to `Halted`. The user can then drop another ROM or reset. Use the same path for ROM load errors, including the size check in `src/Program.cs:29-39`, which still prints to the console (its `TODO` comment already says a popup would be better).
- [ ] ❌ **`Settings` object** (speed, quirks, colors) read by the main loop; the same object backs the `Quirks` config above.
- [ ] ❌ **Hotkeys** for the small settings, with the window title as feedback (e.g. "700 ips · paused"): speed up/down, pause, reset, quirk toggles.
- ➖ Dear ImGui panel later, if hotkeys feel too cramped.

## Debugging tools

Spec: step execution, pause, and registers/memory display.

- [ ] ❌ Pause and single-step keys.
- [ ] ❌ Dump registers, `I`, `PC`, stack and timers (to the console or an on-screen overlay).
- [ ] ❌ Memory viewer.

## Testing

- [x] ✅ IBM Logo runs.
- [x] ✅ Timendus suite is in `roms/` (`1-chip8-logo`, `2-ibm-logo`, `3-corax+`, `4-flags`, `5-quirks`, `6-keypad`, `7-beep`, `8-scrolling`).
- [ ] ❌ Run each one and record which pass. `1-chip8-logo` is the one currently loaded. `8-scrolling` tests SUPER-CHIP, so skip it until that extension exists.
- [ ] ❌ BC_test and corax89 chip8-test-rom, which the guide recommends and which expect the modern defaults.
- [ ] ❌ xUnit project with opcode-level unit tests (set up a `Cpu`, execute, assert on registers, PC and memory).

## Extensions (optional, much later)

- ➖ SUPER-CHIP: 128×64 display, scrolling, larger sprites
- ➖ XO-CHIP: extra colors, sound, 64 KB memory

## Code quality (do after the features above)

- [ ] **Encapsulation.** `Program` reads `Cpu.Pc` and mutates the timers directly (`src/Program.cs:51`, `:125-137`). Add `Cpu.Step()` and `Cpu.TickTimers()`, make fields private, and expose read-only views of the display and keys.
- [ ] Magic `64` / `64 * 32` in `Display` (`src/Display.cs:8-17`) → `DISPLAY_WIDTH` / `DISPLAY_HEIGHT`.
- [ ] `oldPixel ^ true` → `!oldPixel` (`src/Display.cs:41`).
- [ ] Remove the stale "puts 0 to display for testing" comment (`src/Display.cs:24`).
- [ ] Rename the public field `Display.display` (e.g. `Pixels`); use `Array.Clear` for `00E0`.
- [ ] Fix the `case 0xF:` indentation (`src/CPU.cs:157-209`).
- [ ] Unused constants (`src/Constants.cs`): `SPRITE_MAX_HEIGHT`, `FONT_WIDTH`, `FONT_HEIGHT`, `REGISTER_COUNT`, `STACK_SIZE`, `TIMER_FREQUENCY`. Use them (the register and stack arrays hardcode `16`) or delete them.
- [ ] Untrack `src/.idea/` and add it to `.gitignore`.
- [ ] Update `README.md` once input, sound and ROM selection land.

## Suggested order

1. **Keypad:** wire SDL keyboard events, then `EX9E`, `EXA1`, `FX0A`.
2. **Fix what's wrong in finished features:** `DXYN` clipping, VF write ordering, the bounds and stack guards.
3. **UI basics:** `Cpu.Reset()`, `Chip8Exception` + error popup, drag-and-drop ROMs, so progress on the Timendus ROMs is measurable.
4. **Quirks config**, then work through the `quirks` ROM.
5. **Timers and sound:** real 60 Hz tick, then the beep.
6. **Configurable speed, unknown-opcode errors, debugging tools.**
7. **Code quality pass.**
