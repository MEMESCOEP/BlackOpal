using Cosmos.System.Graphics.Fonts;
using Cosmos.Core.Memory;
using Cosmos.System;
using Cosmos.Core;
using IL2CPU.API.Attribs;
using System.Drawing;
using System.Text;
using System;
using BlackOpal.GUI.Component;
using GUI.Component;
using IO.CMD;
using PrismAPI.Hardware.GPU;
using PrismAPI.Graphics;
using Power = Cosmos.System.Power;
using Kernel = BlackOpal.Kernel;
using Color = PrismAPI.Graphics.Color;

namespace GUI
{
    internal class UserInterface
    {
        /* VARIABLES */
        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.Mouse.bmp")]
        static byte[] MouseCursorArray;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.Wallpaper.bmp")]
        static byte[] WallpaperArray;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.Taskbar.bmp")]
        static byte[] TaskbarArray;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.Logo.bmp")]
        static byte[] LogoArray;

        private PCScreenFont Font = PCScreenFont.Default;
        private ImageButton LogoButton = new();
        private TextButton NewWindowButton = new();
        private TextButton ShutdownButton = new();
        private TextButton RestartButton = new();
        private TextButton ExitButton = new();
        private Canvas MouseCursor = Image.FromBitmap(MouseCursorArray);
        private Canvas Wallpaper = Image.FromBitmap(WallpaperArray);
        private Canvas Taskbar = Image.FromBitmap(TaskbarArray);
        private Canvas Logo = Image.FromBitmap(LogoArray);
        private double UsedMemPerentage = 0;
        private string CurrentTime = string.Empty, CurrentDate = string.Empty;
        private bool DrawStartMenu = false, DrawGUI = true;
        public static Display ScreenCanvas;
        public static Point ClickPoint = new Point(0, 0);
        public static ushort ScreenWidth = 1024, ScreenHeight = 768;

        /* FUNCTIONS */
        public void Init()
        {
            try
            {
                // Initialize the canvas
                ConsoleFunctions.PrintLogMSG($"Initializing the canvas ({ScreenWidth}x{ScreenHeight}@32)...\n\r", ConsoleFunctions.LogType.INFO);

                // Choose the best driver for the current display hardware
                ConsoleFunctions.PrintLogMSG($"Determining the best video driver...\n\r", ConsoleFunctions.LogType.INFO);
                ScreenCanvas = Display.GetDisplay(ScreenWidth, ScreenHeight);
                ConsoleFunctions.PrintLogMSG($"Using the \"{ScreenCanvas.GetName()}\" driver in {ScreenWidth}x{ScreenHeight}@32 mode.\n\r", ConsoleFunctions.LogType.INFO);

                // Create text buttons and set their properties
                ConsoleFunctions.PrintLogMSG($"Creating text buttons...\n\r", ConsoleFunctions.LogType.INFO);
                ExitButton.ButtonText = "Exit GUI";
                ExitButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 64));
                ExitButton.ScreenCanvas = ScreenCanvas;
                ExitButton.PressedAction = new Action(() => { DrawGUI = false; });

