using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public struct ParticleData
    {
        public int particleCount = 15;
        public float lifespan = 3;
        public float fade = 0.5f;

        public float minScale = 0.5f;
        public float maxScale = 2;

        public float minDirection = 0;
        public float maxDirection = 2 * MathHelper.Pi;

        public float minSpeed = 1;
        public float maxSpeed = 10;

        public Vector2 acceleration = new Vector2(0, 0f);
        public ParticleData()
        {}
    }
    public class ParticleEmitter
    {
        public bool active = true;
        public Vector2 location;

        ParticleData data;
        private static Random random = new Random();

        public ParticleEmitter(Vector2 location, ParticleData data)
        {
            this.location = location;
            this.data = data;
        }

        public static void Emit(Vector2 location, ParticleData data)
        {
            for (int i = 0; i < data.particleCount; i++)
            {
                float direction = MathHelper.Lerp(data.minDirection, data.maxDirection, (float)random.NextDouble());
                Vector2 velocity = new Vector2((float)Math.Cos(direction), (float)Math.Sin(direction));
                velocity *= MathHelper.Lerp(data.minSpeed, data.maxSpeed, (float)random.NextDouble());
                float scale = MathHelper.Lerp(data.minScale, data.maxScale, (float)random.NextDouble());
                Color color = new Color(200 + random.Next(55), 40 + random.Next(180), 40 + random.Next(80), 255);

                ParticlePoolManager.Instance.SpawnParticle(location, velocity, data.acceleration, data.lifespan, data.fade, scale, color);
            }
        }

    }
}
