using GrapeGL.Graphics.Fonts;
using GUI;
using GUI.Component;
using PrismAPI.Graphics;
using System;
using System.Drawing;
using Color = GrapeGL.Graphics.Color;

namespace BlackOpal.GUI
{
    internal class Terminal
    {
        public SVGAIITerminal.SVGAIITerminal WindowedTerminal;
        public Window TerminalWindow;
        public Canvas TerminalCanvas;
        public Size TerminalSize = new Size(320, 200);

        public void Init()
        {
            // COnfigure the canvas
            TerminalCanvas = new Canvas((ushort)TerminalSize.Width, (ushort)TerminalSize.Height);

            // Configure the window
            TerminalWindow = WindowManager.CreateNewWindow("Terminal", Color.Black, TerminalSize, new Point(200, 200));
            TerminalWindow.CloseAction = new Action(() => { TerminalWindow.Close(); });
            //TerminalWindow.Framebuffer = TerminalCanvas;

            // Configure the terminal
            WindowedTerminal = new SVGAIITerminal.SVGAIITerminal(TerminalSize.Width, TerminalSize.Height, new BtfFontFace(Kernel.TTFFont, 16), new Action(() => { }));
            WindowedTerminal.BackgroundColor = Color.Black;
            WindowedTerminal.ForegroundColor = Color.Green;
            WindowedTerminal.IdleRequest = delegate {
                unsafe
                {
                    WindowedTerminal.Contents.CopyTo(TerminalCanvas.Internal);
                }
            };

            WindowedTerminal.WriteLine(">> ");
        }
    }
}
