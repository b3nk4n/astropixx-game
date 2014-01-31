using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Phone.Tasks;
using AstropiXX.Inputs;

namespace AstropiXX
{
    class HighscoreManager
    {
        #region Members

        LeaderboardManager leaderboardManagerSurvival;
        LeaderboardManager leaderboardManagerDestruction;
        LeaderboardManager leaderboardManagerCombo;

        public enum ScoreState { Local, OnlineAll, OnlineWeek, OnlineMe, OnlineDay, OnlineMonth, MostAddictive };
        public enum HighscoresDisplayState { Selection, Survival, Destruction, Combo };

        private ScoreState scoreState = ScoreState.Local;
        private HighscoresDisplayState displayState = HighscoresDisplayState.Selection;

        private readonly Rectangle GameModeSurvivalSource = new Rectangle(0, 1300,
                                                                       300, 50);
        private readonly Rectangle GameModeDestructionSource = new Rectangle(0, 1350,
                                                                       300, 50);
        private readonly Rectangle GameModeComboSource = new Rectangle(0, 1400,
                                                                       300, 50);

        private readonly Rectangle GameModeSurvivalDestination = new Rectangle(275, 220,
                                                                        250, 42);
        private readonly Rectangle GameModeDestructionDestination = new Rectangle(275, 300,
                                                                        250, 42);
        private readonly Rectangle GameModeComboDestination = new Rectangle(275, 380,
                                                                        250, 42);

        private readonly Rectangle GameModeTitleDestination = new Rectangle(310, 5,
                                                                            180, 30);

        private readonly Rectangle switcherSource = new Rectangle(400, 350, 
                                                                  100, 100);
        private readonly Rectangle switcherRightDestination = new Rectangle(700, 0,
                                                                            100, 100);

        private readonly Rectangle switcherLeftDestination = new Rectangle(0, 0,
                                                                           100, 100);

        private readonly Rectangle browserSource = new Rectangle(400, 700,
                                                                 100, 100);
        private readonly Rectangle browserDestination = new Rectangle(700, 190,
                                                                      100, 100);

        private readonly Rectangle refreshSource = new Rectangle(400, 450,
                                                                 100, 100);
        private readonly Rectangle refreshDestination = new Rectangle(700, 380,
                                                                      100, 100);

        private readonly Rectangle resubmitSource = new Rectangle(300, 700,
                                                                 100, 100);
        private readonly Rectangle resubmitDestination = new Rectangle(700, 380,
                                                                      100, 100);

        private Rectangle leaderboardsSource = new Rectangle(0, 250,
                                                           300, 50);
        private Rectangle leaderboardsDestination = new Rectangle(250, 100,
                                                                300, 50);

        private static HighscoreManager highscoreManager;

        private long currentHighScoreSurvival;
        private long currentHighScoreDestruction;
        private long currentHighScoreCombo;

        private List<Highscore> localTopScoresSurvival = new List<Highscore>(16);
        private List<Highscore> localTopScoresDestruction = new List<Highscore>(16);
        private List<Highscore> localTopScoresCombo = new List<Highscore>(16);

        public const int MaxScores = 10;

        public static Texture2D Texture;
        public static SpriteFont Font;
        private readonly Rectangle LocalTitleSource = new Rectangle(0, 450,
                                                                        300, 50);
        private readonly Rectangle OnlineAllTitleSource = new Rectangle(0, 500,
                                                                        300, 50);
        private readonly Rectangle OnlineMonthTitleSource = new Rectangle(0, 1000,
                                                                        300, 50);
        private readonly Rectangle OnlineWeekTitleSource = new Rectangle(0, 550,
                                                                        300, 50);
        private readonly Rectangle OnlineDayTitleSource = new Rectangle(0, 950,
                                                                        300, 50);
        private readonly Rectangle MostAddictiveTitleSource = new Rectangle(0, 1050,
                                                                        300, 50);
        private readonly Rectangle OnlineMeTitleSource = new Rectangle(0, 800,
                                                                        300, 50);
        private readonly Vector2 TitlePosition = new Vector2(250.0f, 30.0f);

        private string lastName = "Unknown";

        private float opacity = 0.0f;
        private const float OpacityMax = 1.0f;
        private const float OpacityMin = 0.0f;
        private const float OpacityChangeRate = 0.05f;

        private bool isActive = false;

        private WebBrowserTask browser;
        private const string BROWSER_URL = "http://bsautermeister.de/astropixx/requestscores.php?Method=TOP100WEB";
        private const string BROWSER_URL_SURVIVAL = BROWSER_URL + "&GameMode=survival";
        private const string BROWSER_URL_DESTRUCTION = BROWSER_URL + "&GameMode=destruction";
        private const string BROWSER_URL_COMBO = BROWSER_URL + "&GameMode=combo";

