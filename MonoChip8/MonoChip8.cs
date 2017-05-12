using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoChip8.Chip8;

namespace MonoChip8
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class MonoChip8 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D canvas;

        private readonly CPU Chip8;
        
        public MonoChip8()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 640;
            graphics.ApplyChanges();

            Chip8 = new CPU();
            Chip8.LoadGame(Path.Combine(this.Content.RootDirectory, "Games/INVADERS"));
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

            canvas = new Texture2D(GraphicsDevice, 64, 32);

            // TODO: use this.Content to load your game content here

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            for (int i = 0; i < 14; i++)
            {
                Chip8.Step();
            }

            if (Chip8.DrawFlag)
            {
                Color[] data = new Color[64 * 32];
                for (int y = 0; y < 32; ++y)
                {
                    for (int x = 0; x < 64; ++x)
                    {
                        if (Chip8.Graphics[(y * 64) + x] == 0)
                        {
                            data[(y * 64) + x] = Color.Black;
                        }
                        else
                        {
                            data[(y * 64) + x] = Color.White;
                        }
                    }
                }
                canvas.SetData(data);
            }

            Chip8.Key[0x1] = (byte) (Keyboard.GetState().IsKeyDown(Keys.NumPad1) ? 1 : 0);
            Chip8.Key[0x2] = (byte)(Keyboard.GetState().IsKeyDown(Keys.NumPad2) ? 1 : 0);
            Chip8.Key[0x3] = (byte)(Keyboard.GetState().IsKeyDown(Keys.NumPad3) ? 1 : 0);
            Chip8.Key[0xC] = (byte)(Keyboard.GetState().IsKeyDown(Keys.NumPad4) ? 1 : 0);
            Chip8.Key[0x4] = (byte)(Keyboard.GetState().IsKeyDown(Keys.Q) ? 1 : 0);
            Chip8.Key[0x5] = (byte)(Keyboard.GetState().IsKeyDown(Keys.W) ? 1 : 0);
            Chip8.Key[0x6] = (byte)(Keyboard.GetState().IsKeyDown(Keys.E) ? 1 : 0);
            Chip8.Key[0xD] = (byte)(Keyboard.GetState().IsKeyDown(Keys.R) ? 1 : 0);
            Chip8.Key[0x7] = (byte)(Keyboard.GetState().IsKeyDown(Keys.A) ? 1 : 0);
            Chip8.Key[0x8] = (byte)(Keyboard.GetState().IsKeyDown(Keys.S) ? 1 : 0);
            Chip8.Key[0x9] = (byte)(Keyboard.GetState().IsKeyDown(Keys.D) ? 1 : 0);
            Chip8.Key[0xE] = (byte)(Keyboard.GetState().IsKeyDown(Keys.F) ? 1 : 0);
            Chip8.Key[0xA] = (byte)(Keyboard.GetState().IsKeyDown(Keys.Z) ? 1 : 0);
            Chip8.Key[0x0] = (byte)(Keyboard.GetState().IsKeyDown(Keys.X) ? 1 : 0);
            Chip8.Key[0xB] = (byte)(Keyboard.GetState().IsKeyDown(Keys.C) ? 1 : 0);
            Chip8.Key[0xC] = (byte)(Keyboard.GetState().IsKeyDown(Keys.V) ? 1 : 0);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(canvas, new Rectangle(0, 0, 1280, 640), Color.White);
            spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
