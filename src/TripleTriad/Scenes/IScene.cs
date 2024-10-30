namespace TripleTriad.Scenes;

public interface IScene
{
    void Update(GameTime gameTime);
    void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
