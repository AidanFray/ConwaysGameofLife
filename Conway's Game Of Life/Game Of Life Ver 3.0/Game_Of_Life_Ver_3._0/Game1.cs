using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading;

namespace Game_Of_Life_Ver_3._0
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //

        //####################### SIZE OF THE GRID ###############################
        int GRID_SIZE = 800; //Unit is pixels
       
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";


            //FULLSCREEN
            graphics.PreferredBackBufferWidth = GRID_SIZE;
            graphics.PreferredBackBufferHeight = GRID_SIZE;
            //graphics.IsFullScreen = true;
            //

            //ALLOWS THE MOUSE TO BE VISIBLE
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            Window.Title = "Game of Life";


            //-------------------------BUTTON-----------------------------------
           

            //RESETS ALL BUTTONS
            for (int i = 0; i < NUMBER_OF_BUTTONS; i++)
            {
                bState[i] = buttonState.UP;
                button_color[i] = Color.White;
                button_timer[i] = 0.0;
                if (i == RESET_BUTTON_INDEX)
                {
                    int x = GRID_SIZE - 100;
                    int y = GRID_SIZE - 50;
                    button_rectangle[i] = new Rectangle(x, y, BUTTON_WIDTH, BUTTON_HEIGHT);
                }

                if (i == START_BUTTON_INDEX)
                {
                    int x = GRID_SIZE - 100;
                    int y = GRID_SIZE - 95;
                    button_rectangle[i] = new Rectangle(x, y, BUTTON_WIDTH, BUTTON_HEIGHT);
                }
            }
            //


            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //LOADS THE NODE PICTURE
            cellTexture = Content.Load<Texture2D>("NODE");

            //BUTTON
            buttonTEX[RESET_BUTTON_INDEX] = Content.Load<Texture2D>("BUTTONS/reset_button");
            buttonTEX[START_BUTTON_INDEX] = Content.Load<Texture2D>("BUTTONS/start_button");

            //FONT
            font = Content.Load<SpriteFont>("SP1");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();

            mouseCheck();

            keyboardCheck();
            
            // GETS THE ELAPSED TIME
            frame_time = gameTime.ElapsedGameTime.Milliseconds / 1000.0;

            //MOUSE RELATED VARS
            MouseState mouse_state = Mouse.GetState();
            MouseX = mouse_state.X;
            MouseY = mouse_state.Y;
            MouseR = MouseP;
            MouseP = mouse_state.LeftButton == ButtonState.Pressed;

            update_buttons();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateBlue);

            drawAliveCells(aliveCells);

            //STARTS THE GAME LOGIC
            if (start == true)
            {
                activeGOL();
            }
            //
            spriteBatch.Begin();
            string intStr = "" + aliveCells.Count;
            spriteBatch.DrawString(font, intStr, new Vector2(5,0), Color.White);
            spriteBatch.End();


            spriteBatch.Begin();
            //BUTTONS
            for (int i = 0; i < NUMBER_OF_BUTTONS; i++)
            {
                spriteBatch.Draw(buttonTEX[i], button_rectangle[i], button_color[i]);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

       
        
        //============================================= BUTTONS ==========================================
        // wrapper for hit_image_alpha taking Rectangle and Texture
        Boolean hit_image_alpha(Rectangle rect, Texture2D tex, int x, int y)
        {

            return hit_image_alpha(0, 0, tex,
                                   x: tex.Width * (x - rect.X) / rect.Width,
                                   y: tex.Height * (y - rect.Y) / rect.Height);
        }

        // wraps hit_image then determines if hit a transparent part of image 
        Boolean hit_image_alpha(float tx, float ty, Texture2D tex, int x, int y)
        {
            if (hit_image(tx, ty, tex, x, y))
            {
                // MAKES AN ARRAY WITH AS MANY PIXELS AS THE TEXTURE CONTAINS
                uint[] data = new uint[tex.Width * tex.Height];

                //?
                tex.GetData<uint>(data);
                
                if ((x - (int)tx) + (y - (int)ty) * tex.Width < tex.Width * tex.Height)
                {
                    return ((data[(x - (int)tx) + (y - (int)ty) * tex.Width] & 0xFF000000) >> 24) > 20;
                }
            }
            return false;
        }

        // determine if x,y is within rectangle formed by texture located at tx,ty
        Boolean hit_image(float tx, float ty, Texture2D tex, int x, int y)
        {
            return (x >= tx &&
                x <= tx + tex.Width &&
                y >= ty &&
                y <= ty + tex.Height);
        }

        // determine state and color of button
        void update_buttons()
        {
            //LOOPS ROUND THE BUTTONS
            for (int i = 0; i < NUMBER_OF_BUTTONS; i++)
            {
                //IF THE MOUSE IS WITHIN THE BUTTON AREA
                if (hit_image_alpha(button_rectangle[i], buttonTEX[i], MouseX, MouseY) == true)
                {
                    button_timer[i] = 0.0;
                    if (MouseP == true) //IF THE BUTTON IS PRESSED
                    {
                        // mouse is currently down
                        bState[i] = buttonState.CLICKED;
                        button_color[i] = Color.Blue;
                    }
                    else if (!MouseP && MouseR)
                    {
                        // mouse was just released
                        if (bState[i] == buttonState.CLICKED)
                        {
                            // button was just down
                            bState[i] = buttonState.JUST_RELEASED;
                        }
                    }
                    else
                    {
                        bState[i] = buttonState.HOVER;
                        button_color[i] = Color.LightBlue;
                    }
                }
                else
                {
                    bState[i] = buttonState.UP;
                    if (button_timer[i] > 0)
                    {
                        button_timer[i] = button_timer[i] - frame_time;
                    }
                    else
                    {
                        button_color[i] = Color.White;
                    }
                }

                //WHEN  THE USER STOPS THE CLICK
                if (bState[i] == buttonState.JUST_RELEASED)
                {
                    take_action_on_button(i);
                }
            }
        }

        // Logic for each button click goes here
        void take_action_on_button(int i)
        {
            //take action corresponding to which button was clicked
            switch (i)
            {
                case RESET_BUTTON_INDEX:
                    aliveCells.Clear();
                    deadCellsToCheck.Clear();
                    start = false;
                    break;
                case START_BUTTON_INDEX:
                    start = true;
                    activeGOL();
                    break;
                default:
                    break;
            }
        }

        //===================================================================================================

        //SET SHAPE FOR CREATING GLIDERS
        private void createGosperGun()
        {

        }
    }
}
