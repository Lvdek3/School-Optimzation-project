using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class Particle : GameObject
    {
        public float lifespan;
        public float fade;

        public Vector2 velocity;
        public Vector2 acceleration;
        public Vector2 location;

        public float scale;
        public Texture2D sprite;
        public Color color;

        public bool IsActive { get; private set; }

        public void Reset(Vector2 location, Vector2 velocity, Vector2 acceleration, float lifespan, float fade, float scale, Color color)
        {
            this.location = location;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.lifespan = lifespan;
            this.fade = fade;
            this.scale = scale;
            this.color = color;
            this.IsActive = true;
        }

        public Particle()
        {
            this.IsActive = false;
        }

        public override void Load(ContentManager content)
        {
            sprite = content.Load<Texture2D>("Particle");
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            if (lifespan < -fade)
            {
                IsActive = false;
            }

            if (lifespan < 0)
                color.A = (byte)(255 * (fade + lifespan) / fade);

            lifespan -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            velocity += (float)gameTime.ElapsedGameTime.TotalSeconds * acceleration;
            location += (float)gameTime.ElapsedGameTime.TotalSeconds * velocity;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            spriteBatch.Draw(sprite, location, null, color, 0, sprite.Bounds.Center.ToVector2(), scale, SpriteEffects.None, 0);
        }
    }

}
