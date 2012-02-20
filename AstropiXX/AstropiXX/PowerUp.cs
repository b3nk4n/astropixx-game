using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace AstropiXX
{
    class PowerUp
    {
        #region Members

        private Sprite powerUpSprite;

        private const float DEFAULT_SPEED = 150.0f;
        private const int RADIUS = 10;

        public enum PowerUpType { Health25, Health50, CoolWater, LowBonusScore, MediumBonusScore, HighBonusScore, Shield, Overdrive,
                                  OverHeat, Underdrive,
                                  Random };

        private PowerUpType type;

        private bool isActive = true;

        #endregion

        #region Constructor

        public PowerUp(Texture2D texture, Vector2 location, Rectangle initialFrame,
                       int frameCount, PowerUpType type)
            : this(texture, location, initialFrame, frameCount, type, DEFAULT_SPEED)
        {
        }

        public PowerUp(Texture2D texture, Vector2 location, Rectangle initialFrame,
                       int frameCount, PowerUpType type, float speed)
        {
            powerUpSprite = new Sprite(location, texture, initialFrame, new Vector2(-1, 0) * speed);
            powerUpSprite.CollisionRadius = RADIUS;

            this.type = type;
        }

        #endregion

        public void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                powerUpSprite.Update(gameTime);

                if (!IsInScreen)
                {
                    IsActive = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                powerUpSprite.Draw(spriteBatch);
            }
        }

        #region Methods

        public bool isCircleColliding(Vector2 otherCenter, float otherRadius)
        {
            return this.powerUpSprite.IsCircleColliding(otherCenter, otherRadius);
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            // Sprite
            powerUpSprite.Activated(reader);

            this.type = (PowerUpType)Enum.Parse(type.GetType(), reader.ReadLine(), false);

            this.isActive = Boolean.Parse(reader.ReadLine());
        }

        public void Deactivated(StreamWriter writer)
        {
            // Powerup sprite
            powerUpSprite.Deactivated(writer);

            writer.WriteLine(type);

            writer.WriteLine(isActive);
        }

        #endregion

        #region Properties

        public PowerUpType Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                this.isActive = value;
            }
        }

        public bool IsInScreen
        {
            get
            {
                return powerUpSprite.Location.X > -20;
            }
        }

        #endregion
    }
}
