using MonoGame.Extended;

namespace TripleTriad.Configuration;

public sealed record class WindowConfiguration(
    Size Size,
    bool IsFullscreen);
