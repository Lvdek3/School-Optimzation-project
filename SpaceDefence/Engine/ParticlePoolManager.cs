using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class ParticlePoolManager
    {
        private static ParticlePoolManager _instance;
        public static ParticlePoolManager Instance => _instance ?? (_instance = new ParticlePoolManager());

        private List<Particle> _particlePool;

        private int _nextParticleIndex = 0;

        public ParticlePoolManager()
        {
            const int MAX_PARTICLES = 90000;

            _particlePool = new List<Particle>(MAX_PARTICLES);

            for (int i = 0; i < MAX_PARTICLES; i++)
            {
                _particlePool.Add(new Particle());
            }
        }

        public void LoadContent(ContentManager content)
        {
            foreach (var particle in _particlePool)
            {
                particle.Load(content);
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var particle in _particlePool)
            {
                particle.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Rectangle visibleWorld)
        {
            foreach (var particle in _particlePool)
            {
                if (particle.IsActive && visibleWorld.Contains(particle.location))
                {
                    particle.Draw(gameTime, spriteBatch);
                }
            }
        }

        public void SpawnParticle(Vector2 location, Vector2 velocity, Vector2 acceleration, float lifespan, float fade, float scale, Color color)
        {
            Particle particle = _particlePool[_nextParticleIndex];

            particle.Reset(location, velocity, acceleration, lifespan, fade, scale, color);

            _nextParticleIndex++;

            if (_nextParticleIndex >= _particlePool.Count)
            {
                _nextParticleIndex = 0;
            }
        }
    }
}