using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace SpaceShooter
{
    class Enemy
    {
        public Animation EnemyAnimation;
        public Vector2 Position;
        public bool Active;
        public int Health;
        public int Damage;
        public int Value;

        public int Width
        {
            get { return EnemyAnimation.FrameWidth; }
        }

        public int Height
        {
            get { return EnemyAnimation.FrameHeight; }
        }

        public float enemyMoveSpeed;

        public void Initialize(Animation animation,
            Vector2 position)
        {
            EnemyAnimation = animation;
            Position = position;
            Active = true;
            Health = 10;
            Damage = 10;
            enemyMoveSpeed = 8;
            // points enemy is worth upon destruction
            Value = 1000;
        }

        public void Update(GameTime gameTime)
        {
            // the enemy always moves down which is positive Y
            Position.Y += enemyMoveSpeed;
            EnemyAnimation.Position = Position;
            EnemyAnimation.Update(gameTime);

            /* If the enenmy is past the screen or its
             * health reaches 0 then deactivate it.
             * I honestly have no idea why it needs this
             * multiplier to reach the bottom. */
            if (Position.Y > 23 * Height || Health <= 0)
            {
                Active = false;

            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            EnemyAnimation.Draw(spriteBatch);
        }
    }
}
