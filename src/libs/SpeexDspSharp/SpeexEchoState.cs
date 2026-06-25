using SpeexDspSharp.Native;

namespace SpeexDspSharp;

/// <summary>
/// RAII wrapper around a SpeexDSP echo-canceller state. Frames must be exactly
/// <see cref="FrameSize"/> samples (16-bit PCM, mono).
/// </summary>
public sealed class SpeexEchoState : IDisposable
{
    private IntPtr _state;

    public int FrameSize { get; }
    public int FilterLength { get; }
    public int SampleRate { get; }

    public SpeexEchoState(int sampleRate, int frameSize, int filterLength)
    {
        if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (frameSize <= 0) throw new ArgumentOutOfRangeException(nameof(frameSize));
        if (filterLength < frameSize) throw new ArgumentOutOfRangeException(nameof(filterLength));

        SampleRate = sampleRate;
        FrameSize = frameSize;
        FilterLength = filterLength;

        _state = SpeexDspNative.speex_echo_state_init(frameSize, filterLength);
        if (_state == IntPtr.Zero)
            throw new InvalidOperationException("speex_echo_state_init returned null");

        var sr = sampleRate;
        SpeexDspNative.speex_echo_ctl(_state, SpeexConstants.SPEEX_ECHO_SET_SAMPLING_RATE, ref sr);
    }

    /// <summary>
    /// Register a playback (reference) frame. Must be exactly <see cref="FrameSize"/> samples.
    /// </summary>
    public unsafe void RegisterPlayback(ReadOnlySpan<short> play)
    {
        EnsureFrame(play.Length, nameof(play));
        fixed (short* p = play)
        {
            SpeexDspNative.speex_echo_playback(_state, p);
        }
    }

    /// <summary>
    /// Clean a microphone frame in place. <paramref name="rec"/> and <paramref name="cleaned"/>
    /// must each be exactly <see cref="FrameSize"/> samples and may alias.
    /// </summary>
    public unsafe void Capture(ReadOnlySpan<short> rec, Span<short> cleaned)
    {
        EnsureFrame(rec.Length, nameof(rec));
        EnsureFrame(cleaned.Length, nameof(cleaned));
        fixed (short* r = rec)
        fixed (short* o = cleaned)
        {
            SpeexDspNative.speex_echo_capture(_state, r, o);
        }
    }

    public void Reset()
    {
        if (_state != IntPtr.Zero)
            SpeexDspNative.speex_echo_state_reset(_state);
    }

    private void EnsureFrame(int length, string param)
    {
        if (length != FrameSize)
            throw new ArgumentException($"Frame must be exactly {FrameSize} samples, got {length}.", param);
    }

    public void Dispose()
    {
        var ptr = Interlocked.Exchange(ref _state, IntPtr.Zero);
        if (ptr != IntPtr.Zero)
            SpeexDspNative.speex_echo_state_destroy(ptr);
    }
}
