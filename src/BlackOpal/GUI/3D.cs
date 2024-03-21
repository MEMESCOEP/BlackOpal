using Cosmos.System;
using Cosmos.HAL;
using System.Drawing;
using System;
using GUI.Component;
using GUI;
using PrismAPI.Graphics.Rasterizer;
using Color = PrismAPI.Graphics.Color;

namespace BlackOpal.GUI
{
    internal class _3D
    {
        public static void CreateNewRenderer(int FOV = 60, int FPS = 30)
        {
            Window NewWindow = WindowManager.CreateNewWindow("3D Renderer", default, new Size(320, 200), new Point(200, 200));
            var RasterEngine = new Engine((ushort)NewWindow.Size.Width, (ushort)NewWindow.Size.Height, FOV);
            var CubeMesh = Mesh.GetCube(50, 50, 50);
            var PS2 = new PS2Controller();

            CubeMesh.Position = new System.Numerics.Vector3(20, 20, 20);

            foreach (var Triangle in CubeMesh.Triangles)
            {
                Triangle.Color = new Color(255 / CubeMesh.Triangles.IndexOf(Triangle), 255 / CubeMesh.Triangles.IndexOf(Triangle), 255 / CubeMesh.Triangles.IndexOf(Triangle));
            }

            RasterEngine.Objects.Add(CubeMesh);

            NewWindow.Framebuffer = RasterEngine;
            NewWindow.UpdateAction = new Action(() =>
            {
                CubeMesh.Rotation.X += 0.01f;
                CubeMesh.Rotation.Y += 0.01f;
                CubeMesh.Rotation.Z += 0.01f;

                var Scancode = PS2.ReadByteAfterAck();
                bool KeyPressed = KeyboardManager.GetKey(Scancode, out var KeyInfo);

                if (KeyPressed && KeyInfo.Key != ConsoleKeyEx.NoName)
                {
                    if (Scancode == 72 || KeyInfo.Key == ConsoleKeyEx.UpArrow)
                        CubeMesh.Position.Y -= 1f;

                    if (Scancode == 80 || KeyInfo.Key == ConsoleKeyEx.DownArrow)
                        CubeMesh.Position.Y += 1f;

                    if (Scancode == 75 || KeyInfo.Key == ConsoleKeyEx.LeftArrow)
                        CubeMesh.Position.X -= 1f;

                    if (Scancode == 77 || KeyInfo.Key == ConsoleKeyEx.RightArrow)
                        CubeMesh.Position.X += 1f;
                }

                RasterEngine.Render();
                RasterEngine.DrawString(0, 0, $"Scancode: {Scancode}", UserInterface.BIOSFont, Color.Black);
                RasterEngine.DrawString(0, 16, $"Char: '{KeyInfo.KeyChar}'", UserInterface.BIOSFont, Color.Black);
            });
        }
    }
}
