using Cosmos.System;
using Cosmos.System.Graphics;
using System.Drawing;
using System;

namespace CosmosOS_Learning
{
    internal class ImageButton
    {
        /* VARIABLES */
        public Action PressedAction = new Action(() => { });
        public Canvas ScreenCanvas;
        public Bitmap ButtonImage;
        public Color ButtonPressedOverlay = Color.FromArgb(32, Color.Gray.R, Color.Gray.G, Color.Gray.B);
        public Color BackColorPressed = Color.Transparent;
        public Color BackColor = Color.Transparent;
        public Point ButtonBottomRight = new Point(0, 0);
        public Point ButtonPosition = new Point(0, 0);
        public bool DrawWithAlphaChannel = true;
        public bool RequireMouseHeld = false;
        public bool ActionCalled = false;
        public bool Toggled = false;

        /* FUNCTIONS */
        public void Draw()
        {
            // Calculate where the bottom right cornet of the button is
            ButtonBottomRight = new Point((int)(ButtonPosition.X + ButtonImage.Width), (int)(ButtonPosition.Y + ButtonImage.Height));

            // Draw the button at the specified position
            if (IsPressed() || Toggled)
            {
                ScreenCanvas.DrawFilledRectangle(BackColorPressed, ButtonPosition.X, ButtonPosition.Y, (int)ButtonImage.Width, (int)ButtonImage.Height);
                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonPosition.Y);
                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y);
                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X, ButtonBottomRight.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
                ScreenCanvas.DrawLine(Color.White, ButtonBottomRight.X, ButtonPosition.Y + 1, ButtonBottomRight.X, ButtonBottomRight.Y);

                if (ActionCalled == false)
                {
                    PressedAction.Invoke();
                    ActionCalled = true;

                    if (RequireMouseHeld)
                        Toggled = !Toggled;
                }
            }
            else
            {
                ScreenCanvas.DrawFilledRectangle(BackColor, ButtonPosition.X, ButtonPosition.Y, (int)ButtonImage.Width, (int)ButtonImage.Height);
                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X, ButtonPosition.Y, ButtonBottomRight.X, ButtonPosition.Y);
                ScreenCanvas.DrawLine(Color.White, ButtonPosition.X - 1, ButtonPosition.Y + 1, ButtonPosition.X - 1, ButtonBottomRight.Y);
                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X, ButtonBottomRight.Y, ButtonBottomRight.X, ButtonBottomRight.Y);
                ScreenCanvas.DrawLine(Color.Black, ButtonPosition.X + (int)ButtonImage.Width, ButtonPosition.Y + 1, ButtonBottomRight.X, ButtonBottomRight.Y);
            }

            // Draw the image
            if (DrawWithAlphaChannel)
            {
                ScreenCanvas.DrawImageAlpha(ButtonImage, ButtonPosition.X, ButtonPosition.Y);
            }
            else
            {
                ScreenCanvas.DrawImage(ButtonImage, ButtonPosition.X, ButtonPosition.Y);
            }

        }

        public bool IsPressed()
        {
            if (MouseManager.LastMouseState == MouseManager.MouseState || MouseManager.MouseState != MouseState.Left)
            {                
                ActionCalled = false;
                return false;
            }

            return (ButtonPosition.X <= MouseManager.X && MouseManager.X <= ButtonBottomRight.X) && 
                (ButtonPosition.Y <= MouseManager.Y && MouseManager.Y <= ButtonBottomRight.Y);
        }
    }
}
