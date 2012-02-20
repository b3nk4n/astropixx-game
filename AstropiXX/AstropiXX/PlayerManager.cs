using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using System.IO;
using AstropiXX.Inputs;

namespace AstropiXX
{
    class PlayerManager : ILevel
    {
        #region Members

        public Sprite playerSprite;
        public const float PLAYER_SPEED = 225.0f;
        private Rectangle playerAreaLimit;

        private Vector2 startLocation = new Vector2(25, 200);

        // Score by distance
        private long distanceScore = 0;
        private float distanceScore_buffer = 0.0f;
        public const long SCORE_PER_INTERVAL = 50;        // 1000 per second
        public const float SCORE_BUFFER_LIMIT = 0.05f;

        // Score by power ups
        private long collectScore = 0;
        
        // Score by asteroids
        private long destroyedAsteroids = 0;
        public const long SCORE_PER_ASTEROID = 10000; 

        private const int PLAYER_STARTING_LIVES = 0;
        public const int MAX_PLAYER_LIVES = 0;
        private int livesRemaining = PLAYER_STARTING_LIVES;
        public int SpecialShotsRemaining = 5;

        public const int CARLI_ROCKET_EXPLOSION_RADIUS = 150;

        private float shotPower = 50.0f;
        
        // *** Before version 2.0 ***
        //public const float ROCKET_POWER_AT_CENTER = 500.0f;
        public const float ROCKET_POWER_AT_CENTER = 300.0f;

        private float hitPoints = 100.0f;
        public const float MaxHitPoints = 100.0f;
        const float HealRate = 0.5f;

        private Vector2 gunOffset = new Vector2(45, 19);
        private float shotTimer = 0.0f;
        private float rocketTimer = 0.0f;
        private float minEasyShotTimer = 0.175f;
        private float minGreenHornetShotTimer = 0.40f;
        private float minMediumDoubleShotTimer = 0.275f;
        private float minHardQuadShotTimer = 0.5f;
        private float minTankCarliRocketTimer = 1.5f;
        private float minSpeederTrippleShotTimer = 0.40f;
        
        private const int PlayerRadius = 20;
        public ShotManager PlayerShotManager;

        private float overheat = 0.0f;
        private const float OverheatEasySingleShot = 0.087f;
        private const float OverheatGreenHornetShot = 0.2f;
        private const float OverheatMediumDoubleShot = 0.14f;
        private const float OverheatHardQuadShot = 0.24f;
        private const float OverheatTankCarliRocket = 0.68f;
        private const float OverheatSpeederTrippleShot = 0.2f;
        public const float OVERHEAT_MAX = 1.0f;
        public const float OVERHEAT_MIN = 0.0f;
        private const float CoolDownRate = 0.45f;
        private const float OverheatKillRateMax = 0.075f;

        Vector3 currentAccValue = Vector3.Zero;

        Rectangle leftSideScreen;
        Rectangle rightSideScreen;
        Rectangle middleScreen;
        Rectangle upperRightScreen;
        Rectangle upperLeftScreen;

        // Shield
        private float shieldPoints = 0.0f;
        private const float SHIELD_POINTS_MAX = 100.0f;
        private const float SHIELD_DECREASE_RATE = 2.5f; // 40 sec

        private Sprite shieldSprite;
        private readonly Rectangle initialShieldFrame = new Rectangle(250, 400, 70, 70);

        SettingsManager settings = SettingsManager.GetInstance();

        GameInput gameInput;
        private const string ActionLeftSideScreen = "LeftSideScreen";
        private const string ActionRightSideScreen = "RightSideScreen";

        // Over/Underdrive
        private float overdriveTimer = 0.0f;
        public const float OVERDRIVE_DURATION = 20.0f;
        private float overdriveFactor = OVERDRIVE_NORMAL_FACTOR;
        public const float OVERDRIVE_FACTOR = 1.75f;
        public const float OVERDRIVE_NORMAL_FACTOR = 1.0f;
        public const float UNDERDRIVE_FACTOR = 0.70f;

