using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Graphics;
using Cosmos.Core.Memory;
using Cosmos.System;
using Cosmos.Core;
using IL2CPU.API.Attribs;
using System.Drawing;
using System;
using IO;

namespace CosmosOS_Learning
{
    internal class GUI
    {
        /* VARIABLES */
        [ManifestResourceStream(ResourceName = "CosmosOS-Learning.Assets.IMG.Mouse.bmp")]
        static byte[] MouseCursorArray;

        [ManifestResourceStream(ResourceName = "CosmosOS-Learning.Assets.IMG.Wallpaper.bmp")]
        static byte[] WallpaperArray;

        [ManifestResourceStream(ResourceName = "CosmosOS-Learning.Assets.IMG.Taskbar.bmp")]
        static byte[] TaskbarArray;

        [ManifestResourceStream(ResourceName = "CosmosOS-Learning.Assets.IMG.Logo.bmp")]
        static byte[] LogoArray;

        private PCScreenFont Font = PCScreenFont.Default;
        private Canvas ScreenCanvas;
        private uint ScreenWidth = 640, ScreenHeight = 480;
        private Bitmap MouseCursor = new(MouseCursorArray);
        private Bitmap Wallpaper = new(WallpaperArray);
        private Bitmap Taskbar = new(TaskbarArray);
        private Bitmap Logo = new(LogoArray);
        private double UsedMemPerentage = 0;
        private bool DrawStartMenu = false;
        private bool DrawGUI = true;
        private string CurrentTime = string.Empty;
        private string CurrentDate = string.Empty;
        private TextButton ExitButton = new();
        private TextButton RestartButton = new();
        private TextButton ShutdownButton = new();
        private ImageButton LogoButton = new();

        /* FUNCTIONS */
        public void Init()
        {
            try
            {
                // Initialize the canvas
                ConsoleFunctions.PrintLogMSG($"Initializing the canvas ({ScreenWidth}x{ScreenHeight})...\n", ConsoleFunctions.LogType.INFO);
                ScreenCanvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));

                ExitButton.ButtonText = "Exit GUI";
                ExitButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 64));
                ExitButton.ScreenCanvas = ScreenCanvas;
                ExitButton.PressedAction = new Action(() => { DrawGUI = false; });

                ShutdownButton.ButtonText = "Shutdown";
                ShutdownButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 92));
                ShutdownButton.ScreenCanvas = ScreenCanvas;
                ShutdownButton.PressedAction = new Action(() => { Power.Shutdown(); });

                RestartButton.ButtonText = "Restart";
                RestartButton.ButtonPosition = new Point(24, (int)(ScreenHeight - 128));
                RestartButton.ScreenCanvas = ScreenCanvas;
                RestartButton.PressedAction = new Action(() => { Power.Reboot(); });

                LogoButton.ButtonImage = Logo;
                LogoButton.ButtonPosition = new Point(6, (int)(ScreenHeight - (Logo.Height) - 4));
                LogoButton.ScreenCanvas = ScreenCanvas;
                LogoButton.BackColor = Color.Gray;
                LogoButton.BackColorPressed = Color.DimGray;
                LogoButton.PressedAction = new Action(() => { DrawStartMenu = !DrawStartMenu; });
                LogoButton.RequireMouseHeld = false;

                // Initialize the mouse manager
                ConsoleFunctions.PrintLogMSG("Initializing the mouse and setting its properties...\n", ConsoleFunctions.LogType.INFO);
                MouseManager.MouseSensitivity = 1;
                MouseManager.ScreenWidth = ScreenWidth;
                MouseManager.ScreenHeight = ScreenHeight;
                MouseManager.X = ScreenWidth / 2;
                MouseManager.Y = ScreenHeight / 2;

                // Infinite draw loop
                while (DrawGUI)
                {
                    // Get the used RAM
                    Kernel.UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                    UsedMemPerentage = (Kernel.UsedRAM / Kernel.TotalInstalledRAM) * 100f;

                    // Get the current date and time
                    CurrentTime = DateTime.Now.ToLongTimeString();
                    CurrentDate = DateTime.Now.ToShortDateString();

                    // Draw the wallpaper
                    ScreenCanvas.DrawImage(Wallpaper, 0, 0);

                    // Draw the taskbar
                    ScreenCanvas.DrawImage(Taskbar, 0, (int)(ScreenHeight - Taskbar.Height));
                    LogoButton.Draw();

                    // Draw the uptime and memory usage
                    ScreenCanvas.DrawString($"UPTIME: {(DateTime.Now - Kernel.KernelStartTime).ToString()}", Font, Color.Red, 0, 0);
                    ScreenCanvas.DrawString($"MEM: {Kernel.UsedRAM}/{Kernel.TotalInstalledRAM} KB ({UsedMemPerentage}%)", Font, Color.Red, 0, 12);

                    // Draw date and time
                    ScreenCanvas.DrawString(CurrentTime, Font, Color.White, (int)(ScreenWidth - (CurrentTime.Length * 8)) - 4, 444);
                    ScreenCanvas.DrawString(CurrentDate, Font, Color.White, (int)(ScreenWidth - (CurrentDate.Length * 8)) - 4, 458);

                    // Draw the start menu if required
                    if (DrawStartMenu)
                    {
                        ScreenCanvas.DrawFilledRectangle(Color.SlateGray, 0, (int)(ScreenHeight / 2), 128, (int)((ScreenHeight - 40) - (ScreenHeight / 2)));
                        ExitButton.Draw();
                        RestartButton.Draw();
                        ShutdownButton.Draw();
                    }

                    // Draw the mouse pointer bitmap at the mouse position
                    ScreenCanvas.DrawImageAlpha(MouseCursor, (int)MouseManager.X - 4, (int)MouseManager.Y);

                    // Copy the back frame buffer to the visible frame buffer
                    ScreenCanvas.Display();

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

                // Cleanup
                ConsoleFunctions.PrintLogMSG("Disabling the canvas...\n\n", ConsoleFunctions.LogType.INFO);
                ScreenCanvas.Disable();
            }
            catch(Exception ex)
            {
                if (ScreenCanvas != null)
                    ScreenCanvas.Disable();

                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n", ConsoleFunctions.LogType.ERROR);
            }
        }
    }
}
