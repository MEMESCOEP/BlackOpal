using Cosmos.System;
using Cosmos.System.Graphics;
using GUI.Component;
using System.Collections.Generic;
using System.Drawing;

namespace GUI
{
    internal class WindowManager
    {
        public static List<Window> WindowList = new List<Window>();
        public static Canvas ScreenCanvas;
        public static bool BreakDraw = false;
        public static bool FocusingWindow = false;

        // Create a new window
        public static void CreateNewWindow(string Title, Color BGColor, Size WindowSize, Point WindowPosition)
        {
            WindowList.Add(new Window(Title, BGColor, WindowSize, WindowPosition, (uint)(WindowList.Count + 1)));
        }

        // Loop through all windows and draw them (if there are any of course)
        public static void DrawWindows()
        {
            if (WindowList.Count <= 0)
                return;

            if(MouseManager.MouseState != MouseState.Left)
            {
                FocusingWindow = false;
            }

            foreach (var window in WindowList)
            {
                // Break if true so that we don't modify the list while accessing it. That would cause issues
                if (BreakDraw)
                {
                    //BreakDraw = false;
                    break;
                }

                ScreenCanvas.DrawFilledRectangle(window.WindowBGColor, window.Position.X, window.Position.Y + 24, window.Size.Width, window.Size.Height);
                ScreenCanvas.DrawFilledRectangle(Color.DarkMagenta, window.Position.X, window.Position.Y, window.Size.Width, 24);
                ScreenCanvas.DrawFilledCircle(Color.OrangeRed, (window.Position.X + window.Size.Width) - 12, window.Position.Y + 12, 6);
                //ScreenCanvas.DrawRectangle(Color.Black, window.Position.X, window.Position.Y, window.Size.Width, window.Size.Height + 24);
                ScreenCanvas.DrawLine(Color.Black, window.Position.X, window.Position.Y + 24, window.Position.X + window.Size.Width, window.Position.Y + 24);

                // Horizontal border lines
                ScreenCanvas.DrawLine(Color.White, window.Position.X, window.Position.Y, window.Position.X + window.Size.Width, window.Position.Y);
                ScreenCanvas.DrawLine(Color.Black, window.Position.X, window.Position.Y + window.Size.Height + 24, window.Position.X + window.Size.Width, window.Position.Y + window.Size.Height + 24);

                // Vertical border lines
                ScreenCanvas.DrawLine(Color.White, window.Position.X, window.Position.Y, window.Position.X, window.Position.Y + window.Size.Height + 24);
                ScreenCanvas.DrawLine(Color.Black, window.Position.X + window.Size.Width, window.Position.Y, window.Position.X + window.Size.Width, window.Position.Y + window.Size.Height + 24);

                ScreenCanvas.DrawString(window.Title, Cosmos.System.Graphics.Fonts.PCScreenFont.Default, Color.Black, window.Position.X + 4, window.Position.Y + 4);

                /*window.CheckDrag();
                window.CheckControls();
                window.CheckFocus();*/

                window.CheckControls();
            }

            for (var i = WindowList.Count - 1; i >= 0; i--)
            {
                // Break if true so that we don't modify the list while accessing it. That would cause issues
                if (BreakDraw)
                {
                    BreakDraw = false;
                    break;
                }

                WindowList[i].CheckDrag();
                WindowList[i].CheckFocus();
            }
        }
    }
}
