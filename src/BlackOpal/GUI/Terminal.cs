using GUI;
using GUI.Component;
using PrismAPI.Hardware.GPU;
using SVGAIITerminal.TextKit;
using System.Drawing;
using Color = PrismAPI.Graphics.Color;

namespace BlackOpal.GUI
{
    internal class Terminal
    {
        public SVGAIITerminal.SVGAIITerminal WindowedTerminal;
        public Window TerminalWindow;
        public Size TerminalSize = new Size(320, 200);

        public void Init()
        {
            // Configure the window
            TerminalWindow = WindowManager.CreateNewWindow("Terminal", Color.Black, TerminalSize, new Point(200, 200));
            TerminalWindow.CloseAction = new System.Action(() => { TerminalWindow.Close(); });

            // Configure the terminal
            WindowedTerminal = new SVGAIITerminal.SVGAIITerminal(TerminalSize.Width, TerminalSize.Height, new BtfFontFace(Kernel.TTFFont, 16));
            WindowedTerminal.BackgroundColor = Color.Black;
            WindowedTerminal.ForegroundColor = Color.Green;
            TerminalWindow.Framebuffer = WindowedTerminal.Contents;
        }
    }
}
