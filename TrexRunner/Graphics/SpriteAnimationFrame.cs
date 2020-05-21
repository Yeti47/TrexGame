using System;
using System.Collections.Generic;
using System.Text;

namespace TrexRunner.Graphics
{
    public class SpriteAnimationFrame
    {
        private Sprite _sprite;
        
        public Sprite Sprite {
            get
            {
                return _sprite;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "The sprite cannot be null.");

                _sprite = value;

            }
        }

        public float TimeStamp { get; }

        public SpriteAnimationFrame(Sprite sprite, float timeStamp)
        {
            Sprite = sprite;
            TimeStamp = timeStamp;
        }

    }
}
