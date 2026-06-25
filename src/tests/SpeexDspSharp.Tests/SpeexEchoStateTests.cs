using FluentAssertions;
using Xunit;

namespace SpeexDspSharp.Tests;

public class SpeexEchoStateTests
{
    [Fact]
    public void Construct_RejectsInvalidArguments()
    {
        Action badSampleRate = () => _ = new SpeexEchoState(0, 320, 3200);
        Action badFrameSize = () => _ = new SpeexEchoState(16000, 0, 3200);
        Action badFilterLength = () => _ = new SpeexEchoState(16000, 320, 319);

        badSampleRate.Should().Throw<ArgumentOutOfRangeException>();
        badFrameSize.Should().Throw<ArgumentOutOfRangeException>();
        badFilterLength.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Capture_RejectsWrongFrameLength()
    {
        using var state = CreateState();
        var shortFrame = new short[state.FrameSize - 1];
        var output = new short[state.FrameSize];

        Action act = () => state.Capture(shortFrame, output);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterPlaybackAndCapture_ProcessFrame()
    {
        using var state = CreateState();
        var playback = new short[state.FrameSize];
        var microphone = new short[state.FrameSize];
        var cleaned = new short[state.FrameSize];

        for (var i = 0; i < state.FrameSize; i++)
        {
            playback[i] = (short)(Math.Sin(2 * Math.PI * i / state.FrameSize) * 4000);
            microphone[i] = playback[i];
        }

        state.RegisterPlayback(playback);
        state.Capture(microphone, cleaned);

        cleaned.Length.Should().Be(state.FrameSize);
    }

    [Fact]
    public void Reset_AfterCapture_DoesNotThrow()
    {
        using var state = CreateState();
        var frame = new short[state.FrameSize];
        var cleaned = new short[state.FrameSize];

        state.RegisterPlayback(frame);
        state.Capture(frame, cleaned);
        state.Reset();
    }

    private static SpeexEchoState CreateState()
    {
        const int sampleRate = 16000;
        var frameSize = sampleRate / 50;
        var filterLength = sampleRate / 5;
        return new SpeexEchoState(sampleRate, frameSize, filterLength);
    }
}
