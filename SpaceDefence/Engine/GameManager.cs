using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class GameManager
    {
        private static GameManager gameManager;

        private List<GameObject> _gameObjects; 
        private List<GameObject> _toBeRemoved;
        private List<GameObject> _toBeAdded;
        private ContentManager _content;
        private Effect _teamColorEffect;
        public Matrix WorldMatrix { get; set; }

        public Random RNG { get; private set; }
        public InputManager InputManager { get; private set; }
        public Game Game { get; private set; }

        private List<Bullet> _bulletList;
        private int _nextBulletIndex = 0;


        private Dictionary<Point, List<GameObject>> _spatialHash;
        private int _cellSize = 200;

        public static GameManager GetGameManager()
        {
            if(gameManager == null)
                gameManager = new GameManager();
            return gameManager;
        }
        public GameManager()
        {
            _gameObjects = new List<GameObject>();
            _toBeRemoved = new List<GameObject>();
            _toBeAdded = new List<GameObject>();
            InputManager = new InputManager();
            RNG = new Random();
            WorldMatrix = Matrix.CreateScale(.3f);
            //WorldMatrix = Matrix.CreateScale(1f) * Matrix.CreateTranslation(0, -800, 0);

            _spatialHash = new Dictionary<Point, List<GameObject>>();

            const int MAX_BULLETS = 50000;
            _bulletList = new List<Bullet>(MAX_BULLETS);
            for (int i = 0; i < MAX_BULLETS; i++)
            {
                _bulletList.Add(new Bullet());
            }

        }

        public void Initialize(ContentManager content, Game game)
        {
            Game = game;
            _content = content;
        }

        public void PopulateSpatialHash()
        {
            _spatialHash.Clear();

            Action<GameObject> addToHash = (obj) =>
            {
                if (obj.collider != null && obj.CollisionType.HasFlag(CollisionType.Solid))
                {
                    var bounds = obj.GetPosition();
                    int minX = bounds.Left / _cellSize;
                    int maxX = bounds.Right / _cellSize;
                    int minY = bounds.Top / _cellSize;
                    int maxY = bounds.Bottom / _cellSize;

                    for (int x = minX; x <= maxX; x++)
                    {
                        for (int y = minY; y <= maxY; y++)
                        {
                            var cell = new Point(x, y);
                            if (!_spatialHash.ContainsKey(cell))
                            {
                                _spatialHash.Add(cell, new List<GameObject>());
                            }
                            _spatialHash[cell].Add(obj);
                        }
                    }
                }
            };

            foreach (var obj in _gameObjects)
            {
                addToHash(obj);
            }
            foreach (var bullet in _bulletList)
            {
                if (bullet.IsActive)
                {
                    addToHash(bullet);
                }
            }
        }

        public List<GameObject> GetNearbyObjects(GameObject obj)
        {
            var nearbyObjects = new List<GameObject>();
            if (obj.collider == null) return nearbyObjects;

            var uniqueObjects = new HashSet<GameObject>();

            var bounds = obj.GetPosition();
            int centerX = bounds.Center.X / _cellSize;
            int centerY = bounds.Center.Y / _cellSize;

            for (int x = centerX - 1; x <= centerX + 1; x++)
            {
                for (int y = centerY - 1; y <= centerY + 1; y++)
                {
                    var cell = new Point(x, y);
                    if (_spatialHash.ContainsKey(cell))
                    {
                        foreach (var nearbyObj in _spatialHash[cell])
                        {
                            uniqueObjects.Add(nearbyObj);
                        }
                    }
                }
            }

            nearbyObjects.AddRange(uniqueObjects);
            return nearbyObjects;
        }

        public void Load(ContentManager content)
        {
            _teamColorEffect = content.Load<Effect>("TeamColors");

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Load(content);
            }

            foreach (Bullet bullet in _bulletList)
            {
                bullet.Load(content);
            }

            ParticlePoolManager.Instance.LoadContent(content);
        }

        public void HandleInput(InputManager inputManager)
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.HandleInput(this.InputManager);
            }
        }

        public void CheckCollision()
        {
            List<GameObject> collisionCandidates = new List<GameObject>(_gameObjects.Count + _bulletList.Count);
            collisionCandidates.AddRange(_gameObjects);
            foreach (var bullet in _bulletList)
            {
                if (bullet.IsActive)
                {
                    collisionCandidates.Add(bullet);
                }
            }

            if (collisionCandidates.Count < 2) return;

            var endpoints = new List<Endpoint>(collisionCandidates.Count * 2);
            foreach (var obj in collisionCandidates)
            {
                if (obj.collider != null)
                {
                    var bounds = obj.GetPosition();
                    endpoints.Add(new Endpoint { GameObject = obj, Value = bounds.Left, IsStart = true });
                    endpoints.Add(new Endpoint { GameObject = obj, Value = bounds.Right, IsStart = false });
                }
            }
            endpoints.Sort((a, b) => a.Value.CompareTo(b.Value));

            var activeList = new List<GameObject>();
            for (int i = 0; i < endpoints.Count; i++)
            {
                var endpoint = endpoints[i];
                if (endpoint.IsStart)
                {
                    for (int j = 0; j < activeList.Count; j++)
                    {
                        if (endpoint.GameObject.CheckCollision(activeList[j]))
                        {
                            endpoint.GameObject.OnCollision(activeList[j]);
                            activeList[j].OnCollision(endpoint.GameObject);
                        }
                    }
                    activeList.Add(endpoint.GameObject);
                }
                else
                {
                    activeList.Remove(endpoint.GameObject);
                }
            }
        }

        public void Update(GameTime gameTime) 
        {
            InputManager.Update();

            // Handle input
            HandleInput(InputManager);

            PopulateSpatialHash();


            // Update
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Update(gameTime);
            }

            foreach (Bullet bullet in _bulletList)
            {
                bullet.Update(gameTime);
            }

            // Check Collission
            CheckCollision();
            ParticlePoolManager.Instance.Update(gameTime);

            foreach (GameObject gameObject in _toBeAdded)
            {
                gameObject.Load(_content);
                _gameObjects.Add(gameObject);
            }
            _toBeAdded.Clear();

            for(int i = _toBeRemoved.Count - 1; i >= 0; i--)
            {
                GameObject gameObject = _toBeRemoved[i];
                gameObject.Destroy();
                _gameObjects.Remove(gameObject);
                _toBeRemoved.RemoveAt(i);
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Matrix inverseTransform = Matrix.Invert(WorldMatrix);
            Viewport viewport = Game.GraphicsDevice.Viewport;
            Vector2 topLeft = Vector2.Transform(new Vector2(viewport.X, viewport.Y), inverseTransform);
            Vector2 bottomRight = Vector2.Transform(new Vector2(viewport.X + viewport.Width, viewport.Y + viewport.Height), inverseTransform);
            Rectangle visibleWorld = new Rectangle(
                (int)topLeft.X,
                (int)topLeft.Y,
                (int)(bottomRight.X - topLeft.X),
                (int)(bottomRight.Y - topLeft.Y)
            );

            spriteBatch.Begin(transformMatrix: WorldMatrix, effect: _teamColorEffect);

            foreach (GameObject gameObject in _gameObjects)
            {
                if (gameObject.collider == null || visibleWorld.Intersects(gameObject.GetPosition()))
                {
                    gameObject.Draw(gameTime, spriteBatch);
                }
            }

            foreach (Bullet bullet in _bulletList)
            {
                if (bullet.IsActive && visibleWorld.Intersects(bullet.GetPosition()))
                {
                    bullet.Draw(gameTime, spriteBatch);
                }
            }

            ParticlePoolManager.Instance.Draw(spriteBatch, gameTime, visibleWorld);

            spriteBatch.End();
        }

        /// <summary>
        /// Add a new GameObject to the GameManager. 
        /// The GameObject will be added at the start of the next Update step. 
        /// Once it is added, the GameManager will ensure all steps of the game loop will be called on the object automatically. 
        /// </summary>
        /// <param name="gameObject"> The GameObject to add. </param>
        public void AddGameObject(GameObject gameObject)
        {
            _toBeAdded.Add(gameObject);
        }

        /// <summary>
        /// Remove GameObject from the GameManager. 
        /// The GameObject will be removed at the start of the next Update step and its Destroy() mehtod will be called.
        /// After that the object will no longer receive any updates.
        /// </summary>
        /// <param name="gameObject"> The GameObject to Remove. </param>
        public void RemoveGameObject(GameObject gameObject)
        {
            _toBeRemoved.Add(gameObject);
        }

        public List<GameObject> GetGameObjects()
        {
            return _gameObjects;
        }

        /// <summary>
        /// Get a random location on the screen.
        /// </summary>
        public Vector2 RandomScreenLocation()
        {
            return new Vector2(
                RNG.Next(0, Game.GraphicsDevice.Viewport.Width),
                RNG.Next(0, Game.GraphicsDevice.Viewport.Height));
        }

        public class Endpoint
        {
            public GameObject GameObject { get; set; }
            public float Value { get; set; }
            public bool IsStart { get; set; }
        }

        public Ship FindNearestEnemyInGrid(Ship self)
        {
            if (self?.collider == null) return null;

            Vector2 selfPos = self.GetPosition().Center.ToVector2();
            int startCellX = (int)selfPos.X / _cellSize;
            int startCellY = (int)selfPos.Y / _cellSize;

            Ship nearestEnemy = null;
            float minFoundDistSq = float.MaxValue;

            for (int radius = 0; radius < 20; radius++)
            {
                List<Ship> enemiesInRadius = new List<Ship>();

                int minX = startCellX - radius;
                int maxX = startCellX + radius;
                int minY = startCellY - radius;
                int maxY = startCellY + radius;

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        if (radius > 0 && x > minX && x < maxX && y > minY && y < maxY)
                        {
                            continue;
                        }

                        if (_spatialHash.TryGetValue(new Point(x, y), out List<GameObject> cellObjects))
                        {
                            foreach (var obj in cellObjects)
                            {
                                if (obj is Ship otherShip && otherShip != self && (otherShip.CollisionType & CollisionType.Teams) != (self.CollisionType & CollisionType.Teams))
                                {
                                    enemiesInRadius.Add(otherShip);
                                }
                            }
                        }
                    }
                }

                if (enemiesInRadius.Count > 0)
                {
                    foreach (var enemy in enemiesInRadius)
                    {
                        float distSq = Vector2.DistanceSquared(selfPos, enemy.GetPosition().Center.ToVector2());
                        if (distSq < minFoundDistSq)
                        {
                            minFoundDistSq = distSq;
                            nearestEnemy = enemy;
                        }
                    }
                    return nearestEnemy;
                }
            }

            return null;
        }

        public void FireBullet(Vector2 location, Vector2 direction, float speed, CollisionType collisionType)
        {
            Bullet bullet = _bulletList[_nextBulletIndex];

            bullet.Reset(location, direction, speed, collisionType);

            _nextBulletIndex++;
            if (_nextBulletIndex >= _bulletList.Count)
            {
                _nextBulletIndex = 0;
            }
        }
    }
}