using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.IO.IsolatedStorage;

namespace AstropiXX
{
    class InstructionManager
    {
        #region Members

        private float progressTimer = 0.0f;

        public enum InstructionStates { 
            Welcome,                       
            Movement,
            LeaveField,           
            Shot,
            ChangeControls, 
            HitPoints, 
            Shield, 
            Overheat, 
            GameModes,
            Survival,
            Destruction,
            Combo,
            PowerUps, 
            GoodLuck, 
            ReturnWithBackButton};

        private InstructionStates state = InstructionStates.Welcome;

        private const float WelcomeLimit = 2.0f;
        private const float MovementLimit = 5.0f;
        private const float LeaveFieldLimit = 8.0f;
        private const float ShotLimit = 11.0f;
        private const float ChangeControlsLimit = 14.0f;
        private const float HitPointsLimit = 17.0f;
        private const float ShieldLimit = 20.0f;
        private const float OverheatLimit = 23.0f;
        private const float GameModesLimit = 26.0f;
        private const float SurvivalLimit = 29.0f;
        private const float DestructionLimit = 32.0f;
        private const float ComboLimit = 35.0f;
        private const float PowerUpsLimit = 38.0f;

        private SpriteFont font;

        private Texture2D texture;

        private Rectangle screenBounds;

        private Rectangle areaSource = new Rectangle(660, 60, 380, 150);
        private Rectangle arrowRightSource = new Rectangle(100, 460, 40, 20);

        private Rectangle leftTouchDestination = new Rectangle(10, 320, 380, 150);
        private Rectangle rightTouchDestination = new Rectangle(410, 320, 380, 150);
        private Rectangle hitPointsDestination = new Rectangle(575, 25, 40, 20);
        private Rectangle shieldDestination = new Rectangle(575, 45, 40, 20);
        private Rectangle overheatDestination = new Rectangle(575, 65, 40, 20);

        private Color areaTint = Color.Red * 0.5f;
        private Color arrowTint = Color.Red * 0.8f;

        private AsteroidManager asteroidManager;

        private PlayerManager playerManager;

        private PowerUpManager powerUpManager;

        private SettingsManager settings = SettingsManager.GetInstance();

        private readonly string WelcomeText = "Welcome to AstropiXX!";
        private readonly string MovementText = "Activate your height engine by touching here...";
        private readonly string LeaveFieldText = "Do not leave the game field!";
        private readonly string ShotText = "Press here to fire...";
        private readonly string ChangeControlsText = "You can swap controls in the settings menu...";
        private readonly string HitPointsText = "The HUD display your current hit points...";
        private readonly string ShieldText = "your shield level...";
        private readonly string OverheatText = "and your current weapon heat! Avoid overheating!";
        private readonly string GameModesText = "AstropiXX provides 3 different game modes...";
        private readonly string SurvivalText = "In survival mode, you have to fly as far as you can!";
        private readonly string DestructionText = "In destruction mode, you have to destroy asteroids!";
        private readonly string ComboText1 = "In Combo mode, you gain score by distance,";
        private readonly string ComboText2 = "collecting credits and destroying asteroids!";
        private readonly string PowerUpsText = "Gather powerups ... but avoid the red ones!";
        private readonly string GoodLuckText = "Good luck commander!";
        private readonly string ReturnWithBackButtonText = "Press BACK to return...";

        private static bool hasDoneInstructions = false;

        #endregion

        #region Constructors

        public InstructionManager(Texture2D texture, SpriteFont font, Rectangle screenBounds,
                                  AsteroidManager asteroidManager, PlayerManager playerManager,
                                  PowerUpManager powerUpManager)
        {
            this.texture = texture;
            this.font = font;
            this.screenBounds = screenBounds;

            this.asteroidManager = asteroidManager;
            this.asteroidManager.Reset();

            this.playerManager = playerManager;
            this.playerManager.Reset();

            this.powerUpManager = powerUpManager;
            this.powerUpManager.Reset();

            loadHasDoneInstructions();
        }

        #endregion

        #region Methods

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            progressTimer += elapsed;

