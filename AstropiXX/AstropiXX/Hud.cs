using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AstropiXX.Extensions;

namespace AstropiXX
{
    class Hud
    {
        #region Members

        private static Hud hud;

        private long score;
        private int remainingLives;
        private float overHeat;
        private float hitPoints;
        private float shieldPoints;
        private int level;

        private Rectangle screenBounds;
        private Texture2D texture;
        private SpriteFont font;

        private Vector2 scoreLeftLocation = new Vector2(672, -4);
        private Vector2 hitPointLocation = new Vector2(645, 30);
        private Vector2 shieldPointLocation = new Vector2(645, 50);
        private Vector2 overheatLocation = new Vector2(645, 70);

        private readonly Rectangle hitPointSymbolSoruce = new Rectangle(0, 320, 14, 14);
        private readonly Rectangle shieldPointSymbolSoruce = new Rectangle(14, 320, 14, 14);
        private readonly Rectangle overheatSymbolSoruce = new Rectangle(28, 320, 14, 14);

        private Vector2 hitPointSymbolLocation = new Vector2(625, 28);
        private Vector2 shieldPointSymbolLocation = new Vector2(625, 48);
        private Vector2 overheatSymbolLocation = new Vector2(625, 68);

        private Vector2 bossHitPointLocation = new Vector2(345, 40);
        private readonly Rectangle bossHitPointSymbolSoruce = new Rectangle(0, 320, 14, 14);
        private Vector2 bossHitPointSymbolLocation = new Vector2(325, 38);

        Vector2 barOverlayStart = new Vector2(0, 350);

        private int lastLevel = -1;
        private StringBuilder currentLevelText = new StringBuilder(16);
        private const string LEVEL_PRE_TEXT = "Level: ";

        #endregion

        #region Constructors

        private Hud(Rectangle screen, Texture2D texture, SpriteFont font,
                    long score, int lives, float overHeat, float hitPoints, float shieldPoints,
                    int level)
        {
            this.screenBounds = screen;
            this.texture = texture;
            this.font = font;
            this.score = score;
            this.remainingLives = lives;
            this.overHeat = overHeat;
            this.hitPoints = hitPoints;
            this.shieldPoints = shieldPoints;
            this.level = level;
        }

        #endregion

        #region Methods

        public static Hud GetInstance(Rectangle screen, Texture2D texture, SpriteFont font, long score,
                                      int lives, float overHeat, float hitPoints, float shieldPoints,
                                      int level)
        {
            if (hud == null)
            {
                hud = new Hud(screen,
                              texture,
                              font,
                              score,
                              lives,
                              overHeat,
                              hitPoints,
                              shieldPoints,
                              level);
            }

            return hud;
        }

        public void Update(long score, int lives, float overHeat, float hitPoints, float shieldPoints,
                           int level)
        {
            this.score = score;
            this.remainingLives = lives;
            this.overHeat = overHeat;
            this.hitPoints = hitPoints;
            this.shieldPoints = shieldPoints;
            this.level = level;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            drawScore(spriteBatch);

            if (remainingLives >= 0)
            {
                drawLevel(spriteBatch);
                drawHitPoints(spriteBatch);
                drawShieldPoints(spriteBatch);

                if (GameModeManager.SelectedGameMode != GameModeManager.GameMode.Survival)
                {
                    drawOverheat(spriteBatch);
                }
            }       
        }

        private void drawScore(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawInt64WithZeros(font,
                                  score,
                                  scoreLeftLocation,
                                  Color.White * 0.8f,
                                  11);
        }

        private void drawOverheat(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             overheatSymbolLocation,
                             overheatSymbolSoruce,
                             Color.Red * 0.8f);

            spriteBatch.Draw(texture,
                    new Rectangle(
                        (int)overheatLocation.X,
                        (int)overheatLocation.Y,
                        150,
                        10),
                    new Rectangle(
                        (int)barOverlayStart.X,
                        (int)barOverlayStart.Y,
                        150,
                        10),
                    Color.Red * 0.2f);

            spriteBatch.Draw(texture,
                    new Rectangle(
                        (int)overheatLocation.X,
                        (int)overheatLocation.Y,
                        (int)(150 * overHeat),
                        10),
                    new Rectangle(
                        (int)barOverlayStart.X,
                        (int)barOverlayStart.Y,
                        (int)(150 * overHeat),
                        10),
                    Color.Red * 0.5f);
        }

        private void drawHitPoints(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             hitPointSymbolLocation,
                             hitPointSymbolSoruce,
                             Color.Green * 0.8f);

            spriteBatch.Draw(texture,
                    new Rectangle(
                        (int)hitPointLocation.X,
                        (int)hitPointLocation.Y,
                        150,
                        10),
                    new Rectangle(
                        (int)barOverlayStart.X,
                        (int)barOverlayStart.Y,
                        150,
                        10),
                    Color.Green * 0.2f);

            spriteBatch.Draw(texture,
                    new Rectangle(
                        (int)hitPointLocation.X,
                        (int)hitPointLocation.Y,
                        (int)(1.5f * hitPoints),
                        10),
                    new Rectangle(
                        (int)barOverlayStart.X,
                        (int)barOverlayStart.Y,
                        (int)(1.5f * hitPoints),
                        10),
                    Color.Green * 0.5f);
        }

        private void drawShieldPoints(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture,
                             shieldPointSymbolLocation,
                             shieldPointSymbolSoruce,
                             Astropixx.ThemeColor * 0.8f);

            spriteBatch.Draw(texture,
                    new Rectangle(
                        (int)shieldPointLocation.X,
                        (int)shieldPointLocation.Y,
                        150,
                        10),
                    new Rectangle(
                        (int)barOverlayStart.X,
                        (int)barOverlayStart.Y,
                        150,
                        10),
                    Astropixx.ThemeColor * 0.2f);

            spriteBatch.Draw(texture,
                    new Rectangle(
                        (int)shieldPointLocation.X,
                        (int)shieldPointLocation.Y,
                        (int)(1.5f * shieldPoints),
                        10),
                    new Rectangle(
                        (int)barOverlayStart.X,
                        (int)barOverlayStart.Y,
                        (int)(1.5f * shieldPoints),
                        10),
                    Astropixx.ThemeColor * 0.5f);
        }

        private void drawLevel(SpriteBatch spriteBatch)
        {
            if (lastLevel != level || currentLevelText.Length == 0)
            {
                if (currentLevelText.Length != 0)
                    currentLevelText.Clear();

                lastLevel = level;

                currentLevelText.Append(LEVEL_PRE_TEXT)
                                .Append(level);
            }


            spriteBatch.DrawString(font,
                                   currentLevelText,
                                   new Vector2(screenBounds.Width / 2 - (font.MeasureString(currentLevelText).Y / 2),
                                               5),
                                   Color.White * 0.8f);
        }

        #endregion
    }
}