        // gravity
        private Vector2 gravity;
        public readonly Vector2 GRAVITY_GREENHORNET  = new Vector2(0, 900);
        public readonly Vector2 GRAVITY_MEDIUM = new Vector2(0, 800);
        public readonly Vector2 GRAVITY_HARD = new Vector2(0, 750);
        public readonly Vector2 GRAVITY_TANK = new Vector2(0, 650);
        public readonly Vector2 GRAVITY_EASY = new Vector2(0, 1000);
        public readonly Vector2 GRAVITY_SPEEDER = new Vector2(0, 950);

        public enum PlayerType { GreenHornet, Medium, Hard, Tank, Speeder, Easy };
        private PlayerType shipType = PlayerType.Easy;

        Texture2D tex;

        private float wallgrindTimer = 0.0f;
        private const float wallgrindMinTimer = 0.075f;

        private const float OUT_OF_SCREEN_DAMANGE = 2.0f;

        private float powerSoundTimer = 0.0f;
        private const float powerSoundMinTimer = 0.5f;
        private bool powerSoundUnblocked = true;

        #endregion

        #region Constructors

        public PlayerManager(Texture2D texture, Rectangle initialFrame,
                             int frameCount, Rectangle screenBounds,
                             GameInput input)
        {
            tex = texture;

            this.PlayerShotManager = new ShotManager(texture,
                                                     new Rectangle(100, 430, 15, 5),
                                                     4,
                                                     2,
                                                     400.0f,
                                                     screenBounds);

            this.playerAreaLimit = new Rectangle(0,
                                                 0,
                                                 screenBounds.Width,
                                                 screenBounds.Height);

            SelectPlayerType(PlayerType.Easy);

            playerSprite.CollisionRadius = PlayerRadius;

            leftSideScreen = new Rectangle(0,
                                           2 * screenBounds.Height / 3,
                                           screenBounds.Width / 2,
                                           screenBounds.Height / 3);

            rightSideScreen = new Rectangle(screenBounds.Width / 2,
                                           2 * screenBounds.Height / 3,
                                           screenBounds.Width / 2,
                                           screenBounds.Height / 3);

            middleScreen = new Rectangle(0,
                                              screenBounds.Height / 3,
                                              screenBounds.Width,
                                              screenBounds.Height / 3);

            upperLeftScreen = new Rectangle(0, 0,
                                            screenBounds.Width / 2,
                                            screenBounds.Height / 4);

            upperRightScreen = new Rectangle(screenBounds.Width / 2,
                                             0,
                                             screenBounds.Width / 2,
                                             screenBounds.Height / 4);

            this.shieldSprite = new Sprite(Vector2.Zero,
                                           texture,
                                           initialShieldFrame,
                                           Vector2.Zero);

            gameInput = input;
        }

        #endregion

        #region Methods

        public void SetupInputs()
        {
            gameInput.AddTouchTapInput(ActionLeftSideScreen,
                                       leftSideScreen,
                                       false);
            gameInput.AddTouchTapInput(ActionRightSideScreen,
                                       rightSideScreen,
                                       false);
        }

        public void Reset()
        {
            this.PlayerShotManager.Shots.Clear();
            this.PlayerShotManager.Rockets.Clear();

            this.playerSprite.Location = startLocation;
            this.playerSprite.Velocity = Vector2.Zero;

            this.hitPoints = MaxHitPoints;
            this.shieldPoints = 0.0f;
            this.Overheat = PlayerManager.OVERHEAT_MIN;

            this.overdriveFactor = OVERDRIVE_NORMAL_FACTOR;
            this.overdriveTimer = 0.0f;

            destroyedAsteroids = 0;
            collectScore = 0;
            distanceScore = 0;
        }

