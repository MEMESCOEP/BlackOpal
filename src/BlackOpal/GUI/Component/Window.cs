using Cosmos.System;
using System.Drawing;
using System;
using BlackOpal.Calculations;

namespace GUI.Component
{
    internal class Window
    {
        /* VARIABLES */
        public Size Size = new Size(320, 200);
        public Point Position = new Point(0, 0);
        public Color WindowBGColor = Color.DarkGray;
        public string Title = "";
        public uint WindowID = 0;
        private bool IsBeingDragged = false;
        private int DragOffsetX = 0, DragOffsetY = 0;

        /* CONSTRUCTOR */
        public Window(string WindowTitle, Color BGColor, Size WindowSize, Point WindowPosition, uint ID)
        {
            Size = WindowSize;
            WindowBGColor = BGColor;
            WindowID = ID;
            Position = WindowPosition;
            Title = WindowTitle;
        }

        /* FUNCTIONS */
        // Check if the user is dragging the window
        public void CheckDrag()
        {
            // Don't drag if the left mouse button isn't held
            if (IsBeingDragged && MouseManager.MouseState != MouseState.Left || WindowManager.WindowList.IndexOf(this) != WindowManager.WindowList.Count - 1 || MouseManager.LastMouseState != MouseState.Left)
            {
                IsBeingDragged = false;
            }

            // If we just started dragging, calculate the window offsets
            else if (IsBeingDragged == false && MouseManager.MouseState == MouseState.Left && ShapeCollision.IsPointInsideRectangle((int)MouseManager.X, (int)MouseManager.Y, Position.X, Position.Y, Position.X + Size.Width, Position.Y + 24)) //Math.Abs(Position.X) <= MouseManager.X && MouseManager.X <= Math.Abs(Position.X + Size.Width) && Math.Abs(Position.Y) <= MouseManager.Y && MouseManager.Y <= Math.Abs(Position.Y + 24)
            {
                IsBeingDragged = true;
                DragOffsetX = (int)(MouseManager.X - Position.X);
                DragOffsetY = (int)(MouseManager.Y - Position.Y);
            }

            // If we're dragging, update the window position
            else if (IsBeingDragged)
            {
                Position.X = Math.Clamp((int)MouseManager.X - DragOffsetX, 0, (int)(GUI.ScreenWidth - 4));
                Position.Y = Math.Clamp((int)MouseManager.Y - DragOffsetY, 0, (int)(GUI.ScreenHeight - 4));
            }
        }

        // Check to see if the user is interacting with the window controls (minimize, maximize, close, etc)
        public void CheckControls()
        {
            if (MouseManager.MouseState == MouseState.Left && ShapeCollision.IsPointInsideCircle((int)MouseManager.X, (int)MouseManager.Y, (Position.X + Size.Width) - 12, Position.Y + 12, 6))
            {
                WindowManager.BreakDraw = true;
                WindowManager.WindowList.RemoveAt(WindowManager.WindowList.IndexOf(this));
            }
        }

        // Check to see if the user changed focus to the window
        public void CheckFocus()
        {
            //Title = $"{WindowManager.WindowList.IndexOf(this)}, {WindowManager.WindowList.Count - 1}";
            if (WindowManager.WindowList.IndexOf(this) == WindowManager.WindowList.Count - 1 || MouseManager.LastMouseState == MouseState.Left || WindowManager.FocusingWindow)
                return;

            if (MouseManager.MouseState == MouseState.Left && ShapeCollision.IsPointInsideRectangle((int)MouseManager.X, (int)MouseManager.Y, Position.X, Position.Y, Position.X + Size.Width, Position.Y + Size.Height))
            {
                WindowManager.BreakDraw = true;
                var oldIndex = WindowManager.WindowList.IndexOf(this);
                var item = WindowManager.WindowList[oldIndex];
                WindowManager.WindowList.RemoveAt(oldIndex);
                WindowManager.WindowList.Add(item);

                /*var oldIndex = WindowManager.WindowList.IndexOf(this);
                var item = WindowManager.WindowList[oldIndex];
                WindowManager.WindowList.RemoveAt(oldIndex);
                WindowManager.WindowList.Insert(0, item);*/

                WindowManager.FocusingWindow = true;
            }
        }
    }
}
