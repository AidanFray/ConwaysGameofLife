using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Of_Life_Ver_3._0
{
    class GameOfLifeLogic
    {
        //GLOBAL VARS
        private Texture2D cellTexture; //TEXTURE OF AN ALIVE CELL
        private SpriteFont font;

        bool start = false; //BOOL THAT TELLS THE PROGRAM THE GAME HAS STARTED

        //LIST TO HOLD CHECKS
        List<Vector2> aliveCells = new List<Vector2>(); //HOLDS ALL THE CELLS TO BE DRAW I.E ALIVE CELLS
        List<Vector2> deadCellsToCheck = new List<Vector2>();
        List<Vector2> changeList = new List<Vector2>();
        List<Vector2> saveForLater = new List<Vector2>();


        //MOUSE VARS
        MouseState oldMouseState; //STORES THE PREVIOUS MOUSE STATE, USED TO STOP MUTIPLE CLICKING IN A ROW                   
        MouseState stateOfMouse; //CURRENT STATE OF THE MOUSE

        //CHECKS FOR KEYBOARD INPUT
        KeyboardState oldKeyInput;

        //-----------------------------------------------BUTTONS---------------------------------------------------
        //ENUM RELATED TO BUTTON STATE
        enum buttonState
        {
            HOVER,
            CLICKED,
            UP,
            JUST_RELEASED
        }

        //THESE CAN BE CHANGED TO ADD MORE BUTTONS
        const int NUMBER_OF_BUTTONS = 2,
                  BUTTON_HEIGHT = 37,
                  BUTTON_WIDTH = 90;
        //#########################################

        //ADDING NEW BUTTONS
        //--CHANGE NUMBER_OF_BUTTONS
        //--ADD A BUTTON INDEX BELLOW
        //--ADD A LOAD TEXTURE LINE TO LoadContent()
        //--ADD A LINE TO THE Initialize()
        //--ADD CODE TO THE take_action_on_button()


        //ADD BUTTON INDEXS
        const int RESET_BUTTON_INDEX = 0;
        const int START_BUTTON_INDEX = 1;
        //

        //ATTRIBUTES
        Color[] button_color = new Color[NUMBER_OF_BUTTONS];
        Rectangle[] button_rectangle = new Rectangle[NUMBER_OF_BUTTONS];
        buttonState[] bState = new buttonState[NUMBER_OF_BUTTONS];
        Texture2D[] buttonTEX = new Texture2D[NUMBER_OF_BUTTONS];
        double[] button_timer = new double[NUMBER_OF_BUTTONS];

        //MOUSE PRESSED AND RELEASED
        bool MouseP, MouseR = false;
        //MOUSE LOCATION
        int MouseX, MouseY;
        double frame_time;

        //USED TO DRAW THE ALIVE CELLS
        private void drawAliveCells(List<Vector2> aliveCells)
        {
            spriteBatch.Begin();
            foreach (Vector2 i in aliveCells)
            {
                spriteBatch.Draw(cellTexture, i, Color.White);
            }
            spriteBatch.End();
        }

        //USED TO CHECK AND DEAL WITH USER INPUT
        private void mouseCheck()
        {
            if (start == false) //MAKES SURE THE USER CANNOT CLICK WHEN GAME IS RUNNING
            {
                oldMouseState = stateOfMouse;

                stateOfMouse = Mouse.GetState();  //MOUSE CLICK

                //IF THE MOUSE IS CLICKED
                if (stateOfMouse.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
                {
                    //MAKES SURE THE ADDED SQAURE IS WITHIN A 4x4 GRID
                    int gridLockX = (stateOfMouse.X / 4) * 4;
                    int gridLockY = (stateOfMouse.Y / 4) * 4;

                    Vector2 positionOfMouse = new Vector2(gridLockX, gridLockY);

                    bool Checked = false;
                    //IF THE USER CLICKS A BLANK AREA
                    if (aliveCells.Contains(positionOfMouse) == false && Checked == false)
                    {
                        if (IsActive == true) //USED TO CHECK IF THE USER IS CLICKING ON THE GAME
                        {
                            aliveCells.Add(positionOfMouse);
                            addSurroudingCells(positionOfMouse);
                            Checked = true;
                        }
                    }

                    //IF THE USER CLICKS ON AN ALIVE CELL
                    if (aliveCells.Contains(positionOfMouse) == true && Checked == false)
                    {
                        aliveCells.Remove(positionOfMouse);
                        Checked = true;
                    }
                }
                //

            }
        }


        private void keyboardCheck()
        {
            KeyboardState keyState = Keyboard.GetState();

            //SPACE CLICKED
            if (keyState.IsKeyDown(Keys.Space))
            {
                if (!oldKeyInput.IsKeyDown(Keys.Space)) //'!' AT THE START IS EQUAL TO '!= true'
                {
                    start = true;//STARTS THE GAME OF LIFE
                }
            }
            oldKeyInput = keyState;
        }


        //THE GAME LOGIC
        private void activeGOL()
        {

            //COPYING THE LIST 
            foreach (Vector2 y in aliveCells)
            {
                changeList.Add(y);
            }

            int aliveCount = 0;
            //


            //-----------------------------------ALIVE------------------------------------------
            foreach (Vector2 vec in aliveCells)
            {
                //CHECKS FOR ALIVE CELLS
                aliveCount += checkTopRow(vec);

                aliveCount += checkMiddleRow(vec);

                aliveCount += checkBottomRow(vec);


                //RULE 1
                if (aliveCount < 2)
                {
                    changeList.Remove(vec);
                    saveForLater.Add(vec);
                }


                //RULE 3
                if (aliveCount > 3)
                {
                    changeList.Remove(vec);
                    saveForLater.Add(vec);
                }
                //RESET
                aliveCount = 0;
            }

            //---------------------------------DEAD---------------------------------------
            foreach (Vector2 vecDead in deadCellsToCheck)
            {
                //MAKES SURE IT'S NOT CHECKING LIVE CELLS
                if (aliveCells.Contains(new Vector2(vecDead.X, vecDead.Y)) == false)
                {
                    //CHECKS FOR ALIVE CELLS
                    aliveCount += checkTopRow(vecDead);

                    aliveCount += checkMiddleRow(vecDead);

                    aliveCount += checkBottomRow(vecDead);
                    //
                }
                //RULE 2
                if (aliveCount == 3)
                {
                    changeList.Add(vecDead);
                }

                //RESET
                aliveCount = 0;
            }
            deadCellsToCheck.Clear();

            //UPDATES THE DEAD CELLS TO CHECK
            foreach (Vector2 x in changeList)
            {
                addSurroudingCells(x);
            }
            foreach (Vector2 x in saveForLater)
            {
                if (deadCellsToCheck.Contains(x) == false)
                {
                    deadCellsToCheck.Add(x);
                }

            }

            //COPYING THE ChangeList TO aliveCells
            aliveCells.Clear();
            foreach (Vector2 item in changeList)
            {
                if (aliveCells.Contains(item) == false)
                {
                    aliveCells.Add(item);
                }
            }

            //CLEARING
            changeList.Clear();
            saveForLater.Clear();
            //


            //---------------------------------BUTTONS--------------------------------------



        }

        //CHECKS FOR NEIGHBOURING ALIVE CELLS
        private int checkTopRow(Vector2 vec)
        {
            int aliveC = 0;

            //LEFT
            if (aliveCells.Contains(new Vector2(vec.X - 4, vec.Y - 4)))
            {
                aliveC++;
            }

            //MIDDLE
            if (aliveCells.Contains(new Vector2(vec.X, vec.Y - 4)))
            {
                aliveC++;
            }

            //RIGHT
            if (aliveCells.Contains(new Vector2(vec.X + 4, vec.Y - 4)))
            {
                aliveC++;
            }

            return aliveC;
        }
        private int checkMiddleRow(Vector2 vec)
        {
            int aliveC = 0;

            //LEFT
            if (aliveCells.Contains(new Vector2(vec.X - 4, vec.Y)))
            {
                aliveC++;
            }

            //RIGHT
            if (aliveCells.Contains(new Vector2(vec.X + 4, vec.Y)))
            {
                aliveC++;
            }
            return aliveC;
        }
        private int checkBottomRow(Vector2 vec)
        {
            int aliveC = 0;

            //LEFT
            if (aliveCells.Contains(new Vector2(vec.X - 4, vec.Y + 4)))
            {
                aliveC++;
            }

            //MIDDLE
            if (aliveCells.Contains(new Vector2(vec.X, vec.Y + 4)))
            {
                aliveC++;
            }

            //RIGHT
            if (aliveCells.Contains(new Vector2(vec.X + 4, vec.Y + 4)))
            {
                aliveC++;
            }
            return aliveC;
        }

        //ADDS THE CELLS AROUND THE ALIVE NODES THAT NEED TO BE CHECKED
        private void addSurroudingCells(Vector2 addCell)
        {
            //TOP LEFT
            if (aliveCells.Contains(new Vector2(addCell.X - 4, addCell.Y - 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X - 4, addCell.Y - 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X - 4, addCell.Y - 4));
                }
            }

            //TOP MIDDLE
            if (aliveCells.Contains(new Vector2(addCell.X, addCell.Y - 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X, addCell.Y - 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X, addCell.Y - 4));
                }

            }

            //TOP RIGHT
            if (aliveCells.Contains(new Vector2(addCell.X + 4, addCell.Y - 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X + 4, addCell.Y - 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X + 4, addCell.Y - 4));
                }
            }



            //MIDDLE LEFT
            if (aliveCells.Contains(new Vector2(addCell.X - 4, addCell.Y)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X - 4, addCell.Y)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X - 4, addCell.Y));
                }
            }

            //MIDDLE RIGHT
            if (aliveCells.Contains(new Vector2(addCell.X, addCell.Y - 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X, addCell.Y - 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X, addCell.Y - 4));
                }
            }



            //BOTTOM LEFT 
            if (aliveCells.Contains(new Vector2(addCell.X - 4, addCell.Y + 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X - 4, addCell.Y + 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X - 4, addCell.Y + 4));
                }
            }

            //BOTTOM MIDDLE 
            if (aliveCells.Contains(new Vector2(addCell.X, addCell.Y + 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X, addCell.Y + 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X, addCell.Y + 4));
                }
            }

            //BOTTOM RIGHT   
            if (aliveCells.Contains(new Vector2(addCell.X + 4, addCell.Y + 4)) == false)
            {
                if (deadCellsToCheck.Contains(new Vector2(addCell.X + 4, addCell.Y + 4)) == false)
                {
                    deadCellsToCheck.Add(new Vector2(addCell.X + 4, addCell.Y + 4));
                }
            }
        }
    }
}
