using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Graphics;
using Cosmos.Core.Multiboot;
using Cosmos.Core.Memory;
using Cosmos.System;
using Cosmos.Core;
using Cosmos.HAL;
using IL2CPU.API.Attribs;
using System.Drawing;
using System;
using GUI.Component;
using IO.CMD;
using Power = Cosmos.System.Power;
using Kernel = BlackOpal.Kernel;

namespace GUI
{
    internal class GUI
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
        private TextButton ExitButton = new();
        private TextButton RestartButton = new();
        private TextButton ShutdownButton = new();
        private TextButton NewWindowButton = new();
        private Bitmap MouseCursor = new(MouseCursorArray);
        private Bitmap Wallpaper = new(WallpaperArray);
        private Bitmap Taskbar = new(TaskbarArray);
        private Bitmap Logo = new(LogoArray);
        private Canvas ScreenCanvas;
        private double UsedMemPerentage = 0;
        private string CurrentTime = string.Empty, CurrentDate = string.Empty;
        private bool DrawStartMenu = false, DrawGUI = true;
        public static uint ScreenWidth = 640, ScreenHeight = 480;

        /* FUNCTIONS */
        public void Init()
        {
            try
            {
                // Initialize the canvas
                ConsoleFunctions.PrintLogMSG($"Initializing the canvas ({ScreenWidth}x{ScreenHeight}@32)...\n", ConsoleFunctions.LogType.INFO);

                // Choose the best driver for the current video configuration
                ConsoleFunctions.PrintLogMSG($"Determining the best video driver...\n", ConsoleFunctions.LogType.INFO);
                ScreenCanvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));
                ConsoleFunctions.PrintLogMSG($"Using the {ScreenCanvas.Name()} driver.\n", ConsoleFunctions.LogType.INFO);

                // THIS CAUSES THE KERNEL TO EXPLODE, DO NOT USE!!! 💀
                // Set the canvas mode
                //ConsoleFunctions.PrintLogMSG($"Setting the canvas mode ({ScreenWidth}x{ScreenHeight}@32)...\n", ConsoleFunctions.LogType.INFO);
                //ScreenCanvas.Mode = new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32);

                // Create text buttons and set their properties
                ConsoleFunctions.PrintLogMSG($"Creating text buttons...\n", ConsoleFunctions.LogType.INFO);
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
                NewWindowButton.PressedAction = new Action(() => { WindowManager.CreateNewWindow($"Test window {WindowManager.WindowList.Count}", Color.DarkGray, new Size(320, 200), new Point(64, 64)); });

                // Create image buttons and set their properties
                ConsoleFunctions.PrintLogMSG($"Creating image buttons...\n", ConsoleFunctions.LogType.INFO);
                LogoButton.ButtonImage = Logo;
                LogoButton.ButtonPosition = new Point(6, (int)(ScreenHeight - Logo.Height - 4));
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

                // Set the window manager's canvas
                ConsoleFunctions.PrintLogMSG("Configuring the window manager...\n", ConsoleFunctions.LogType.INFO);
                WindowManager.ScreenCanvas = ScreenCanvas;

                // Create a test window
                ConsoleFunctions.PrintLogMSG("Creating test windows...\n", ConsoleFunctions.LogType.INFO);
                if (WindowManager.WindowList.Count <= 0)
                {
                    WindowManager.CreateNewWindow($"Test window {WindowManager.WindowList.Count}", Color.DarkGray, new Size(320, 200), new Point(64, 64));
                }

                // Infinite draw loop
                ConsoleFunctions.PrintLogMSG($"Init done. Drawing...\n", ConsoleFunctions.LogType.INFO);
                while (DrawGUI)
                {
                    // Get the used RAM
                    Kernel.UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                    UsedMemPerentage = Kernel.UsedRAM / Kernel.TotalInstalledRAM * 100f;

                    // Get the current date and time
                    CurrentTime = DateTime.Now.ToLongTimeString();
                    CurrentDate = DateTime.Now.ToShortDateString();

                    // Draw the wallpaper
                    ScreenCanvas.DrawImage(Wallpaper, 0, 0);

                    // Draw the uptime and memory usage
                    ScreenCanvas.DrawString($"{Kernel.OSName} {Kernel.OSVersion} - {Kernel.OSDate}", Font, Color.Black, 0, 0);
                    ScreenCanvas.DrawString($"UPTIME: {(DateTime.Now - Kernel.KernelStartTime).ToString()}", Font, Color.Black, 0, 12);
                    ScreenCanvas.DrawString($"DRIVER: {ScreenCanvas.Name()}", Font, Color.Black, 0, 24);
                    ScreenCanvas.DrawString($"MEM: {Kernel.UsedRAM}/{Kernel.TotalInstalledRAM} KB ({UsedMemPerentage}%)", Font, Color.Black, 0, 36);

                    // Draw any windows
                    WindowManager.DrawWindows();

                    // Draw the taskbar
                    ScreenCanvas.DrawImage(Taskbar, 0, (int)(ScreenHeight - Taskbar.Height));
                    LogoButton.Draw();

                    // Draw date and time
                    ScreenCanvas.DrawString(CurrentTime, Font, Color.White, (int)(ScreenWidth - CurrentTime.Length * 8) - 4, 444);
                    ScreenCanvas.DrawString(CurrentDate, Font, Color.White, (int)(ScreenWidth - CurrentDate.Length * 8) - 4, 458);

                    // Draw the start menu if required
                    if (DrawStartMenu)
                    {
                        ScreenCanvas.DrawFilledRectangle(Color.SlateGray, 0, (int)(ScreenHeight / 2), 128, (int)(ScreenHeight - 40 - ScreenHeight / 2));
                        ScreenCanvas.DrawRectangle(Color.Black, 0, (int)(ScreenHeight / 2), 128, (int)(ScreenHeight - 40 - ScreenHeight / 2));
                        ExitButton.Draw();
                        RestartButton.Draw();
                        ShutdownButton.Draw();
                        NewWindowButton.Draw();
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
            catch (Exception ex)
            {
                if (ScreenCanvas != null)
                    ScreenCanvas.Disable();

                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n", ConsoleFunctions.LogType.ERROR);
            }
        }

        // Check if VBE is available
        private bool IsVBEAvailable()
        {
            if (FullScreenCanvas.BGAExists())
            {
                return true;
            }

            if (PCI.Exists(VendorID.VirtualBox, DeviceID.VBVGA))
            {
                return true;
            }

            if (PCI.Exists(VendorID.Bochs, DeviceID.BGA))
            {
                return true;
            }

            if (Multiboot2.IsVBEAvailable)
            {
                return true;
            }

            return false;
        }
    }
}
