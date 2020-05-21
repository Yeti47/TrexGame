using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using TrexRunner.Entities;
using TrexRunner.Graphics;
using TrexRunner.System;
using TrexRunner.Extensions;
using SharpDX.MediaFoundation;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TrexRunner
{

    public class TRexRunnerGame : Game
    {
        public enum DisplayMode
        {
            Default,
            Zoomed
        }

        public const string GAME_TITLE = "T-Rex Runner";

        private const string ASSET_NAME_SPRITESHEET = "TrexSpritesheet";
        private const string ASSET_NAME_SFX_HIT = "hit";
        private const string ASSET_NAME_SFX_SCORE_REACHED = "score-reached";
        private const string ASSET_NAME_SFX_BUTTON_PRESS = "button-press";

        public const int WINDOW_WIDTH = 600;
        public const int WINDOW_HEIGHT = 150;

        public const int TREX_START_POS_Y = WINDOW_HEIGHT - 16;
        public const int TREX_START_POS_X = 1;

        private const int SCORE_BOARD_POS_X = WINDOW_WIDTH - 130;
        private const int SCORE_BOARD_POS_Y = 10;

        private const float FADE_IN_ANIMATION_SPEED = 820f;

        private const string SAVE_FILE_NAME = "Save.dat";

        public const int DISPLAY_ZOOM_FACTOR = 2;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SoundEffect _sfxHit;
        private SoundEffect _sfxButtonPress;
        private SoundEffect _sfxScoreReached;

        private Texture2D _spriteSheetTexture;
        private Texture2D _fadeInTexture;
        private Texture2D _invertedSpriteSheet;

        private float _fadeInTexturePosX;

        private Trex _trex;
        private ScoreBoard _scoreBoard;

        private InputController _inputController;

        private GroundManager _groundManager;
        private ObstacleManager _obstacleManager;
        private SkyManager _skyManager;
        private GameOverScreen _gameOverScreen;

        private EntityManager _entityManager;

        private KeyboardState _previousKeyboardState;

        private DateTime _highscoreDate;

        private Matrix _transformMatrix = Matrix.Identity;

        public GameState State { get; private set; }

        public DisplayMode WindowDisplayMode { get; set; } = DisplayMode.Default;

        public float ZoomFactor => WindowDisplayMode == DisplayMode.Default ? 1 : DISPLAY_ZOOM_FACTOR;

        public TRexRunnerGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _entityManager = new EntityManager();
            State = GameState.Initial;
            _fadeInTexturePosX = Trex.TREX_DEFAULT_SPRITE_WIDTH;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Window.Title = GAME_TITLE;

            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _sfxButtonPress = Content.Load<SoundEffect>(ASSET_NAME_SFX_BUTTON_PRESS);
            _sfxHit = Content.Load<SoundEffect>(ASSET_NAME_SFX_HIT);
            _sfxScoreReached = Content.Load<SoundEffect>(ASSET_NAME_SFX_SCORE_REACHED);

            _spriteSheetTexture = Content.Load<Texture2D>(ASSET_NAME_SPRITESHEET);
            _invertedSpriteSheet = _spriteSheetTexture.InvertColors(Color.Transparent);

            _fadeInTexture = new Texture2D(GraphicsDevice, 1, 1);
            _fadeInTexture.SetData(new Color[] { Color.White });

            _trex = new Trex(_spriteSheetTexture, new Vector2(TREX_START_POS_X, TREX_START_POS_Y - Trex.TREX_DEFAULT_SPRITE_HEIGHT), _sfxButtonPress);
            _trex.DrawOrder = 100;
            _trex.JumpComplete += trex_JumpComplete;
            _trex.Died += trex_Died;

            _scoreBoard = new ScoreBoard(_spriteSheetTexture, new Vector2(SCORE_BOARD_POS_X, SCORE_BOARD_POS_Y), _trex, _sfxScoreReached);
            //_scoreBoard.Score = 498;
            //_scoreBoard.HighScore = 12345;

            _inputController = new InputController(_trex);

            _groundManager = new GroundManager(_spriteSheetTexture, _entityManager, _trex);
            _obstacleManager = new ObstacleManager(_entityManager, _trex, _scoreBoard, _spriteSheetTexture);
            _skyManager = new SkyManager(_trex, _spriteSheetTexture, _invertedSpriteSheet, _entityManager, _scoreBoard);

            _gameOverScreen = new GameOverScreen(_spriteSheetTexture, this);
            _gameOverScreen.Position = new Vector2(WINDOW_WIDTH / 2 - GameOverScreen.GAME_OVER_SPRITE_WIDTH / 2, WINDOW_HEIGHT / 2 - 30);

            _entityManager.AddEntity(_trex);
            _entityManager.AddEntity(_groundManager);
            _entityManager.AddEntity(_scoreBoard);
            _entityManager.AddEntity(_obstacleManager);
            _entityManager.AddEntity(_gameOverScreen);
            _entityManager.AddEntity(_skyManager);

            _groundManager.Initialize();

            LoadSaveState();

        }

        private void trex_Died(object sender, EventArgs e)
        {
            State = GameState.GameOver;
            _obstacleManager.IsEnabled = false;
            _gameOverScreen.IsEnabled = true;

            _sfxHit.Play();

            Debug.WriteLine("Game Over: " +  DateTime.Now);

            if(_scoreBoard.DisplayScore > _scoreBoard.HighScore)
            {
                Debug.WriteLine("New highscore set: " + _scoreBoard.DisplayScore);
                _scoreBoard.HighScore = _scoreBoard.DisplayScore;
                _highscoreDate = DateTime.Now;

                SaveGame();

            }

        }

        private void trex_JumpComplete(object sender, EventArgs e)
        {
            
            if(State == GameState.Transition)
            {
                State = GameState.Playing;
                _trex.Initialize();

                _obstacleManager.IsEnabled = true;

            }

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();

            if (State == GameState.Playing)
                _inputController.ProcessControls(gameTime);
            else if (State == GameState.Transition)
                _fadeInTexturePosX += (float)gameTime.ElapsedGameTime.TotalSeconds * FADE_IN_ANIMATION_SPEED;
            else if (State == GameState.Initial)
            {

                bool isStartKeyPressed = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space);
                bool wasStartKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Up) || _previousKeyboardState.IsKeyDown(Keys.Space);

                if (isStartKeyPressed && !wasStartKeyPressed)
                {
                    StartGame();

                }

            }

            _entityManager.Update(gameTime);

            if(keyboardState.IsKeyDown(Keys.F8) && !_previousKeyboardState.IsKeyDown(Keys.F8)) {

                ResetSaveState();

            }

            if (keyboardState.IsKeyDown(Keys.F12) && !_previousKeyboardState.IsKeyDown(Keys.F12))
            {

                ToggleDisplayMode();

            }

            _previousKeyboardState = keyboardState;

        }

        protected override void Draw(GameTime gameTime)
        {
            if (_skyManager == null)
                GraphicsDevice.Clear(Color.White);
            else
                GraphicsDevice.Clear(_skyManager.ClearColor);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix);

            _entityManager.Draw(_spriteBatch, gameTime);

            if(State == GameState.Initial || State == GameState.Transition)
            {

                _spriteBatch.Draw(_fadeInTexture, new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);

            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool StartGame()
        {
            if (State != GameState.Initial)
                return false;

            _scoreBoard.Score = 0;
            State = GameState.Transition;
            _trex.BeginJump();

            return true;
        }

        public bool Replay()
        {
            if (State != GameState.GameOver)
                return false;

            State = GameState.Playing;
            _trex.Initialize();

            _obstacleManager.Reset();
            _obstacleManager.IsEnabled = true;

            _gameOverScreen.IsEnabled = false;
            _scoreBoard.Score = 0;

            _groundManager.Initialize();

            _inputController.BlockInputTemporarily();

            return true;

        }

        public void SaveGame()
        {
            SaveState saveState = new SaveState
            {
                Highscore = _scoreBoard.HighScore,
                HighscoreDate = _highscoreDate
            };

            try
            {
                using (FileStream fileStream = new FileStream(SAVE_FILE_NAME, FileMode.Create))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, saveState);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("An error occurred while saving the game: " + ex.Message);
            }

        }

        public void LoadSaveState()
        {
            try
            {
                using (FileStream fileStream = new FileStream(SAVE_FILE_NAME, FileMode.OpenOrCreate))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    SaveState saveState = binaryFormatter.Deserialize(fileStream) as SaveState;

                    if(saveState != null)
                    {
                        if(_scoreBoard != null)
                            _scoreBoard.HighScore = saveState.Highscore;

                        _highscoreDate = saveState.HighscoreDate;

                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while loading the game: " + ex.Message);
            }
        }

        private void ResetSaveState()
        {
            _scoreBoard.HighScore = 0;
            _highscoreDate = default(DateTime);

            SaveGame();

        }

        private void ToggleDisplayMode()
        {
            if(WindowDisplayMode == DisplayMode.Default)
            {
                WindowDisplayMode = DisplayMode.Zoomed;
                _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT * DISPLAY_ZOOM_FACTOR;
                _graphics.PreferredBackBufferWidth = WINDOW_WIDTH * DISPLAY_ZOOM_FACTOR;
                _transformMatrix = Matrix.Identity * Matrix.CreateScale(DISPLAY_ZOOM_FACTOR, DISPLAY_ZOOM_FACTOR, 1);
            }
            else
            {
                WindowDisplayMode = DisplayMode.Default;
                _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
                _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
                _transformMatrix = Matrix.Identity;
            }

            _graphics.ApplyChanges();

        }

    }
}