        public void SelectPlayerType(PlayerType type)
        {
            this.shipType = type;

            switch (type)
            {
                case PlayerType.GreenHornet:
                    playerSprite =  new Sprite(new Vector2(500, 500),
                                           tex,
                                           new Rectangle(0, 150, 50, 50),
                                           Vector2.Zero);

                    for (int x = 0; x < 6; x++)
                    {
                        this.playerSprite.AddFrame(new Rectangle(0 + (x * 50),
                                                                 150,
                                                                 50,
                                                                 50));
                    }

                    gravity = GRAVITY_GREENHORNET;
                    break;

                case PlayerType.Easy:
                    playerSprite = new Sprite(new Vector2(500, 500),
                                           tex,
                                           new Rectangle(0, 200, 50, 50),
                                           Vector2.Zero);

                    for (int x = 0; x < 4; x++)
                    {
                        this.playerSprite.AddFrame(new Rectangle(0 + (x * 50),
                                                                 200,
                                                                 50,
                                                                 50));
                    }

                    gravity = GRAVITY_EASY;
                    break;

                case PlayerType.Medium:
                    playerSprite =  new Sprite(new Vector2(500, 500),
                                           tex,
                                           new Rectangle(0, 250, 50, 50),
                                           Vector2.Zero);

                    for (int x = 0; x < 6; x++)
                    {
                        this.playerSprite.AddFrame(new Rectangle(0 + (x * 50),
                                                                 250,
                                                                 50,
                                                                 50));
                    }

                    gravity = GRAVITY_MEDIUM;
                    break;

                case PlayerType.Hard:
                    playerSprite =  new Sprite(new Vector2(500, 500),
                                           tex,
                                           new Rectangle(350, 200, 50, 50),
                                           Vector2.Zero);

                    for (int x = 0; x < 6; x++)
                    {
                        this.playerSprite.AddFrame(new Rectangle(350 + (x * 50),
                                                                 200,
                                                                 50,
                                                                 50));
                    }

                    gravity = GRAVITY_HARD;
                    break;

                case PlayerType.Speeder:
                    playerSprite = new Sprite(new Vector2(500, 500),
                                           tex,
                                           new Rectangle(350, 250, 50, 50),
                                           Vector2.Zero);

                    for (int x = 0; x < 6; x++)
                    {
                        this.playerSprite.AddFrame(new Rectangle(350 + (x * 50),
                                                                 250,
                                                                 50,
                                                                 50));
                    }

                    gravity = GRAVITY_SPEEDER;
                    break;

                case PlayerType.Tank:
                    playerSprite =  new Sprite(new Vector2(500, 500),
                                           tex,
                                           new Rectangle(350, 150, 50, 50),
                                           Vector2.Zero);

                    for (int x = 0; x < 6; x++)
                    {
                        this.playerSprite.AddFrame(new Rectangle(350 + (x * 50),
                                                                 150,
                                                                 50,
                                                                 50));
                    }

                    gravity = GRAVITY_TANK;
                    break;
                default:
                    break;
            }

            playerSprite.Location = startLocation;
            playerSprite.CollisionRadius = PlayerRadius;
        }

        public void ResetSpecialWeapons()
        {
            this.SpecialShotsRemaining = 3;
        }

        public void ResetRemainingLives()
        {
            this.LivesRemaining = PLAYER_STARTING_LIVES;
        }

        private void fireEasyShot()
        {
            if (shotTimer <= 0.0f)
            {
                overheat += OverheatEasySingleShot;

                if (overheat < OVERHEAT_MAX)
                {
                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset,
                                                    new Vector2(1, 0),
                                                    true,
                                                    new Color(1.0f, 1.0f, 0.1f),
                                                    true);
                    shotTimer = minEasyShotTimer / overdriveFactor;
                }
                else
                {
                    shotTimer = minEasyShotTimer * 1.5f / overdriveFactor;
                }
            }
        }

        private void fireGreenHornetShot()
        {
            if (shotTimer <= 0.0f)
            {
                overheat += (OverheatGreenHornetShot);

                if (overheat < OVERHEAT_MAX)
                {
                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset + new Vector2(10, 0),
                                                    Vector2.UnitX,
                                                    true,
                                                    new Color(0, 255, 33),
                                                    true);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset + new Vector2(0, 13),
                                                    Vector2.UnitX,
                                                    true,
                                                    new Color(0, 255, 33),
                                                    true);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset - new Vector2(0, 13),
                                                    Vector2.UnitX,
                                                    true,
                                                    new Color(0, 255, 33),
                                                    false);