            if (playerManager.IsDestroyed)
            {
                this.state = InstructionStates.ReturnWithBackButton;

                asteroidManager.Update(gameTime);
                powerUpManager.Update(gameTime);

                return;
            }
            else if (progressTimer < WelcomeLimit)
            {
                this.state = InstructionStates.Welcome;
            }
            else if (progressTimer < MovementLimit)
            {
                this.state = InstructionStates.Movement;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < LeaveFieldLimit)
            {
                this.state = InstructionStates.LeaveField;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < ShotLimit)
            {
                this.state = InstructionStates.Shot;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < ChangeControlsLimit)
            {
                this.state = InstructionStates.ChangeControls;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < HitPointsLimit)
            {
                this.state = InstructionStates.HitPoints;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < ShieldLimit)
            {
                this.state = InstructionStates.Shield;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < OverheatLimit)
            {
                this.state = InstructionStates.Overheat;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < GameModesLimit)
            {
                this.state = InstructionStates.GameModes;

                playerManager.Update(gameTime);
            }
            else if (progressTimer < SurvivalLimit)
            {
                this.state = InstructionStates.Survival;

                playerManager.Update(gameTime);
                asteroidManager.Update(gameTime);
            }
            else if (progressTimer < DestructionLimit)
            {
                this.state = InstructionStates.Destruction;

                playerManager.Update(gameTime);
                asteroidManager.Update(gameTime);
                powerUpManager.Update(gameTime);
            }
            else if (progressTimer < ComboLimit)
            {
                this.state = InstructionStates.Combo;

                playerManager.Update(gameTime);
                asteroidManager.Update(gameTime);
                powerUpManager.Update(gameTime);
            }
            else if (progressTimer < PowerUpsLimit)
            {
                this.state = InstructionStates.PowerUps;

                playerManager.Update(gameTime);
                asteroidManager.Update(gameTime);
                powerUpManager.Update(gameTime);
            }
            else
            {
                this.state = InstructionStates.GoodLuck;

                playerManager.Update(gameTime);
                asteroidManager.Update(gameTime);
                powerUpManager.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch(this.state)
            {
                case InstructionStates.Welcome:
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, WelcomeText);
                    break;

                case InstructionStates.Movement:
                    playerManager.Draw(spriteBatch);

                    if (settings.ControlPosition == SettingsManager.ControlPositionValues.Left)
                    {
                        spriteBatch.Draw(texture,
                                         leftTouchDestination,
                                         areaSource,
                                         areaTint,
                                         0.0f,
                                         Vector2.Zero,
                                         SpriteEffects.None,
                                         0.0f);
                    }
                    else
                    {
                        spriteBatch.Draw(texture,
                                         rightTouchDestination,
                                         areaSource,
                                         areaTint,
                                         0.0f,
                                         Vector2.Zero,
                                         SpriteEffects.FlipHorizontally,
                                         0.0f);
                    }
                    drawRedCenteredText(spriteBatch, MovementText);
                    break;

                case InstructionStates.LeaveField:
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, LeaveFieldText);
                    break;

                case InstructionStates.Shot:
                    playerManager.Draw(spriteBatch);

                    if (settings.ControlPosition == SettingsManager.ControlPositionValues.Right)
                    {
                        spriteBatch.Draw(texture,
                                         leftTouchDestination,
                                         areaSource,
                                         areaTint,
                                         0.0f,
                                         Vector2.Zero,
                                         SpriteEffects.None,
                                         0.0f);
                    }
                    else
                    {
                        spriteBatch.Draw(texture,
                                         rightTouchDestination,
                                         areaSource,
                                         areaTint,
                                         0.0f,
                                         Vector2.Zero,
                                         SpriteEffects.FlipHorizontally,
                                         0.0f);
                    }
                    drawRedCenteredText(spriteBatch, ShotText);
                    break;

                case InstructionStates.ChangeControls:
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, ChangeControlsText);
                    break;

                case InstructionStates.HitPoints:
                    playerManager.Draw(spriteBatch);

                    spriteBatch.Draw(texture,
                                     hitPointsDestination,
                                     arrowRightSource,
                                     arrowTint);
                    drawRedCenteredText(spriteBatch, HitPointsText);
                    break;

                case InstructionStates.Shield:
                    playerManager.Draw(spriteBatch);

