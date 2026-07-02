namespace TripleTriad.Systems;

public sealed class Input
{
    public static KeyboardStateExtended Keyboard { get => KeyboardExtended.GetState(); }

    public static MouseStateExtended Mouse { get => MouseExtended.GetState(); }

    public static void Update()
    {
        KeyboardExtended.Update();
        MouseExtended.Update();
    }
}
