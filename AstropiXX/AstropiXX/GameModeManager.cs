using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AstropiXX.Inputs;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;


namespace AstropiXX
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    class GameModeManager
    {
        private Texture2D menuTexture;
        private SpriteFont font;
        private readonly Rectangle GameModeTitleSource = new Rectangle(0, 1200,
                                                                       300, 50);
        private readonly Vector2 TitlePosition = new Vector2(250.0f, 100.0f);

        private readonly Rectangle GameModeSurvivalSource = new Rectangle(0, 1300,
                                                                       300, 50);
        private readonly Rectangle GameModeDestructionSource = new Rectangle(0, 1350,
                                                                       300, 50);
        private readonly Rectangle GameModeComboSource = new Rectangle(0, 1400,
                                                                       300, 50);

        private readonly Rectangle GameModeDestination = new Rectangle(250, 200,
                                                                        300, 50);

        private readonly Rectangle ArrowRightSource = new Rectangle(400, 350,
                                                                  100, 100);
        private readonly Rectangle ArrowRightDestination = new Rectangle(550, 175,
                                                                       100, 100);
        private readonly Rectangle ArrowLeftDestination = new Rectangle(150, 175,
                                                                       100, 100);

        private readonly Vector2 DescriptionCenter = new Vector2(400, 290);

        private readonly Rectangle cancelSource = new Rectangle(0, 750,
                                                                300, 50);
        private readonly Rectangle cancelDestination = new Rectangle(450, 370,
                                                                     300, 50);

        private readonly Rectangle goSource = new Rectangle(0, 1100,
                                                                  300, 50);
        private readonly Rectangle goDestination = new Rectangle(50, 370,
                                                                     300, 50);
        public static GameInput GameInput;
        private const string LeftAction = "SelectLeftMode";
        private const string RightAction = "SelectRightMode";
        private const string SelectAction = "Select";
        private const string CancelAction = "CancelModeSelection";

        private float opacity = 0.0f;
        private const float OpacityMax = 1.0f;
        private const float OpacityMin = 0.0f;
        private const float OpacityChangeRate = 0.05f;

        private bool isActive = false;

        private PlayerManager playerManager;
        private HighscoreManager highscoreManager;

        private bool cancelClicked = false;
        private bool selectClicked = false;

        private float switchModeTimer = 0.0f;
        private const float SwitchModeMinTimer = 0.25f;

        public enum GameMode { Survival, Destruction, Combo };
        public static GameMode SelectedGameMode = GameMode.Survival;

        private const string DESCRIPTION_SURVIVAL = "Stay alive! As long as you can!!!";
        private const string DESCRIPTION_DESTRUCTION = "Destroy the asteroids! As much as you can!!!";
        private const string DESCRIPTION_COMBO = "Gain the highest score by destroying asteroids and collecting credits!";

        public GameModeManager(Texture2D mtex, SpriteFont font,
                                PlayerManager player, HighscoreManager highscoreManager)
        {
            this.menuTexture = mtex;
            this.font = font;
            this.playerManager = player;
            this.highscoreManager = highscoreManager;
        }

        #region Methods

        public void SetupInputs()
        {
            GameInput.AddTouchGestureInput(LeftAction,
                                           GestureType.Tap,
                                           ArrowLeftDestination);
            GameInput.AddTouchGestureInput(RightAction,
                                           GestureType.Tap,
                                           ArrowRightDestination);

            GameInput.AddTouchSlideInput(LeftAction,
                                         Input.Direction.Right,
                                         30.0f);
            GameInput.AddTouchSlideInput(RightAction,
                                         Input.Direction.Left,
                                         30.0f);

            GameInput.AddTouchGestureInput(SelectAction,
                                           GestureType.Tap,
                                           goDestination);
            GameInput.AddTouchGestureInput(CancelAction,
                                           GestureType.Tap,
                                           cancelDestination);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isActive)
            {
                switchModeTimer += elapsed;

                if (this.opacity < OpacityMax)
                    this.opacity += OpacityChangeRate;
            }

            handleTouchInputs();
        }

        private void handleTouchInputs()
        {
            // Left
            if (GameInput.IsPressed(LeftAction) && switchModeTimer > SwitchModeMinTimer)
            {
                switchModeTimer = 0.0f;

                toggleGameModeLeft();
            }
            // Right
            if (GameInput.IsPressed(RightAction) && switchModeTimer > SwitchModeMinTimer)
            {
                switchModeTimer = 0.0f;

                toggleGameModeRight();
            }
            // Select
            if (GameInput.IsPressed(SelectAction))
            {
                    selectClicked = true;
            }
            // Cancel
            if (GameInput.IsPressed(CancelAction))
            {
                cancelClicked = true;
            }
        }

        private void toggleGameModeRight()
        {
            switch (GameModeManager.SelectedGameMode)
            {
                case GameMode.Survival:
                    SelectedGameMode = GameMode.Destruction;
                    break;
                case GameMode.Destruction:
                    SelectedGameMode = GameMode.Combo;
                    break;
                case GameMode.Combo:
                    SelectedGameMode = GameMode.Survival;
                    break;
            }
            
        }

        private void toggleGameModeLeft()
        {
            switch (GameModeManager.SelectedGameMode)
            {
                case GameMode.Survival:
                    SelectedGameMode = GameMode.Combo;
                    break;
                case GameMode.Destruction:
                    SelectedGameMode = GameMode.Survival;
                    break;
                case GameMode.Combo:
                    SelectedGameMode = GameMode.Destruction;
                    break;
            }
        }

        public static void SetupForInstructions()
        {
            SelectedGameMode = GameMode.Combo;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(menuTexture,
                             TitlePosition,
                             GameModeTitleSource,
                             Color.White * opacity);


            // Arrow left
            spriteBatch.Draw(menuTexture,
                             ArrowLeftDestination,
                             ArrowRightSource,
                             Astropixx.ThemeColor * opacity,
                             0.0f,
                             Vector2.Zero,
                             SpriteEffects.FlipHorizontally,
                             0.0f);

            // Arrow right
            spriteBatch.Draw(menuTexture,
                             ArrowRightDestination,
                             ArrowRightSource,
                             Astropixx.ThemeColor * opacity,
                             0.0f,
                             Vector2.Zero,
                             SpriteEffects.None,
                             0.0f);

            //Mode
            drawGameMode(spriteBatch);

            // Button select
            spriteBatch.Draw(menuTexture,
                                goDestination,
                                goSource,
                                Color.White * opacity);

            // Button cancel
            spriteBatch.Draw(menuTexture,
                             cancelDestination,
                             cancelSource,
                             Color.White * opacity);


        }

        private void drawGameMode(SpriteBatch spriteBatch)
        {
            Rectangle src;
            string desc;

            switch (SelectedGameMode)
            {
                case GameMode.Destruction:
                    src = GameModeDestructionSource;
                    desc = DESCRIPTION_DESTRUCTION;
                    break;
                case GameMode.Combo:
                    src = GameModeComboSource;
                    desc = DESCRIPTION_COMBO;
                    break;
                
                default:
                    src = GameModeSurvivalSource;
                    desc = DESCRIPTION_SURVIVAL;
                    break;
            }

            spriteBatch.Draw(menuTexture,
                             GameModeDestination,
                             src,
                             Color.White * opacity,
                             0.0f,
                             Vector2.Zero,
                             SpriteEffects.None,
                             0.0f);

            spriteBatch.DrawString(font,
                                desc,
                                new Vector2(DescriptionCenter.X - font.MeasureString(desc).X / 2,
                                            DescriptionCenter.Y),
                                Astropixx.ThemeColor * opacity);
            
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            this.cancelClicked = Boolean.Parse(reader.ReadLine());
            this.selectClicked = Boolean.Parse(reader.ReadLine());
            this.opacity = Single.Parse(reader.ReadLine());
            this.isActive = Boolean.Parse(reader.ReadLine());
            SelectedGameMode = (GameMode)Enum.Parse(SelectedGameMode.GetType(), reader.ReadLine(), false);
        }

        public void Deactivated(StreamWriter writer)
        {
            writer.WriteLine(cancelClicked);
            writer.WriteLine(selectClicked);
            writer.WriteLine(opacity);
            writer.WriteLine(isActive);
            writer.WriteLine(SelectedGameMode);
        }

        #endregion

        #region Properties

        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                this.isActive = value;

                if (isActive == false)
                {
                    this.opacity = OpacityMin;
                    this.selectClicked = false;
                    this.cancelClicked = false;
                }
            }
        }

        public bool CancelClicked
        {
            get
            {
                return this.cancelClicked;
            }
        }

        public bool SelectClicked
        {
            get
            {
                return this.selectClicked;
            }
        }

        #endregion
    }
}