                    spriteBatch.Draw(texture,
                                     shieldDestination,
                                     arrowRightSource,
                                     arrowTint);
                    drawRedCenteredText(spriteBatch, ShieldText);
                    break;

                case InstructionStates.Overheat:
                    playerManager.Draw(spriteBatch);

                    spriteBatch.Draw(texture,
                                     overheatDestination,
                                     arrowRightSource,
                                     arrowTint);
                    drawRedCenteredText(spriteBatch, OverheatText);
                    break;

                case InstructionStates.GameModes:
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, GameModesText);
                    break;

                case InstructionStates.Survival:
                    asteroidManager.Draw(spriteBatch);
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, SurvivalText);
                    break;

                case InstructionStates.Destruction:
                    powerUpManager.Draw(spriteBatch);
                    asteroidManager.Draw(spriteBatch);
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, DestructionText);
                    break;

                case InstructionStates.Combo:
                    powerUpManager.Draw(spriteBatch);
                    asteroidManager.Draw(spriteBatch);
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, ComboText1, ComboText2);
                    break;

                case InstructionStates.PowerUps:
                    powerUpManager.Draw(spriteBatch);
                    asteroidManager.Draw(spriteBatch);
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, PowerUpsText);
                    break;

                case InstructionStates.GoodLuck:
                    powerUpManager.Draw(spriteBatch);
                    asteroidManager.Draw(spriteBatch);
                    playerManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, GoodLuckText);
                    break;

                case InstructionStates.ReturnWithBackButton:
                    powerUpManager.Draw(spriteBatch);
                    asteroidManager.Draw(spriteBatch);

                    drawRedCenteredText(spriteBatch, ReturnWithBackButtonText);
                    break;
            }
        }

        private void drawRedCenteredText(SpriteBatch spriteBatch, string text)
        {
            spriteBatch.DrawString(font,
                                   text,
                                   new Vector2(screenBounds.Width / 2 - font.MeasureString(text).X / 2,
                                               screenBounds.Height / 2 - font.MeasureString(text).Y / 2),
                                   Color.Red);
        }

        private void drawRedCenteredText(SpriteBatch spriteBatch, string text1, string text2)
        {
            spriteBatch.DrawString(font,
                                   text1,
                                   new Vector2(screenBounds.Width / 2 - font.MeasureString(text1).X / 2,
                                               screenBounds.Height / 2 - font.MeasureString(text1).Y / 2 - 15),
                                   Color.Red);

            spriteBatch.DrawString(font,
                                   text2,
                                   new Vector2(screenBounds.Width / 2 - font.MeasureString(text2).X / 2,
                                               screenBounds.Height / 2 - font.MeasureString(text2).Y / 2 + 15),
                                   Color.Red);
        }

        public void Reset()
        {
            this.progressTimer = 0.0f;
            this.state = InstructionStates.Welcome;
            GameModeManager.SetupForInstructions();
            playerManager.SelectPlayerType(PlayerManager.PlayerType.Easy);
        }

        public void SaveHasDoneInstructions()
        {

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream("instructions.txt", FileMode.Create, isf))
                {
                    using (StreamWriter sw = new StreamWriter(isfs))
                    {
                        sw.WriteLine(hasDoneInstructions);

                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }

        private void loadHasDoneInstructions()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool hasExisted = isf.FileExists(@"instructions.txt");

                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(@"instructions.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, isf))
                {
                    if (hasExisted)
                    {
                        using (StreamReader sr = new StreamReader(isfs))
                        {
                            hasDoneInstructions = Boolean.Parse(sr.ReadLine());
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(isfs))
                        {
                            sw.WriteLine(hasDoneInstructions);

                            // ... ? 
                        }
                    }
                }
            }
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            this.progressTimer = Single.Parse(reader.ReadLine());
            hasDoneInstructions = Boolean.Parse(reader.ReadLine());
        }

        public void Deactivated(StreamWriter writer)
        {
            writer.WriteLine(progressTimer);
            writer.WriteLine(hasDoneInstructions);
        }

        #endregion

        #region Properties

        public static bool HasDoneInstructions
        {
            get
            {
                return hasDoneInstructions;
            }
            set
            {
                hasDoneInstructions = value;
            }
        }

        #endregion
    }
}