                    shotTimer = minGreenHornetShotTimer / overdriveFactor;
                }
                else
                {
                    shotTimer = minGreenHornetShotTimer * 2.0f / overdriveFactor;
                }
            }
        }

        private void fireMediumDoubleShot()
        {
            if (shotTimer <= 0.0f)
            {
                overheat += OverheatMediumDoubleShot;

                if (overheat < OVERHEAT_MAX)
                {
                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset + new Vector2(0, 13),
                                                    new Vector2(1, 0),
                                                    true,
                                                    new Color(0.8f, 0.5f, 1.0f),
                                                    true);
                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset - new Vector2(0, 13),
                                                    new Vector2(1, 0),
                                                    true,
                                                    new Color(0.8f, 0.5f, 1.0f),
                                                    true);
                    shotTimer = minMediumDoubleShotTimer / overdriveFactor;
                }
                else
                {
                    shotTimer = minMediumDoubleShotTimer * 1.5f / overdriveFactor;
                }
            }
        }

        private void fireHardQuadShot()
        {
            if (shotTimer <= 0.0f)
            {
                overheat += (OverheatHardQuadShot);

                if (overheat < OVERHEAT_MAX)
                {
                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset + new Vector2(0, 7),
                                                    Vector2.UnitX,
                                                    true,
                                                    new Color(1.0f, 0.1f, 0.1f),
                                                    false);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset - new Vector2(0, 7),
                                                    Vector2.UnitX,
                                                    true,
                                                    new Color(1.0f, 0.1f, 0.1f),
                                                    false);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset + new Vector2(0, 13),
                                                    new Vector2((float)Math.Sin(MathHelper.ToRadians(87.5f)), (float)Math.Cos(MathHelper.ToRadians(87.5f))),
                                                    true,
                                                    new Color(1.0f, 0.1f, 0.1f),
                                                    true);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset - new Vector2(0, 13),
                                                    new Vector2((float)Math.Sin(MathHelper.ToRadians(92.5f)), (float)Math.Cos(MathHelper.ToRadians(92.5f))),
                                                    true,
                                                    new Color(1.0f, 0.1f, 0.1f),
                                                    true);

                    shotTimer = minHardQuadShotTimer / overdriveFactor;
                }
                else
                {
                    shotTimer = minHardQuadShotTimer * 2.0f / overdriveFactor;
                }
            }
        }

        private void fireSpeederTrippleShot()
        {
            if (shotTimer <= 0.0f)
            {
                overheat += (OverheatSpeederTrippleShot);

                if (overheat < OVERHEAT_MAX)
                {
                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset,
                                                    new Vector2(1, 0),
                                                    true,
                                                    new Color(0.1f, 1.0f, 1.0f),
                                                    true);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset + new Vector2(0, 13),
                                                    new Vector2((float)Math.Sin(MathHelper.ToRadians(87.5f)), (float)Math.Cos(MathHelper.ToRadians(87.5f))),
                                                    true,
                                                    new Color(0.1f, 1.0f,1.0f),
                                                    true);

                    this.PlayerShotManager.FireShot(this.playerSprite.Location + gunOffset - new Vector2(0, 13),
                                                    new Vector2((float)Math.Sin(MathHelper.ToRadians(92.5f)), (float)Math.Cos(MathHelper.ToRadians(92.5f))),
                                                    true,
                                                    new Color(0.1f, 1.0f, 1.0f),
                                                    false);

                    shotTimer = minSpeederTrippleShotTimer / overdriveFactor;
                }
                else
                {
                    shotTimer = minSpeederTrippleShotTimer * 2.0f / overdriveFactor;
                }
            }
        }

        private void fireTankCarliRocket()
        {
            if (rocketTimer <= 0.0f &&
                overheat < (OVERHEAT_MAX - OverheatTankCarliRocket))
            {
                overheat += (OverheatTankCarliRocket);

                if (overheat < OVERHEAT_MAX)
                {
                    this.PlayerShotManager.FireRocket(this.playerSprite.Center,
                                                           new Vector2(1, 0),
                                                           true,
                                                           Color.White,
                                                           true);

                    rocketTimer = minTankCarliRocketTimer / overdriveFactor;
                }
            }
        }

        private void HandleTouchInput(TouchCollection touches, float elapsed)
        {
            bool fire = false;
            bool up = false;

            if (touches.Count == 1)
            {
                if (gameInput.IsPressed(ActionLeftSideScreen))
                {
                    if (settings.ControlPosition == SettingsManager.ControlPositionValues.Left)
                        up = true;
                    else
                        fire = true;
                }

                if (gameInput.IsPressed(ActionRightSideScreen))
                {
                    if (settings.ControlPosition == SettingsManager.ControlPositionValues.Right)
                        up = true;
                    else
                        fire = true;
                }
            }
            else if (touches.Count == 2)
            {
                if ((leftSideScreen.Contains(new Point((int)touches[0].Position.X, (int)touches[0].Position.Y)) &&
                    rightSideScreen.Contains(new Point((int)touches[1].Position.X, (int)touches[1].Position.Y))) ||
                    (leftSideScreen.Contains(new Point((int)touches[1].Position.X, (int)touches[1].Position.Y)) &&
                    rightSideScreen.Contains(new Point((int)touches[0].Position.X, (int)touches[0].Position.Y))))
                {
                    up = true;
                    fire = true;
                }
            }

            if (up)
            {
                playerSprite.Velocity -= 2 * gravity * elapsed;

                if (powerSoundTimer >= powerSoundMinTimer && powerSoundUnblocked)
                {
                    powerSoundTimer = 0.0f;
                    SoundManager.PlaySpaceshipPowerSound();
                    powerSoundUnblocked = false;
                }
            }
            else
            {
                powerSoundUnblocked = true;
            }

            if (fire && GameModeManager.SelectedGameMode != GameModeManager.GameMode.Survival)
            {
                if (shipType == PlayerType.GreenHornet)
                    fireGreenHornetShot();
                else if (shipType == PlayerType.Easy)
                    fireEasyShot();
                else if (shipType == PlayerType.Medium)
                    fireMediumDoubleShot();
                else if (shipType == PlayerType.Hard)
                    fireHardQuadShot();
                else if (shipType == PlayerType.Speeder)
                    fireSpeederTrippleShot();
                else if (shipType == PlayerType.Tank)
                    fireTankCarliRocket();
            }
        }

        private void HandleKeyboardInput(KeyboardState state, float elapsed)
        {
            #if DEBUG

            bool fire = false;
            bool up = false;

            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                up = true;
            }
            if (state.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                fire = true;
            }

            if (up)
            {
                playerSprite.Velocity -= 2 * gravity * elapsed;

                if (powerSoundTimer >= powerSoundMinTimer && powerSoundUnblocked)
                {
                    powerSoundTimer = 0.0f;
                    SoundManager.PlaySpaceshipPowerSound();
                    powerSoundUnblocked = false;
                }
            }
            else
            {
                powerSoundUnblocked = true;
            }

            if (fire && GameModeManager.SelectedGameMode != GameModeManager.GameMode.Survival)
            {
                if (shipType == PlayerType.GreenHornet)
                    fireGreenHornetShot();
                else if (shipType == PlayerType.Easy)
                    fireEasyShot();
                else if (shipType == PlayerType.Medium)
                    fireMediumDoubleShot();
                else if (shipType == PlayerType.Hard)
                    fireHardQuadShot();
                else if (shipType == PlayerType.Speeder)
                    fireSpeederTrippleShot();
                else if (shipType == PlayerType.Tank)
                    fireTankCarliRocket();
            }

