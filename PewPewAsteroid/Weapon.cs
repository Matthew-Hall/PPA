using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceShooter
{
    class Weapon
    {
        public Animation LaserAnimation;
        float laserMoveSpeed = 30f;
        public Vector2 Position;
        public int Damage = 10;
        public bool Active;
        public int Range;

        public int Width
        {
            get { return LaserAnimation.FrameWidth; }
        }

        public int Height
        {
            get { return LaserAnimation.FrameHeight; }
        }

        public void Initialize(Animation animation, Vector2 position)
        {
            LaserAnimation = animation;
            Position = position;
            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            // The laser needs to move up heading towards the "enemy"
            Position.Y -= laserMoveSpeed;
            LaserAnimation.Position = Position;
            LaserAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            LaserAnimation.Draw(spriteBatch);
        }
    }
}
