---
name: unity-editor
description: Drive the user's open Unity editor from the main checkout with the Unity CLI. Use to refresh assets, recompile, run EditMode tests, or prepare a PR branch for the user to test.
---

# Unity editor (main checkout only)

- Binary: `$LOCALAPPDATA/Unity/bin/unity.exe`. Run from `Unity/`.
- Pass `--caller plugin --skill unity-cli --no-banner` to `unity command ...`. `unity status --json` rejects these flags.
- Custom commands are `[CliCommand]` static methods in `Unity/Assets/Editor/Cli/` (attributes in assembly `Unity.Pipeline.Attributes`). Make a command for each repeated operation. Do not re-send `eval` snippets.

## Commands

```sh
unity command asset refresh              # import files, generate .meta files
unity command recompile                  # then poll: unity command recompile_status
unity command console status             # compile errors, console counts
unity command test run --mode EditMode   # long output: read the Summary only
unity command scene reopen-main          # reload Main.unity after a branch switch
unity command build player <store|studio> <android|webgl>   # needs the platform module; output in Builds/<Target>-<Config>/
```

## Prepare a PR for testing

After a branch switch or `gh pr checkout`: asset refresh, recompile (poll), console status, scene reopen-main. `reopen-main` refuses in play mode or with unsaved scene changes. Tell the user if it does. A stale in-memory `Main.unity` gives null refs on play.

## When it fails

- No connection, or every command times out with `compiling: true` and an old `sampledUtc`: ask the user to focus Unity. Safe Mode means compile errors.
- In `eval_file`, open the scene before you load assets (`OpenScene` unloads unused assets). Use `--timeout 120`.
