using System.Drawing;
using System;
using Cosmos.Core.Memory;
using Cosmos.System;
using Cosmos.Core;
using Cosmos.HAL;
using IL2CPU.API.Attribs;
using BlackOpal.Utilities.Calculations;
using BlackOpal.GUI.Component;
using BlackOpal.GUI;
using GUI.Component;
using IO.CMD;
using GrapeGL.Hardware.GPU;
using GrapeGL.Graphics;
using Kernel = BlackOpal.Kernel;
using Power = Cosmos.System.Power;
using Color = GrapeGL.Graphics.Color;
using System.IO;
using GrapeGL.Graphics.Fonts;

namespace GUI
{
    public class UserInterface
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

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.3DRendererIcon.bmp")]
        static byte[] RendererArray;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.PowerOffIcon.bmp")]
        static byte[] PowerArray;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.DebugIcon.bmp")]
        static byte[] DebugArray;

        private ImageButton LogoButton = new();
        private ImageButton _3DRendererButton = new();
        private ImageButton PowerIconButton = new();
        private ImageButton DebugIconButton = new();
        private TextButton DebugButton = new(new Point(36, ScreenHeight - 148));
        private TextButton PowerButton = new(new Point(36, ScreenHeight - 120));
        private TextButton ExitButton = new(new Point(24, ScreenHeight - 92));
        private Canvas MouseCursor = Image.FromBitmap(MouseCursorArray);
        private Canvas Wallpaper = Image.FromBitmap(WallpaperArray);
        private Canvas Taskbar = Image.FromBitmap(TaskbarArray);
        private Canvas Logo = Image.FromBitmap(LogoArray);
        private double UsedMemPerentage = 0;
        private string CurrentTime = string.Empty, CurrentDate = string.Empty;
        private bool DrawStartMenu = false;
        private int PreviousSecond = 0, PreviousHour = 0;
        public static BtfFontFace BIOSFont;
        public static Display ScreenCanvas;
        public static Point ClickPoint = new Point(0, 0);
        public static ushort ScreenWidth = 1024, ScreenHeight = 768;
        public static bool HideMouse = false, DrawGUI = true;
        public static int FrameCount = 0;

