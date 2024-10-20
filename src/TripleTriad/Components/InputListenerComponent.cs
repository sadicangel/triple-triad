namespace TripleTriad.Components;

public sealed class InputListenerComponent(TripleTriadGame game)
    : GameComponent(game)
{
    public KeyboardStateExtended KeyboardState { get; private set; }

    public MouseStateExtended MouseState { get; private set; }

    public override void Update(GameTime gameTime)
    {
        KeyboardExtended.Update();
        MouseExtended.Update();

        KeyboardState = KeyboardExtended.GetState();
        MouseState = MouseExtended.GetState();
    }
}
