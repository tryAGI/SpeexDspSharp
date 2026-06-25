using System.Reflection;
using System.Runtime.InteropServices;

namespace SpeexDspSharp.Native;

/// <summary>
/// Resolves <c>libspeexdsp</c> across platforms. Mirrors the OpusSharp loader so the
/// runtime layout (packaged <c>runtimes/&lt;rid&gt;/native/</c>, multi-arch Debian paths,
/// Homebrew, etc.) stays consistent.
/// </summary>
internal static class NativeLibraryLoader
{
    private static readonly object _lock = new();
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            NativeLibrary.SetDllImportResolver(typeof(SpeexDspNative).Assembly, ResolveDllImport);
            _initialized = true;
        }
    }

    private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != SpeexDspNative.LibraryName)
            return IntPtr.Zero;

        foreach (var path in GetLibraryPaths())
        {
            try
            {
                if (NativeLibrary.TryLoad(path, out var handle))
                {
                    // Probe a well-known export to make sure we loaded a real SpeexDSP build.
                    if (NativeLibrary.TryGetExport(handle, "speex_echo_state_init", out _))
                    {
                        return handle;
                    }

                    try { NativeLibrary.Free(handle); } catch { /* ignore */ }
                }
            }
            catch
            {
                // try the next candidate
            }
        }

        throw new DllNotFoundException(
            $"Could not load SpeexDSP native library. Tried: {string.Join(", ", GetLibraryPaths())}");
    }

    private static string[] GetLibraryPaths()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
        var paths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            paths.AddRange(new[]
            {
                Path.Combine(assemblyDir, "libspeexdsp.so"),
                Path.Combine(assemblyDir, "runtimes", "linux-x64", "native", "libspeexdsp.so"),
                Path.Combine(assemblyDir, "runtimes", "linux-arm64", "native", "libspeexdsp.so"),
                "/usr/lib/x86_64-linux-gnu/libspeexdsp.so.1",
                "/usr/lib/aarch64-linux-gnu/libspeexdsp.so.1",
                "/lib/x86_64-linux-gnu/libspeexdsp.so.1",
                "/lib/aarch64-linux-gnu/libspeexdsp.so.1",
                "/usr/lib/libspeexdsp.so.1",
                "/usr/local/lib/libspeexdsp.so",
                "libspeexdsp.so.1",
                "libspeexdsp.so",
                "speexdsp",
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            paths.AddRange(new[]
            {
                Path.Combine(assemblyDir, "runtimes", "osx", "native", "libspeexdsp.dylib"),
                Path.Combine(assemblyDir, "runtimes", "osx-x64", "native", "libspeexdsp.dylib"),
                Path.Combine(assemblyDir, "runtimes", "osx-arm64", "native", "libspeexdsp.dylib"),
                Path.Combine(assemblyDir, "libspeexdsp.dylib"),
                "/opt/homebrew/opt/speexdsp/lib/libspeexdsp.dylib",
                "/opt/homebrew/lib/libspeexdsp.dylib",
                "/usr/local/lib/libspeexdsp.dylib",
                "libspeexdsp.dylib",
                "speexdsp",
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            paths.AddRange(new[]
            {
                Path.Combine(assemblyDir, "speexdsp.dll"),
                Path.Combine(assemblyDir, "runtimes", "win-x64", "native", "speexdsp.dll"),
                Path.Combine(assemblyDir, "libspeexdsp.dll"),
                Path.Combine(assemblyDir, "libspeexdsp-1.dll"),
                "speexdsp.dll",
                "libspeexdsp.dll",
                // MSYS2 native filename — handy when the DLL is on PATH from a vcpkg/MSYS2 install.
                "libspeexdsp-1.dll",
                "speexdsp",
            });
        }

        return paths.ToArray();
    }
}