#endif
        }

        private void checkMovementLimits()
        {
            Vector2 location = playerSprite.Location;
            bool hasCollided = false;

            if (location.Y < playerAreaLimit.Y - playerSprite.Source.Height / 2)
            {
                if (location.Y < playerAreaLimit.Y - playerSprite.Source.Height)
                {
                    location.Y = playerAreaLimit.Y - playerSprite.Source.Height;
                    playerSprite.Velocity = Vector2.Zero;
                }

                hasCollided = true;
            }

            if (location.Y > (playerAreaLimit.Bottom - playerSprite.Source.Height / 2))
            {
                if (location.Y > playerAreaLimit.Bottom)
                {
                    location.Y = playerAreaLimit.Bottom;
                    playerSprite.Velocity = Vector2.Zero;
                }

                hasCollided = true;
            }

            if (hasCollided)
            {
                if (wallgrindTimer >= wallgrindMinTimer)
                {
                    if (shieldPoints > 0)
                        shieldPoints -= OUT_OF_SCREEN_DAMANGE;
                    else
                        hitPoints -= OUT_OF_SCREEN_DAMANGE;

                    wallgrindTimer = 0.0f;
                    VibrationManager.Vibrate(0.02f);
                }
            }
            
            playerSprite.Location = location;

            // death by leaving the game region
            if (HitPoints <= 0)
            {
                EffectManager.AddLargeExplosion(playerSprite.Center, Vector2.Zero);
                VibrationManager.Vibrate(0.25f);
            }
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            PlayerShotManager.Update(gameTime);

            if (!IsDestroyed)
            {
                shotTimer -= elapsed;
                rocketTimer -= elapsed;
                wallgrindTimer += elapsed;
                powerSoundTimer += elapsed;

                HandleTouchInput(TouchPanel.GetState(), elapsed);
                HandleKeyboardInput(Keyboard.GetState(), elapsed);

                // go down (gravity)
                playerSprite.Velocity += gravity * elapsed;

                playerSprite.Update(gameTime);
                checkMovementLimits();

                if (Overheat > 0.95f)
                    Overheat -= CoolDownRate * elapsed * 0.5f * overdriveFactor;
                else if (Overheat > 0.9f)
                    Overheat -= CoolDownRate * elapsed * 0.6f * overdriveFactor;
                else if (Overheat > 0.85f)
                    Overheat -= CoolDownRate * elapsed * 0.7f * overdriveFactor;
                else if (Overheat > 0.80f)
                    Overheat -= CoolDownRate * elapsed * 0.8f * overdriveFactor;
                else if (Overheat > 0.75f)
                    Overheat -= CoolDownRate * elapsed * 0.9f * overdriveFactor;
                else
                    Overheat -= CoolDownRate * elapsed * overdriveFactor;

                if (HitPoints != 0.0f)
                {
                    this.IncreaseHitPoints(HealRate * elapsed);
                }

                if (this.overheat > 0.80f)
                {
                    this.DecreaseHitPoints(OverheatKillRateMax);
                    this.hitPoints = Math.Max(1.0f, this.hitPoints);
                }

                float factor = (float)Math.Max((this.hitPoints / 100.0f), 0.75f);
                this.playerSprite.TintColor = new Color(factor, factor, factor);

                // Shield
                if (IsShieldActive)
                {
                    shieldPoints -= elapsed * SHIELD_DECREASE_RATE;
                    shieldPoints = Math.Max(0, this.shieldPoints);

                    shieldSprite.Update(gameTime);
                    shieldSprite.Rotation += (float)Math.PI / 25;
                    shieldSprite.Rotation = (float)Math.Max(shieldSprite.Rotation, Math.PI * 2);
                }

                // Overdrive/Underdrive
                if (IsOverUnderdrive)
                {
                    overdriveTimer -= elapsed;

                    if (overdriveTimer <= 0.0f)
                    {
                        overdriveFactor = OVERDRIVE_NORMAL_FACTOR;
                    }
                }

                // score by distance
                distanceScore_buffer += elapsed;

                if (distanceScore_buffer >= SCORE_BUFFER_LIMIT)
                {
                    distanceScore_buffer = 0.0f;

                    distanceScore += SCORE_PER_INTERVAL;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PlayerShotManager.Draw(spriteBatch);

            if (!IsDestroyed)
            {
                if (IsOverdrive)
                {
                    playerSprite.TintColor = new Color(0.5f, 1.0f, 0.5f);
                }
                else if (IsUnderdrive)
                {
                    playerSprite.TintColor = new Color(1.0f, 0.5f, 0.5f);
                }
                
                playerSprite.Draw(spriteBatch);

                if (IsShieldActive)
                {
                    shieldSprite.TintColor = Color.Blue * (0.4f + 0.15f * shieldPoints / SHIELD_POINTS_MAX);
                    shieldSprite.Location = playerSprite.Location - new Vector2(10, 10);
                    shieldSprite.Draw(spriteBatch);
                }
            }
        }

        public void SetLevel(int lvl)
        {
            if (lvl != 1)
            {
                this.SpecialShotsRemaining += 1;
            }
        }

        public void StartOverdrive()
        {
            this.overdriveTimer = OVERDRIVE_DURATION;
            this.overdriveFactor = OVERDRIVE_FACTOR;
        }

        public void StartUnderdrive()
        {
            this.overdriveTimer = OVERDRIVE_DURATION;
            this.overdriveFactor = UNDERDRIVE_FACTOR;
        }

        public void ActivateShield()
        {
            this.shieldPoints = SHIELD_POINTS_MAX;
        }

        public void IncreasePlayerScore(long score)
        {
            this.collectScore += score;
        }

        public void SetHitPoints(float hp)
        {
            this.hitPoints = MathHelper.Clamp(hp, 0.0f, MaxHitPoints);
        }

        public void IncreaseHitPoints(float hp)
        {
            if (hp < 0)
                throw new ArgumentException("Negative values are not allowed.");

            this.hitPoints += hp;
            this.hitPoints = MathHelper.Clamp(hitPoints, 0.0f, MaxHitPoints);
        }

        public void DecreaseHitPoints(float hp)
        {
            if (hp < 0)
                throw new ArgumentException("Positive values are not allowed.");

            float diff = Math.Max(0, hp - this.shieldPoints);

            this.shieldPoints -= hp;
            this.shieldPoints = Math.Max(0, shieldPoints);

            this.hitPoints -= diff;
            this.hitPoints = MathHelper.Clamp(hitPoints, 0.0f, MaxHitPoints);
        }

        public void ResetPlayerScore()
        {
            this.collectScore = 0;
        }

        public void IncreaseDestroyedAsteroids()
        {
            destroyedAsteroids += 1;
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            //Player sprite
            //this.playerSprite.Location = new Vector2(Single.Parse(reader.ReadLine()),
            //                                         Single.Parse(reader.ReadLine()));
            //this.playerSprite.Rotation = Single.Parse(reader.ReadLine());
            //this.playerSprite.Velocity = new Vector2(Single.Parse(reader.ReadLine()),
            //                                         Single.Parse(reader.ReadLine()));
            playerSprite.Activated(reader);

            this.distanceScore = Int64.Parse(reader.ReadLine());
            this.distanceScore_buffer = Single.Parse(reader.ReadLine());

            this.collectScore = Int64.Parse(reader.ReadLine());

            this.destroyedAsteroids = Int64.Parse(reader.ReadLine());

            this.livesRemaining = Int32.Parse(reader.ReadLine());
            this.SpecialShotsRemaining = Int32.Parse(reader.ReadLine());

            this.shotPower = Single.Parse(reader.ReadLine());
            this.hitPoints = Single.Parse(reader.ReadLine());

            this.shotTimer = Single.Parse(reader.ReadLine());
            this.rocketTimer = Single.Parse(reader.ReadLine());

            PlayerShotManager.Activated(reader);

            this.overheat = Single.Parse(reader.ReadLine());

            this.shieldPoints = Single.Parse(reader.ReadLine());

            //Shield sprite
            this.shieldSprite.Location = new Vector2(Single.Parse(reader.ReadLine()),
                                                     Single.Parse(reader.ReadLine()));
            this.shieldSprite.Rotation = Single.Parse(reader.ReadLine());
            this.shieldSprite.Velocity = new Vector2(Single.Parse(reader.ReadLine()),
                                                     Single.Parse(reader.ReadLine()));

            this.overdriveTimer = Single.Parse(reader.ReadLine());
            this.overdriveFactor = Single.Parse(reader.ReadLine());

            this.shipType = (PlayerType)Enum.Parse(shipType.GetType(), reader.ReadLine(), false);

            this.wallgrindTimer = Single.Parse(reader.ReadLine());

            SelectPlayerType(this.shipType);
        }

        public void Deactivated(StreamWriter writer)
        {
            // Player sprite
            playerSprite.Deactivated(writer);

            writer.WriteLine(this.distanceScore);
            writer.WriteLine(this.distanceScore_buffer);

            writer.WriteLine(this.collectScore);

            writer.WriteLine(this.destroyedAsteroids);

            writer.WriteLine(this.livesRemaining);
            writer.WriteLine(this.SpecialShotsRemaining);

            writer.WriteLine(this.shotPower);
            writer.WriteLine(this.hitPoints);

            writer.WriteLine(this.shotTimer);
            writer.WriteLine(this.rocketTimer);

            PlayerShotManager.Deactivated(writer);

            writer.WriteLine(this.overheat);

            writer.WriteLine(this.shieldPoints);

            // Shield sprite
            writer.WriteLine(shieldSprite.Location.X);
            writer.WriteLine(shieldSprite.Location.Y);
            writer.WriteLine(shieldSprite.Rotation);
            writer.WriteLine(shieldSprite.Velocity.X);
            writer.WriteLine(shieldSprite.Velocity.Y);

            writer.WriteLine(this.overdriveTimer);
            writer.WriteLine(this.overdriveFactor);

            writer.WriteLine(this.shipType);

            writer.WriteLine(this.wallgrindTimer);
        }

        public long GetPlayerResultScore()
        {
            if (GameModeManager.SelectedGameMode == GameModeManager.GameMode.Survival)
                return distanceScore;
            else if (GameModeManager.SelectedGameMode == GameModeManager.GameMode.Destruction)
                return destroyedAsteroids;
            else
                return distanceScore + collectScore + destroyedAsteroids * SCORE_PER_ASTEROID;
        }

        #endregion

        #region Properties

        public int LivesRemaining
        {
            get
            {
                return this.livesRemaining;
            }
            set
            {
                this.livesRemaining = (int)MathHelper.Clamp(value, -1, MAX_PLAYER_LIVES);
            }
        }

        public float Overheat
        {
            get
            {
                return this.overheat;
            }
            set
            {
                this.overheat = MathHelper.Clamp(value, OVERHEAT_MIN, OVERHEAT_MAX);
            }
        }

        public float HitPoints
        {
            get
            {
                return this.hitPoints;
            }
        }

        public bool IsDestroyed
        {
            get
            {
                return this.hitPoints <= 0.0f;
            }
        }

        public float ShotPower
        {
            get
            {
                return this.shotPower;
            }
        }

        public bool IsOverUnderdrive
        {
            get
            {
                return overdriveFactor != OVERDRIVE_NORMAL_FACTOR;
            }
        }

        public bool IsOverdrive
        {
            get
            {
                return overdriveFactor == OVERDRIVE_FACTOR;
            }
        }

        public bool IsUnderdrive
        {
            get
            {
                return overdriveFactor == UNDERDRIVE_FACTOR;
            }
        }

        public float ShieldPoints
        {
            get
            {
                return this.shieldPoints;
            }
        }

        public bool IsShieldActive
        {
            get
            {
                return this.shieldPoints > 0.0f;
            }
        }

        public PlayerType ShipType
        {
            get
            {
                return shipType;
            }
        }

        public long DestroyedAsteroids
        {
            get
            {
                return destroyedAsteroids;
            }
        }

        public long DistanceScore
        {
            get
            {
                return distanceScore;
            }
        }

        public long CollectScore
        {
            get
            {
                return collectScore;
            }
        }

        #endregion

        #region Events

        #endregion
    }
}
