using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace AstropiXX
{
    class AsteroidManager : ILevel
    {
        #region Members

        private int screenWidth = 800;
        private int screenHeight = 480;
        private int screenPadding = 10;

        private Rectangle initialFrame;
        private int asteroidFrames;
        private Texture2D texture;

        private List<Sprite> asteroids = new List<Sprite>(32);
        private const int INITIAL_MIN_SPEED = 200;
        private int minSpeed = 200;
        private const int MIN_SPEED_OFFSET_PER_LEVEL = 5;
        private const int INITIAL_MAX_SPEED = 400;
        private int maxSpeed = INITIAL_MAX_SPEED;
        private const int MAX_SPEED_OFFSET_PER_LEVEL = 10;

        private Random rand = new Random();

        private readonly int initialCount;
        private int count;
        private const int MaxAsteroidsCount = 20;

        private bool isActive = true;

        public const int CRASH_POWER_MIN = 33;
        public const int CRASH_POWER_MAX = 50;

        #endregion

        #region Constructors

        public AsteroidManager(int asteroidCount, Texture2D texture, Rectangle initialFrame,
                               int asteroidFrames, int screenWidth, int screenHeight)
        {
            this.texture = texture;
            this.initialFrame = initialFrame;
            this.asteroidFrames = asteroidFrames;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.initialCount = asteroidCount;
            this.count = asteroidCount;

            for (int x = 0; x < this.count; x++)
            {
                AddAsteroid();
            }
        }

        #endregion

        #region Methods

        public void AddAsteroid()
        {
            Sprite newAsteroid = new Sprite(new Vector2(-500, -500),
                                            texture,
                                            initialFrame,
                                            Vector2.Zero);

            for (int x = 0; x < asteroidFrames; x++)
            {
                newAsteroid.AddFrame(new Rectangle(initialFrame.X + (x * initialFrame.Width),
                                                   initialFrame.Y,
                                                   initialFrame.Width,
                                                   initialFrame.Height));
            }
            
            newAsteroid.Rotation = MathHelper.ToRadians((float)rand.Next(0, 360));
            newAsteroid.CollisionRadius = 15;
            Asteroids.Add(newAsteroid);
        }

        private void AddAsteroidAfterResume(float locationX, float locationY, float rotation,
                                            float velocityX, float velocityY)
        {
            Sprite newAsteroid = new Sprite(new Vector2(locationX, locationY),
                                            texture,
                                            initialFrame,
                                            Vector2.Zero);

            for (int x = 0; x < asteroidFrames; x++)
            {
                newAsteroid.AddFrame(new Rectangle(initialFrame.X + (x * initialFrame.Width),
                                                   initialFrame.Y,
                                                   initialFrame.Width,
                                                   initialFrame.Height));
            }

            newAsteroid.Rotation = rotation;
            newAsteroid.Velocity = new Vector2(velocityX, velocityY);
            newAsteroid.CollisionRadius = 15;
            Asteroids.Add(newAsteroid);
        }

        public void Clear()
        {
            Asteroids.Clear();
        }

        private Vector2 randomLocation()
        {
            Vector2 location = Vector2.Zero;
            bool locationOK = true;
            int tryCount = 0;

            do
            {
                locationOK = true;

                location.X = rand.Next(screenWidth + 10, 2 * screenWidth);
                location.Y = rand.Next(0, screenHeight);

                foreach (var asteroid in Asteroids)
                {
                    if (asteroid.isBoxColliding(new Rectangle((int)location.X,
                                                              (int)location.Y,
                                                              initialFrame.Width,
                                                              initialFrame.Height)))
                    {
                        locationOK = false;
                        break;
                    }
                }

                ++tryCount;

                if (tryCount > 5 && locationOK == false)
                {
                    location = new Vector2(-500, -500);
                    locationOK = true;
                }
            } while (locationOK == false);

            return location;
        }

        private Vector2 randomVelocity()
        {
            Vector2 velocity = new Vector2(rand.Next(-250, -50),
                                           rand.Next(0, 51) - 25);
            velocity.Normalize();
            velocity *= rand.Next(minSpeed, maxSpeed);
            return velocity;
        }

        private bool isOnScreen(Sprite asteroid)
        {
            if (asteroid.Destination.Intersects(new Rectangle(-screenPadding,
                                                              -screenPadding,
                                                              2 * screenWidth + screenPadding,
                                                              screenHeight + screenPadding)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (this.count > Asteroids.Count)
            {
                this.AddAsteroid();
            }
            else if (this.count < Asteroids.Count)
            {
                for (int i = 0; i < Asteroids.Count; i++)
                {
                    if (!isOnScreen(Asteroids[i]))
                    {
                        // Remove just one Asteroid per loop
                        Asteroids.RemoveAt(i);
                        break;
                    }
                }
            }

            foreach (var asteroid in Asteroids)
            {
                asteroid.Update(gameTime);
                if (!isOnScreen(asteroid) && isActive)
                {
                    asteroid.Location = randomLocation();
                    asteroid.Velocity = randomVelocity();
                }
            }

            for (int x = 0; x < Asteroids.Count; x++)
            {
                for (int y = x + 1; y < Asteroids.Count; y++)
                {
                    if (Asteroids[x].IsCircleColliding(Asteroids[y].Center,
                                                       Asteroids[y].CollisionRadius))
                    {
                        BounceAsteroids(Asteroids[x], Asteroids[y]);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var asteroid in Asteroids)
            {
                asteroid.Draw(spriteBatch);
            }
        }

        private void BounceAsteroids(Sprite asteroid1, Sprite asteroid2)
        {
            Vector2 cOfMass = (asteroid1.Velocity + asteroid2.Velocity) / 2;

            Vector2 normal1 = asteroid2.Center - asteroid1.Center;
            normal1.Normalize();
            Vector2 normal2 = asteroid1.Center - asteroid2.Center;
            normal2.Normalize();

            asteroid1.Velocity -= cOfMass;
            asteroid1.Velocity = Vector2.Reflect(asteroid1.Velocity, normal1);
            asteroid1.Velocity += cOfMass;

            asteroid2.Velocity -= cOfMass;
            asteroid2.Velocity = Vector2.Reflect(asteroid2.Velocity, normal2);
            asteroid2.Velocity += cOfMass;
        }

        public void Reset()
        {
            foreach (var asteroid in Asteroids)
            {
                asteroid.Location = new Vector2(-500, -500);
            }

            this.maxSpeed = INITIAL_MAX_SPEED;
        }

        public void SetLevel(int lvl)
        {
            int newCount = (int)(initialCount + Math.Sqrt(lvl - 1) + (lvl - 1) * 0.1f);

            this.count = Math.Min(newCount, MaxAsteroidsCount);

            this.maxSpeed = INITIAL_MAX_SPEED + (lvl - 1) * MAX_SPEED_OFFSET_PER_LEVEL;
            this.minSpeed = INITIAL_MIN_SPEED + (lvl - 1) * MIN_SPEED_OFFSET_PER_LEVEL;
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            this.count = Int32.Parse(reader.ReadLine());

            asteroids.Clear();

            for (int i = 0; i < this.count; ++i)
            {
                AddAsteroidAfterResume(Single.Parse(reader.ReadLine()),
                                       Single.Parse(reader.ReadLine()),
                                       Single.Parse(reader.ReadLine()),
                                       Single.Parse(reader.ReadLine()),
                                       Single.Parse(reader.ReadLine()));
            }

            this.isActive = Boolean.Parse(reader.ReadLine());
        }

        public void Deactivated(StreamWriter writer)
        {
            int realAsteroidsCount = Math.Min(this.count, asteroids.Count);
            
            writer.WriteLine(realAsteroidsCount);
            
            for (int i = 0; i < realAsteroidsCount; ++i)
            {
                writer.WriteLine(asteroids[i].Location.X);
                writer.WriteLine(asteroids[i].Location.Y);
                writer.WriteLine(asteroids[i].Rotation);
                writer.WriteLine(asteroids[i].Velocity.X);
                writer.WriteLine(asteroids[i].Velocity.Y);
            }

            writer.WriteLine(this.isActive);
        }

        #endregion

        #region Properties

        public List<Sprite> Asteroids
        {
            get
            {
                return this.asteroids;
            }
        }

        public bool IsActive
        {
            set
            {
                this.isActive = value;
            }
            get
            {
                return this.isActive;
            }
        }

        #endregion
    }
}