        /* FUNCTIONS */
        public void Init()
        {
            try
            {
                MemoryStream FontStream = new MemoryStream(Kernel.TTFFont);
                //BIOSFont = new GrapeGL.Graphics.Fonts.AcfFontFace(FontStream);
                BIOSFont = new BtfFontFace(Kernel.TTFFont, 16);

                // Make sure the GUI does not immediately exit if it is restarted
                DrawGUI = true;

                // Get the date and time
                CurrentDate = DateTime.Now.ToShortDateString();
                CurrentTime = DateTime.Now.ToLongTimeString();
                PreviousSecond = RTC.Second;
                PreviousHour = RTC.Hour;

                // Only initialize if the canvas is null
                if (ScreenCanvas == null)
                {
                    // Initialize the canvas
                    ConsoleFunctions.PrintLogMSG($"Initializing the canvas ({ScreenWidth}x{ScreenHeight}@32)...\n\r", ConsoleFunctions.LogType.INFO);

                    // Choose the best driver for the current display hardware
                    ConsoleFunctions.PrintLogMSG($"Getting reference to the terminal's internal canvas...\n\r", ConsoleFunctions.LogType.INFO);
                    ScreenCanvas = (Display)Kernel.Terminal.Contents;
                    ConsoleFunctions.PrintLogMSG($"Using the \"{ScreenCanvas.GetName()}\" driver in {ScreenWidth}x{ScreenHeight}@32 mode.\n\r", ConsoleFunctions.LogType.INFO);

                    // Create text buttons and set their properties
                    ConsoleFunctions.PrintLogMSG($"Creating text buttons...\n\r", ConsoleFunctions.LogType.INFO);
                    
                    // GUI exit button
                    ExitButton.ButtonText = "Exit GUI";
                    ExitButton.ScreenCanvas = ScreenCanvas;
                    ExitButton.PressedAction = new Action(() => { DrawGUI = false; });

                    // Power menu button
                    PowerButton.ButtonText = "Power";
                    PowerButton.ScreenCanvas = ScreenCanvas;
                    PowerButton.PressedAction = new Action(() => {
                        Window NewWindow = WindowManager.CreateNewWindow("Power Options", Color.LightGray, new Size(140, 75), new Point(64, 64));
                        WindowElement CloseWindowButtonElement = new WindowElement();
                        WindowElement ShutdownButtonElement = new WindowElement();
                        WindowElement RestartButtonElement = new WindowElement();
                        TextButton ShutdownTextButton = new TextButton(new Point(34, 10));
                        TextButton RestartTextButton = new TextButton(new Point(38, 30));
                        TextButton CloseTextButton = new TextButton(new Point(42, 50));

                        // Close window button
                        CloseTextButton.PressedAction = new Action(() => {
                            ScreenCanvas.Update();
                            NewWindow.Close();
                        });
                        CloseTextButton.ButtonText = "Cancel";
                        CloseTextButton.ButtonColor = Color.StackOverflowWhite;
                        CloseTextButton.ButtonHighlightColor = Color.Green;
                        CloseTextButton.ButtonPressedColor = Color.Red;
                        CloseWindowButtonElement.Type = WindowElement.ElementType.TEXT_BUTTON;
                        CloseWindowButtonElement.ElementData = CloseTextButton;
                        CloseWindowButtonElement.ElementPosition = CloseTextButton.ButtonGlobalPosition;

                        // Shutdown button
                        ShutdownTextButton.PressedAction = new Action(() => {
                            ScreenCanvas.Update();
                            Power.Shutdown();
                        });
                        ShutdownTextButton.ButtonText = "Shutdown";
                        ShutdownTextButton.ButtonColor = Color.StackOverflowWhite;
                        ShutdownTextButton.ButtonHighlightColor = Color.RubyRed;
                        ShutdownTextButton.ButtonPressedColor = Color.Red;
                        ShutdownButtonElement.Type = WindowElement.ElementType.TEXT_BUTTON;
                        ShutdownButtonElement.ElementData = ShutdownTextButton;
                        ShutdownButtonElement.ElementPosition = ShutdownTextButton.ButtonGlobalPosition;

                        // Restart button
                        RestartTextButton.PressedAction = new Action(() => {
                            ScreenCanvas.Update();
                            Power.Reboot();
                        });
                        RestartTextButton.ButtonText = "Restart";
                        RestartTextButton.ButtonColor = Color.StackOverflowWhite;
                        RestartTextButton.ButtonHighlightColor = Color.Yellow;
                        RestartTextButton.ButtonPressedColor = Color.Red;
                        RestartButtonElement.Type = WindowElement.ElementType.TEXT_BUTTON;
                        RestartButtonElement.ElementData = RestartTextButton;
                        RestartButtonElement.ElementPosition = RestartTextButton.ButtonGlobalPosition;

                        NewWindow.WindowElements.Add(CloseWindowButtonElement);
                        NewWindow.WindowElements.Add(ShutdownButtonElement);
                        NewWindow.WindowElements.Add(RestartButtonElement);
                    });

                    // Sysinfo button
                    DebugButton.ButtonText = "Debug";
                    DebugButton.ScreenCanvas = ScreenCanvas;
                    DebugButton.PressedAction = new Action(() => {
                        Window NewWindow = WindowManager.CreateNewWindow("Debug Information", Color.LightGray, new Size(240, 75), new Point(256, 256));
                        WindowElement MousePositionElement = new WindowElement();
                        WindowElement MemoryUsageElement = new WindowElement();
                        WindowElement UptimeElement = new WindowElement();
                        WindowElement DriverElement = new WindowElement();
                        WindowElement FPSElement = new WindowElement();

                        // Add useful information
                        MousePositionElement.Type = WindowElement.ElementType.STRING;
                        MousePositionElement.ElementData = $"MOUSE POS: {MouseManager.X}, {MouseManager.Y}";
                        MousePositionElement.ElementPosition = new Point(5, 5);

                        UptimeElement.Type = WindowElement.ElementType.STRING;
                        UptimeElement.ElementData = $"UPTIME: {DateTime.Now - Kernel.KernelStartTime}";
                        UptimeElement.ElementPosition = new Point(5, 17);

                        DriverElement.Type = WindowElement.ElementType.STRING;
                        DriverElement.ElementData = $"DRIVER: {ScreenCanvas.GetName()}";
                        DriverElement.ElementPosition = new Point(5, 29);

                        MemoryUsageElement.Type = WindowElement.ElementType.STRING;
                        MemoryUsageElement.ElementData = $"MEM: {Kernel.UsedRAM}/{Kernel.TotalInstalledRAM} KB ({UsedMemPerentage}%)";
                        MemoryUsageElement.ElementPosition = new Point(5, 41);

                        FPSElement.Type = WindowElement.ElementType.STRING;
                        FPSElement.ElementData = $"FPS: {ScreenCanvas.GetFPS()} ({FrameCount})";
                        FPSElement.ElementPosition = new Point(5, 53);

                        NewWindow.WindowElements.Add(MousePositionElement);
                        NewWindow.WindowElements.Add(MemoryUsageElement);
                        NewWindow.WindowElements.Add(UptimeElement);
                        NewWindow.WindowElements.Add(DriverElement);
                        NewWindow.WindowElements.Add(FPSElement);

                        NewWindow.UpdateAction = new Action(() => { 
                            if (FrameCount % 60 == 0)
                            {
                                MemoryUsageElement.ElementData = $"MEM: {Kernel.UsedRAM}/{Kernel.TotalInstalledRAM} KB ({UsedMemPerentage}%)";
                                UptimeElement.ElementData = $"UPTIME: {DateTime.Now - Kernel.KernelStartTime}";
                                FPSElement.ElementData = $"FPS: {ScreenCanvas.GetFPS()} ({FrameCount})";
                            }

                            if (FrameCount % 10 == 0)
                            {
                                MousePositionElement.ElementData = $"MOUSE POS: {MouseManager.X}, {MouseManager.Y}";
                            }
                        });
                    });

                    // Create image buttons and set their properties
                    ConsoleFunctions.PrintLogMSG($"Creating image buttons...\n\r", ConsoleFunctions.LogType.INFO);
                    LogoButton.ButtonImage = Logo;
                    LogoButton.ButtonPosition = new Point(6, ScreenHeight - Logo.Height - 4);
                    LogoButton.ScreenCanvas = ScreenCanvas;
                    LogoButton.BackColor = Color.LightGray;
                    LogoButton.BackColorPressed = Color.DeepGray;
                    LogoButton.HighlightedBackColor = Color.LightGray;
                    LogoButton.PressedAction = new Action(() => { DrawStartMenu = !DrawStartMenu; });
                    LogoButton.RequireMouseHeld = false;

                    // 3D Renderer button
                    _3DRendererButton.DrawBorder = false;
                    _3DRendererButton.HighlightedBackColor = new Color(192, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
                    _3DRendererButton.BackColorPressed = new Color(64, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
                    _3DRendererButton.ScreenCanvas = ScreenCanvas;
                    _3DRendererButton.ButtonPosition = new Point(16, 0);
                    _3DRendererButton.ButtonImage = Image.FromBitmap(RendererArray);
                    _3DRendererButton.PressedAction = new Action(() => { _3D.CreateNewRenderer(); });

                    // Power menu icon
                    PowerIconButton.DrawBorder = false;
                    PowerIconButton.HighlightedBackColor = new Color(192, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
                    PowerIconButton.BackColorPressed = new Color(64, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
                    PowerIconButton.ScreenCanvas = ScreenCanvas;
                    PowerIconButton.ButtonPosition = new Point(16, 96);
                    PowerIconButton.ButtonImage = Image.FromBitmap(PowerArray);
                    PowerIconButton.PressedAction = PowerButton.PressedAction;

                    // Debug icon
                    DebugIconButton.DrawBorder = false;
                    DebugIconButton.HighlightedBackColor = new Color(192, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
                    DebugIconButton.BackColorPressed = new Color(64, Color.LightGray.R, Color.LightGray.G, Color.LightGray.B);
                    DebugIconButton.ScreenCanvas = ScreenCanvas;
                    DebugIconButton.ButtonPosition = new Point(20, 192);
                    DebugIconButton.ButtonImage = Image.FromBitmap(DebugArray);
                    DebugIconButton.PressedAction = DebugButton.PressedAction;

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
                }

                // Infinite draw loop
                ConsoleFunctions.PrintLogMSG($"Init done.\n\r", ConsoleFunctions.LogType.INFO);
                Messagebox.ShowMessage("Welcome!", "Welcome to the BlackOpal GUI!\nThis is still a work in progress, so you should expect bugs.\n- memescoep", BlackOpal.GUI.Messagebox.MessageType.INFO);

                for (;;) 
                {
                    if (DrawGUI == false)
                        break;

                    // Call the garbage collector every 4 frames
                    Heap.Collect();

                    try
                    {
                        // Get the point where the mouse was clicked
                        if (MouseManager.LastMouseState != MouseState.Left && MouseManager.MouseState == MouseState.Left)
                        {
                            ClickPoint.X = (int)MouseManager.X;
                            ClickPoint.Y = (int)MouseManager.Y;
                        }

                        // Draw the wallpaper
                        ScreenCanvas.DrawImage(0, 0, Wallpaper, false);

                        // Draw buttons
                        _3DRendererButton.Draw();
                        PowerIconButton.Draw();
                        DebugIconButton.Draw();

                        // Draw text
                        ScreenCanvas.DrawString(48, 72, "3D Renderer", BIOSFont, Color.Black, true);
                        ScreenCanvas.DrawString(48, 168, "Power", BIOSFont, Color.Black, true);
                        ScreenCanvas.DrawString(48, 260, "Debug", BIOSFont, Color.Black, true);

                        // Draw any windows
                        WindowManager.DrawWindows();

                        // Draw the taskbar
                        ScreenCanvas.DrawImage(0, ScreenHeight - Taskbar.Height, Taskbar, false);
                        LogoButton.Draw();

                        // Draw windows in the taskbar
                        int TaskWindowPosition = 36;
                        int WindowCount = 0;

                        for (int WindowIndex = WindowManager.WindowList.Count - 1; WindowIndex >= 0; WindowIndex--)
                        {
                            var TaskbarWindow = WindowManager.WindowList[WindowIndex];
                            var CorrectedStr = TaskbarWindow.Title;

                            if (CorrectedStr.Length > 10)
                            {
                                CorrectedStr = CorrectedStr.Substring(0, 10) + "...";
                            }

                            if (WindowCount > 6)
                            {
                                ScreenCanvas.DrawFilledRectangle(TaskWindowPosition, ScreenHeight - Taskbar.Height + 4, 32, 20, 0, Color.LightGray);
                                ScreenCanvas.DrawString(TaskWindowPosition + 4, ScreenHeight - Taskbar.Height + 6, "...", BIOSFont, Color.Black);
                                break;
                            }

                            ScreenCanvas.DrawFilledRectangle(TaskWindowPosition, ScreenHeight - Taskbar.Height + 4, 116, 20, 0, Color.LightGray);

                            if (WindowIndex == WindowManager.WindowList.Count - 1)
                            {
                                ScreenCanvas.DrawRectangle(TaskWindowPosition, ScreenHeight - Taskbar.Height + 4, 116, 20, 0, Color.Magenta);
                            }

                            ScreenCanvas.DrawString(TaskWindowPosition + 58, ScreenHeight - Taskbar.Height + 14, CorrectedStr, BIOSFont, Color.Black, true);
                            TaskWindowPosition += 124;
                            WindowCount++;
                        }

                        // Draw the date and time
                        ScreenCanvas.DrawString((ScreenWidth - CurrentTime.Length * 8) - 4, ScreenHeight - 32, CurrentTime, BIOSFont, Color.White);
                        ScreenCanvas.DrawString((ScreenWidth - CurrentDate.Length * 8) - 4, ScreenHeight - 18, CurrentDate, BIOSFont, Color.White);

                        // Draw the start menu if required
                        if (DrawStartMenu)
                        {
                            var StartMenuY = ScreenHeight - 288;

                            ScreenCanvas.DrawFilledRectangle(0, StartMenuY, 128, 256, 0, Color.LighterBlack);
                            ScreenCanvas.DrawRectangle(0, StartMenuY, 128, 256, 0, Color.Black);
                            DebugButton.Draw();
                            PowerButton.Draw();
                            ExitButton.Draw();
                        }

                        // Draw a rectangle if the mouse is dragged
                        /*if (!WindowManager.FocusingWindow && MouseManager.MouseState == MouseState.Left && MouseManager.LastMouseState == MouseManager.MouseState)
                        {
                            ScreenCanvas.DrawRectangle(ClickPoint.X, ClickPoint.Y, (ushort)(MouseManager.X - ClickPoint.X), (ushort)(MouseManager.Y - ClickPoint.Y), 0, new Color(0, 0, 255));
                        }*/

                        // Draw the mouse pointer bitmap at the mouse position
                        if (HideMouse == false)
                        {
                            ScreenCanvas.DrawImage((int)MouseManager.X, (int)MouseManager.Y, MouseCursor);
                        }

                        // Copy the back (invisible) frame buffer to the front (visible) frame buffer
                        ScreenCanvas.Update();

                        // Handle keyboard events
                        if (System.Console.KeyAvailable)
                        {
                            switch (System.Console.ReadKey().Key)
                            {
                                // Close the focused window on ALT+F4
                                case ConsoleKey.F4:
                                    if (KeyboardManager.AltPressed)
                                    {
                                        foreach(var WindowToClose in WindowManager.WindowList)
                                        {
                                            if (WindowToClose.WindowID == WindowManager.FocusedWindowID)
                                            {
                                                WindowToClose.Close(true);
                                                break;
                                            }
                                        }
                                    }

                                    break;

                                // Toggle the start menu if the user presses either Windows key
                                case ConsoleKey.LeftWindows:
                                case ConsoleKey.RightWindows:
                                    DrawStartMenu = !DrawStartMenu;
                                    break;

                                // Create a new 3D renderer window when the user presses the F11 key
                                case ConsoleKey.F10:
                                    if (KeyboardManager.ShiftPressed)
                                    {
                                        Terminal WindowedTerminal = new Terminal();
                                        WindowedTerminal.Init();
                                    }

                                    break;

                                // Create a new 3D renderer window when the user presses the F11 key
                                case ConsoleKey.F11:
                                    _3D.CreateNewRenderer();
                                    break;

                                // Open the debug menu when the user presses the F12 key
                                case ConsoleKey.F12:
                                    DebugButton.PressedAction.Invoke();
                                    break;
                            }
                        }

                        var RTCSecond = RTC.Second;
                        var RTCHour = RTC.Hour;

                        if (RTCSecond != PreviousSecond)
                        {
                            // Get the current date and time
                            PreviousSecond = RTCSecond;
                            PreviousHour = RTCHour;
                            CurrentTime = DateTime.Now.ToLongTimeString();

                            if (RTCHour != PreviousHour)
                            {
                                CurrentDate = DateTime.Now.ToShortDateString();
                            }

                            // Get the used RAM
                            Kernel.UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                            UsedMemPerentage = MathHelpers.TruncateToDecimalPlace((Kernel.UsedRAM / Kernel.TotalInstalledRAM) * 100f, 4);
                        }

                        // Imcrement the frame counter after every successful draw loop
                        FrameCount++;
                    }
                    catch(Exception ex)
                    {
                        ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\r", ConsoleFunctions.LogType.ERROR);
                    }
                }

                // Cleanup
                ConsoleFunctions.PrintLogMSG("Disabling the GUI...\n\r", ConsoleFunctions.LogType.INFO);
                Kernel.Terminal.Clear();
                Heap.Collect();
            }
            catch (Exception ex)
            {
                Kernel.Terminal.Clear();
                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                DrawGUI = false;
            }
        }
    }
}
