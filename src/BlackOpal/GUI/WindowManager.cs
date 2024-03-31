using Cosmos.System;
using System.Collections.Generic;
using System.Drawing;
using System;
using BlackOpal.GUI.Component;
using GUI.Component;
using GrapeGL.Hardware.GPU;
using GrapeGL.Graphics;
using Color = GrapeGL.Graphics.Color;

namespace GUI
{
    internal class WindowManager
    {
        public static Display ScreenCanvas;
        public static List<Window> WindowList = new List<Window>();
        public static bool FocusingWindow = false;
        public static uint FocusedWindowID = 9999;
        private static Color TitlebarColor = new Color(139, 0, 139);
        private static Random RNG = new Random();

        // Create a new window
        public static Window CreateNewWindow(string Title, Color BGColor, Size WindowSize, Point WindowPosition)
        {
            uint WindowID = (uint)RNG.Next(10000, 99999);

            // Make sure the window ID isn't already in use
            CheckWindowIDs:
                foreach (var WindowToCheck in WindowList)
                {
                    if (WindowToCheck.WindowID == WindowID)
                    {
                        WindowID = (uint)RNG.Next(10000, 99999);
                        goto CheckWindowIDs;
                    }
                }

            Window NewWindow = new Window(Title, BGColor, WindowSize, WindowPosition, WindowID);
            WindowList.Add(NewWindow);
            FocusedWindowID = WindowID;
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
                // Draw he background color
                window.Framebuffer.DrawFilledRectangle(0, 0, (ushort)window.Size.Width, (ushort)window.Size.Height, 0, window.WindowBGColor);

                // Window elements
                foreach (var Element in window.WindowElements)
                {
                    if (Element.Type == WindowElement.ElementType.STRING)
                    {
                        window.Framebuffer.DrawString(Element.ElementPosition.X, Element.ElementPosition.Y, (string)Element.ElementData, UserInterface.BIOSFont, Element.ElementColor);
                    }

                    else if (Element.Type == WindowElement.ElementType.IMAGE)
                    {
                        window.Framebuffer.DrawImage(Element.ElementPosition.X, Element.ElementPosition.Y, (Canvas)Element.ElementData);
                    }

                    else if (Element.Type == WindowElement.ElementType.TEXT_BUTTON)
                    {
                        var TextElement = (TextButton)Element.ElementData;
                        TextElement.DrawFromLocalPosition = true;
                        TextElement.UpdateScreenOnAction = false;
                        TextElement.CalculateBounds = false;
                        TextElement.ScreenCanvas = (Display)window.Framebuffer;
                        TextElement.ButtonGlobalPosition.X = window.Position.X + TextElement.ButtonLocalPosition.X;
                        TextElement.ButtonGlobalPosition.Y = window.Position.Y + 24 + TextElement.ButtonLocalPosition.Y;
                        TextElement.ButtonBottomRight.X = TextElement.ButtonGlobalPosition.X + TextElement.ButtonPixelLength;
                        TextElement.ButtonBottomRight.Y = TextElement.ButtonGlobalPosition.Y + 17;
                        TextElement.Draw();

                        //BlackOpal.Kernel.Terminal.SetCursorPosition(0, 0);
                        //BlackOpal.Kernel.Terminal.WriteLine($"{TextElement.ButtonGlobalPosition.X}, {TextElement.ButtonGlobalPosition.Y}, {TextElement.ButtonBottomRight.X}, {TextElement.ButtonBottomRight.Y}");
                        //ScreenCanvas.DrawFilledRectangle(TextElement.ButtonGlobalPosition.X, TextElement.ButtonGlobalPosition.Y, (ushort)(Math.Abs(TextElement.ButtonGlobalPosition.X - TextElement.ButtonBottomRight.X)), 17, 0, Color.Yellow);
                    }

                    else if (Element.Type == WindowElement.ElementType.IMAGE_BUTTON)
                    {
                        ((ImageButton)Element.ElementData).ScreenCanvas = (Display)window.Framebuffer;
                        ((ImageButton)Element.ElementData).Draw();
                    }
                }

                ScreenCanvas.DrawImage(window.Position.X, window.Position.Y + 24, window.Framebuffer, false);

                // Draw the titlebar if required
                if (window.DrawTitleBar)
                {
                    if (WindowList.IndexOf(window) == WindowList.Count - 1)
                    {
                        if (TitlebarColor.ARGB != window.UnfocusedTitlebarColor.ARGB)
                        {
                            TitlebarColor = window.UnfocusedTitlebarColor;
                            FocusedWindowID = window.WindowID;
                        }
                    }
                    else
                    {
                        if (TitlebarColor.ARGB != window.FocusedTitlebarColor.ARGB)
                            TitlebarColor = window.FocusedTitlebarColor;
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
                window.WindowCloseButton.ButtonLocalPosition.X = (window.Position.X + window.Size.Width) - 26;
                window.WindowCloseButton.ButtonLocalPosition.Y = window.Position.Y + 4;
                window.WindowCloseButton.ButtonGlobalPosition.X = window.WindowCloseButton.ButtonLocalPosition.X;
                window.WindowCloseButton.ButtonGlobalPosition.Y = window.WindowCloseButton.ButtonLocalPosition.Y;
                window.WindowCloseButton.Draw();

                // Window title
                ScreenCanvas.DrawString(window.Position.X + 6, window.Position.Y + 4, window.Title, UserInterface.BIOSFont, Color.StackOverflowWhite);

                //window.CheckControls();
                window.CheckDrag();
                window.CheckFocus();

                // Call the update action
                window.UpdateAction.Invoke();
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
