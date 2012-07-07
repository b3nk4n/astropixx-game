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


namespace AstropiXX
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    class SpaceshipManager
    {
        private Texture2D menuTexture;
        private Texture2D spriteSheet;
        private SpriteFont font;
        private readonly Rectangle SelectShipTitleSource = new Rectangle(0, 1150,
                                                                       300, 50);
        private readonly Vector2 TitlePosition = new Vector2(250.0f, 100.0f);

        private readonly Rectangle SpaceshipGreenHornetSource = new Rectangle(0, 150,
                                                                       50, 50);
        private readonly Rectangle SpaceshipEasySource = new Rectangle(150, 200,
                                                                       50, 50);
        private readonly Rectangle SpaceshipMediumSource = new Rectangle(0, 250,
                                                                       50, 50);
        private readonly Rectangle SpaceshipHardSource = new Rectangle(350, 200,
                                                                       50, 50);
        private readonly Rectangle SpaceshipTankSource = new Rectangle(350, 150,
                                                                       50, 50);
        private readonly Rectangle SpaceshipSpeederSource = new Rectangle(350, 250,
                                                                       50, 50);

        private readonly Rectangle SpaceshipDestination = new Rectangle(350, 175,
                                                                        100, 100);

        private readonly Rectangle LockSource = new Rectangle(450, 300,
                                                              50, 50);
        private readonly Rectangle LockDestination = new Rectangle(375, 225,
                                                                   50, 50);

        public const long DISTANCE_TO_UNLOCK_GREENHORNET = 10000000;
        public const long ASTEROIDS_TO_UNLOCK_MEDIUM = 250;
        public const long DISTANCE_TO_UNLOCK_SPEEDER = 5000000;
        public const long CREDITS_TO_UNLOCK_HARD = 5000000;
        public const long ASTEROIDS_TO_UNLOCK_TANK = 1000;

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

        private readonly Rectangle goSource = new Rectangle(0, 1250,
                                                                  300, 50);
        private readonly Rectangle goDestination = new Rectangle(50, 370,
                                                                     300, 50);
        public static GameInput GameInput;
        private const string LeftAction = "SelectLeftShip";
        private const string RightAction = "SelectRightShip";
        private const string GoAction = "Go";
        private const string CancelAction = "CancelShipSelection";

        private float opacity = 0.0f;
        private const float OpacityMax = 1.0f;
        private const float OpacityMin = 0.0f;
        private const float OpacityChangeRate = 0.05f;

        private bool isActive = false;

        private PlayerManager playerManager;
        private HighscoreManager highscoreManager;

        private bool cancelClicked = false;
        private bool goClicked = false;

        private float switchShipTimer = 0.0f;
        private const float SwitchShipMinTimer = 0.25f;

        private const string NAME_GREEN_HORNET = "Green Hornet G1";
        private const string NAME_EASY = "Light Drone 'Locuz'";
        private const string NAME_MEDIUM = "Starship S2";
        private const string NAME_HARD = "Heavy Battleship T21";
        private const string NAME_SPEEDER = "Speeder Z11";
        private const string NAME_TANK = "Tankship Rocket X23";

        private string descriptionGreenHornet = null;
        private string descriptionMedium = null;
        private string descriptionHard = null;
        private string descriptionSpeeder = null;
        private string descriptionTank = null;

        public SpaceshipManager(Texture2D mtex, Texture2D spriteSheet, SpriteFont font,
                                PlayerManager player, HighscoreManager highscoreManager)
        {
            this.menuTexture = mtex;
            this.spriteSheet = spriteSheet;
            this.font = font;
            this.playerManager = player;
            this.highscoreManager = highscoreManager;

            PrepareStrings();
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

            GameInput.AddTouchGestureInput(GoAction,
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
                switchShipTimer += elapsed;

                if (this.opacity < OpacityMax)
                    this.opacity += OpacityChangeRate;
            }

            handleTouchInputs();
        }

        private bool isCurrentSelectionUnlocked()
        {
            switch (playerManager.ShipType)
            {
                case PlayerManager.PlayerType.GreenHornet:
                    return highscoreManager.TotalDistanceScore >= DISTANCE_TO_UNLOCK_GREENHORNET;

                case PlayerManager.PlayerType.Medium:
                    return highscoreManager.TotalDestroyedAsteroids >= ASTEROIDS_TO_UNLOCK_MEDIUM;

                case PlayerManager.PlayerType.Hard:
                    return highscoreManager.TotalCollectScore >= CREDITS_TO_UNLOCK_HARD;

                case PlayerManager.PlayerType.Tank:
                    return highscoreManager.TotalDestroyedAsteroids >= ASTEROIDS_TO_UNLOCK_TANK;

                case PlayerManager.PlayerType.Speeder:
                    return highscoreManager.TotalDistanceScore >= DISTANCE_TO_UNLOCK_SPEEDER;

                default:
                    return true;
            }
        }

        private void handleTouchInputs()
        {
            // Left
            if (GameInput.IsPressed(LeftAction) && switchShipTimer > SwitchShipMinTimer)
            {
                switchShipTimer = 0.0f;

                toggleSpaceshipLeft();
            }
            // Right
            if (GameInput.IsPressed(RightAction) && switchShipTimer > SwitchShipMinTimer)
            {
                switchShipTimer = 0.0f;

                toggleSpaceshipRight();
            }
            // Select
            if (GameInput.IsPressed(GoAction))
            {
                if (isCurrentSelectionUnlocked())
                    goClicked = true;
            }
            // Cancel
            if (GameInput.IsPressed(CancelAction))
            {
                cancelClicked = true;
            }
        }

        private void toggleSpaceshipRight()
        {
            switch (playerManager.ShipType)
            {
                case PlayerManager.PlayerType.Easy:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Medium);
                    break;
                case PlayerManager.PlayerType.Medium:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Speeder);
                    break;
                case PlayerManager.PlayerType.Speeder:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.GreenHornet);
                    break;
                case PlayerManager.PlayerType.GreenHornet:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Hard);
                    break;
                case PlayerManager.PlayerType.Hard:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Tank);
                    break;
                case PlayerManager.PlayerType.Tank:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Easy);
                    break;
            }
        }

        private void toggleSpaceshipLeft()
        {
            switch (playerManager.ShipType)
            {
                case PlayerManager.PlayerType.Easy:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Tank);
                    break;
                case PlayerManager.PlayerType.Medium:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Easy);
                    break;
                case PlayerManager.PlayerType.Speeder:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Medium);
                    break;
                case PlayerManager.PlayerType.GreenHornet:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Speeder);
                    break;
                case PlayerManager.PlayerType.Hard:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.GreenHornet);
                    break;
                case PlayerManager.PlayerType.Tank:
                    playerManager.SelectPlayerType(PlayerManager.PlayerType.Hard);
                    break;
                default:
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(menuTexture,
                             TitlePosition,
                             SelectShipTitleSource,
                             Color.White * opacity);

            drawSpaceship(spriteBatch);

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

            // Button select
            if (isCurrentSelectionUnlocked())
                spriteBatch.Draw(menuTexture,
                                 goDestination,
                                 goSource,
                                 Color.White * opacity);
            else
                spriteBatch.Draw(menuTexture,
                                 goDestination,
                                 goSource,
                                 Color.White * (opacity * 0.5f));

            // Button cancel
            spriteBatch.Draw(menuTexture,
                             cancelDestination,
                             cancelSource,
                             Color.White * opacity);

            
        }

        /// <summary>
        /// Prepares the variable strings (called each time navigating to the select spaceship screen).
        /// </summary>
        public void PrepareStrings()
        {
            descriptionGreenHornet = string.Format("Fly a total distance of {0}ly! [{1}%]",
                                                   DISTANCE_TO_UNLOCK_GREENHORNET,
                                                   (int)(100.0f * (highscoreManager.TotalDistanceScore / (float)DISTANCE_TO_UNLOCK_GREENHORNET)));

            descriptionMedium = string.Format("Destroy {0} asteroids in destruction mode! [{1}%]",
                                              ASTEROIDS_TO_UNLOCK_MEDIUM,
                                              (int)(100.0f * (highscoreManager.TotalDestroyedAsteroids / (float)ASTEROIDS_TO_UNLOCK_MEDIUM)));

            descriptionHard = string.Format("Collect {0} credits in combo mode! [{1}%]",
                                            CREDITS_TO_UNLOCK_HARD,
                                            (int)(100.0f * (highscoreManager.TotalCollectScore / (float)CREDITS_TO_UNLOCK_HARD)));

            descriptionSpeeder = string.Format("Fly a total distance of {0}ly! [{1}%]",
                                               DISTANCE_TO_UNLOCK_SPEEDER,
                                               (int)(100.0f * (highscoreManager.TotalDistanceScore / (float)DISTANCE_TO_UNLOCK_SPEEDER)));

            descriptionTank = string.Format("Destroy {0} asteroids in destruction mode! [{1}%]",
                                            ASTEROIDS_TO_UNLOCK_TANK,
                                            (int)(100.0f * (highscoreManager.TotalDestroyedAsteroids / (float)ASTEROIDS_TO_UNLOCK_TANK)));
        }

        private void drawSpaceship(SpriteBatch spriteBatch)
        {
            Rectangle src;
            string desc;

            switch (playerManager.ShipType)
            {
                case PlayerManager.PlayerType.GreenHornet:
                    src = SpaceshipGreenHornetSource;
                    if (isCurrentSelectionUnlocked())
                        desc = NAME_GREEN_HORNET;
                    else
                        desc = descriptionGreenHornet;
                    break;
                case PlayerManager.PlayerType.Medium:
                    src = SpaceshipMediumSource;
                    if (isCurrentSelectionUnlocked())
                        desc = NAME_MEDIUM;
                    else
                        desc = descriptionMedium;
                    break;
                case PlayerManager.PlayerType.Hard:
                    src = SpaceshipHardSource;
                    if (isCurrentSelectionUnlocked())
                        desc = NAME_HARD;
                    else
                        desc = descriptionHard;
                    break;
                case PlayerManager.PlayerType.Speeder:
                    src = SpaceshipSpeederSource;
                    if (isCurrentSelectionUnlocked())
                        desc = NAME_SPEEDER;
                    else
                        desc = descriptionSpeeder;
                    break;
                case PlayerManager.PlayerType.Tank:
                    src = SpaceshipTankSource;
                    if (isCurrentSelectionUnlocked())
                        desc = NAME_TANK;
                    else
                        desc = descriptionTank;
                    break;
                default:
                    src = SpaceshipEasySource;
                    desc = NAME_EASY;
                    break;
            }

            if (isCurrentSelectionUnlocked())
            {
                spriteBatch.Draw(spriteSheet,
                                 SpaceshipDestination,
                                 src,
                                 Color.White * opacity,
                                 0.0f,
                                 Vector2.Zero,
                                 SpriteEffects.None,
                                 0.0f);

            }
            else
            {
                spriteBatch.Draw(spriteSheet,
                                 SpaceshipDestination,
                                 src,
                                 Color.White * (opacity * 0.5f),
                                 0.0f,
                                 Vector2.Zero,
                                 SpriteEffects.None,
                                 0.0f);

                spriteBatch.Draw(menuTexture,
                             LockDestination,
                             LockSource,
                             Astropixx.ThemeColor * opacity);

            }

            if (desc != null)
            {
                spriteBatch.DrawString(font,
                                       desc,
                                       new Vector2(DescriptionCenter.X - font.MeasureString(desc).X / 2,
                                                   DescriptionCenter.Y),
                                       Astropixx.ThemeColor * opacity);
            }
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
                    this.goClicked = false;
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

        public bool GoClicked
        {
            get
            {
                return this.goClicked;
            }
        }

        #endregion
    }
}
