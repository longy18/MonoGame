using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;


// Thanks to Ziggyware tutorial found in: http://www.ziggyware.com/readarticle.php?article_id=72
namespace Microsoft.Xna.Samples.Storage
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;		


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
			
			graphics.IsFullScreen = true;		
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
			
			// Save Game Data
			SaveGameStorage storage = new SaveGameStorage();
			
			SaveGame sg = new SaveGame();

            sg.Name = "Ziggy";
            sg.HiScore = 1000;
            sg.Date = DateTime.Now;
            sg.DontKeep = 123;
			
			storage.Save(sg);
			
			//load the data back in to test if it was successful
            SaveGame loaded = storage.Load();

            Console.WriteLine("Name: " + loaded.Name);
            Console.WriteLine("Hi Score: " + loaded.HiScore.ToString());
            Console.WriteLine("Date: " + loaded.Date.ToString());
            Console.WriteLine("Dont Keep: " + loaded.DontKeep.ToString());			
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			
            base.Draw(gameTime);
        }
    }
}