        private const string TEXT_ME = "Your best personal online achievements:";
        private const string TEXT_RANK = "Best Rank:";
        private const string TEXT_SCORE = "Best Score:";
        private const string TEXT_LEVEL = "Best Level:";
        private const string TEXT_TOTAL_SCORE = "Addiction Score:";
        private const string TEXT_TOTAL_LEVEL = "Addiction Level:";

        public static GameInput GameInput;
        private const string RefreshAction = "Refresh";
        private const string GoLeftAction = "GoLeft";
        private const string GoRightAction = "GoRight";
        private const string BrowserAction = "Browser";
        private const string ResubmitAction = "Resubmit";
        private const string SurvivalAction = "SurvivalHighscoreAction";
        private const string DestructionAction = "DestructionHighscoreAction";
        private const string ComboAction = "ComboHighscoreAction";

        private float switchPageTimer = 0.0f;
        private const float SwitchPageMinTimer = 0.25f;

        private const int NamePositionX = 110;
        private const int ScorePositionX = 410;
        private const int LevelPositionX = 600;

        private long totalDestroyedAsteroids = 0;
        private long totalCollectScore = 0;
        private long totalDistanceScore = 0;

        private const string SCORE_FILE_SURVIVAL = "survival_local_scores.txt";
        private const string SCORE_FILE_DESTRUCTION = "destruction_local_scores.txt";
        private const string SCORE_FILE_COMBO = "combo_local_scores.txt";

        private const string USERDATA_FILE = "userdata.txt";

        private const string LEADERBOARDNAME_SURVIVAL = "survival";
        private const string LEADERBOARDNAME_DESTRUCTION = "destruction";
        private const string LEADERBOARDNAME_COMBO = "combo";

        private const string DOTS3 = ". . . ";
        private const string DOTS6 = ". . . . . . ";
        private const string DOTS12 = ". . . . . . . . . . . . ";
        private const string DOTS21 = ". . . . . . . . . . . . . . . . . . . . . ";

        #endregion

        #region Constructors

        private HighscoreManager()
        {
            leaderboardManagerSurvival = new LeaderboardManager(LEADERBOARDNAME_SURVIVAL);
            leaderboardManagerDestruction = new LeaderboardManager(LEADERBOARDNAME_DESTRUCTION);
            leaderboardManagerCombo = new LeaderboardManager(LEADERBOARDNAME_COMBO);

            browser = new WebBrowserTask();

            this.LoadAllHighscores();

            this.loadUserData();
        }

        #endregion

        #region Methods

        public void SetupInputs()
        {
            GameInput.AddTouchGestureInput(RefreshAction,
                                           GestureType.Tap,
                                           refreshDestination);
            GameInput.AddTouchGestureInput(GoLeftAction,
                                           GestureType.Tap,
                                           switcherLeftDestination);
            GameInput.AddTouchGestureInput(GoRightAction,
                                           GestureType.Tap,
                                           switcherRightDestination);
            GameInput.AddTouchGestureInput(BrowserAction,
                                           GestureType.Tap,
                                           browserDestination);
            GameInput.AddTouchGestureInput(ResubmitAction,
                                           GestureType.Tap,
                                           resubmitDestination);

            GameInput.AddTouchGestureInput(SurvivalAction,
                                           GestureType.Tap,
                                           GameModeSurvivalDestination);
            GameInput.AddTouchGestureInput(DestructionAction,
                                           GestureType.Tap,
                                           GameModeDestructionDestination);
            GameInput.AddTouchGestureInput(ComboAction,
                                           GestureType.Tap,
                                           GameModeComboDestination);

            GameInput.AddTouchSlideInput(GoLeftAction,
                                         Input.Direction.Right,
                                         40.0f);
            GameInput.AddTouchSlideInput(GoRightAction,
                                         Input.Direction.Left,
                                         40.0f);
        }

        public static HighscoreManager GetInstance()
        {
            if (highscoreManager == null)
            {
                highscoreManager = new HighscoreManager();
            }

            return highscoreManager;
        }

        public void ChangeDisplayState(HighscoresDisplayState state)
        {
            displayState = state;
            this.opacity = 0.0f;
        }

