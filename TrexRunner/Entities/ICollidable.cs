using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrexRunner.Entities
{
    public interface ICollidable
    {
        Rectangle CollisionBox { get; }
    }
}
