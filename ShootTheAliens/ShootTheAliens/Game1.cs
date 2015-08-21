using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace ShootTheAliens
{
    public class playerShip
    {
        private Texture2D texture;
        private int currentFrame;
        private Vector2 pos;
        private Projectile bullet;

        private const int FRAMES = 6;

        public playerShip(Texture2D textureShip)
        {
            this.texture = textureShip;
            bullet = new Projectile();
            currentFrame = 0;
            pos = new Vector2(210, 480);
        }

        public void Update()
        {
            currentFrame = ++currentFrame % FRAMES;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int frame = texture.Width / FRAMES;

            Rectangle sourceRect = new Rectangle(frame * currentFrame, 0, frame, texture.Height);
            Rectangle destRect = new Rectangle((int)pos.X, (int)pos.Y, frame, texture.Height);
            spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
        }
        #region Movement
        public Vector2 getPos()
        {
            return pos;
        }

        public void moveLeft()
        {
            if (pos.X >= 0)
                pos.X -= 5;
        }

        public void moveRight()
        {
            if(pos.X <= 400)
                pos.X += 5;
        }

        public void moveUp()
        {
            if (pos.Y >= 0)
                pos.Y -= 5;
        }

        public void moveDown()
        {
            if (pos.Y <= 477)
                pos.Y += 5;
        }
        #endregion

        public bool isFire()
        {
            return bullet.isShooting;
        }

        public Vector2 getBulletPos()
        {
            return bullet.pos;
        }

        public void fire()
        {
            bullet.isShooting = true;
            bullet.pos.X = pos.X + 25;
            bullet.pos.Y = pos.Y;
        }

        public void updateBulletPos()
        {
            if (bullet.pos.Y >= 0)
                bullet.pos.Y -= 20;
            else
                bullet.isShooting = false;
        }

        public void missileHit()
        {
            bullet.isShooting = false;
        }

        private struct Projectile
        {
            public Vector2 pos;
            public bool isShooting;
        }
    }
    public class enemy
    {
        private Texture2D texture;
        private int currentFrame;
        private int count;
        private Vector2 pos;
        private Vector2 vel;
        private bool isAlive;
        private Random rand;
        private int bornTime;

        private const int FRAMES = 3;

        public enemy(Texture2D texture, int seed, int bornT)
        {
            this.texture = texture;
            currentFrame = 0;
            count = 0;
            rand = new Random((seed * bornT) % 100);
            isAlive = false;
            bornTime = bornT;
        }

        public void res(int gameTime)
        {
            if (!isAlive && gameTime >= bornTime)
            {
                isAlive = true;
                pos = new Vector2(rand.Next(100, 300), -120);
                vel = new Vector2(rand.Next(-13, 13), rand.Next(-13, 13));
            }
        }

        public void Update()
        {
            if (count == 8)
            {
                currentFrame = ++currentFrame % FRAMES;
                count = 0;

                if (pos.X <= 0 || pos.X >= 358)
                {
                    vel.X *= -1;
                }

                if (pos.Y >= 583)
                {
                    pos = new Vector2(rand.Next(100, 300), -200);
                }
                pos += vel;
            }
            else count++;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int frame = texture.Width / FRAMES;
            Rectangle sourceRect = new Rectangle(frame * currentFrame, 0, frame, texture.Height);
            Rectangle destRect = new Rectangle((int)pos.X, (int)pos.Y, frame, texture.Height);
            spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
        }

        public bool IsAlive()
        {
            return isAlive;
        }

        public Vector2 getPos()
        {
            return pos;
        }

        public void kill(int gameTime)
        {
            isAlive = false;
            bornTime = gameTime + rand.Next(10,30);
        }
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //GAME STATES
        enum GameState { Start, Level1, Level2, Gameover, GameWin, Level2Menu, EndLevel2 };
        GameState currentGameState = GameState.Start;
        //SOUNDS
        Song loseSound;
        Song winSound;
        SoundEffectInstance loseSoundInstance;
        SoundEffectInstance winSoundInstance;
        Song backgroundMusic;
        //Fonts
        SpriteFont kootenayFont;
        Vector2 scorePos = new Vector2(10, 10);
        Vector2 scorePointsPos = new Vector2(70, 10);
        string scoreText = "Score:";

        Vector2 livesPos = new Vector2(10, 30);
        Vector2 livesLeftPos = new Vector2(70, 30);
        string livesText = "Lives:";
        //scores
        int score = 0;
        int lives = 3;
        //Sprites
        playerShip player;
        enemy[] enemies;
        //Textures
        Texture2D rocket;
        Texture2D rock;
        Texture2D control;
        Texture2D background;
        Texture2D projectile;

        struct Collision
        {
            public bool playerCollided { get; set; }
            public bool bulletCollided { get; set; }
            public enemy target { get; set; }
        }

        Collision collisionInfo;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
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
            enemies = new enemy[5];

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
            //Textures
            rocket = this.Content.Load<Texture2D>("Textures\\rocket");
            control = this.Content.Load<Texture2D>("Textures\\control");
            background = this.Content.Load<Texture2D>("Textures\\background");
            projectile = this.Content.Load<Texture2D>("Textures\\missle");
            rock = this.Content.Load<Texture2D>("Textures\\meteor");
            //Fonts
            kootenayFont = Content.Load<SpriteFont>("Fonts\\Kootenay");
            //Sounds
            backgroundMusic = Content.Load<Song>(@"Audio\background");
            winSound = Content.Load<Song>(@"Audio\excellent");
            loseSound = Content.Load<Song>(@"Audio\gameover");
            //player
            player = new playerShip(rocket);
            //enemies
            for (int i = 0; i < 5; i++)
            {
                enemies[i] = new enemy(rock, i * 10, i * 10);
            }
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            rock.Dispose();
            rocket.Dispose();
            control.Dispose();
            projectile.Dispose();
            background.Dispose();
            backgroundMusic.Dispose();
            winSound.Dispose();
            loseSound.Dispose();
            winSoundInstance.Dispose();
            loseSoundInstance.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || lives == 0)
                this.Exit();

            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.Volume = 0.25f;
                MediaPlayer.Play(backgroundMusic);
                MediaPlayer.IsRepeating = true;
            }

            TouchCollection touchPlaces = TouchPanel.GetState();
            foreach (TouchLocation touch in touchPlaces)
            {
                Vector2 touchPosition = touch.Position;

                if (touch.State == TouchLocationState.Moved)
                {

                    double circleL = Math.Pow(touchPosition.X - 40.0, 2.0) + Math.Pow(touchPosition.Y - 691.0, 2.0);
                    double circleR = Math.Pow(touchPosition.X - 175.0, 2.0) + Math.Pow(touchPosition.Y - 691.0, 2.0);
                    double circleU = Math.Pow(touchPosition.X - 109.0, 2.0) + Math.Pow(touchPosition.Y - 630.0, 2.0);
                    double circleD = Math.Pow(touchPosition.X - 109.0, 2.0) + Math.Pow(touchPosition.Y - 750.0, 2.0);


                    if (circleL <= 1600) player.moveLeft();
                    if (circleR <= 1600) player.moveRight();
                    if (circleU <= 1600) player.moveUp();
                    if (circleD <= 1600) player.moveDown();
                }
                else if (touch.State == TouchLocationState.Pressed)
                {
                    double circleF = Math.Pow(touchPosition.X - 372.0, 2.0) + Math.Pow(touchPosition.Y - 696.0, 2.0);

                    if (circleF <= 1600 && !player.isFire()) player.fire();
                }
            }

            player.Update();
            if (player.isFire()) player.updateBulletPos();

            collisionInfo = DetectCollision();

            foreach (enemy enemy in enemies)
            {
                if (!enemy.IsAlive())
                {
                    enemy.res((int)gameTime.TotalGameTime.TotalSeconds);
                }
                else enemy.Update();
            }

            if (collisionInfo.bulletCollided && player.isFire())
            {
                collisionInfo.target.kill((int)gameTime.TotalGameTime.TotalSeconds);
                player.missileHit();
            }
            if (collisionInfo.playerCollided)
            {
                collisionInfo.target.kill((int)gameTime.TotalGameTime.TotalSeconds);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(background, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(kootenayFont, scoreText, scorePos, Color.Fuchsia);
            spriteBatch.DrawString(kootenayFont, score.ToString(), scorePointsPos, Color.Fuchsia);

            spriteBatch.DrawString(kootenayFont, livesText, livesPos, Color.Fuchsia);
            spriteBatch.DrawString(kootenayFont, lives.ToString(), livesLeftPos, Color.Fuchsia);
            if (player.isFire()) spriteBatch.Draw(projectile, player.getBulletPos(), Color.White);

            foreach (enemy enemy in enemies)
            {
                if (enemy.IsAlive()) enemy.Draw(spriteBatch);
            }

            player.Draw(spriteBatch);
            spriteBatch.Draw(control, new Vector2(0, 583), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
        private Collision DetectCollision()
        {
            Collision collision = new Collision();

            Rectangle playerRectangle = new Rectangle((int)player.getPos().X + 10, (int)player.getPos().Y, 70, 85);
            Rectangle missleRectangle = new Rectangle((int)player.getBulletPos().X, (int)player.getBulletPos().Y, 15, 51);
            Rectangle meteorRectangle;

            foreach (enemy enemy in enemies)
            {
                if (enemy.IsAlive())
                {

                    meteorRectangle = new Rectangle((int)enemy.getPos().X, (int)enemy.getPos().Y, 122, 104);

                    if (playerRectangle.Intersects(meteorRectangle))
                    {
                        collision.playerCollided = true;
                        collision.target = enemy;
                        lives = lives - 1;
                    }

                    if (player.isFire())
                    {
                        if (missleRectangle.Intersects(meteorRectangle))
                        {
                            collision.bulletCollided = true;
                            collision.target = enemy;
                            score = score + 1;
                        }
                    }
                }
            }
            return collision;
        }
    }
}
