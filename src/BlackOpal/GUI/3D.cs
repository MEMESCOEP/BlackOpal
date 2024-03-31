using Cosmos.System;
using Cosmos.HAL;
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System;
using BlackOpal.Utilities.Calculations;
using BlackOpal.GUI.Component;
using GUI.Component;
using GUI;
using PrismAPI.Graphics.Rasterizer;
using Color = PrismAPI.Graphics.Color;

namespace BlackOpal.GUI
{
    internal class _3D
    {
        /* VARIABLES */
        // Public
        public static List<GameObject> GameObjects = new List<GameObject>() { };
        public static GrapeGL.Graphics.Canvas RenderCanvas = new GrapeGL.Graphics.Canvas((ushort)WindowSize.Width, (ushort)WindowSize.Height);
        public static Engine RasterEngine;
        public static Window RenderWindow;
        public static Color AmbientColor = new Color(255, 255, 255);
        public static Size WindowSize = new Size(320, 240);
        public static float MouseSensitivityX = 0.001f;
        public static float MouseSensitivityY = 0.001f;
        public static float MaxLookAngle = 1;
        public static float MoveSpeed = 5f;
        public static float TargetFPS = 60f;
        public static byte Scancode = 0, PreviousScancode = 0;
        public static int FOV = 75;

        // Private
        private static PIT.PITTimer UpdateTimer;
        private static ulong ThreadSleepTime = (ulong)((1000f / TargetFPS) * 1000000);
        private static bool MouseLocked = true;

