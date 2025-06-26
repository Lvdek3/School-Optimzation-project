using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class Bullet : GameObject
    {
        private Texture2D _texture;
        private CircleCollider _circleCollider;
        private Vector2 _velocity;
        public float bulletSize = 4;
        public float LifeTime = 3;

        public bool IsActive { get; private set; }

        public Bullet()
        {
            _circleCollider = new CircleCollider(Vector2.Zero, bulletSize);
            SetCollider(_circleCollider);
            IsActive = false;
        }

        public void Reset(Vector2 location, Vector2 direction, float speed, CollisionType collisionType)
        {
            this.CollisionType = collisionType & ~CollisionType.Solid;
            this._circleCollider.Center = location;
            this._velocity = direction * speed;
            this.LifeTime = 3;
            this.IsActive = true;
        }


        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Bullet");
            base.Load(content);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            base.Update(gameTime);
            _circleCollider.Center += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            LifeTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (LifeTime < 0)
            {
                IsActive = false; 
            }
        }

        public override void OnCollision(GameObject other)
        {
            if (!IsActive) return;

            base.OnCollision(other);
            if (other is Ship && (other.CollisionType & CollisionType) == 0)
            {
                IsActive = false;
                ParticleData data = new ParticleData();
                data.maxScale = 0.2f;
                data.minScale = 0.1f;
                ParticleEmitter.Emit(GetPosition().Center.ToVector2(), data);
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            spriteBatch.Draw(_texture, _circleCollider.GetBoundingBox(), Color.Red);
            base.Draw(gameTime, spriteBatch);
        }
    }
}