                ShutdownButton.ButtonText = "Shutdown";
                ShutdownButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 92));
                ShutdownButton.ScreenCanvas = ScreenCanvas;
                ShutdownButton.PressedAction = new Action(() => { Power.Shutdown(); });

                RestartButton.ButtonText = "Restart";
                RestartButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 120));
                RestartButton.ScreenCanvas = ScreenCanvas;
                RestartButton.PressedAction = new Action(() => { Power.Reboot(); });

                NewWindowButton.ButtonText = "New Window";
                NewWindowButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 148));
                NewWindowButton.ScreenCanvas = ScreenCanvas;
                NewWindowButton.PressedAction = new Action(() => { 
                    Window NewWindow = WindowManager.CreateNewWindow("Test Window", Color.LightGray, new Size(320, 200), new Point(64, 64));
                    WindowElement NewStringElement = new WindowElement();
                    WindowElement NewImageElement = new WindowElement();

                    NewStringElement.Type = WindowElement.ElementType.STRING;
                    NewStringElement.ElementData = Encoding.ASCII.GetBytes($"This is test window #{WindowManager.WindowList.Count}.");
                    NewStringElement.ElementPosition = new Point(5, 5);

                    NewImageElement.Type = WindowElement.ElementType.IMAGE;
                    NewImageElement.ElementData = LogoArray;
                    NewImageElement.ElementPosition = new Point(5, 25);

                    NewWindow.WindowElements.Add(NewStringElement);
                    NewWindow.WindowElements.Add(NewImageElement);
                });

                // Create image buttons and set their properties
                ConsoleFunctions.PrintLogMSG($"Creating image buttons...\n\r", ConsoleFunctions.LogType.INFO);
                LogoButton.ButtonImage = Logo;
                LogoButton.ButtonPosition = new Point(6, (ScreenHeight - Logo.Height - 4));
                LogoButton.ScreenCanvas = ScreenCanvas;
                LogoButton.BackColor = Color.LightGray;
                LogoButton.BackColorPressed = Color.DeepGray;
                LogoButton.HighlightedBackColor = Color.LightGray;
                LogoButton.PressedAction = new Action(() => { DrawStartMenu = !DrawStartMenu; });
                LogoButton.RequireMouseHeld = false;

                // Initialize the mouse manager
                ConsoleFunctions.PrintLogMSG("Initializing the mouse and setting its properties...\n\r", ConsoleFunctions.LogType.INFO);
                MouseManager.MouseSensitivity = 1;
                MouseManager.ScreenWidth = ScreenWidth;
                MouseManager.ScreenHeight = ScreenHeight;
                MouseManager.X = (uint)ScreenWidth / 2;
                MouseManager.Y = (uint)ScreenHeight / 2;

                // Set the window manager's canvas
                ConsoleFunctions.PrintLogMSG("Configuring the window manager...\n\r", ConsoleFunctions.LogType.INFO);
                WindowManager.ScreenCanvas = ScreenCanvas;

                // Infinite draw loop
                ConsoleFunctions.PrintLogMSG($"Init done. Drawing...\n\r", ConsoleFunctions.LogType.INFO);
                for(;;) 
                {
                    if (DrawGUI == false)
                        break;

                    try
                    {
                        // Get the used RAM
                        Kernel.UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                        UsedMemPerentage = BlackOpal.Calculations.MathHelpers.TruncateToDecimalPlace((Kernel.UsedRAM / Kernel.TotalInstalledRAM) * 100f, 4);

                        // Get the current date and time
                        CurrentTime = DateTime.Now.ToLongTimeString();
                        CurrentDate = DateTime.Now.ToShortDateString();

                        // Get the point where the mouse was clicked
                        if (MouseManager.LastMouseState != MouseState.Left && MouseManager.MouseState == MouseState.Left)
                        {
                            ClickPoint.X = (int)MouseManager.X;
                            ClickPoint.Y = (int)MouseManager.Y;
                        }

                        // Draw the wallpaper
                        ScreenCanvas.DrawImage(0, 0, Wallpaper, false);

                        // Draw any windows
                        WindowManager.DrawWindows();

                        // Draw the taskbar
                        ScreenCanvas.DrawImage(0, (ScreenHeight - Taskbar.Height), Taskbar, false);
                        LogoButton.Draw();

                        // Draw date and time
                        ScreenCanvas.DrawString((ScreenWidth - CurrentTime.Length * 8) - 4, (ScreenHeight - 32), CurrentTime, default, Color.White);
                        ScreenCanvas.DrawString((ScreenWidth - CurrentDate.Length * 8) - 4, (ScreenHeight - 18), CurrentDate, default, Color.White);

                        // Draw the start menu if required
                        if (DrawStartMenu)
                        {
                            ScreenCanvas.DrawFilledRectangle(0, (ScreenHeight - 288), 128, 256, 0, Color.LighterBlack);
                            ScreenCanvas.DrawRectangle(0, (ScreenHeight - 288), 128, 256, 0, Color.Black);

                            NewWindowButton.Draw();
                            ShutdownButton.Draw();
                            RestartButton.Draw();
                            ExitButton.Draw();
                        }

                        // Draw the uptime and memory usage
                        ScreenCanvas.DrawString(0, 0, $"{Kernel.OSName} {Kernel.OSVersion} - {Kernel.OSDate}", default, Color.Black);
                        ScreenCanvas.DrawString(0, 12, $"MOUSE POS: X={MouseManager.X}, Y={MouseManager.Y}", default, Color.Black);
                        ScreenCanvas.DrawString(0, 24, $"UPTIME: {DateTime.Now - Kernel.KernelStartTime}", default, Color.Black);
                        ScreenCanvas.DrawString(0, 36, $"DRIVER: {ScreenCanvas.GetName()}", default, Color.Black);
                        ScreenCanvas.DrawString(0, 48, $"MEM: {Kernel.UsedRAM}/{Kernel.TotalInstalledRAM} KB ({UsedMemPerentage}%)", default, Color.Black);
                        ScreenCanvas.DrawString(0, 64, $"FPS: {ScreenCanvas.GetFPS()}", default, Color.Black);

                        // Draw the mouse pointer bitmap at the mouse position
                        ScreenCanvas.DrawImage((int)MouseManager.X, (int)MouseManager.Y, MouseCursor);

                        // Copy the back frame buffer to the visible frame buffer
                        ScreenCanvas.Update();

                        // Exit the GUI if the user presses escape
                        if (System.Console.KeyAvailable)
                        {
                            switch (System.Console.ReadKey().Key)
                            {
                                case ConsoleKey.Escape:
                                    DrawGUI = false;
                                    break;

                                case ConsoleKey.LeftWindows:
                                case ConsoleKey.RightWindows:
                                    DrawStartMenu = !DrawStartMenu;
                                    break;
                            }
                        }

                        // Call the garbage collector
                        Heap.Collect();
                    }
                    catch(Exception ex)
                    {
                        ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\r", ConsoleFunctions.LogType.ERROR);
                    }
                }

                // Cleanup
                ConsoleFunctions.PrintLogMSG("Disabling the GUI...\n\r", ConsoleFunctions.LogType.INFO);
                Kernel.Terminal.Clear();
            }
            catch (Exception ex)
            {
                if (ScreenCanvas != null && ScreenCanvas.IsEnabled)
                    ScreenCanvas.IsEnabled = false;

                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\r", ConsoleFunctions.LogType.ERROR);
                DrawGUI = false;
            }
        }
    }
}
