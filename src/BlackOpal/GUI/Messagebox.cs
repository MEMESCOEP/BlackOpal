using IL2CPU.API.Attribs;
using System.Drawing;
using System;
using BlackOpal.GUI.Component;
using GUI.Component;
using GUI;
using GrapeGL.Graphics;
using Color = GrapeGL.Graphics.Color;

namespace BlackOpal.GUI
{
    internal class Messagebox
    {
        /* ENUMS */
        public enum MessageType
        {
            QUESTION,
            WARNING,
            ERROR,
            INFO
        }

        /* VARIABLES */
        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.InfoIcon.bmp")]
        static byte[] InfoIcon;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.QuestionIcon.bmp")]
        static byte[] QuestionIcon;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.WarningIcon.bmp")]
        static byte[] WarningIcon;

        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.IMG.ErrorIcon.bmp")]
        static byte[] ErrorIcon;

        /* FUNCTIONS */
        public static void ShowMessage(string Title, string Message, MessageType MSGType)
        {
            var LongestMsgPart = Message;

            // Calculate the messagebox size using th longest line in the message
            if (Message.Contains('\n'))
            {
                LongestMsgPart = "";

                foreach (var MsgPart in Message.Split('\n'))
                {
                    if (MsgPart.Length > LongestMsgPart.Length)
                    {
                        LongestMsgPart = MsgPart;
                    }
                }
            }            

            var WindowSize = new Size(Math.Max((ushort)100, UserInterface.BIOSFont.MeasureString(LongestMsgPart)) + 58, Math.Max(75, UserInterface.BIOSFont.GetHeight() * Message.Split('\n').Length - 1 + (UserInterface.BIOSFont.GetHeight() * 2)));

            Window MSGWindow = WindowManager.CreateNewWindow(Title, Color.LightGray, WindowSize, new Point((UserInterface.ScreenWidth / 2) - (WindowSize.Width / 2), (int)((UserInterface.ScreenHeight / 2) - (WindowSize.Height / 0.5f))));
            WindowElement MessageElement = new WindowElement();
            WindowElement ImageElement = new WindowElement();
            WindowElement OKElement = new WindowElement();
            TextButton OKButton = new TextButton(new Point(WindowSize.Width - 35, WindowSize.Height - 25));
            
            // Set up the message
            MessageElement.Type = WindowElement.ElementType.STRING;
            MessageElement.ElementData = Message;
            MessageElement.ElementPosition = new Point(50, 5);

            // Set up the image
            ImageElement.Type = WindowElement.ElementType.IMAGE;

            switch (MSGType)
            {
                case MessageType.INFO:
                    ImageElement.ElementData = Image.FromBitmap(InfoIcon);
                    break;

                case MessageType.QUESTION:
                    ImageElement.ElementData = Image.FromBitmap(QuestionIcon);
                    break;

                case MessageType.WARNING:
                    ImageElement.ElementData = Image.FromBitmap(WarningIcon);
                    break;

                case MessageType.ERROR:
                    ImageElement.ElementData = Image.FromBitmap(ErrorIcon);
                    break;
            }

            ImageElement.ElementPosition = new Point(10, 10);

            // Set up the OK/Close button
            OKButton.ButtonText = "OK";
            OKButton.ButtonColor = new Color(255f, 185f, 185f, 185f);
            OKButton.PressedAction = new Action(() => { MSGWindow.Close(); });
                
            // Set up the OK/Close element
            OKElement.Type = WindowElement.ElementType.TEXT_BUTTON;
            OKElement.ElementData = OKButton;
            OKElement.ElementPosition = OKButton.ButtonGlobalPosition;

            // Add elements to the window's element list
            MSGWindow.WindowElements.Add(MessageElement);
            MSGWindow.WindowElements.Add(ImageElement);
            MSGWindow.WindowElements.Add(OKElement);
        }
    }
}
