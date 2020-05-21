using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrexRunner.Graphics;

namespace TrexRunner.Entities
{

    public class GroundManager : IGameEntity
    {
        private const float GROUND_TILE_POS_Y = 119;

        private const int SPRITE_WIDTH = 600;
        private const int SPRITE_HEIGHT = 14;

        private const int SPRITE_POS_X = 2;
        private const int SPRITE_POS_Y = 54;

        private Texture2D _spriteSheet;
        private readonly EntityManager _entityManager;

        private readonly List<GroundTile> _groundTiles;

        private Sprite _regularSprite;
        private Sprite _bumpySprite;

        private Trex _trex;

        private Random _random;

        public int DrawOrder { get; set; }

        public GroundManager(Texture2D spriteSheet, EntityManager entityManager, Trex trex)
        {
            _spriteSheet = spriteSheet;
            _groundTiles = new List<GroundTile>();
            _entityManager = entityManager;

            _regularSprite = new Sprite(spriteSheet, SPRITE_POS_X, SPRITE_POS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);
            _bumpySprite = new Sprite(spriteSheet, SPRITE_POS_X + SPRITE_WIDTH, SPRITE_POS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);

            _trex = trex;
            _random = new Random();

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        public void Update(GameTime gameTime)
        {

            if(_groundTiles.Any())
            {
                float maxPosX = _groundTiles.Max(g => g.PositionX);

                if (maxPosX < 0)
                    SpawnTile(maxPosX);


            }

            List<GroundTile> tilesToRemove = new List<GroundTile>();

            foreach(GroundTile gt in _groundTiles)
            {
                gt.PositionX -= _trex.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(gt.PositionX < -SPRITE_WIDTH)
                {
                    _entityManager.RemoveEntity(gt);
                    tilesToRemove.Add(gt);

                }

            }

            foreach (GroundTile gt in tilesToRemove)
                _groundTiles.Remove(gt);

        }

        public void Initialize()
        {
            _groundTiles.Clear();

            foreach(GroundTile gt in _entityManager.GetEntitiesOfType<GroundTile>())
            {
                _entityManager.RemoveEntity(gt);
            }

            GroundTile groundTile = CreateRegularTile(0);
            _groundTiles.Add(groundTile);

            _entityManager.AddEntity(groundTile);

        }

        private GroundTile CreateRegularTile(float positionX)
        {
            GroundTile groundTile = new GroundTile(positionX, GROUND_TILE_POS_Y, _regularSprite);

            return groundTile;

        }

        private GroundTile CreateBumpyTile(float positionX)
        {
            GroundTile groundTile = new GroundTile(positionX, GROUND_TILE_POS_Y, _bumpySprite);
            
            return groundTile;

        }

        private void SpawnTile(float maxPosX)
        {
            double randomNumber = _random.NextDouble();

            float posX = maxPosX + SPRITE_WIDTH;

            GroundTile groundTile;

            if (randomNumber > 0.5)
                groundTile = CreateBumpyTile(posX);
            else
                groundTile = CreateRegularTile(posX);

            _entityManager.AddEntity(groundTile);
            _groundTiles.Add(groundTile);

        }

    }
}