        /* FUNCTIONS */
        public static void CreateNewRenderer()
        {
            // Create required objects
            RenderWindow = WindowManager.CreateNewWindow("3D Engine", default, WindowSize, new Point(200, 200));
            RasterEngine = new Engine((ushort)WindowSize.Width, (ushort)WindowSize.Height, FOV);
            var SceneLight = new Light(new Vector3(10, 10, 10), new Vector3(0, 0, 0), LightTypes.Directional, Color.RubyRed);
            var PS2 = new PS2Controller();

            // Add a cube to the scene
            var GameObjects = new List<GameObject>();
            var WallGO = new GameObject();

            WallGO.ObjectMesh = Mesh.GetCube(20, 20, 20);
            WallGO.Position = new Vector3(0, 0, 0);

            for (int Triangle = 0; Triangle < WallGO.ObjectMesh.Triangles.Count; Triangle += 2)
            {
                WallGO.ObjectMesh.Triangles[Triangle].Color = new Color(255 / (Triangle + 1), 255 / (Triangle + 1), 255 / (Triangle + 1));
                WallGO.ObjectMesh.Triangles[Triangle + 1].Color = new Color(255 / (Triangle + 1), 255 / (Triangle + 1), 255 / (Triangle + 1));
            }

            RasterEngine.Objects.Add(WallGO.ObjectMesh);
                        
            // Configure the scene
            RasterEngine.Lights.Add(SceneLight);
            RasterEngine.SkyColor = Color.GoogleBlue;

            // Reset the scroll delta
            MouseManager.ResetScrollDelta();

            // Hide the mouse cursor
            UserInterface.HideMouse = true;

            // Create a new PIT timer
            UpdateTimer = new PIT.PITTimer(UpdateScreen, ThreadSleepTime, true);
            Cosmos.HAL.Global.PIT.RegisterTimer(UpdateTimer);

            // Close action
            RenderWindow.CloseAction = new Action(() => { UserInterface.HideMouse = false; });

            // Main render loop
            RenderWindow.Framebuffer = RenderCanvas;
            RenderWindow.UpdateAction = new Action(() =>
            {
                WallGO.ObjectMesh.Rotation.X += 0.01f;
                WallGO.ObjectMesh.Rotation.Y += 0.01f;
                WallGO.ObjectMesh.Rotation.Z += 0.01f;

                foreach (var Object in GameObjects)
                {
                    RasterEngine.Camera.Position = Vector3.Clamp(RasterEngine.Camera.Position, new Vector3(0, 0, -999), new Vector3(WindowSize.Width, WindowSize.Height, 999));
                }

                if (WindowManager.FocusedWindowID == RenderWindow.WindowID)
                {
                    // Read a key from the keyboard
                    PS2.ReadByteAfterAckWithTimeout(ref Scancode);
                    bool KeyPressed = KeyboardManager.GetKey(Scancode, out var KeyInfo);

                    // Reset the scroll delta
                    if (UserInterface.FrameCount % 8 == 0)
                        MouseManager.ResetScrollDelta();

                    // Handle camera controls
                    if (UserInterface.HideMouse)
                    {
                        RasterEngine.Camera.Rotation.X -= MouseManager.DeltaX * MouseSensitivityX;
                        RasterEngine.Camera.Rotation.Y -= MouseManager.DeltaY * MouseSensitivityY;
                    }

                    RasterEngine.Zoom += MouseManager.ScrollDelta;

                    // Lock the mouse to the center of the window if the mouse is hidden
                    if (UserInterface.HideMouse)
                    {
                        MouseManager.X = (uint)(RenderWindow.Position.X + (WindowSize.Width / 2));
                        MouseManager.Y = (uint)(RenderWindow.Position.Y + (WindowSize.Height / 2));
                    }

                    // Handle keyboard events
                    if (KeyPressed)
                    {
                        // Frame limited
                        if (UserInterface.FrameCount % 5 == 0)
                        {
                            // Camera controls
                            // Up arrow
                            if (Scancode == 72)
                                RasterEngine.Camera.Position.Y -= MoveSpeed;

                            // Down arrow
                            else if (Scancode == 80)
                                RasterEngine.Camera.Position.Y += MoveSpeed;

                            // Left arrow, D
                            else if (Scancode == 75 || Scancode == 32)
                                RasterEngine.Camera.Position.X -= MoveSpeed;

                            // Right arrow, A
                            else if (Scancode == 77 || Scancode == 30)
                                RasterEngine.Camera.Position.X += MoveSpeed;

                            // W, E
                            else if (Scancode == 18 || Scancode == 17)
                                RasterEngine.Camera.Position.Z -= MoveSpeed;

                            // S, Q
                            else if (Scancode == 16 || Scancode == 31)
                                RasterEngine.Camera.Position.Z += MoveSpeed;

                            // +
                            else if (Scancode == 13 && KeyboardManager.ShiftPressed)
                                FOV = Math.Max(175, FOV + 5);

                            // -
                            else if (Scancode == 12)
                                FOV = Math.Min(30, FOV - 5);

                            // R
                            else if (Scancode == 19 || KeyInfo.Key == ConsoleKeyEx.R)
                                if (KeyboardManager.ShiftPressed)
                                {
                                    AmbientColor = new Color(AmbientColor.R - 5, AmbientColor.G, AmbientColor.B);
                                }
                                else
                                {
                                    AmbientColor = new Color(AmbientColor.R + 5, AmbientColor.G, AmbientColor.B);
                                }

                            // G
                            else if (Scancode == 34 || KeyInfo.Key == ConsoleKeyEx.G)
                                if (KeyboardManager.ShiftPressed)
                                {
                                    AmbientColor = new Color(AmbientColor.R, AmbientColor.G - 5, AmbientColor.B);
                                }
                                else
                                {
                                    AmbientColor = new Color(AmbientColor.R, AmbientColor.G + 5, AmbientColor.B);
                                }

                            // B
                            else if (Scancode == 48 || KeyInfo.Key == ConsoleKeyEx.B)
                                if (KeyboardManager.ShiftPressed)
                                {
                                    AmbientColor = new Color(AmbientColor.R, AmbientColor.G, AmbientColor.B - 5);
                                }
                                else
                                {
                                    AmbientColor = new Color(AmbientColor.R, AmbientColor.G, AmbientColor.B + 5);
                                }
                        }

                        // Non-frame count limited
                        else
                        {
                            // Escape
                            if (Scancode == 1 && PreviousScancode != 1)
                            {
                                UserInterface.HideMouse = !UserInterface.HideMouse;
                                MouseLocked = !MouseLocked;
                            }

                            // Reset the scene if the user presses home
                            if (KeyInfo.Key == ConsoleKeyEx.Home)
                            {
                                RasterEngine.Camera.Position = Vector3.Zero;
                                RasterEngine.Camera.Rotation = Vector3.Zero;
                                RasterEngine.Camera.FOV = 75f;
                                WallGO.Position = Vector3.Zero;
                                WallGO.Rotation = Vector3.Zero;
                                AmbientColor = Color.White;
                            }

                            // Close the window if the user presses end
                            if (KeyInfo.Key == ConsoleKeyEx.End)
                            {
                                UserInterface.HideMouse = false;
                                RenderWindow.Close();
                            }
                        }

                        PreviousScancode = Scancode;
                    }

                    if (MouseLocked && UserInterface.HideMouse == false)
                    { 
                        UserInterface.HideMouse = true;
                    }
                }
                else if (UserInterface.HideMouse)
                {
                    UserInterface.HideMouse = false;
                }

                // Sleep for n milliseconds to reach the target framerate
                /*if (UserInterface.ScreenCanvas.GetFPS() > FPS)
                   Thread.Sleep(ThreadSleepTime - (int)Watch.ElapsedMilliseconds);*/

                UpdateScreen();
            });
        }

        public static void UpdateScreen()
        {
            // Get the canvas's FPS
            var CurrentFPS = UserInterface.ScreenCanvas.GetFPS();

            // Draw everything in the window's framebuffer
            RasterEngine.Camera.Ambient = AmbientColor;
            RasterEngine.Render();

            unsafe
            {
                RasterEngine.CopyTo(RenderCanvas.Internal);
            }

            RenderCanvas.DrawString(5, 0, $"POS: '{RasterEngine.Camera.Position}'", UserInterface.BIOSFont, GrapeGL.Graphics.Color.Black);
            
            //RasterEngine.DrawString(5, 16, $"Scancode: {Scancode}", UserInterface.BIOSFont, Color.Black);
            //RasterEngine.DrawString(5, 32, $"Render time: {Watch.ElapsedMilliseconds}", UserInterface.BIOSFont, Color.Black);

            // Update the window title
            RenderWindow.Title = $"3D Engine ({CurrentFPS}/{TargetFPS}, {MathHelpers.TruncateToDecimalPlace((CurrentFPS / TargetFPS) * 100f, 2)}%)";
        }
    }
}
