# SpeexDspSharp native libraries

These prebuilt binaries are checked in so CI/local builds don't need network
access. `SpeexDspSharp.csproj` links them into `runtimes/<rid>/native/` on
publish so `NativeLibraryLoader` resolves them across platforms.

| File | RID | Source |
|------|-----|--------|
| `linux/libspeexdsp-arm64.so` | `linux-arm64` (Fargate prod) | Debian bookworm `libspeexdsp1_1.2.1-1_arm64.deb` |
| `linux/libspeexdsp.so` | `linux-x64` (local Linux dev) | Debian bookworm `libspeexdsp1_1.2.1-1_amd64.deb` |
| `macos/libspeexdsp.dylib` | `osx-arm64` (Aspire dev on Apple Silicon) | Homebrew `speexdsp` 1.2.1 |
| `windows/speexdsp.dll` | `win-x64` (local Windows dev) | MSYS2 `mingw-w64-clang-x86_64-speexdsp` 1.2.1-1 (clang64 build — no mingw runtime deps) |

## Refreshing

To bump SpeexDSP versions or restage binaries, run:

```bash
natives/refresh.sh
```

The script pulls fresh `.deb` packages from the Debian mirror (linux arm64 +
amd64) and copies the macOS dylib from Homebrew. Re-commit the resulting
binaries.

`SpeexDSP` is BSD-licensed (see `usr/share/doc/libspeexdsp1/copyright` inside
the upstream `.deb`); attribution is preserved via the upstream package
metadata. No source modifications.
