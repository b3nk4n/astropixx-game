using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Phone.Info;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.IO;

namespace AstropiXX
{
    class LeaderboardManager
    {
        #region Members

        private List<Highscore> topScoresAll = new List<Highscore>();
        private List<Highscore> topScoresMonth = new List<Highscore>();
        private List<Highscore> topScoresWeek = new List<Highscore>();
        private List<Highscore> topScoresDay = new List<Highscore>();
        private List<Highscore> topScoresMostAddictive = new List<Highscore>();

        private int topRankMe;
        private long topScoreMe;
        private int topLevelMe;
        private long totalScoreMe;
        private int totalLevelMe;

        WebClient wc = new WebClient();

        public static Texture2D Texture;
        public static SpriteFont Font;

        private readonly Vector2 TitlePosition = new Vector2(250.0f, 40.0f);

        private readonly Rectangle HighscoreTitleDestination = new Rectangle(0, 250,
                                                                             300, 50);

        private string xml;

        private int rank;

        private string statusText = TEXT_NONE;

        public const string TEXT_START_SUBMIT = "Submitting...";
        public const string TEXT_START_RESUBMIT = "Resubmitting local top score...";
        public const string TEXT_END_SUBMIT_FORMAT = "Submission successful! (Ranked: {0})";
        public const string TEXT_START_REFRESH = "Refreshing...";
        public const string TEXT_END_REFRESH = "Refreshing successful!";
        public const string TEXT_SERVER_ERROR = "Server is not reachable!";
        public const string TEXT_SUBMIT_ERROR = "Submission failed!";
        public const string TEXT_RESUBMIT_EXIST = "Local top score has already been submitted!";
        public const string TEXT_NONE = "";

        private string PHONE_ID;

        public const string SUBMIT = "SUBMIT";
        public const string RESUBMIT = "RESUBMIT";

        private readonly string gameMode;
        private readonly string fileName;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new LeaderboardManager instance.
        /// Load and parses the stored xml, if availavle.
        /// </summary>
        public LeaderboardManager(string gameMode)
        {
            this.gameMode = gameMode;
            fileName = string.Format("leaderboardsxml_{0}.txt", gameMode);

            this.loadXml();

            tryParseXml(this.xml);

            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);

            InitPhoneID();
        }