        private void handleTouchInputs()
        {
            if (displayState == HighscoresDisplayState.Selection)
            {
                if (GameInput.IsPressed(SurvivalAction))
                {
                    ChangeDisplayState(HighscoresDisplayState.Survival);
                    leaderboardManagerSurvival.Receive();
                }
                else if (GameInput.IsPressed(DestructionAction))
                {
                    ChangeDisplayState(HighscoresDisplayState.Destruction);
                    leaderboardManagerDestruction.Receive();
                }
                else if (GameInput.IsPressed(ComboAction))
                {
                    ChangeDisplayState(HighscoresDisplayState.Combo);
                    leaderboardManagerCombo.Receive();
                }
            }
            else
            {
                // Switcher right
                if (GameInput.IsPressed(GoRightAction) && switchPageTimer > SwitchPageMinTimer)
                {
                    switchPageTimer = 0.0f;

                    if (scoreState == ScoreState.Local)
                        scoreState = ScoreState.OnlineAll;
                    else if (scoreState == ScoreState.OnlineAll)
                        scoreState = ScoreState.OnlineMonth;
                    else if (scoreState == ScoreState.OnlineMonth)
                        scoreState = ScoreState.OnlineWeek;
                    else if (scoreState == ScoreState.OnlineWeek)
                        scoreState = ScoreState.OnlineDay;
                    else if (scoreState == ScoreState.OnlineDay)
                        scoreState = ScoreState.OnlineMe;
                    else if (scoreState == ScoreState.OnlineMe)
                        scoreState = ScoreState.MostAddictive;
                    else
                        scoreState = ScoreState.Local;
                }
                // Switcher left
                if (GameInput.IsPressed(GoLeftAction) && switchPageTimer > SwitchPageMinTimer)
                {
                    switchPageTimer = 0.0f;

                    if (scoreState == ScoreState.Local)
                        scoreState = ScoreState.MostAddictive;
                    else if (scoreState == ScoreState.MostAddictive)
                        scoreState = ScoreState.OnlineMe;
                    else if (scoreState == ScoreState.OnlineMe)
                        scoreState = ScoreState.OnlineDay;
                    else if (scoreState == ScoreState.OnlineDay)
                        scoreState = ScoreState.OnlineWeek;
                    else if (scoreState == ScoreState.OnlineWeek)
                        scoreState = ScoreState.OnlineMonth;
                    else if (scoreState == ScoreState.OnlineMonth)
                        scoreState = ScoreState.OnlineAll;
                    else
                        scoreState = ScoreState.Local;
                }
                // Resubmit
                if (GameInput.IsPressed(ResubmitAction))
                {
                    if (scoreState == ScoreState.Local)
                    {
                        resubmitCurrentLocalTopScore();
                    }
                }
                // Browser - Top100
                if (GameInput.IsPressed(BrowserAction))
                {
                    if (scoreState != ScoreState.Local)
                    {
                        if (displayState == HighscoresDisplayState.Survival)
                            browser.Uri = new Uri(BROWSER_URL_SURVIVAL);
                        if (displayState == HighscoresDisplayState.Destruction)
                            browser.Uri = new Uri(BROWSER_URL_DESTRUCTION);
                        if (displayState == HighscoresDisplayState.Combo)
                            browser.Uri = new Uri(BROWSER_URL_COMBO);
                        browser.Show();
                    }
                }
                // Refresh
                if (GameInput.IsPressed(RefreshAction))
                {
                    if (scoreState != ScoreState.Local)
                    {
                        refreshCurrentLeaderboard();
                    }
                }
            }
        }

        public void SubmitScore(string method, string name, long score, int level, GameModeManager.GameMode gameMode)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
            {
                leaderboardManagerSurvival.Submit(method, name, score, level);
            }
            else if (gameMode == GameModeManager.GameMode.Destruction)
            {
                leaderboardManagerDestruction.Submit(method, name, score, level);
            }
            else
            {
                leaderboardManagerCombo.Submit(method, name, score, level);
            }
        }

        private void refreshCurrentLeaderboard()
        {
            if (displayState == HighscoresDisplayState.Survival)
                leaderboardManagerSurvival.Receive();
            else if (displayState == HighscoresDisplayState.Destruction)
                leaderboardManagerDestruction.Receive();
            else if (displayState == HighscoresDisplayState.Combo)
                leaderboardManagerCombo.Receive(); 
        }

        private void resubmitCurrentLocalTopScore()
        {
            if (displayState == HighscoresDisplayState.Survival)
                if (localTopScoresSurvival.Count > 0 && localTopScoresSurvival[0].Score > 0)
                    leaderboardManagerSurvival.Submit(LeaderboardManager.RESUBMIT,                                        
                                                      localTopScoresSurvival[0].Name,
                                                      localTopScoresSurvival[0].Score,
                                                      localTopScoresSurvival[0].Level);
            else if (displayState == HighscoresDisplayState.Destruction)
                if (localTopScoresDestruction.Count > 0 && localTopScoresDestruction[0].Score > 0)
                    leaderboardManagerDestruction.Submit(LeaderboardManager.RESUBMIT,
                                                         localTopScoresDestruction[0].Name,
                                                         localTopScoresDestruction[0].Score,
                                                         localTopScoresDestruction[0].Level);
            else if (displayState == HighscoresDisplayState.Combo)
                    if (localTopScoresCombo.Count > 0 && localTopScoresCombo[0].Score > 0)
                    leaderboardManagerCombo.Submit(LeaderboardManager.RESUBMIT,
                                                   localTopScoresCombo[0].Name,
                                                   localTopScoresCombo[0].Score,
                                                   localTopScoresCombo[0].Level);
        }

