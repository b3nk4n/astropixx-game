using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using AstropiXX.Inputs;
using AstropiXX.Extensions;

namespace AstropiXX
{
    class SubmissionManager
    {
        #region Members

        HighscoreManager highscoreManager;

        private readonly Rectangle submitSource = new Rectangle(0, 700, 
                                                                300, 50);
        private readonly Rectangle submitDestination = new Rectangle(50, 370,
                                                                     300, 50);

        private readonly Rectangle cancelSource = new Rectangle(0, 750,
                                                                300, 50);
        private readonly Rectangle cancelDestination = new Rectangle(450, 370,
                                                                     300, 50);

        private readonly Rectangle retrySource = new Rectangle(0, 1450,
                                                                  300, 50);
        private readonly Rectangle retryDestination = new Rectangle(50, 370,
                                                                    300, 50);

        private static SubmissionManager submissionManager;

        public const int MaxScores = 10;

        public static Texture2D Texture;
        public static SpriteFont Font;
        private readonly Rectangle TitleSource = new Rectangle(0, 600,
                                                               500, 100);
        private readonly Vector2 TitlePosition = new Vector2(150.0f, 80.0f);

        private float opacity = 0.0f;
        private const float OpacityMax = 1.0f;
        private const float OpacityMin = 0.0f;
        private const float OpacityChangeRate = 0.05f;

        private bool isActive = false;

        private string name = string.Empty;
        private long score;
        private int level;

        private bool cancelClicked = false;
        private bool retryClicked = false;


        private const string TEXT_SUBMIT = "You have now the ability to submit your score!";
        private const string TEXT_NAME = "Name:";
        private const string TEXT_SCORE = "Score:";
        private const string TEXT_LEVEL = "Level:";

        private enum SubmitState { Submit, Submitted };
        private SubmitState submitState = SubmitState.Submit;

        public static GameInput GameInput;
        private const string SubmitAction = "Submit";
        private const string CancelAction = "Cancel";
        private const string RetryAction = "Retry";

        #endregion

        #region Constructors

        private SubmissionManager()
        {
            highscoreManager = HighscoreManager.GetInstance();
        }

        #endregion

        #region Methods

        public void SetupInputs()
        {
            GameInput.AddTouchGestureInput(SubmitAction,
                                           GestureType.Tap,
                                           submitDestination);
            GameInput.AddTouchGestureInput(CancelAction,
                                           GestureType.Tap,
                                           cancelDestination);
            GameInput.AddTouchGestureInput(RetryAction,
                                           GestureType.Tap,
                                           retryDestination);
        }

        public static SubmissionManager GetInstance()
        {
            if (submissionManager == null)
            {
                submissionManager = new SubmissionManager();
            }

            return submissionManager;
        }

        private void handleTouchInputs()
        {
            if (submitState == SubmitState.Submit)
            {
                // Submit
                if (GameInput.IsPressed(SubmitAction))
                {
                    highscoreManager.SubmitScore(LeaderboardManager.SUBMIT,
                                              name,
                                              score,
                                              level,
                                              GameModeManager.SelectedGameMode);
                    submitState = SubmitState.Submitted;
                }
            }
            else
            {
                // Retry
                if (GameInput.IsPressed(RetryAction))
                {
                    if (submitState == SubmitState.Submitted)
                    {
                        retryClicked = true;
                    }
                }
            }

            if (GameInput.IsPressed(CancelAction))
            {
                highscoreManager.SetStatusText(GameModeManager.SelectedGameMode,
                                                   LeaderboardManager.TEXT_NONE);
                cancelClicked = true;
            }
        }

        public void SetUp(string name, long score, int level)
        {
            this.name = name;
            this.score = score;
            this.level = level;
        }

        public void Update(GameTime gameTime)
        {
            if (isActive)
            {
                if (this.opacity < OpacityMax)
                    this.opacity += OpacityChangeRate;
            }

            handleTouchInputs();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture,
                                 cancelDestination,
                                 cancelSource,
                                 Color.White * opacity);

            if (submitState == SubmitState.Submit)
            {
                spriteBatch.Draw(Texture,
                                 submitDestination,
                                 submitSource,
                                 Color.White * opacity);
            }
            else if (submitState == SubmitState.Submitted)
            {
                spriteBatch.Draw(Texture,
                                 retryDestination,
                                 retrySource,
                                 Color.White * opacity);

                spriteBatch.DrawString(Font,
                                   highscoreManager.GetStatusText(GameModeManager.SelectedGameMode),
                                   new Vector2(800 / 2 - Font.MeasureString(highscoreManager.GetStatusText(GameModeManager.SelectedGameMode)).X / 2,
                                               440),
                                   Astropixx.ThemeColor * opacity);

            }

            spriteBatch.DrawString(Font,
                                   TEXT_SUBMIT,
                                   new Vector2(800 / 2 - Font.MeasureString(TEXT_SUBMIT).X / 2,
                                               180),
                                   Astropixx.ThemeColor * opacity);

            // Title:
            spriteBatch.DrawString(Font,
                                   TEXT_NAME,
                                   new Vector2(300,
                                               230),
                                   Astropixx.ThemeColor * opacity);

            spriteBatch.DrawString(Font,
                                   TEXT_SCORE,
                                   new Vector2(300,
                                               270),
                                   Astropixx.ThemeColor * opacity);

            spriteBatch.DrawString(Font,
                                   TEXT_LEVEL,
                                   new Vector2(300,
                                               310),
                                   Astropixx.ThemeColor * opacity);

            // Content:
            spriteBatch.DrawString(Font,
                                   name,
                                   new Vector2(450,
                                               230),
                                   Astropixx.ThemeColor * opacity);

            spriteBatch.DrawInt64(Font,
                                  score,
                                  new Vector2(450,
                                              270),
                                  Astropixx.ThemeColor * opacity);

            spriteBatch.DrawInt64(Font,
                                  level,
                                  new Vector2(450,
                                              310),
                                  Astropixx.ThemeColor * opacity);


            spriteBatch.Draw(Texture,
                             TitlePosition,
                             TitleSource,
                             Color.White * opacity);
        }

        #endregion

        #region Activate/Deactivate

        public void Activated(StreamReader reader)
        {
            this.opacity = Single.Parse(reader.ReadLine());
            this.isActive = Boolean.Parse(reader.ReadLine());
            this.name = reader.ReadLine();
            this.score = Int64.Parse(reader.ReadLine());
            this.level = Int32.Parse(reader.ReadLine());
            this.submitState = (SubmitState)Enum.Parse(submitState.GetType(), reader.ReadLine(), false);
        }

        public void Deactivated(StreamWriter writer)
        {
            writer.WriteLine(opacity);
            writer.WriteLine(isActive);
            writer.WriteLine(name);
            writer.WriteLine(score);
            writer.WriteLine(level);
            writer.WriteLine(submitState);
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
                    this.retryClicked = false;
                    this.cancelClicked = false;
                    this.submitState = SubmitState.Submit;
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

        public bool RetryClicked
        {
            get
            {
                return this.retryClicked;
            }
        }

        #endregion
    }
}
