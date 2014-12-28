#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace WindowsPhoneGame2
{
    class MainGameScreen : GameScreen
    {
        private GestureSample Gestures;

        int ROW = 8; //dong
        int COL = 5; //cot
        float DEFAULTTIME = 180;
        //int m_level = 1;
        float m_gameTime = 0.0f;
        int[,] m_board = {
	        {1,1,1,1,2,2,2,2},
	        {3,3,3,3,4,4,4,4},
	        {5,5,5,5,6,6,6,6},
	        {7,7,7,7,8,8,8,8},
	        {9,9,10,10,11,11,12,12}
        };
        public List<int> m_listBoard = new List<int>();

        SpriteFont gameFont;
        GameType m_state = GameType.Running;

        int m_numCard = 0; //so card da pick
        CardType m_currentType; //luu CardType cua card pick lan thu nhat, dung de so sanh voi CardType cua card pick lan thu 2
        int m_currentCard1; // luu vi tri cua card pick lan thu nhat
        int m_currentCard2; // luu vi tri cua card pick lan thu hai

        public List<Card> m_listCard = new List<Card>();

        float m_timeUp = 0.0f;


        Texture2D m_bgTexture;
        Texture2D m_winPopup;
        Texture2D m_losePopup;
        Texture2D m_pausePopup;
        Texture2D m_musicOff;
        bool winGame = false;
        
        public MainGameScreen()
        {
            SoundManager.PlaySong(ESong.Background);
            Gestures = new GestureSample();
            m_currentCard1 = -1;
            m_currentCard2 = -1;
            SetGameTime();
        }
        void InitBoardList()
        {
            for (int i = 0; i < COL; i++)
            {
                for (int j = 0; j < ROW; j++)
                {
                    m_listBoard.Add(m_board[i, j]);
                }
            }
        }
        void RandomBoard()
        {
            m_listBoard.Shuffle();
            int countList = 0;
            for (int i = 0; i < COL; i++)
            {
                for (int j = 0; j < ROW; j++)
                {
                    m_board[i, j] = m_listBoard[countList];
                    countList++;
                }
            }
        }

        void SetGameTime()
        {
            m_gameTime = DEFAULTTIME + 20 - Global.level * 20.0f;
            m_gameTime = Math.Max(m_gameTime, 40.0f);
        }
        public override void LoadContent()
        {
            base.LoadContent();
            m_bgTexture = screenContent.Load<Texture2D>("bgMainGame");
            gameFont = screenContent.Load<SpriteFont>("SpriteFont1");
            m_winPopup = screenContent.Load<Texture2D>("winPopup");
            m_losePopup = screenContent.Load<Texture2D>("losePopup");
            m_pausePopup = screenContent.Load<Texture2D>("pausePopup");
            m_musicOff = screenContent.Load<Texture2D>("buttonMusicOff");
            InitBoardList();
            RandomBoard();
            

            Card tempCard;
            for (int i = 0; i < COL; i++)
            {
                for (int j = 0; j < ROW; j++)
                {
                    if (m_board[i, j] != 0)
                    {
                        CardType type = (CardType)(m_board[i, j] - 1);
                        tempCard = new Card(screenContent, i, j,type);
                        m_listCard.Add(tempCard);
                        m_listBoard.Add(m_board[i, j]);
                    }
                }
            }
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (Global.Back == true)
            {
                SoundManager.PlaySound(ESound.SelectButton);
                MenuScreen menuScreen = new MenuScreen();
                ScreenManager.AddScreen(menuScreen);
                this.ExitScreen();
                return;
            }
            if (m_state == GameType.Running || m_state == GameType.Wait)
                m_gameTime -= (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            if (TouchPanel.IsGestureAvailable)
            {
                Gestures = TouchPanel.ReadGesture();
            }
            else
            {
                Gestures = new GestureSample();
            }

            if (m_state == GameType.Wait)
            {

                m_timeUp += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (m_timeUp >= 1.0f)
                {
                    m_timeUp = 0.0f;
                    m_state = GameType.Running;
                    m_listCard[m_currentCard1].ChangeState();
                    m_listCard[m_currentCard2].ChangeState();
                    m_currentCard1 = -1;
                    m_currentCard2 = -1;

                }
            }

            if (Gestures.GestureType == GestureType.Tap)
            {
                Vector2 posTap = Gestures.Position;
                if (m_state == GameType.Pause)
                {
                    SoundManager.ResumeSong();
                    m_state = GameType.Running;
                    return;
                }
                if (m_state != GameType.End)
                    DoOption(posTap);
                if ((m_state == GameType.End))
                {
                    if (winGame == true)
                    {
                        Global.level++;
                    }
                    ResetGame();
                    return;
                }
                if ((m_numCard < 2) && (m_state == GameType.Running))
                {
                    for (int i = 0; i < m_listCard.Count; i++)
                    {
                        if (m_listCard[i].IsContain(posTap))
                        {
                            if (m_listCard[i].m_state == CardState.Done || m_listCard[i].m_state == CardState.Up)
                                return;
                            m_numCard++;
                            if (m_numCard == 1)
                            {
                                m_currentType = m_listCard[i].m_type;
                                m_listCard[i].ChangeState();
                                m_currentCard1 = i;
                            }
                            if (m_numCard == 2)
                            {
                                m_numCard = 0;
                                if (m_listCard[i].m_type == m_listCard[m_currentCard1].m_type)
                                {
                                    m_listCard[i].m_state = CardState.Done;
                                    m_listCard[m_currentCard1].m_state = CardState.Done;
                                    m_currentCard1 = -1;
                                    m_currentCard2 = -1;
                                    SoundManager.PlaySound(ESound.Success);
                                    return;
                                }
                                else
                                {
                                    m_listCard[i].ChangeState();
                                    m_state = GameType.Wait;
                                    m_currentCard2 = i;
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            if (IsWinGame() && (m_state != GameType.End))
            {
                winGame = true;
                m_state = GameType.End;
                SoundManager.PlaySound(ESound.Win);
            }
            if ((m_gameTime <= 0) && (m_state != GameType.End))
            {
                if (!IsWinGame())
                {
                    winGame = false;
                    m_state = GameType.End;
                    SoundManager.PlaySound(ESound.Lose);
                }
                else
                {
                    winGame = true;
                    m_state = GameType.End;
                    SoundManager.PlaySound(ESound.Win);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        void DoOption(Vector2 pos)
        {
            Point posTap = new Point((int)pos.X, (int)pos.Y);
            Rectangle resetButton = new Rectangle(722, 404, 58, 58);
            Rectangle menuButton = new Rectangle(648, 404, 58, 58);
            Rectangle soundButton = new Rectangle(648, 330, 58, 58);
            Rectangle pauseButton = new Rectangle(722, 330, 58, 58);

            if (menuButton.Contains(posTap))
            {
                SoundManager.PlaySound(ESound.SelectButton);
                MenuScreen menuScreen = new MenuScreen();
                ScreenManager.AddScreen(menuScreen);
                this.ExitScreen();
                return;
            }

            if (resetButton.Contains(posTap))
            {
                SoundManager.PlaySound(ESound.SelectButton);
                ResetGame();
                return;
            }

            if (pauseButton.Contains(posTap))
            {
                SoundManager.PlaySound(ESound.SelectButton);
                m_state = GameType.Pause;
                SoundManager.PauseSong();
                return;
            }

            if (soundButton.Contains(posTap))
            {
                SoundManager.PlaySound(ESound.SelectButton);
                if (Global.SOUND == true)
                {
                    Global.SOUND = false;
                    SoundManager.StopSongs();
                }
                else
                {
                    Global.SOUND = true;
                    SoundManager.PlaySong(ESong.Background);
                }
                return;
            }
        }

        bool IsWinGame()
        {
            for (int i = 0; i < m_listCard.Count; i++)
            {
                if (m_listCard[i].m_state != CardState.Done)
                    return false;
            }
            return true;
        }

        void ResetGame()
        {
            m_state = GameType.Running;
            m_timeUp = 0.0f;
            m_numCard = 0; //so card da pick

            Gestures = new GestureSample();
            m_currentCard1 = -1;
            m_currentCard2 = -1;
            SetGameTime();
            RandomBoard();

            m_listCard.Clear();
            Card tempCard;
            for (int i = 0; i < COL; i++)
            {
                for (int j = 0; j < ROW; j++)
                {
                    if (m_board[i, j] != 0)
                    {
                        CardType type = (CardType)(m_board[i, j] - 1);
                        tempCard = new Card(screenContent, i, j, type);
                        m_listCard.Add(tempCard);
                    }
                }
            }
        }
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            spriteBatch.Begin();
            spriteBatch.Draw(m_bgTexture, new Vector2(0, 0), Color.White);

            for (int i = 0; i < m_listCard.Count; i++)
            {
                m_listCard[i].Draw(spriteBatch);
            }

            if ((winGame == true) && (m_state == GameType.End))
            {
                spriteBatch.Draw(m_winPopup, new Vector2(viewport.Width / 2 - m_winPopup.Width / 2, viewport.Height / 2 - m_winPopup.Height / 2), Color.White);
            }
            if ((winGame == false) && (m_state == GameType.End))
            {
                spriteBatch.Draw(m_losePopup, new Vector2(viewport.Width / 2 - m_losePopup.Width / 2, viewport.Height / 2 - m_losePopup.Height / 2), Color.White);
            }

            if (m_state == GameType.Pause)
            {
                spriteBatch.Draw(m_pausePopup, new Vector2(viewport.Width / 2 - m_pausePopup.Width / 2, viewport.Height / 2 - m_pausePopup.Height / 2), Color.White);
            }

            if (Global.SOUND == false)
            {
                spriteBatch.Draw(m_musicOff, new Vector2(648, 330), Color.White);
            }

            int tempGameTime = ((int)m_gameTime);
            if(tempGameTime <= 0)
                tempGameTime = 0;
            spriteBatch.DrawString(gameFont, tempGameTime.ToString(), new Vector2(450, -5), Color.White);
            spriteBatch.DrawString(gameFont, Global.level.ToString(), new Vector2(167, -5), Color.White);
            
            spriteBatch.End();
            
        }

    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }


    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
