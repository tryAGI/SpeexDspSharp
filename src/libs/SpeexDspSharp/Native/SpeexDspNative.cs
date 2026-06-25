using System.Runtime.InteropServices;

namespace SpeexDspSharp.Native;

/// <summary>
/// P/Invoke bindings for SpeexDSP. Mirrors the OpusSharp convention:
/// classic <see cref="DllImportAttribute"/> so the custom
/// <see cref="NativeLibraryLoader"/> resolver can probe RID-specific paths.
/// </summary>
internal static class SpeexDspNative
{
    internal const string LibraryName = "speexdsp";

    static SpeexDspNative()
    {
        try
        {
            NativeLibraryLoader.Initialize();
        }
        catch (Exception ex)
        {
            // Don't throw in static ctor — let the first real call surface the error.
            System.Diagnostics.Debug.WriteLine($"[SpeexDsp] Failed to initialize native loader: {ex.Message}");
        }
    }

    // ----- Echo canceller -----

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr speex_echo_state_init(int frame_size, int filter_length);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr speex_echo_state_init_mc(int frame_size, int filter_length, int nb_mic, int nb_speakers);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void speex_echo_state_destroy(IntPtr state);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void speex_echo_state_reset(IntPtr state);

    /// <summary>
    /// Cancel echo using paired mic + reference frames (must be exactly frame_size samples each).
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void speex_echo_cancellation(
        IntPtr state,
        short* rec,
        short* play,
        short* outBuf);

    /// <summary>
    /// Streaming variant: cleans a mic frame using whatever reference frames have been registered
    /// via <see cref="speex_echo_playback"/>. Allows independent capture/playback cadence and lets
    /// SpeexDSP absorb some playback delay.
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void speex_echo_capture(IntPtr state, short* rec, short* outBuf);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe void speex_echo_playback(IntPtr state, short* play);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int speex_echo_ctl(IntPtr state, int request, ref int value);

    // ----- Preprocessor (optional, kept for future use) -----

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr speex_preprocess_state_init(int frame_size, int sampling_rate);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void speex_preprocess_state_destroy(IntPtr state);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe int speex_preprocess_run(IntPtr state, short* x);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int speex_preprocess_ctl(IntPtr state, int request, ref int value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int speex_preprocess_ctl_ptr(IntPtr state, int request, IntPtr value);
}
