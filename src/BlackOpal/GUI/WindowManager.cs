using Cosmos.System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BlackOpal.GUI.Component;
using GUI.Component;
using PrismAPI.Hardware.GPU;
using PrismAPI.Graphics;
using Color = PrismAPI.Graphics.Color;
using Cosmos.Core;

namespace GUI
{
    internal class WindowManager
    {
        public static List<Window> WindowList = new List<Window>();
        public static Display ScreenCanvas;
        public static bool FocusingWindow = false;
        private static Color TitlebarColor = new Color(139, 0, 139);

        // Create a new window
        public static Window CreateNewWindow(string Title, Color BGColor, Size WindowSize, Point WindowPosition)
        {
            Window NewWindow = new Window(Title, BGColor, WindowSize, WindowPosition, (uint)(WindowList.Count + 1));
            WindowList.Add(NewWindow);
            return NewWindow;
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
                ScreenCanvas.DrawFilledRectangle(window.Position.X, window.Position.Y + 24, (ushort)window.Size.Width, (ushort)window.Size.Height, 0, window.WindowBGColor);
                
                if (window.DrawTitleBar)
                {
                    if (WindowList.IndexOf(window) == WindowList.Count - 1)
                    {
                        if (TitlebarColor.ARGB != window.UnfocusedTitlebarColor.ARGB)
                        {
                            TitlebarColor = window.UnfocusedTitlebarColor;
                            //window.Title = TitlebarColor.ARGB.ToString("X") + "||" + ScreenCanvas.GetFPS().ToString();
                        }
                    }
                    else if (TitlebarColor.ARGB != window.FocusedTitlebarColor.ARGB)
                    {
                        TitlebarColor = window.FocusedTitlebarColor;
                        //window.Title = TitlebarColor.ARGB.ToString("X") + "||" + ScreenCanvas.GetFPS().ToString();
                    }

                    ScreenCanvas.DrawFilledRectangle(window.Position.X, window.Position.Y, (ushort)window.Size.Width, 24, 0, TitlebarColor);
                }

                // Horizontal border lines
                ScreenCanvas.DrawLine(window.Position.X, window.Position.Y + 24, window.Position.X + window.Size.Width, window.Position.Y + 24, Color.Black);
                ScreenCanvas.DrawLine(window.Position.X, window.Position.Y, window.Position.X + window.Size.Width, window.Position.Y, Color.White);
                ScreenCanvas.DrawFilledRectangle(window.Position.X + 4, window.Position.Y + window.Size.Height + 24, (ushort)(window.Size.Width - 4), 4, 0, Color.Black);

                // Vertical border lines
                ScreenCanvas.DrawLine(window.Position.X, window.Position.Y, window.Position.X, window.Position.Y + window.Size.Height + 24, Color.White);
                ScreenCanvas.DrawFilledRectangle(window.Position.X + window.Size.Width, window.Position.Y + 4, 4, (ushort)(window.Size.Height + 24), 0, Color.Black);

                // Close button
                window.WindowCloseButton.ButtonPosition.X = (window.Position.X + window.Size.Width) - 22;
                window.WindowCloseButton.ButtonPosition.Y = window.Position.Y + 4;
                window.WindowCloseButton.Draw();

                // Window title
                ScreenCanvas.DrawString(window.Position.X + 4, window.Position.Y + 4, window.Title, default, Color.Black);

                // Window elements
                foreach (var Element in window.WindowElements)
                {
                    if (Element.Type == WindowElement.ElementType.STRING)
                    {
                        ScreenCanvas.DrawString(window.Position.X + Element.ElementPosition.X, window.Position.Y + 24 + Element.ElementPosition.Y, Encoding.ASCII.GetString(Element.ElementData), default, Element.ElementColor);
                    }

                    // This is causing memory leaks; I believe it's due to the Image.FromBitmap function
                    if (Element.Type == WindowElement.ElementType.IMAGE)
                    {
                        var ImageData = Image.FromBitmap(Element.ElementData);
                        ScreenCanvas.DrawImage(window.Position.X + Element.ElementPosition.X, window.Position.Y + 24 + Element.ElementPosition.Y, ImageData);
                        GCImplementation.Free(ImageData);
                        //ScreenCanvas.DrawImage(window.Position.X + Element.ElementPosition.X, window.Position.Y + 24 + Element.ElementPosition.Y, Image.FromBitmap(Element.ElementData));
                    }
                }

                window.CheckControls();
                window.CheckDrag();
                window.CheckFocus();
            }

            /*for (var i = WindowList.Count - 1; i >= 0; i--)
            {
                var window = WindowList[i];

                window.CheckControls();
                window.CheckDrag();
                if (window.CheckFocus())
                {
                    break;
                }
            }*/
        }
    }
}
