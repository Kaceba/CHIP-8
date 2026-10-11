# CHIP-8 TODO

Last audited against the source on 2026-10-11. Line numbers are approximate and will drift.

## Correctness bugs

- [ ] **`DXYN` wraps pixels instead of clipping** (`src/Display.cs:31-32`). The starting coordinate wraps (mod 64/32), but pixels that run past the edge must be clipped, not wrapped.
- [ ] **`8XY4/5/6/7/E` write `VF` before the result** (`src/CPU.cs:109-129`). When `x == 0xF` the result clobbers the flag. Compute, write `VX`, then write `VF`.
- [ ] **`EX9E` / `EXA1` are empty** (`src/CPU.cs:154-156`). `Cpu.Keys` exists but nothing sets it; `SDL_KEYDOWN`/`SDL_KEYUP` aren't handled (`src/Program.cs:66-70`). Use the standard 1234/QWER/ASDF/ZXCV mapping.
- [ ] **`FX0A` is a no-op that falls through** (`src/CPU.cs:204-207`). Rewind with `Pc -= 2` until a key is pressed (and released, per the original VIP).
- [ ] **Mask `I` to 12 bits** (`src/CPU.cs`). `FX1E` can push `_index` past 4095 (`:160`), which makes `DXYN`, `FX33`, `FX55` and `FX65` throw `IndexOutOfRangeException`.
- [ ] **Stack guards.** `00EE` underflows the `byte` stack pointer to 255 (`src/CPU.cs:60`); `2NNN` overflows `_stack[16]` (`:72`).
- [ ] **`LoadProgram` size check** (`src/CPU.cs:216`). Throws if the ROM is larger than 3584 bytes; report a clear error instead.
- [ ] **`FX29` should use `VX & 0xF`** (`src/CPU.cs:164`). Values above 0xF point `I` outside the font. Replace the `* 5` with `FONT_HEIGHT`.
- [ ] **Unknown opcodes are silently ignored** (`src/CPU.cs`, including the `0x0` and `8XY?` sub-switches). Log or throw.

## Quirks

Add a `Quirks` config class (defaults = the modern behavior the `quirks` test ROM expects), then wire each of these to it:

- [ ] `8XY1/2/3` reset `VF` to 0 (original VIP) (`src/CPU.cs:100-108`).
- [ ] `8XY6/E` shift `VY` into `VX` (original) vs. shift `VX` in place (modern) (`src/CPU.cs:118-129`).
- [ ] `BNNN` jumps to `NNN + V0` (original) vs. `BXNN` jumps to `XNN + VX` (SCHIP) (`src/CPU.cs:139`).
- [ ] `FX55` / `FX65` leave `I = I + x + 1` (original) vs. leave `I` unchanged (modern). Currently unchanged, undocumented (`src/CPU.cs:175-194`).
- [ ] `FX1E` sets `VF` when `I` overflows past 0xFFF (Amiga behavior some games rely on) (`src/CPU.cs:160`).
- [ ] `DXYN` display-wait (vblank) quirk.

## Timing, sound, I/O

- [ ] **Decouple the 60 Hz timers from the render loop** (`src/Program.cs:32`, `:56`). Frame length is `SDL_Delay(16)` plus render time, so this is not a true 60 Hz. Tick from a `Stopwatch`, or use `SDL_RENDERER_PRESENTVSYNC`. `TIMER_FREQUENCY` is already defined.
- [ ] **Sound output** (`src/Program.cs:114-118`). `SoundTimer` counts down but nothing plays. Needs `SDL_INIT_AUDIO` and a square wave while `SoundTimer > 0`.
- [ ] **Scale the display with the window** (`src/Program.cs:96-97`). The window is resizable but the scale is fixed at startup. Use `SDL_RenderSetLogicalSize(64, 32)` instead of drawing 2048 rects by hand.
- [ ] **SDL error checking.** `SDL_Init`, `SDL_CreateWindow` and `SDL_CreateRenderer` results are never checked; `int something = SDL_RenderClear(...)` (`src/Program.cs:55`) is a placeholder. Remove the stray `SDL_RenderDrawPoint(0, 0)` (`:94`).
- [ ] **ROM selection.** The path is hardcoded and relative to the working directory (`src/Program.cs:28`). Take it from the command line and add `<None CopyToOutputDirectory>` for `roms/` in `src/CHIP-8.csproj`.
- [ ] **Gate the per-instruction `Console.WriteLine`** (`src/Program.cs:44`) behind a debug flag.

## Code quality

- [ ] **Encapsulation.** `Program` reads `Cpu.Pc` and mutates timers directly (`src/Program.cs:36-38`, `:107-119`). Add `Cpu.Step()` and `Cpu.TickTimers()`, make fields private, and expose read-only views of the display and keys. Move font loading (`src/Program.cs:19-23`) into the `Cpu` constructor.
- [ ] **Magic `64` / `64 * 32` in `Display`** (`src/Display.cs:8-17`). Use `DISPLAY_WIDTH` / `DISPLAY_HEIGHT`.
- [ ] `oldPixel ^ true` → `!oldPixel` (`src/Display.cs:41`).
- [ ] Remove the stale "puts 0 to display for testing" comment (`src/Display.cs:24`).
- [ ] Rename the public field `Display.display` (e.g. `Pixels`) and use `Array.Clear` for `00E0`.
- [ ] Fix the `case 0xF:` indentation (`src/CPU.cs:157-209`).
- [ ] **Unused constants** (`src/Constants.cs`): `SPRITE_MAX_HEIGHT`, `FONT_WIDTH`, `FONT_HEIGHT`, `REGISTER_COUNT`, `STACK_SIZE`, `TIMER_FREQUENCY`. Either use them (the register and stack arrays hardcode `16`) or delete them.
- [ ] Untrack `src/.idea/` and add it to `.gitignore`.

## Testing and docs

- [ ] Add the Timendus test ROMs under `roms/` (`1-chip8-logo`, `3-corax+`, `4-flags`, `5-quirks`, `6-keypad`, `7-beep`) and record which pass.
- [ ] Add an xUnit project with opcode-level unit tests (set up a `Cpu`, execute, assert on registers/PC/memory).
- [ ] Update `README.md` once input, sound and ROM selection land (status section, "currently loads IBM Logo" line).

## Suggested order

1. Fix `DXYN` clipping, `VF` write ordering, and the bounds/stack guards.
2. Wire keyboard input; implement `EX9E`, `EXA1`, `FX0A`.
3. CLI ROM selection + drop in the Timendus ROMs so progress is measurable.
4. Add `Quirks` and work through the `quirks` ROM.
5. Fix timers and add sound.
6. Cleanup and encapsulation pass.
