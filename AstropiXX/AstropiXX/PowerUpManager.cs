using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace AstropiXX
{
    class PowerUpManager : ILevel
    {
        #region Members

        private List<PowerUp> powerUps = new List<PowerUp>();
        private Texture2D texture;

        private const int SPAWN_CHANCE = 15; // 15%

        private PowerUp.PowerUpType lastPowerUp = PowerUp.PowerUpType.Health50;

        private float moneySpawnTimer = 0.0f;
        private const float moneySpawnMinTimer = 5.0f;
        private const float MONEY_LESS_SPAWN_RATE_PER_LEVEL = 0.05f;

        private Random rand = new Random();

        private readonly Vector2 PowerUpOffset = new Vector2(-12.5f, -12.5f);

        private int currentLevel = 1;
        private const float MONEY_SPEED_PER_LEVEL = 250.0f;
        private const float MONEY_EXTRA_SPEED_PER_LEVEL = 10.0f;

        #endregion

        #region Constructors

        public PowerUpManager(Texture2D texture)
        {
            this.texture = texture;
        }

        #endregion

        #region Methods

        public void ProbablySpawnPowerUp(Vector2 location)
        {
            int spawnChance = rand.Next(100);
            
            if (spawnChance >= SPAWN_CHANCE)
                return;

            int rnd = rand.Next(25);

            PowerUp.PowerUpType type = PowerUp.PowerUpType.Health25;
            Rectangle initialFrame = new Rectangle(0, 0, 25, 25);

            switch(rnd)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    type = PowerUp.PowerUpType.Health25;
                    initialFrame = new Rectangle(0, 0, 25, 25);
                    break;

                case 4:
                case 5:
                    type = PowerUp.PowerUpType.Health50;
                    initialFrame = new Rectangle(0, 25, 25, 25);
                    break;

                case 6:
                case 7:
                    type = PowerUp.PowerUpType.LowBonusScore;
                    initialFrame = new Rectangle(0, 150, 25, 25);
                    break;

                case 8:
                case 9:
                    type = PowerUp.PowerUpType.MediumBonusScore;
                    initialFrame = new Rectangle(0, 175, 25, 25);
                    break;

                case 10:
                    type = PowerUp.PowerUpType.HighBonusScore;
                    initialFrame = new Rectangle(0, 200, 25, 25);
                    break;

                case 11:
                case 12:
                    type = PowerUp.PowerUpType.CoolWater;
                    initialFrame = new Rectangle(0, 225, 25, 25);
                    break;

                case 13:
                case 14:
                    type = PowerUp.PowerUpType.Shield;
                    initialFrame = new Rectangle(0, 275, 25, 25);
                    break;

                case 15:
                case 16:
                case 17:
                case 18:
                    type = PowerUp.PowerUpType.Random;
                    initialFrame = new Rectangle(0, 375, 25, 25);
                    break;

                case 19:
                case 20:
                    type = PowerUp.PowerUpType.OverHeat;
                    initialFrame = new Rectangle(0, 400, 25, 25);
                    break;

                case 21:
                case 22:
                    type = PowerUp.PowerUpType.Overdrive;
                    initialFrame = new Rectangle(0, 500, 25, 25);
                    break;
                
                case 23:
                case 24:
                    type = PowerUp.PowerUpType.Underdrive;
                    initialFrame = new Rectangle(0, 525, 25, 25);
                    break;
            }

            // only spawn bonus score power ups in combo mode
            if (GameModeManager.SelectedGameMode != GameModeManager.GameMode.Combo &&
                (type == PowerUp.PowerUpType.LowBonusScore ||
                type == PowerUp.PowerUpType.MediumBonusScore ||
                type == PowerUp.PowerUpType.HighBonusScore))
            {
                return;
            }

            // check, that the new powerup to drop is not equal
            // to the last one
            if (type == lastPowerUp)
                return;
            else
                lastPowerUp = type;

            PowerUp p = new PowerUp(texture,
                                    location + PowerUpOffset,
                                    initialFrame,
                                    1,
                                    type);

            powerUps.Add(p);
        }

        public PowerUp.PowerUpType GetPowerUpNotRandom()
        {
            int rnd = rand.Next(10);
            PowerUp.PowerUpType type = PowerUp.PowerUpType.Health50;

            switch (rnd)
            {
                case 0:
                    type =  PowerUp.PowerUpType.Health25;
                    break;

                case 1:
                    type = PowerUp.PowerUpType.Health50;
                    break;

                case 2:
                    type = PowerUp.PowerUpType.CoolWater;
                    break;

                case 3:
                    type = PowerUp.PowerUpType.LowBonusScore;
                    break;

                case 4:
                    type = PowerUp.PowerUpType.MediumBonusScore;
                    break;

                case 5:
                    type = PowerUp.PowerUpType.HighBonusScore;
                    break;

                case 6:
                    type = PowerUp.PowerUpType.OverHeat;
                    break;

                case 7:
                    type = PowerUp.PowerUpType.Shield;
                    break;

                case 8:
                    type = PowerUp.PowerUpType.Overdrive;
                    break;

                case 9:
                    type = PowerUp.PowerUpType.Underdrive;
                    break;
            }

            // only spawn bonus score power ups in combo mode
            if (GameModeManager.SelectedGameMode != GameModeManager.GameMode.Combo &&
                (type == PowerUp.PowerUpType.LowBonusScore ||
                type == PowerUp.PowerUpType.MediumBonusScore ||
                type == PowerUp.PowerUpType.HighBonusScore))
            {
                return PowerUp.PowerUpType.Health25;
            }

            return type;
        }

        public void Reset()
        {
            this.PowerUps.Clear();
            this.lastPowerUp = PowerUp.PowerUpType.Health50;

            this.moneySpawnTimer = 0.0f;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int x = powerUps.Count - 1; x >= 0; --x)
            {
                powerUps[x].Update(gameTime);

                if (!powerUps[x].IsActive)
                {
                    powerUps.RemoveAt(x);
                }
            }

            if (GameModeManager.SelectedGameMode == GameModeManager.GameMode.Combo)
            {
                moneySpawnTimer += elapsed;

                if (moneySpawnTimer >= (moneySpawnMinTimer - (currentLevel - 1) * MONEY_LESS_SPAWN_RATE_PER_LEVEL ))
                {
                    spawnAnyMoneyBlock();

                    moneySpawnTimer = 0.0f;
                }
            }
        }

        private void spawnAnyMoneyBlock()
        {
            int rnd = rand.Next(0, 11);

            switch (rnd)
            {
                // vertical line
                case 0:
                    spawnMoneyVertical();
                    break;

                // horizontal line
                case 1:
                    spawnMoneyHorizontal();
                    break;

                // diagonal line (from top left)
                case 2:
                    spawnMoneyDiagonalDownwards();
                    break;

                // diagonal line (from bottom left)
                case 3:
                    spawnMoneyDiagonaUpwards();
                    break;

                // rectangle
                case 4:
                    spawnMoneyRect();
                    break;

                // small arrow
                case 5:
                    spawnMoneySmallArrow();
                    break;

                // big arrow
                case 6:
                    spawnMoneyBigArrow();
                    break;

                // big arrow
                case 7:
                    spawnMoneyX();
                    break;

                // circle
                case 8:
                    spawnMoneyCircle();
                    break;

                // arrow up
                case 9:
                    spawnMoneyArrowUp();
                    break;

                // arrow down
                case 10:
                    spawnMoneyArrowDown();
                    break;
            }
        }

        private void spawnMoneyHorizontal()
        {
            int y = rand.Next(50, 430);
            for (int x = 850; x <= 970; x += 30)
            {
                spawnMoneyHigh(new Vector2(x, y));  
            }
        }

        private void spawnMoneyVertical()
        {
            int fromY = rand.Next(50, 310);
            for (int y = fromY; y <= fromY + 120; y += 30)
            {
                spawnMoneyHigh(new Vector2(850, y));
            }
        }

        private void spawnMoneyDiagonalDownwards()
        {
            int fromY = rand.Next(50, 310);
            for (int offset = 0; offset <= 120; offset += 30)
            {
                spawnMoneyHigh(new Vector2(850 + offset, fromY + offset));
            }
        }

        private void spawnMoneyDiagonaUpwards()
        {
            int fromY = rand.Next(170, 430);
            for (int offset = 0; offset <= 120; offset += 30)
            {
                spawnMoneyHigh(new Vector2(850 + offset, fromY - offset));
            }
        }

        private void spawnMoneyRect()
        {
            int y = rand.Next(50, 370);
            for (int x = 850; x <= 940; x += 30)
            {
                spawnMoneyMedium(new Vector2(x, y));
            }
            for (int x = 850; x <= 940; x += 30)
            {
                spawnMoneyHigh(new Vector2(x, y + 30));
            }
            for (int x = 850; x <= 940; x += 30)
            {
                spawnMoneyMedium(new Vector2(x, y + 60));
            }
        }

        private void spawnMoneySmallArrow()
        {
            int y = rand.Next(50, 370);
            
            spawnMoneyLow(new Vector2(850, y));
            spawnMoneyLow(new Vector2(850, y + 30));
            spawnMoneyLow(new Vector2(850, y + 60));
            spawnMoneyMedium(new Vector2(880, y + 15));
            spawnMoneyMedium(new Vector2(880, y + 45));
            spawnMoneyHigh(new Vector2(910, y + 30));
            
        }

        private void spawnMoneyBigArrow()
        {
            int y = rand.Next(50, 340);

            spawnMoneyLow(new Vector2(850, y));
            spawnMoneyLow(new Vector2(850, y + 30));
            spawnMoneyLow(new Vector2(850, y + 60));
            spawnMoneyLow(new Vector2(850, y + 90));
            spawnMoneyMedium(new Vector2(880, y + 15));
            spawnMoneyHigh(new Vector2(880, y + 45));
            spawnMoneyMedium(new Vector2(880, y + 75));
            spawnMoneyMedium(new Vector2(910, y + 30));
            spawnMoneyMedium(new Vector2(910, y + 60));
            spawnMoneyMedium(new Vector2(940, y + 45));
        }

        private void spawnMoneyX()
        {
            int y = rand.Next(50, 310);

            spawnMoneyHigh(new Vector2(850, y));
            spawnMoneyHigh(new Vector2(850, y + 120));
            spawnMoneyLow(new Vector2(880, y + 30));
            spawnMoneyLow(new Vector2(880, y + 90));
            spawnMoneyMedium(new Vector2(910, y + 60));
            spawnMoneyLow(new Vector2(940, y + 30));
            spawnMoneyLow(new Vector2(940, y + 90));
            spawnMoneyHigh(new Vector2(970, y));
            spawnMoneyHigh(new Vector2(970, y + 120));
        }

        private void spawnMoneyCircle()
        {
            int y = rand.Next(50, 310);

            spawnMoneyLow(new Vector2(850, y + 60));
            spawnMoneyMedium(new Vector2(880, y + 30));
            spawnMoneyMedium(new Vector2(880, y + 90));
            spawnMoneyLow(new Vector2(910, y));
            spawnMoneyLow(new Vector2(910, y + 120));
            spawnMoneyMedium(new Vector2(940, y + 30));
            spawnMoneyMedium(new Vector2(940, y + 90));
            spawnMoneyLow(new Vector2(970, y + 60));
        }

        private void spawnMoneyArrowDown()
        {
            int y = rand.Next(50, 370);

            spawnMoneyMedium(new Vector2(850, y));
            spawnMoneyMedium(new Vector2(880, y + 30));
            spawnMoneyHigh(new Vector2(910, y + 60));
            spawnMoneyMedium(new Vector2(940, y + 30));
            spawnMoneyMedium(new Vector2(970, y));
        }

        private void spawnMoneyArrowUp()
        {
            int y = rand.Next(50, 370);

            spawnMoneyMedium(new Vector2(850, y + 60));
            spawnMoneyMedium(new Vector2(880, y + 30));
            spawnMoneyHigh(new Vector2(910, y));
            spawnMoneyMedium(new Vector2(940, y + 30));
            spawnMoneyMedium(new Vector2(970, y + 60));
        }

        private void spawnMoneyLow(Vector2 center)
        {
            PowerUp p = new PowerUp(texture,
                            center + PowerUpOffset,
                            new Rectangle(0, 150, 25, 25),
                            1,
                            PowerUp.PowerUpType.LowBonusScore,
                            MONEY_SPEED_PER_LEVEL + MONEY_EXTRA_SPEED_PER_LEVEL * (currentLevel - 1));
            powerUps.Add(p);
        }

        private void spawnMoneyMedium(Vector2 center)
        {
            PowerUp p = new PowerUp(texture,
                            center + PowerUpOffset,
                            new Rectangle(0, 175, 25, 25),
                            1,
                            PowerUp.PowerUpType.MediumBonusScore,
                            MONEY_SPEED_PER_LEVEL + MONEY_EXTRA_SPEED_PER_LEVEL * (currentLevel - 1));
            powerUps.Add(p);
        }

        private void spawnMoneyHigh(Vector2 center)
        {
            PowerUp p = new PowerUp(texture,
                            center + PowerUpOffset,
                            new Rectangle(0, 200, 25, 25),
                            1,
                            PowerUp.PowerUpType.HighBonusScore,
                            MONEY_SPEED_PER_LEVEL + MONEY_EXTRA_SPEED_PER_LEVEL * (currentLevel - 1));
            powerUps.Add(p);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var powerUp in powerUps)
            {
                powerUp.Draw(spriteBatch);
            }
        }

        private Rectangle getInitialFrameByType(PowerUp.PowerUpType type)
        {
            switch (type)
            {
                case PowerUp.PowerUpType.Health25:
                    return new Rectangle(0, 0, 25, 25);
                case PowerUp.PowerUpType.Health50:
                    return new Rectangle(0, 25, 25, 25);
                case PowerUp.PowerUpType.CoolWater:
                    return new Rectangle(0, 225, 25, 25);
                case PowerUp.PowerUpType.LowBonusScore:
                    return new Rectangle(0, 150, 25, 25);
                case PowerUp.PowerUpType.MediumBonusScore:
                    return new Rectangle(0, 175, 25, 25);
                case PowerUp.PowerUpType.HighBonusScore:
                    return new Rectangle(0, 200, 25, 25);
                case PowerUp.PowerUpType.Shield:
                    return new Rectangle(0, 275, 25, 25);
                case PowerUp.PowerUpType.OverHeat:
                    return new Rectangle(0, 400, 25, 25);
                case PowerUp.PowerUpType.Random:
                    return new Rectangle(0, 375, 25, 25);
                case PowerUp.PowerUpType.Overdrive:
                    return new Rectangle(0, 500, 25, 25);
                case PowerUp.PowerUpType.Underdrive:
                    return new Rectangle(0, 525, 25, 25);
                default:
                    return new Rectangle(0, 0, 25, 25);
            }
        }

        public void SetLevel(int lvl)
        {
            currentLevel = lvl;
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            // Powerups
            int powerUpsCount = Int32.Parse(reader.ReadLine());
            
            powerUps.Clear();

            for (int i = 0; i < powerUpsCount; ++i)
            {
                PowerUp.PowerUpType type = PowerUp.PowerUpType.Random;
                type = (PowerUp.PowerUpType)Enum.Parse(type.GetType(), reader.ReadLine(), false);
                PowerUp p = new PowerUp(texture,
                                    Vector2.Zero,
                                    getInitialFrameByType(type),
                                    1,
                                    type);
                p.Activated(reader);
                powerUps.Add(p);
            }

            this.lastPowerUp = (PowerUp.PowerUpType)Enum.Parse(lastPowerUp.GetType(), reader.ReadLine(), false);
            this.currentLevel = Int32.Parse(reader.ReadLine());
        }

        public void Deactivated(StreamWriter writer)
        {
            // Powerups
            writer.WriteLine(powerUps.Count);

            for (int i = 0; i < powerUps.Count; ++i)
            {
                writer.WriteLine(powerUps[i].Type);
                powerUps[i].Deactivated(writer);
            }

            writer.WriteLine(lastPowerUp);
            writer.WriteLine(currentLevel);
        }

        #endregion

        #region Properties

        public List<PowerUp> PowerUps
        {
            get
            {
                return powerUps;
            }
        }

        #endregion
    }
}