        public string GetStatusText(GameModeManager.GameMode gameMode)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
                return leaderboardManagerSurvival.StatusText;
            else if (gameMode == GameModeManager.GameMode.Destruction)
                return leaderboardManagerDestruction.StatusText;
            else 
                return leaderboardManagerCombo.StatusText;
        }

        public void SetStatusText(GameModeManager.GameMode gameMode, string text)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
                leaderboardManagerSurvival.StatusText = text;
            else if (gameMode == GameModeManager.GameMode.Destruction)
                leaderboardManagerDestruction.StatusText = text;
            else
                leaderboardManagerCombo.StatusText = text;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isActive)
            {
                switchPageTimer += elapsed;

                if (this.opacity < OpacityMax)
                    this.opacity += OpacityChangeRate;
            }

            handleTouchInputs();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (displayState == HighscoresDisplayState.Selection)
            {
                drawLeaderboardsSelection(spriteBatch);
            }
            else
            {
                if (displayState == HighscoresDisplayState.Survival)
                {
                    spriteBatch.Draw(Texture,
                                     GameModeTitleDestination,
                                     GameModeSurvivalSource,
                                     Color.White * opacity);

                    drawLeaderboards(spriteBatch, localTopScoresSurvival, leaderboardManagerSurvival);
                }
                else if (displayState == HighscoresDisplayState.Destruction)
                {
                    spriteBatch.Draw(Texture,
                                     GameModeTitleDestination,
                                     GameModeDestructionSource,
                                     Color.White * opacity);

                    drawLeaderboards(spriteBatch, localTopScoresDestruction, leaderboardManagerDestruction);
                }
                else if (displayState == HighscoresDisplayState.Combo)
                {
                    spriteBatch.Draw(Texture,
                                     GameModeTitleDestination,
                                     GameModeComboSource,
                                     Color.White * opacity);

                    drawLeaderboards(spriteBatch, localTopScoresCombo, leaderboardManagerCombo);
                }
            }
        }

        private void drawLeaderboards(SpriteBatch spriteBatch, List<Highscore> localTopScores, LeaderboardManager leaderboardManager)
        {
            spriteBatch.Draw(Texture,
                             switcherRightDestination,
                             switcherSource,
                             Astropixx.ThemeColor * opacity);

            spriteBatch.Draw(Texture,
                             switcherLeftDestination,
                             switcherSource,
                             Astropixx.ThemeColor * opacity,
                             0.0f,
                             Vector2.Zero,
                             SpriteEffects.FlipHorizontally,
                             0.0f);

            spriteBatch.DrawString(Font,
                                   leaderboardManager.StatusText,
                                   new Vector2(800 / 2 - Font.MeasureString(leaderboardManager.StatusText).X / 2,
                                               440),
                                   Astropixx.ThemeColor * opacity);

            if (scoreState == ScoreState.Local)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 LocalTitleSource,
                                 Color.White * opacity);

                if (localTopScores.Count > 0 && localTopScores[0].Score > 0)
                {
                    spriteBatch.Draw(Texture,
                                     resubmitDestination,
                                     resubmitSource,
                                     Astropixx.ThemeColor * opacity);
                }

                for (int i = 0; i < MaxScores; i++)
                {
                    string scoreText;
                    string nameText;
                    string levelText;

                    if (localTopScores[i].Score > 0)
                    {
                        Highscore h = new Highscore(localTopScores[i].Name, localTopScores[i].Score, localTopScores[i].Level);

                        scoreText = h.ScoreText;
                        nameText = h.Name;
                        levelText = h.LevelText;
                    }
                    else
                    {
                        scoreText = DOTS12;
                        nameText = DOTS21;
                        levelText = DOTS3;
                    }

                    spriteBatch.DrawString(Font,
                           string.Format("{0:d}.", i + 1),
                           new Vector2(50, 100 + (i * 33)),
                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           nameText,
                                           new Vector2(NamePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           scoreText,
                                           new Vector2(ScorePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           levelText,
                                           new Vector2(LevelPositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);
                }
            }

            if (scoreState == ScoreState.OnlineAll)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 OnlineAllTitleSource,
                                 Color.White * opacity);

                spriteBatch.Draw(Texture,
                                 browserDestination,
                                 browserSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.Draw(Texture,
                                 refreshDestination,
                                 refreshSource,
                                 Astropixx.ThemeColor * opacity);

                for (int i = 0; i < MaxScores; i++)
                {
                    string scoreText;
                    string nameText;
                    string levelText;

                    if (leaderboardManager.TopScoresAll.Count > i)
                    {
                        Highscore h = new Highscore(leaderboardManager.TopScoresAll[i].Name, leaderboardManager.TopScoresAll[i].Score, leaderboardManager.TopScoresAll[i].Level);

                        scoreText = h.ScoreText;
                        nameText = h.Name;
                        levelText = h.LevelText;
                    }
                    else
                    {
                        scoreText = DOTS12;
                        nameText = DOTS21;
                        levelText = DOTS3;
                    }

                    spriteBatch.DrawString(Font,
                           string.Format("{0:d}.", i + 1),
                           new Vector2(50, 100 + (i * 33)),
                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           nameText,
                                           new Vector2(NamePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           scoreText,
                                           new Vector2(ScorePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           levelText,
                                           new Vector2(LevelPositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);
                }
            }

            if (scoreState == ScoreState.OnlineMonth)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 OnlineMonthTitleSource,
                                 Color.White * opacity);

                spriteBatch.Draw(Texture,
                                 browserDestination,
                                 browserSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.Draw(Texture,
                                 refreshDestination,
                                 refreshSource,
                                 Astropixx.ThemeColor * opacity);

                for (int i = 0; i < MaxScores; i++)
                {
                    string scoreText;
                    string nameText;
                    string levelText;

                    if (leaderboardManager.TopScoresMonth.Count > i)
                    {
                        Highscore h = new Highscore(leaderboardManager.TopScoresMonth[i].Name, leaderboardManager.TopScoresMonth[i].Score, leaderboardManager.TopScoresMonth[i].Level);

                        scoreText = h.ScoreText;
                        nameText = h.Name;
                        levelText = h.LevelText;
                    }
                    else
                    {
                        scoreText = DOTS12;
                        nameText = DOTS21;
                        levelText = DOTS3;
                    }

                    spriteBatch.DrawString(Font,
                           string.Format("{0:d}.", i + 1),
                           new Vector2(50, 100 + (i * 33)),
                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           nameText,
                                           new Vector2(NamePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           scoreText,
                                           new Vector2(ScorePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           levelText,
                                           new Vector2(LevelPositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);
                }
            }

            if (scoreState == ScoreState.OnlineWeek)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 OnlineWeekTitleSource,
                                 Color.White * opacity);

                spriteBatch.Draw(Texture,
                                 browserDestination,
                                 browserSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.Draw(Texture,
                                 refreshDestination,
                                 refreshSource,
                                 Astropixx.ThemeColor * opacity);

                for (int i = 0; i < MaxScores; i++)
                {
                    string scoreText;
                    string nameText;
                    string levelText;

                    if (leaderboardManager.TopScoresWeek.Count > i)
                    {
                        Highscore h = new Highscore(leaderboardManager.TopScoresWeek[i].Name, leaderboardManager.TopScoresWeek[i].Score, leaderboardManager.TopScoresWeek[i].Level);

                        scoreText = h.ScoreText;
                        nameText = h.Name;
                        levelText = h.LevelText;
                    }
                    else
                    {
                        scoreText = DOTS12;
                        nameText = DOTS21;
                        levelText = DOTS3;
                    }

                    spriteBatch.DrawString(Font,
                           string.Format("{0:d}.", i + 1),
                           new Vector2(50, 100 + (i * 33)),
                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           nameText,
                                           new Vector2(NamePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           scoreText,
                                           new Vector2(ScorePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           levelText,
                                           new Vector2(LevelPositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);
                }
            }

            if (scoreState == ScoreState.OnlineDay)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 OnlineDayTitleSource,
                                 Color.White * opacity);

                spriteBatch.Draw(Texture,
                                 browserDestination,
                                 browserSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.Draw(Texture,
                                 refreshDestination,
                                 refreshSource,
                                 Astropixx.ThemeColor * opacity);

                for (int i = 0; i < MaxScores; i++)
                {
                    string scoreText;
                    string nameText;
                    string levelText;

                    if (leaderboardManager.TopScoresDay.Count > i)
                    {
                        Highscore h = new Highscore(leaderboardManager.TopScoresDay[i].Name, leaderboardManager.TopScoresDay[i].Score, leaderboardManager.TopScoresDay[i].Level);

                        scoreText = h.ScoreText;
                        nameText = h.Name;
                        levelText = h.LevelText;
                    }
                    else
                    {
                        scoreText = DOTS12;
                        nameText = DOTS21;
                        levelText = DOTS3;
                    }

                    spriteBatch.DrawString(Font,
                           string.Format("{0:d}.", i + 1),
                           new Vector2(50, 100 + (i * 33)),
                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           nameText,
                                           new Vector2(NamePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           scoreText,
                                           new Vector2(ScorePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           levelText,
                                           new Vector2(LevelPositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);
                }
            }

            if (scoreState == ScoreState.MostAddictive)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 MostAddictiveTitleSource,
                                 Color.White * opacity);

                spriteBatch.Draw(Texture,
                                 browserDestination,
                                 browserSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.Draw(Texture,
                                 refreshDestination,
                                 refreshSource,
                                 Astropixx.ThemeColor * opacity);

                for (int i = 0; i < MaxScores; i++)
                {
                    string scoreText;
                    string nameText;
                    string levelText;

                    if (leaderboardManager.TopScoresMostAddictive.Count > i)
                    {
                        Highscore h = new Highscore(leaderboardManager.TopScoresMostAddictive[i].Name, leaderboardManager.TopScoresMostAddictive[i].Score, leaderboardManager.TopScoresMostAddictive[i].Level);

                        scoreText = h.ScoreText;
                        nameText = h.Name;
                        levelText = string.Format("{0:0000}", h.Level);
                    }
                    else
                    {
                        scoreText = DOTS12;
                        nameText = DOTS21;
                        levelText = DOTS3;
                    }

                    spriteBatch.DrawString(Font,
                           string.Format("{0:d}.", i + 1),
                           new Vector2(50, 100 + (i * 33)),
                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           nameText,
                                           new Vector2(NamePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           scoreText,
                                           new Vector2(ScorePositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);

                    spriteBatch.DrawString(Font,
                                           levelText,
                                           new Vector2(LevelPositionX, 100 + (i * 33)),
                                           Astropixx.ThemeColor * opacity);
                }
            }

            if (scoreState == ScoreState.OnlineMe)
            {
                spriteBatch.Draw(Texture,
                                 TitlePosition,
                                 OnlineMeTitleSource,
                                 Color.White * opacity);

                spriteBatch.Draw(Texture,
                                 browserDestination,
                                 browserSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.Draw(Texture,
                                 refreshDestination,
                                 refreshSource,
                                 Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                   TEXT_ME,
                                   new Vector2(800 / 2 - Font.MeasureString(TEXT_ME).X / 2,
                                               140),
                                   Astropixx.ThemeColor * opacity);

                // Title:
                spriteBatch.DrawString(Font,
                                       TEXT_RANK,
                                       new Vector2(200,
                                                   210),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       TEXT_SCORE,
                                       new Vector2(200,
                                                   250),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       TEXT_LEVEL,
                                       new Vector2(200,
                                                   290),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       TEXT_TOTAL_SCORE,
                                       new Vector2(200,
                                                   330),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       TEXT_TOTAL_LEVEL,
                                       new Vector2(200,
                                                   370),
                                       Astropixx.ThemeColor * opacity);


                // Content:
                int topRank = leaderboardManager.TopRankMe;
                long topScore = leaderboardManager.TopScoreMe;
                int topLevel = leaderboardManager.TopLevelMe;
                long totalScore = leaderboardManager.TotalScoreMe;
                int totalLevel = leaderboardManager.TotalLevelMe;
                string topRankText;
                string topScoreText;
                string topLevelText;
                string totalScoreText;
                string totalLevelText;

                if (topRank == 0)
                    topRankText = DOTS6;
                else
                    topRankText = string.Format("{0:00000}", leaderboardManager.TopRankMe);

                if (topScore == 0)
                    topScoreText = DOTS12;
                else
                    topScoreText = string.Format("{0:000000000000}", leaderboardManager.TopScoreMe);

                if (topLevel == 0)
                    topLevelText = DOTS3;
                else
                    topLevelText = string.Format("{0:00}", leaderboardManager.TopLevelMe);

                if (totalScore == 0)
                    totalScoreText = DOTS12;
                else
                    totalScoreText = string.Format("{0:000000000000}", leaderboardManager.TotalScoreMe);

                if (totalLevel == 0)
                    totalLevelText = DOTS3;
                else
                    totalLevelText = string.Format("{0:00}", leaderboardManager.TotalLevelMe);

                spriteBatch.DrawString(Font,
                                       topRankText,
                                       new Vector2(600 - Font.MeasureString(topRankText).X,
                                                   210),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       topScoreText,
                                       new Vector2(600 - Font.MeasureString(topScoreText).X,
                                                   250),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       topLevelText,
                                       new Vector2(600 - Font.MeasureString(topLevelText).X,
                                                   290),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       totalScoreText,
                                       new Vector2(600 - Font.MeasureString(totalScoreText).X,
                                                   330),
                                       Astropixx.ThemeColor * opacity);

                spriteBatch.DrawString(Font,
                                       totalLevelText,
                                       new Vector2(600 - Font.MeasureString(totalLevelText).X,
                                                   370),
                                       Astropixx.ThemeColor * opacity);
            }
        }

        private void drawLeaderboardsSelection(SpriteBatch spriteBatch)
        {
            // Title
            spriteBatch.Draw(Texture,
                             leaderboardsDestination,
                             leaderboardsSource,
                             Color.White * opacity);

            // Buttons
            spriteBatch.Draw(Texture,
                             GameModeSurvivalDestination,
                             GameModeSurvivalSource,
                             Color.White * opacity);
            spriteBatch.Draw(Texture,
                             GameModeDestructionDestination,
                             GameModeDestructionSource,
                             Color.White * opacity);
            spriteBatch.Draw(Texture,
                             GameModeComboDestination,
                             GameModeComboSource,
                             Color.White * opacity);
        }

        public void SaveHighScore(string name, long resultscore, long collectscore, long distancescore,long asteroids, int level, GameModeManager.GameMode gameMode)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
            {
                increaseTotalDistanceScore(distancescore);
                saveHighScore(name, resultscore, level, localTopScoresSurvival, SCORE_FILE_SURVIVAL);
                currentHighScoreSurvival = maxScore(localTopScoresSurvival);
            }
            else if (gameMode == GameModeManager.GameMode.Destruction)
            {
                increaseTotalDestroyedAsteroids(asteroids);
                increaseTotalDistanceScore(distancescore);
                saveHighScore(name, resultscore, level, localTopScoresDestruction, SCORE_FILE_DESTRUCTION);
                currentHighScoreDestruction = maxScore(localTopScoresDestruction);
            }
            else
            {
                increaseTotalCollectScore(collectscore);
                increaseTotalDistanceScore(distancescore);
                saveHighScore(name, resultscore, level, localTopScoresCombo, SCORE_FILE_COMBO);
                currentHighScoreCombo = maxScore(localTopScoresCombo);
            }
        }

        /// <summary>
        /// Saves the current highscore to a text file.
        /// </summary>
        private void saveHighScore(string name, long score, int level, List<Highscore> localTopScores, string fileName)
        {
            this.lastName = name;

            if(this.IsInScoreboard(score,
                                   GameModeManager.SelectedGameMode))
            {
                Highscore newScore = new Highscore(name, score, level);

                localTopScores.Add(newScore);
                this.sortScoreList(localTopScores);
                this.trimScoreList(localTopScores);

                //this.lastName = name;

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Create, isf))
                    {
                        using (StreamWriter sw = new StreamWriter(isfs))
                        {
                            for (int i = 0; i < MaxScores; i++)
                            {
                                sw.WriteLine(localTopScores[i]);
                            }

                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }

            this.saveUserData();
        }

        /// <summary>
        /// Loads all local highscores.
        /// </summary>
        private void LoadAllHighscores()
        {
            loadHighScore(localTopScoresSurvival, SCORE_FILE_SURVIVAL);
            currentHighScoreSurvival = maxScore(localTopScoresSurvival);

            loadHighScore(localTopScoresDestruction, SCORE_FILE_DESTRUCTION);
            currentHighScoreDestruction = maxScore(localTopScoresDestruction);

            loadHighScore(localTopScoresCombo, SCORE_FILE_COMBO);
            currentHighScoreCombo = maxScore(localTopScoresCombo);
        }

        /// <summary>
        /// Loads the high score from a text file.
        /// </summary>
        private void loadHighScore(List<Highscore> localTopScores, string fileName)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool hasExisted = isf.FileExists(fileName);

                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, isf))
                {
                    if (hasExisted)
                    {
                        using (StreamReader sr = new StreamReader(isfs))
                        {
                            for (int i = 0; i < MaxScores; i++)
                            {
                                localTopScores.Add(new Highscore(sr.ReadLine()));
                            }

                            this.sortScoreList(localTopScores);
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(isfs))
                        {
                            for (int i = 0; i < MaxScores; i++)
                            {
                                Highscore newScore = new Highscore();
                                localTopScores.Add(newScore);
                                sw.WriteLine(newScore);
                            }
                        }
                    }  
                }
            }
        }

        private void sortScoreList(List<Highscore> localTopScores)
        {
            for (int i = 0; i < localTopScores.Count; i++)
            {
                for (int j = 0; j < localTopScores.Count - 1; j++)
                {
                    if (localTopScores[j].Score < localTopScores[j + 1].Score)
                    {
                        Highscore tmp = localTopScores[j];
                        localTopScores[j] = localTopScores[j + 1];
                        localTopScores[j + 1] = tmp;
                    }
                }
            }
        }

        private void trimScoreList(List<Highscore> localTopScores)
        {
            while (localTopScores.Count > MaxScores)
            {
                localTopScores.RemoveAt(localTopScores.Count - 1);
            }
        }

        private long maxScore(List<Highscore> localTopScores)
        {
            long max = 0;

            for (int i = 0; i < localTopScores.Count; i++)
            {
                max = Math.Max(max, localTopScores[i].Score);
            }

            return max;
        }

        public long GetCurrentHighscore(GameModeManager.GameMode gameMode)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
                return currentHighScoreSurvival;
            else if (gameMode == GameModeManager.GameMode.Destruction)
                return currentHighScoreDestruction;
            else 
                return currentHighScoreCombo;
        }

        /// <summary>
        /// Checks wheather the score reaches top 10.
        /// </summary>
        /// <param name="score">The score to check</param>
        /// <returns>True if the player is under the top 1.</returns>
        public bool IsInScoreboard(long score, GameModeManager.GameMode gameMode)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
                return score > localTopScoresSurvival[MaxScores - 1].Score;
            else if (gameMode == GameModeManager.GameMode.Destruction)
                return score > localTopScoresDestruction[MaxScores - 1].Score;
            else
                return score > localTopScoresCombo[MaxScores - 1].Score;
        }

        /// <summary>
        /// Calculates the rank of the new score.
        /// </summary>
        /// <param name="score">The new score</param>
        /// <returns>Returns the calculated rank (-1, if the score is not top 10).</returns>
        public int GetRank(long score, GameModeManager.GameMode gameMode)
        {
            if (gameMode == GameModeManager.GameMode.Survival)
            {
                if (localTopScoresSurvival.Count < 0)
                    return 1;

                for (int i = 0; i < localTopScoresSurvival.Count; i++)
                {
                    if (localTopScoresSurvival[i].Score < score)
                        return i + 1;
                }
            }

            if (gameMode == GameModeManager.GameMode.Destruction)
            {
                if (localTopScoresDestruction.Count < 0)
                    return 1;

                for (int i = 0; i < localTopScoresDestruction.Count; i++)
                {
                    if (localTopScoresDestruction[i].Score < score)
                        return i + 1;
                }
            }

            if (gameMode == GameModeManager.GameMode.Combo)
            {
                if (localTopScoresCombo.Count < 0)
                    return 1;

                for (int i = 0; i < localTopScoresCombo.Count; i++)
                {
                    if (localTopScoresCombo[i].Score < score)
                        return i + 1;
                }
            }

            return -1;
        }

        private void increaseTotalDestroyedAsteroids(long value)
        {
            this.totalDestroyedAsteroids += value;
        }

        private void increaseTotalCollectScore(long value)
        {
            this.totalCollectScore += value;
        }

        private void increaseTotalDistanceScore(long value)
        {
            this.totalDistanceScore += value;
        }

        private void saveUserData()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(USERDATA_FILE, FileMode.Create, isf))
                {
                    using (StreamWriter sw = new StreamWriter(isfs))
                    {
                        sw.WriteLine(this.LastName);
                        sw.WriteLine(this.totalDestroyedAsteroids);
                        sw.WriteLine(this.totalCollectScore);
                        sw.WriteLine(this.totalDistanceScore);
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
        }

        private void loadUserData()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool hasExisted = isf.FileExists(USERDATA_FILE);

                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(USERDATA_FILE, FileMode.OpenOrCreate, FileAccess.ReadWrite, isf))
                {
                    if (hasExisted)
                    {
                        using (StreamReader sr = new StreamReader(isfs))
                        {
                            this.lastName = sr.ReadLine();
                            this.totalDestroyedAsteroids = Int64.Parse(sr.ReadLine());
                            this.totalCollectScore = Int64.Parse(sr.ReadLine());
                            this.totalDistanceScore = Int64.Parse(sr.ReadLine());
                        }
                    }
                    else
                    {
                        using (StreamWriter sw = new StreamWriter(isfs))
                        {
                            sw.WriteLine(this.lastName);
                            sw.WriteLine(this.totalDestroyedAsteroids);
                            sw.WriteLine(this.totalCollectScore);
                            sw.WriteLine(this.totalDistanceScore);

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
            this.currentHighScoreSurvival = Int64.Parse(reader.ReadLine());
            this.currentHighScoreDestruction = Int64.Parse(reader.ReadLine());
            this.currentHighScoreCombo = Int64.Parse(reader.ReadLine());
            this.lastName = reader.ReadLine();
            this.opacity = Single.Parse(reader.ReadLine());
            this.isActive = Boolean.Parse(reader.ReadLine());
            this.scoreState = (ScoreState)Enum.Parse(scoreState.GetType(), reader.ReadLine(), false);
            this.displayState = (HighscoresDisplayState)Enum.Parse(displayState.GetType(), reader.ReadLine(), false);
        }

        public void Deactivated(StreamWriter writer)
        {
            writer.WriteLine(currentHighScoreSurvival);
            writer.WriteLine(currentHighScoreDestruction);
            writer.WriteLine(currentHighScoreCombo);
            writer.WriteLine(lastName);
            writer.WriteLine(opacity);
            writer.WriteLine(isActive);
            writer.WriteLine(scoreState);
            writer.WriteLine(displayState);
        }

        #endregion

        #region Properties

        public string LastName
        {
            get
            {
                return this.lastName;
            }
        }

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
                }
            }
        }

        public long TotalDestroyedAsteroids
        {
            get
            {
                return this.totalDestroyedAsteroids;
            }
        }

        public long TotalCollectScore
        {
            get
            {
                return this.totalCollectScore;
            }
        }

        public long TotalDistanceScore
        {
            get
            {
                return this.totalDistanceScore;
            }
        }

        public HighscoresDisplayState DisplayState
        {
            get
            {
                return displayState;
            }
        }

        public bool IsSelectionState
        {
            get
            {
                return displayState == HighscoresDisplayState.Selection;
            }
        }

        #endregion
    }
}
