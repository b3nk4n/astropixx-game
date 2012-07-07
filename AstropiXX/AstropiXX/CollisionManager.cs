using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Devices;

namespace AstropiXX
{
    class CollisionManager
    {
        #region Members

        private AsteroidManager asteroidManager;
        private PlayerManager playerManager;
        private PowerUpManager powerUpManager;
        private Vector2 offScreen = new Vector2(-500, -500);
        private Vector2 shotToAsteroidImpact = new Vector2(0, -20);

        Random rand = new Random();

        private const string INFO_HEALTH25 = "+25% Repair-Kit";
        private const string INFO_HEALTH50 = "+50% Repair-Kit";
        private const string INFO_COOLINGWATER = "Cooling Water";
        private const string INFO_OVERHEAT = "Overheating Problem!";
        private const string INFO_SHIELDS = "Activated Shields";
        private const string INFO_OVERDRIVE = "Overdrive!";
        private const string INFO_UNDERDRIVE = "Underdrive!";

        #endregion

        #region Constructors

        public CollisionManager(AsteroidManager asteroidManager, PlayerManager playerManager,
                                PowerUpManager powerUpManager)
        {
            this.asteroidManager = asteroidManager;
            this.playerManager = playerManager;
            this.powerUpManager = powerUpManager;
        }

        #endregion

        #region Methods

        private void checkRocketToAsteroidCollisions()
        {
            foreach (var rocket in playerManager.PlayerShotManager.Rockets)
            {
                foreach (var asteroid in asteroidManager.Asteroids)
                {
                    if (rocket.IsCircleColliding(asteroid.Center,
                                               asteroid.CollisionRadius))
                    {
                        EffectManager.AddRocketExplosion(asteroid.Center,
                                                         asteroid.Velocity / 10);

                        

                        powerUpManager.ProbablySpawnPowerUp(asteroid.Center);

                        playerManager.IncreaseDestroyedAsteroids();

                        if ((rocket.Center - playerManager.playerSprite.Center).Length() < 30)
                        {
                            playerManager.DecreaseHitPoints(3.0f);

                            if (playerManager.IsDestroyed)
                            {
                                EffectManager.AddLargeExplosion(playerManager.playerSprite.Center,
                                                                playerManager.playerSprite.Velocity / 10);

                                VibrationManager.Vibrate(0.5f);
                            }
                            else
                            {
                                EffectManager.AddExplosion(playerManager.playerSprite.Center,
                                                           playerManager.playerSprite.Velocity / 10);

                                VibrationManager.Vibrate(0.2f);
                            }
                        }
                        
                        rocket.Location = offScreen;
                        asteroid.Location = offScreen;

                        break; // skip other checks, because they are not neccessary
                    }
                }
            }
        }

        private void checkPlayerShotToAsteroidCollisions()
        {
            foreach (var asteroid in asteroidManager.Asteroids)
            {
                foreach (var shot in playerManager.PlayerShotManager.Shots)
                {
                    if (shot.IsCircleColliding(asteroid.Center,
                                               asteroid.CollisionRadius))
                    {
                        EffectManager.AddLargeSparksEffect(shot.Location,
                                                      shot.Velocity,
                                                      asteroid.Velocity,
                                                      Color.Gray);
                        shot.Location = offScreen;
                        Vector2 direction = shot.Velocity;
                        direction.Normalize();
                        direction *= 40;
                        asteroid.Velocity += direction;

                        if (asteroid.Velocity.X > -150)
                        {
                            EffectManager.AddAsteroidExplosion(asteroid.Center,
                                                   asteroid.Velocity / 10);
                            
                            powerUpManager.ProbablySpawnPowerUp(asteroid.Center);

                            playerManager.IncreaseDestroyedAsteroids();
                            
                            asteroid.Location = offScreen;
                        }

                        break; // skip other checks, because they are not neccessary
                    }
                }
            }
        }

