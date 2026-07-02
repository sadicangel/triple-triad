namespace TripleTriad.Scenes;

public abstract class Scene
{
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}
