# SpeexDspSharp

C# bindings for SpeexDSP echo cancellation and preprocessing with bundled native
runtime libraries.

The NuGet package id is `tryAGI.SpeexDspSharp`. The public namespace remains
`SpeexDspSharp`.

## Installation

```bash
dotnet add package tryAGI.SpeexDspSharp
```

## Echo Cancellation

```csharp
using SpeexDspSharp;

var sampleRate = 16000;
var frameSize = sampleRate / 50; // 20 ms
var filterLength = sampleRate / 5; // 200 ms

using var echo = new SpeexEchoState(sampleRate, frameSize, filterLength);

var playback = new short[frameSize];
var microphone = new short[frameSize];
var cleaned = new short[frameSize];

echo.RegisterPlayback(playback);
echo.Capture(microphone, cleaned);
```

`SpeexEchoState` wraps SpeexDSP's streaming API. Playback and capture frames must
be exactly `FrameSize` 16-bit mono samples.

## Native Libraries

The package contains native runtime assets under NuGet `runtimes/<rid>/native/`:

- `runtimes/win-x64/native/speexdsp.dll`
- `runtimes/osx-x64/native/libspeexdsp.dylib`
- `runtimes/osx-arm64/native/libspeexdsp.dylib`
- `runtimes/linux-x64/native/libspeexdsp.so`
- `runtimes/linux-arm64/native/libspeexdsp.so`

The native binaries are committed in `natives/` so consumers do not need a
system SpeexDSP installation.

## Refreshing Native Binaries

Run the refresh script from the repository root:

```bash
natives/refresh.sh
```

The script stages Linux binaries from Debian packages, Windows x64 from MSYS2,
and macOS from Homebrew.

## Build And Test

```bash
dotnet build SpeexDspSharp.slnx
dotnet test src/tests/SpeexDspSharp.Tests/SpeexDspSharp.Tests.csproj
dotnet pack src/libs/SpeexDspSharp/SpeexDspSharp.csproj -c Release
```

## License

SpeexDspSharp is licensed under MIT. Bundled SpeexDSP native binaries retain the
upstream BSD license; see `THIRD-PARTY-NOTICES.md`.
