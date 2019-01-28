using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace SpaceShooter
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Player player;
        //Song song;
        //SoundEffect soundEffect;
        SpriteFont font;
        Texture2D mainBackground;
        Rectangle rectBackground;
        Background bgLayer1;
        Background bgLayer2;
        float playerMoveSpeed;
        const float Scale = 1f;
        Texture2D enemyTexture;
        List<Enemy> enemies;
        List<Weapon> lasers;
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;
        Random random;
        Texture2D laserTexture;
        TimeSpan laserSpawnTime;
        TimeSpan previousLaserSpawnTime;

        // Keyboard states to determine key presses
        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        // Gamepad states to determine button presses
        GamePadState currentGamePadState;
        GamePadState previousGamePadState;

        // Mouse states to track Mouse button presses
        MouseState currentMouseState;
        MouseState previousMouseState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 720;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            rectBackground = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            bgLayer1 = new Background();
            bgLayer2 = new Background();
            player = new Player();
            playerMoveSpeed = 9.0f;
            // Enable FreeDrag gesture
            TouchPanel.EnabledGestures = GestureType.FreeDrag;
            enemies = new List<Enemy>();
            previousSpawnTime = TimeSpan.Zero;
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);
            random = new Random();
            // Lasers are going to fire rapid
            lasers = new List<Weapon>();
            const float SECONDS_IN_MINUTE = 60f;
            const float RATE_OF_FIRE = 200f;
            laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / RATE_OF_FIRE);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            mainBackground = Content.Load<Texture2D>("Graphics/bg1");
            bgLayer1.Initialize(Content, "Graphics/bg2", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -1);
            bgLayer2.Initialize(Content, "Graphics/bg3", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -2);
            font = Content.Load<SpriteFont>("Score");
            // Load player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("Graphics\\player");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 76, 96, 30, 30, Color.White, 1f, true);
            enemyTexture = Content.Load<Texture2D>("Graphics\\asteroids1");
            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X + GraphicsDevice.Viewport.TitleSafeArea.Width / 2, GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);
            laserTexture = Content.Load<Texture2D>("Graphics\\laser");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            currentKeyboardState = Keyboard.GetState();

            player.Score++;
            UpdatePlayer(gameTime);
            bgLayer1.Update(gameTime);
            bgLayer2.Update(gameTime);
            UpdateEnemies(gameTime);
            UpdateCollision();
            UpdateWeaponTracking(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(mainBackground, rectBackground, Color.White);
            bgLayer1.Draw(spriteBatch);
            bgLayer2.Draw(spriteBatch);
            foreach (var l in lasers)
            {
                l.Draw(spriteBatch);
            }
            player.Draw(spriteBatch);
            foreach (var e in enemies)
            {
                e.Draw(spriteBatch);
            };

            spriteBatch.DrawString(font, "Score: " + player.Score, new Vector2(30, 30), Color.ForestGreen);
            spriteBatch.DrawString(font, "Health: " + player.Health, new Vector2(30, 60), Color.ForestGreen);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;
                AddEnemy();
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime);
                if (!enemies[i].Active)
                {
                    enemies.Remove(enemies[i]);
                    // 100 points for dodging asteroids
                    player.Score += 100;
                }
            }
        }

        private void UpdateWeaponTracking(GameTime gameTime)
        {
            for (var i = 0; i < lasers.Count; i++)
            {
                lasers[i].Update(gameTime);
                // Remove the beam when its deactivated or is at the end of the screen.
                if (!lasers[i].Active || lasers[i].Position.Y > GraphicsDevice.Viewport.Height)
                {
                    lasers.Remove(lasers[i]);
                }
            }
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();
            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 33, 31, 8, 30, Color.White, 1f, true); // #AsteroidAnimation
            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(random.Next(70, GraphicsDevice.Viewport.Width - 70), -GraphicsDevice.Viewport.Height + enemyTexture.Height); //GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(70, GraphicsDevice.Viewport.Height - 70)
            // Create an enemy
            Enemy enemy = new Enemy();
            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);
            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);
            // Touch Gestures
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                if (gesture.GestureType == GestureType.FreeDrag)
                {
                    player.Position += gesture.Delta;
                }
                // touch firing
                if (gesture.GestureType == GestureType.DoubleTap)
                {
                    FireLaser(gameTime);
                }
            }

            // mouse movement
            Vector2 mousePosition = new Vector2(currentMouseState.X, currentMouseState.Y);
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 posDelta = mousePosition - player.Position;
                posDelta.Normalize();
                posDelta = posDelta * playerMoveSpeed;
                player.Position = player.Position + posDelta;
            }

            // gamepad controls
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

            // keyboard or dpad controls
            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                player.Position.X -= playerMoveSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                player.Position.X += playerMoveSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                player.Position.Y -= playerMoveSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                player.Position.Y += playerMoveSpeed;
            }

            // firing
            if (currentMouseState.RightButton == ButtonState.Pressed)
            {
                FireLaser(gameTime);
            }

            if (currentKeyboardState.IsKeyDown(Keys.Space) || currentGamePadState.Buttons.A == ButtonState.Pressed || currentKeyboardState.IsKeyDown(Keys.Z))
            {
                FireLaser(gameTime);
            }


            // Dualshock4 dpad seems to refuse to calculate diagonals. Testing other game pads #DPADISSUE

            // Limiting out of bounds movement
            player.Position.X = MathHelper.Clamp(player.Position.X, player.Width * player.PlayerAnimation.scale / 2, GraphicsDevice.Viewport.Width - player.Width * player.PlayerAnimation.scale / 2);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, player.Height * player.PlayerAnimation.scale / 2, GraphicsDevice.Viewport.Height - player.Height * player.PlayerAnimation.scale / 2);

            if (!player.Active)
            {
                Initialize();
            }
        }

        private void UpdateCollision()
        {
            Rectangle playerRect;
            Rectangle enemyRect;
            Rectangle laserRect;
            // Player hitbox
            playerRect = new Rectangle((int)player.Position.X - 20, (int)player.Position.Y - 25, player.Width - 5, player.Height - 10);
            // Asteroid hitbox
            for (var i = 0; i < enemies.Count; i++)
            {
                enemyRect = new Rectangle((int)enemies[i].Position.X + 2, (int)enemies[i].Position.Y, enemies[i].Width, enemies[i].Height);

                if (playerRect.Intersects(enemyRect))
                {
                    enemies[i].Health = 0;
                    player.Health -= enemies[i].Damage;
                    player.Score += enemies[i].Value;

                    if (player.Health <= 0)
                    {
                        player.Active = false;
                    }
                }

                for (var l = 0; l < lasers.Count; l++)
                {
                    laserRect = new Rectangle((int)lasers[l].Position.X + 15, (int)lasers[l].Position.Y, lasers[l].Width, lasers[l].Height);

                    // laser hitbox collision calc
                    if (laserRect.Intersects(enemyRect))
                    {
                        enemies[i].Health -= lasers[l].Damage;
                        if (enemies[i].Health <= 0)
                        {
                            player.Score += enemies[i].Value;
                        }
                        // despawn laser
                        lasers[l].Active = false;
                    }
                }
            }
        }

        protected void FireLaser(GameTime gameTime)
        {
            // rate of fire calc
            if (gameTime.TotalGameTime - previousLaserSpawnTime > laserSpawnTime)
            {
                previousLaserSpawnTime = gameTime.TotalGameTime;
                AddLaser();
            }
        }

        protected void AddLaser()
        {
            Animation laserAnimation = new Animation();
            laserAnimation.Initialize(laserTexture, player.Position, 4, 10, 1, 30, Color.White, 1f, true);
            Weapon laser = new Weapon();
            var laserPosition = player.Position;
            // Adjust these 2 values if not centered, or not in desired position (like player missile area)
            laserPosition.Y += 0;
            laserPosition.X += 0;

            laser.Initialize(laserAnimation, laserPosition);
            lasers.Add(laser);
        }
    }
}
