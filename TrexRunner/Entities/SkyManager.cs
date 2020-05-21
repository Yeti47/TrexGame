using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace TrexRunner.Entities
{
    public class SkyManager : IGameEntity, IDayNightCycle
    {
        private const float EPSILON = 0.01f;

        private const int CLOUD_DRAW_ORDER = -1;
        private const int STAR_DRAW_ORDER = -3;
        private const int MOON_DRAW_ORDER = -2;

        private const int CLOUD_MIN_POS_Y = 20;
        private const int CLOUD_MAX_POS_Y = 70;

        private const int STAR_MIN_POS_Y = 10;
        private const int STAR_MAX_POS_Y = 60;

        private const int CLOUD_MIN_DISTANCE = 150;
        private const int CLOUD_MAX_DISTANCE = 400;

        private const int STAR_MIN_DISTANCE = 120;
        private const int STAR_MAX_DISTANCE = 380;

        private const int MOON_POS_Y = 20;

        private const int NIGHT_TIME_SCORE = 700;
        private const int NIGHT_TIME_DURATION_SCORE = 250;

        private const float TRANSITION_DURATION = 2f;

        private float _normalizedScreenColor = 1f;
        private int _previousScore;
        private int _nightTimeStartScore;
        private bool _isTransitioningToNight = false;
        private bool _isTransitioningToDay = false;

        private readonly EntityManager _entityManager;
        private readonly ScoreBoard _scoreBoard;
        private readonly Trex _trex;
        private Texture2D _spriteSheet;
        private Texture2D _invertedSpriteSheet;
        private Moon _moon;

        private Texture2D _overlay;

        private int _targetCloudDistance;
        private int _targetStarDistance;

        private Random _random;

        private Color[] _textureData;
        private Color[] _invertedTextureData;

        public Color ClearColor => new Color(_normalizedScreenColor, _normalizedScreenColor, _normalizedScreenColor);

        public int DrawOrder => int.MaxValue;

        public int NightCount { get; private set; }

        public bool IsNight => _normalizedScreenColor < 0.5f;

        private float OverlayVisibility
        {
            get
            {
                return MathHelper.Clamp((0.25f - MathHelper.Distance(0.5f, _normalizedScreenColor)) / 0.25f, 0, 1);
            }
        }

        public SkyManager(Trex trex, Texture2D spriteSheet, Texture2D invertedSpriteSheet, EntityManager entityManager, ScoreBoard scoreBoard)
        {
            this._entityManager = entityManager;
            this._scoreBoard = scoreBoard;
            _random = new Random();
            this._trex = trex;
            _spriteSheet = spriteSheet;
            _invertedSpriteSheet = invertedSpriteSheet;

            _textureData = new Color[_spriteSheet.Width * _spriteSheet.Height];
            _invertedTextureData = new Color[_invertedSpriteSheet.Width * _invertedSpriteSheet.Height];

            _spriteSheet.GetData(_textureData);
            _invertedSpriteSheet.GetData(_invertedTextureData);

            _overlay = new Texture2D(spriteSheet.GraphicsDevice, 1, 1);
            Color[] overlayData = new[] { Color.Gray };
            _overlay.SetData(overlayData);

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if(OverlayVisibility > EPSILON)
                spriteBatch.Draw(_overlay, new Rectangle(0, 0, TRexRunnerGame.WINDOW_WIDTH, TRexRunnerGame.WINDOW_HEIGHT), Color.White * OverlayVisibility);

        }

        public void Update(GameTime gameTime)
        {
            if(_moon == null)
            {
                _moon = new Moon(this, _spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, MOON_POS_Y));
                _moon.DrawOrder = MOON_DRAW_ORDER;
                _entityManager.AddEntity(_moon);
            }

            HandleCloudSpawning();
            HandleStarSpawning();

            foreach(SkyObject skyObject in _entityManager.GetEntitiesOfType<SkyObject>().Where(s => s.Position.X < -100))
            {
                if(skyObject is Moon moon)
                {
                    moon.Position = new Vector2(TRexRunnerGame.WINDOW_WIDTH, MOON_POS_Y);
                }
                else
                    _entityManager.RemoveEntity(skyObject);
            }

            if(_previousScore != 0 && _previousScore < _scoreBoard.DisplayScore && _previousScore / NIGHT_TIME_SCORE != _scoreBoard.DisplayScore / NIGHT_TIME_SCORE)
            {
                TransitionToNightTime();
            }

            if(IsNight && (_scoreBoard.DisplayScore - _nightTimeStartScore >= NIGHT_TIME_DURATION_SCORE))
            {
                TransitionToDayTime();

            }

            if(_scoreBoard.DisplayScore < NIGHT_TIME_SCORE && (IsNight || _isTransitioningToNight))
            {
                TransitionToDayTime();
            }

            UpdateTransition(gameTime);

            _previousScore = _scoreBoard.DisplayScore;

        }

        private void UpdateTransition(GameTime gameTime)
        {
            if(_isTransitioningToNight)
            {
                _normalizedScreenColor -= (float)gameTime.ElapsedGameTime.TotalSeconds / TRANSITION_DURATION;

                if (_normalizedScreenColor < 0)
                    _normalizedScreenColor = 0;

                if(_normalizedScreenColor < 0.5f)
                {
                    InvertTextures();
                }

            }
            else if (_isTransitioningToDay)
            {
                _normalizedScreenColor += (float)gameTime.ElapsedGameTime.TotalSeconds / TRANSITION_DURATION;

                if (_normalizedScreenColor > 1)
                    _normalizedScreenColor = 1;

                if(_normalizedScreenColor >= 0.5f)
                {
                    InvertTextures();
                }

            }

        }

        private void InvertTextures()
        {
            if(IsNight)
            {
                _spriteSheet.SetData(_invertedTextureData);
            }
            else
            {
                _spriteSheet.SetData(_textureData);
            }
        }

        private bool TransitionToNightTime()
        {
            if (IsNight || _isTransitioningToNight)
                return false;

            _nightTimeStartScore = _scoreBoard.DisplayScore;
            _isTransitioningToNight = true;
            _isTransitioningToDay = false;
            _normalizedScreenColor = 1f;
            NightCount++;

            return true;
        }

        private bool TransitionToDayTime()
        {
            if (!IsNight || _isTransitioningToDay)
                return false;

            _isTransitioningToNight = false;
            _isTransitioningToDay = true;
            _normalizedScreenColor = 0;

            return true;

        }

        private void HandleCloudSpawning()
        {
            IEnumerable<Cloud> clouds = _entityManager.GetEntitiesOfType<Cloud>();

            if (clouds.Count() <= 0 || (TRexRunnerGame.WINDOW_WIDTH - clouds.Max(c => c.Position.X)) >= _targetCloudDistance)
            {
                _targetCloudDistance = _random.Next(CLOUD_MIN_DISTANCE, CLOUD_MAX_DISTANCE + 1);
                int posY = _random.Next(CLOUD_MIN_POS_Y, CLOUD_MAX_POS_Y + 1);

                Cloud cloud = new Cloud(_spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, posY));
                cloud.DrawOrder = CLOUD_DRAW_ORDER;

                _entityManager.AddEntity(cloud);

            }
        }

        private void HandleStarSpawning()
        {
            IEnumerable<Star> stars = _entityManager.GetEntitiesOfType<Star>();

            if(stars.Count() <= 0 || (TRexRunnerGame.WINDOW_WIDTH - stars.Max(c => c.Position.X)) >= _targetStarDistance)
            {
                _targetStarDistance = _random.Next(STAR_MIN_DISTANCE, STAR_MAX_DISTANCE + 1);
                int posY = _random.Next(STAR_MIN_POS_Y, STAR_MAX_POS_Y + 1);

                Star star = new Star(this, _spriteSheet, _trex, new Vector2(TRexRunnerGame.WINDOW_WIDTH, posY));
                star.DrawOrder = STAR_DRAW_ORDER;

                _entityManager.AddEntity(star);

            }

        }

    }
}
