# CHIP-8 TODO

## Bugs / correctness

- [ ] **`8XY4/5/6/7/E` — fix VF write ordering** (`src/CPU.cs:112-132`). Write the result to `VX` first, then set `VF`. Otherwise when `x == 0xF` the result clobbers the carry/borrow/shift flag.
- [ ] **`FX55` / `FX65` — pick a quirk and document it** (`src/CPU.cs:177-192`). Original COSMAC leaves `I = I + x + 1` after the loop; modern SCHIP doesn't. Code currently does the modern thing silently — make it a flag or at least comment the target.
- [ ] **`8XY6` / `8XYE` — shift quirk** (`src/CPU.cs:121-132`). Currently shifts `VX` in place (modern). Original VIP shifted `VY` into `VX`. Make configurable or document.
- [ ] **PC bounds check** (`src/Program.cs:36`). Replace the silent `break` with a real halt/log path; check belongs inside `FetchInstruction`, not the frame loop.
- [ ] **Implement `EX9E` / `EXA1`** (`src/CPU.cs:157-159`). Needs keypad input wired up.
- [ ] **Implement `FX0A`** (`src/CPU.cs:205-210`). Wait for keypress, store key in `VX`.
- [ ] **Decouple timers from render rate** (`src/Program.cs:30-52`). 60 Hz timer tick on wall-clock (`Stopwatch`), separate from CPU step rate and SDL frame pacing.
- [ ] **Sound timer should actually beep** (`src/Program.cs:109-113`). Square wave via SDL audio while `soundTimer > 0`.
- [ ] **ROM path is CWD-relative** (`src/Program.cs:28`). Resolve from `AppContext.BaseDirectory` or copy ROMs to output via `<None CopyToOutputDirectory>` in the csproj.
- [ ] **Stack overflow / underflow guards** (`src/CPU.cs:75-77` and `00EE` at `:59-63`). Check `SP < 16` before `CALL`, `SP > 0` before `RET`.

## Code quality

- [ ] **Magic `64` in Display** (`src/Display.cs:13, 17`). Use `DISPLAY_WIDTH`.
- [ ] **`oldPixel ^ true`** (`src/Display.cs:42`). Replace with `!oldPixel`.
- [ ] **Stale comment** "this puts 0 to display for testing" (`src/Display.cs:24`).
- [ ] **Naming inconsistency** — `getPixel`/`setPixel` are camelCase; rest of the codebase is PascalCase. Rename to `GetPixel`/`SetPixel`.
- [ ] **Remove unused `using System.Globalization;`** (`src/CPU.cs:1`).
- [ ] **`Console.WriteLine` per instruction** (`src/Program.cs:42`). Gate behind a debug flag — at ~600 instr/s this floods the terminal and slows the loop.
- [ ] **Encapsulation** — `Cpu` exposes everything as `internal` fields and `Program.cs` mutates timers and reads `PC` directly. Tighten the surface to `StepCycle()`, `TickTimers()`, `LoadProgram()`, plus read-only views of display/keys.
- [ ] **Render loop ordering** (`src/Program.cs:49-50`). Switch to Clear → Draw → Present (currently Draw → Present → Clear, which makes the `RenderClear` dead code).
- [ ] **Drop unused constants** (`src/Constants.cs`) — `SPRITE_MAX_HEIGHT`, `TIMER_FREQUENCY`, `FONT_WIDTH` are declared but never referenced.

## Missing features (to actually run games)

- [ ] Keypad input — wire `SDL_KEYDOWN`/`SDL_KEYUP` into `_cpu.keys[]` using the standard 1234/QWER/ASDF/ZXCV mapping.
- [ ] Sound output (depends on sound-timer item above).
- [ ] CLI/arg-driven ROM selection instead of hard-coded `IBM Logo.ch8`.
- [ ] Add Timendus test ROMs (`corax+`, `flags`, `quirks`, `keypad`) under `roms/` and verify they pass.

## Suggested order of work

1. Fix `VF` write ordering in `8XY4/5/6/7/E`.
2. Wire keyboard input + implement `EX9E` / `EXA1` / `FX0A`.
3. Decouple the 60 Hz timer tick from the render loop.
4. Drop in Timendus test ROMs and confirm they pass.
5. Cleanup pass: magic numbers, naming, `Console.WriteLine`, stale comments, encapsulation.