        /// <summary>
        /// Initializes the phone ID on startup.
        /// Required, because loading consumes much performance.
        /// </summary>
        private void InitPhoneID()
        {
            string phoneid;

            try
            {
                phoneid = UserExtendedProperties.GetValue("ANID") as string;

                if (phoneid == null)
                {
                    phoneid = "##00000000000000000000000000000000";
                }
            }
            catch (UnauthorizedAccessException)
            {
                phoneid = "##00000000000000000000000000000000";
            }

            phoneid = phoneid.Substring(2, 32);

            PHONE_ID = phoneid;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts submitting a new score.
        /// </summary>
        /// <param name="name">The players name.</param>
        /// <param name="score">The players score.</param>
        /// <param name="level">The players level.</param>
        public void Submit(string method, string name, long score, int level)
        {
            string phoneid = PHONE_ID;

            string hash = MD5Core.GetHashString(phoneid + name + score + level).ToLower();
            if (!wc.IsBusy)
            {
                if (method.Equals(RESUBMIT))
                    statusText = TEXT_START_RESUBMIT;
                else
                    statusText = TEXT_START_SUBMIT;

                wc.DownloadStringAsync(new Uri(string.Format("http://bsautermeister.de/astropixx/newscore.php?Method={0}&PhoneID={1}&Name={2}&Score={3}&Level={4}&Hash={5}&GameMode={6}",
                                                             method, 
                                                             phoneid, 
                                                             name, 
                                                             score, 
                                                             level, 
                                                             hash,
                                                             gameMode)));
            }
        }

        /// <summary>
        /// Starts receiving the top 10 online scores.
        /// </summary>
        public void Receive()
        {
            if (!wc.IsBusy)
            {
                statusText = TEXT_START_REFRESH;
                wc.DownloadStringAsync(new Uri(string.Format("http://bsautermeister.de/astropixx/requestscores.php?Method=TOP10PHONE&PhoneID={0}&GameMode={1}",
                                               PHONE_ID,
                                               gameMode)));
            }
        }

        /// <summary>
        /// Parses the top 10 xml file.
        /// </summary>
        /// <param name="xml">The downloaded xml string.</param>
        private void tryParseXml(string xml)
        {
            if (!string.IsNullOrEmpty(xml))
            {
                try
                {
                    XDocument xmlDoc = XDocument.Parse(xml);

                    topScoresAll.Clear();
                    topScoresMonth.Clear();
                    topScoresWeek.Clear();
                    topScoresDay.Clear();
                    topScoresMostAddictive.Clear();

                    XElement xmlToplist = xmlDoc.Element("toplist");

                    if (xmlToplist.HasElements)
                    {
                        XElement xmlMe = xmlToplist.Element("me");

                        if (xmlMe != null && xmlMe.HasElements)
                        {
                            topRankMe = Int32.Parse(xmlMe.Element("rank").Value);
                            topScoreMe = Int64.Parse(xmlMe.Element("score").Value);
                            topLevelMe = Int32.Parse(xmlMe.Element("level").Value);
                            totalScoreMe = Int64.Parse(xmlMe.Element("sscore").Value);
                            totalLevelMe = Int32.Parse(xmlMe.Element("slevel").Value);
                        }

                        XElement xmlAll = xmlToplist.Element("all");

                        if (xmlAll != null && xmlAll.HasElements)
                        {
                            foreach (XElement xmlPlayer in xmlAll.Elements("player"))
                            {
                                string name = xmlPlayer.Element("name").Value;
                                long score = Int64.Parse(xmlPlayer.Element("score").Value);
                                int level = Int32.Parse(xmlPlayer.Element("level").Value);
                                topScoresAll.Add(new Highscore(name, score, level));
                            }
                        }

                        XElement xmlMonth = xmlToplist.Element("month");

                        if (xmlMonth != null && xmlMonth.HasElements)
                        {
                            foreach (XElement xmlPlayer in xmlMonth.Elements("player"))
                            {
                                string name = xmlPlayer.Element("name").Value;
                                long score = Int64.Parse(xmlPlayer.Element("score").Value);
                                int level = Int32.Parse(xmlPlayer.Element("level").Value);
                                topScoresMonth.Add(new Highscore(name, score, level));
                            }
                        }

                        XElement xmlWeek = xmlToplist.Element("week");

                        if (xmlWeek != null && xmlWeek.HasElements)
                        {
                            foreach (XElement xmlPlayer in xmlWeek.Elements("player"))
                            {
                                string name = xmlPlayer.Element("name").Value;
                                long score = Int64.Parse(xmlPlayer.Element("score").Value);
                                int level = Int32.Parse(xmlPlayer.Element("level").Value);
                                topScoresWeek.Add(new Highscore(name, score, level));
                            }
                        }

                        XElement xmlDay = xmlToplist.Element("day");

                        if (xmlDay != null && xmlDay.HasElements)
                        {
                            foreach (XElement xmlPlayer in xmlDay.Elements("player"))
                            {
                                string name = xmlPlayer.Element("name").Value;
                                long score = Int64.Parse(xmlPlayer.Element("score").Value);
                                int level = Int32.Parse(xmlPlayer.Element("level").Value);
                                topScoresDay.Add(new Highscore(name, score, level));
                            }
                        }

                        XElement xmlMostAddictive = xmlToplist.Element("addictive");

                        if (xmlMostAddictive != null && xmlMostAddictive.HasElements)
                        {
                            foreach (XElement xmlPlayer in xmlMostAddictive.Elements("player"))
                            {
                                string name = xmlPlayer.Element("name").Value;
                                long score = Int64.Parse(xmlPlayer.Element("score").Value);
                                int level = Int32.Parse(xmlPlayer.Element("level").Value);
                                topScoresMostAddictive.Add(new Highscore(name, score, level));
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Saves the XML-string to isolated storage.
        /// </summary>
        private void saveXml()
        {

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Create, isf))
                {
                    using (StreamWriter sw = new StreamWriter(isfs))
                    {
                        if (!string.IsNullOrEmpty(xml))
                        {
                            sw.WriteLine(this.xml);

                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the XML-string from isolated storage.
        /// </summary>
        private void loadXml()
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
                            this.xml = sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Is fired when the request to the server was finished.
        /// </summary>
        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                statusText = TEXT_SERVER_ERROR;
                return;
            }
            
            string res = e.Result;

            // if server is not reachable
            if (string.IsNullOrEmpty(res))
            {
                statusText = TEXT_SERVER_ERROR;
            }

            // XML/Scores received
            if (res.Length > 10)
            {
                xml = res;

                statusText = TEXT_END_REFRESH;

                tryParseXml(xml);

                saveXml();
            }
            // Scores uploaded
            else
            {
                int x = -1;
                Int32.TryParse(res, out x);

                if (x == -1)
                    statusText = TEXT_SUBMIT_ERROR;
                else if (x == -2)
                    statusText = TEXT_RESUBMIT_EXIST;
                else if (x > 0)
                {
                    rank = x;
                    statusText = string.Format(TEXT_END_SUBMIT_FORMAT, rank);
                }
                else
                    statusText = TEXT_NONE;
            }
            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the players rank.
        /// </summary>
        public int Rank
        {
            get
            {
                return this.rank;
            }
        }

        /// <summary>
        /// Gets the online-all stop 10.
        /// </summary>
        public List<Highscore> TopScoresAll
        {
            get
            {
                return topScoresAll;
            }
        }

        /// <summary>
        /// gets the online-month top 10.
        /// </summary>
        public List<Highscore> TopScoresMonth
        {
            get
            {
                return topScoresMonth;
            }
        }

        /// <summary>
        /// gets the online-week top 10.
        /// </summary>
        public List<Highscore> TopScoresWeek
        {
            get
            {
                return topScoresWeek;
            }
        }

        /// <summary>
        /// gets the online-day top 10.
        /// </summary>
        public List<Highscore> TopScoresDay
        {
            get
            {
                return topScoresDay;
            }
        }

        /// <summary>
        /// gets the most addicitve top 10.
        /// </summary>
        public List<Highscore> TopScoresMostAddictive
        {
            get
            {
                return topScoresMostAddictive;
            }
        }

        /// <summary>
        /// Gets the top rank of the phones user.
        /// </summary>
        public int TopRankMe
        {
            get
            {
                return topRankMe;
            }
        }

        /// <summary>
        /// Gets the top score of the phones user.
        /// </summary>
        public long TopScoreMe
        {
            get
            {
                return topScoreMe;
            }
        }

        /// <summary>
        /// Gets the top level of the phones user.
        /// </summary>
        public int TopLevelMe
        {
            get
            {
                return topLevelMe;
            }
        }

        /// <summary>
        /// Gets the total score of the phones user.
        /// </summary>
        public long TotalScoreMe
        {
            get
            {
                return totalScoreMe;
            }
        }

        /// <summary>
        /// Gets the total level of the phones user.
        /// </summary>
        public int TotalLevelMe
        {
            get
            {
                return totalLevelMe;
            }
        }

        /// <summary>
        /// Gets or sets the status text for refreshing or submitting.
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                this.statusText = value;
            }
        }

        #endregion
    }
}
