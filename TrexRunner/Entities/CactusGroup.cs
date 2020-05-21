using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TrexRunner.Graphics;

namespace TrexRunner.Entities
{
    public class CactusGroup : Obstacle
    {
        public enum GroupSize
        {
            Small = 0,
            Medium = 1,
            Large = 2
        }

        private const int SMALL_CACTUS_SPRITE_HEIGHT = 36;
        private const int SMALL_CACTUS_SPRITE_WIDTH = 17;

        private const int SMALL_CACTUS_TEXTURE_POS_X = 228;
        private const int SMALL_CACTUS_TEXTURE_POS_Y = 0;

        private const int LARGE_CACTUS_TEXTURE_POS_X = 332;
        private const int LARGE_CACTUS_TEXTURE_POS_Y = 0;

        private const int LARGE_CACTUS_SPRITE_HEIGHT = 51;
        private const int LARGE_CACTUS_SPRITE_WIDTH = 25;

        private const int COLLISION_BOX_INSET = 5;

        public override Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y), Sprite.Width, Sprite.Height);
                box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public bool IsLarge { get; }

        public GroupSize Size { get; }

        public Sprite Sprite { get; private set; }

        public CactusGroup(Texture2D spriteSheet, bool isLarge, GroupSize size, Trex trex, Vector2 position) : base(trex, position)
        {
            IsLarge = isLarge;
            Size = size;
            Sprite = GenerateSprite(spriteSheet);
        }

        private Sprite GenerateSprite(Texture2D spriteSheet)
        {

            Sprite sprite = null;

            int spriteWidth = 0;
            int spriteHeight = 0;
            int posX = 0;
            int posY = 0;

            if(!IsLarge) // Create small cactus group
            {
                spriteWidth = SMALL_CACTUS_SPRITE_WIDTH;
                spriteHeight = SMALL_CACTUS_SPRITE_HEIGHT;

                posX = SMALL_CACTUS_TEXTURE_POS_X;
                posY = SMALL_CACTUS_TEXTURE_POS_Y;

            }
            else // Create large cactus group
            {
                spriteWidth = LARGE_CACTUS_SPRITE_WIDTH;
                spriteHeight = LARGE_CACTUS_SPRITE_HEIGHT;

                posX = LARGE_CACTUS_TEXTURE_POS_X;
                posY = LARGE_CACTUS_TEXTURE_POS_Y;
            }

            int offsetX = 0;
            int width = spriteWidth;

            if (Size == GroupSize.Small)
            {
                offsetX = 0;
                width = spriteWidth;
            }
            else if (Size == GroupSize.Medium)
            {
                offsetX = 1;
                width = spriteWidth * 2;
            }
            else
            {
                offsetX = 3;
                width = spriteWidth * 3;
            }

            sprite = new Sprite
            (
                spriteSheet,
                posX + offsetX * spriteWidth,
                posY,
                width,
                spriteHeight
            );

            return sprite;

        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Sprite.Draw(spriteBatch, Position);
        }

    }
}
