using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Media;

namespace AstropiXX
{
    public static class SoundManager
    {
        #region Members

        private static SettingsManager settings;

        private static List<SoundEffect> explosions = new List<SoundEffect>();
        private static int explosionsCount = 4;

        private static SoundEffect playerShot;

        private static SoundEffect coinSound;
        private static SoundEffect repairSound;
        private static SoundEffect coolWaterSound;
        private static SoundEffect extraShipSound;
        private static SoundEffect rocketSound;
        private static SoundEffect overheatSound;
        private static SoundEffect shieldSound;

        private static List<SoundEffect> hitSounds = new List<SoundEffect>();
        private static int hitSoundsCount = 6;

        private static List<SoundEffect> asteroidHitSounds = new List<SoundEffect>();
        private static int asteroidHitSoundsCount = 5;

        private static SoundEffect spaceshipPowerSound;

        private static Random rand = new Random();

        private static Song backgroundSound;

        // performance improvements
        public const float MinTimeBetweenHitSound = 0.2f;
        private static float hitTimer = 0.0f;
        private static float asteroidHitTimer = 0.0f;

        public const float MinTimeBetweenExplosionSound = 0.1f;
        private static float explosionTimer = 0.0f;

        #endregion

        #region Methods

        public static void Initialize(ContentManager content)
        {
            try
            {
                settings = SettingsManager.GetInstance();

                playerShot = content.Load<SoundEffect>(@"Sounds\Shot1");

                repairSound = content.Load<SoundEffect>(@"Sounds\Repair");
                coinSound = content.Load<SoundEffect>(@"Sounds\Coin");
                coolWaterSound = content.Load<SoundEffect>(@"Sounds\CoolWater");
                extraShipSound = content.Load<SoundEffect>(@"Sounds\Life");
                rocketSound = content.Load<SoundEffect>(@"Sounds\Rocket");
                overheatSound = content.Load<SoundEffect>(@"Sounds\Overheat");
                shieldSound = content.Load<SoundEffect>(@"Sounds\Shield");

                for (int x = 1; x <= explosionsCount; x++)
                {
                    explosions.Add(content.Load<SoundEffect>(@"Sounds\Explosion"
                                                             + x.ToString()));
                }

                for (int x = 1; x <= hitSoundsCount; x++)
                {
                    hitSounds.Add(content.Load<SoundEffect>(@"Sounds\Hit"
                                                             + x.ToString()));
                }

                for (int x = 1; x <= asteroidHitSoundsCount; x++)
                {
                    asteroidHitSounds.Add(content.Load<SoundEffect>(@"Sounds\AsteroidHit"
                                                             + x.ToString()));
                }

                spaceshipPowerSound = content.Load<SoundEffect>(@"Sounds\spaceshippower");

                backgroundSound = content.Load<Song>(@"Sounds\GameSound");
            }
            catch
            {
                Debug.WriteLine("SoundManager: Content not found.");
            }
        }

        public static void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SoundManager.hitTimer += elapsed;
            SoundManager.asteroidHitTimer += elapsed;
            SoundManager.explosionTimer += elapsed;
        }

        public static void PlayExplosion()
        {
            if (SoundManager.explosionTimer > SoundManager.MinTimeBetweenExplosionSound)
            {
                try
                {
                    SoundEffectInstance s = explosions[rand.Next(0, explosionsCount)].CreateInstance();
                    s.Volume = settings.GetSfxValue();
                    s.Play();
                }
                catch
                {
                    Debug.WriteLine("SoundManager: Play explosion failed.");
                }

                SoundManager.explosionTimer = 0.0f;
            }
        }

        public static void PlayPlayerShot()
        {
            try
            {
                SoundEffectInstance s = playerShot.CreateInstance();
                s.Volume = 0.75f * settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play player shot failed.");
            }
        }

        public static void PlayCoinSound()
        {
            try
            {
                SoundEffectInstance s = coinSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play enemy shot failed.");
            }
        }

        public static void PlayRepairSound()
        {
            try
            {
                SoundEffectInstance s = repairSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play enemy shot failed.");
            }
        }

        public static void PlayCoolWaterSound()
        {
            try
            {
                SoundEffectInstance s = coolWaterSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play enemy shot failed.");
            }
        }

        public static void PlayOverdriveSound()
        {
            try
            {
                SoundEffectInstance s = extraShipSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play enemy shot failed.");
            }
        }

        public static void PlayHitSound()
        {
            if (SoundManager.hitTimer > SoundManager.MinTimeBetweenHitSound)
            {
                try
                {
                    SoundEffectInstance s = hitSounds[rand.Next(0, hitSoundsCount)].CreateInstance();
                    s.Volume = settings.GetSfxValue();
                    s.Play();
                }
                catch
                {
                    Debug.WriteLine("SoundManager: Play explosion failed.");
                }

                SoundManager.hitTimer = 0.0f;
            }
        }

        public static void PlayAsteroidHitSound()
        {
            if (SoundManager.asteroidHitTimer > SoundManager.MinTimeBetweenHitSound)
            {
                try
                {
                    SoundEffectInstance s = asteroidHitSounds[rand.Next(0, asteroidHitSoundsCount)].CreateInstance();
                    s.Volume = settings.GetSfxValue();
                    s.Play();
                }
                catch
                {
                    Debug.WriteLine("SoundManager: Play explosion failed.");
                }

                SoundManager.asteroidHitTimer = 0.0f;
            }
        }

        public static void PlayRocketSound()
        {
            try
            {
                SoundEffectInstance s = rocketSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play explosion failed.");
            }
        }

        public static void PlayOverheatSound()
        {
            try
            {
                SoundEffectInstance s = overheatSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play overhet sound failed.");
            }
        }

        public static void PlayShieldSound()
        {
            try
            {
                SoundEffectInstance s = shieldSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play explosion failed.");
            }
        }

        public static void PlaySpaceshipPowerSound()
        {
            try
            {
                SoundEffectInstance s = spaceshipPowerSound.CreateInstance();
                s.Volume = settings.GetSfxValue();
                s.Play();
            }
            catch
            {
                Debug.WriteLine("SoundManager: Play spaceship power sound failed.");
            }
        }

        public static void PlayBackgroundSound()
        {
            try
            {
                if (MediaPlayer.GameHasControl)
                {
                    MediaPlayer.Play(backgroundSound);
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Volume = settings.GetMusicValue();
                }
            }
            catch (UnauthorizedAccessException)
            {
                // play no music...
            }
            catch (InvalidOperationException)
            {
                // play no music (because of Zune on PC)
            }
        }

        public static void RefreshMusicVolume()
        {
            float val = settings.GetMusicValue();
            MediaPlayer.Volume = val;
        }

        #endregion
    }
}