        private void checkAsteroidToPlayerCollisions()
        {
            foreach (var asteroid in asteroidManager.Asteroids)
            {
                if (asteroid.IsCircleColliding(playerManager.playerSprite.Center,
                                               playerManager.playerSprite.CollisionRadius))
                {
                    EffectManager.AddAsteroidExplosion(asteroid.Center,
                                                   asteroid.Velocity / 10);

                    playerManager.DecreaseHitPoints((int)(asteroid.Velocity.Length() / 8));

                    if (playerManager.IsDestroyed)
                    {
                        EffectManager.AddLargeExplosion(playerManager.playerSprite.Center,
                                                        playerManager.playerSprite.Velocity / 10);

                        VibrationManager.Vibrate(0.5f);
                    }
                    else
                    {
                        EffectManager.AddExplosion(playerManager.playerSprite.Center,
                                                   playerManager.playerSprite.Velocity / 10);

                        VibrationManager.Vibrate(0.2f);
                    }
                    
                    asteroid.Location = offScreen;
                }
            }
        }

        private void checkPowerUpToPlayerCollision()
        {
            foreach (var powerUp in powerUpManager.PowerUps)
            {
                if (powerUp.isCircleColliding(playerManager.playerSprite.Center, playerManager.playerSprite.CollisionRadius))
                {
                    if (powerUp.Type == PowerUp.PowerUpType.Random)
                        powerUp.Type = powerUpManager.GetPowerUpNotRandom();

                    // Activate power-up
                    switch (powerUp.Type)
                    {
                        case PowerUp.PowerUpType.Health25:
                            playerManager.IncreaseHitPoints(25.0f);
                            SoundManager.PlayRepairSound();
                            ZoomTextManager.ShowInfo(INFO_HEALTH25);
                            break;

                        case PowerUp.PowerUpType.Health50:
                            playerManager.IncreaseHitPoints(50.0f);
                            ZoomTextManager.ShowInfo(INFO_HEALTH50);
                            SoundManager.PlayRepairSound();
                            break;

                        case PowerUp.PowerUpType.CoolWater:
                            playerManager.Overheat = 0.0f;
                            SoundManager.PlayCoolWaterSound();
                            ZoomTextManager.ShowInfo(INFO_COOLINGWATER);
                            break;

                        case PowerUp.PowerUpType.LowBonusScore:
                            playerManager.IncreasePlayerScore(1000);
                            SoundManager.PlayCoinSound();
                            break;

                        case PowerUp.PowerUpType.MediumBonusScore:
                            playerManager.IncreasePlayerScore(2000);
                            SoundManager.PlayCoinSound();
                            break;

                        case PowerUp.PowerUpType.HighBonusScore:
                            playerManager.IncreasePlayerScore(3000);
                            SoundManager.PlayCoinSound();
                            break;

                        case PowerUp.PowerUpType.OverHeat:
                            playerManager.Overheat = PlayerManager.OVERHEAT_MAX;
                            SoundManager.PlayOverheatSound();
                            ZoomTextManager.ShowInfo(INFO_OVERHEAT);
                            break;

                        case PowerUp.PowerUpType.Shield:
                            playerManager.ActivateShield();
                            SoundManager.PlayShieldSound();
                            ZoomTextManager.ShowInfo(INFO_SHIELDS);
                            break;

                        case PowerUp.PowerUpType.Overdrive:
                            playerManager.StartOverdrive();
                            SoundManager.PlayOverdriveSound();
                            ZoomTextManager.ShowInfo(INFO_OVERDRIVE);
                            break;

                        case PowerUp.PowerUpType.Underdrive:
                            playerManager.StartUnderdrive();
                            SoundManager.PlayOverheatSound();
                            ZoomTextManager.ShowInfo(INFO_UNDERDRIVE);
                            break;
                    }

                    powerUp.IsActive = false;
                }
            }
        }

        public void Update()
        {
            checkPlayerShotToAsteroidCollisions();
            checkRocketToAsteroidCollisions();

            if (!playerManager.IsDestroyed)
            {
                checkAsteroidToPlayerCollisions();
                checkPowerUpToPlayerCollision();
            }
        }

        #endregion
    }
}
