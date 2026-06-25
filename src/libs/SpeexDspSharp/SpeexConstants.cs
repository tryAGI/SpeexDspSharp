namespace SpeexDspSharp;

/// <summary>
/// CTL request codes for SpeexDSP echo canceller and preprocessor.
/// Values taken from upstream <c>speex/speex_echo.h</c> and <c>speex/speex_preprocess.h</c>.
/// </summary>
public static class SpeexConstants
{
    // Echo canceller
    public const int SPEEX_ECHO_GET_FRAME_SIZE = 3;
    public const int SPEEX_ECHO_SET_SAMPLING_RATE = 24;
    public const int SPEEX_ECHO_GET_SAMPLING_RATE = 25;

    // Preprocessor (kept for future use; not wired in v1)
    public const int SPEEX_PREPROCESS_SET_DENOISE = 0;
    public const int SPEEX_PREPROCESS_SET_AGC = 2;
    public const int SPEEX_PREPROCESS_SET_VAD = 4;
    public const int SPEEX_PREPROCESS_SET_ECHO_STATE = 24;
    public const int SPEEX_PREPROCESS_SET_DEREVERB = 8;
}
