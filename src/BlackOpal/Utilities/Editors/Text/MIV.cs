using System.IO;
using System;
using BlackOpal;
using System.Globalization;
using IO.CMD;

namespace MIV
{
    class MIV
    {
        public static void printMIVStartScreen()
        {
            Kernel.Terminal.Clear();
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~                               MIV - MInimalistic Vi");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~                                  version 1.2");
            Kernel.Terminal.WriteLine("~                             by Denis Bartashevich");
            Kernel.Terminal.WriteLine("~                            Minor additions by CaveSponge");
            Kernel.Terminal.WriteLine("~                    MIV is open source and freely distributable");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~                     type :help<Enter>          for information");
            Kernel.Terminal.WriteLine("~                     type :q<Enter>             to exit");
            Kernel.Terminal.WriteLine("~                     type :wq<Enter>            save to file and exit");
            Kernel.Terminal.WriteLine("~                     press i                    to write");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.WriteLine("~");
            Kernel.Terminal.Write("~");
        }

        public static String stringCopy(String value)
        {
            String newString = String.Empty;

            for (int i = 0; i < value.Length - 1; i++)
            {
                newString += value[i];
            }

            return newString;
        }

        public static void printMIVScreen(char[] chars, int pos, String infoBar, Boolean editMode)
        {
            int countNewLine = 0;
            int countChars = 0;
            //delay(10000000);
            Kernel.Terminal.Clear();

            for (int i = 0; i < pos; i++)
            {
                if (chars[i] == '\n')
                {
                    Kernel.Terminal.WriteLine("");
                    countNewLine++;
                    countChars = 0;
                }
                else
                {
                    Kernel.Terminal.Write(chars[i]);
                    countChars++;

                    if (countChars % Kernel.Terminal.Width == Kernel.Terminal.Width - 1)
                    {
                        countNewLine++;
                    }
                }
            }

            Kernel.Terminal.Write("/");

            for (int i = 0; i < (Kernel.Terminal.Height - 2) - countNewLine; i++)
            {
                Kernel.Terminal.WriteLine("");
                Kernel.Terminal.Write("~");
            }

            //PRINT INSTRUCTION
            Kernel.Terminal.WriteLine();
            for (int i = 0; i < Kernel.Terminal.Width - 8; i++)
            {
                if (i < infoBar.Length)
                {
                    Kernel.Terminal.Write(infoBar[i]);
                }
                else
                {
                    Kernel.Terminal.Write(" ");
                }
            }

            if (editMode)
            {
                Kernel.Terminal.Write(countNewLine + 1 + "," + countChars);
            }

        }

        public static String miv(String start)
        {
            Boolean editMode = false;
            int pos = 0;
            char[] chars = new char[6144];
            String infoBar = String.Empty;

            if (start == null)
            {
                printMIVStartScreen();
            }
            else
            {
                pos = start.Length;

                for (int i = 0; i < start.Length; i++)
                {
                    chars[i] = start[i];
                }
                printMIVScreen(chars, pos, infoBar, editMode);
            }

            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Kernel.Terminal.ReadKey(true);

                if (isForbiddenKey(keyInfo.Key)) continue;

                else if (!editMode && keyInfo.KeyChar == ':')
                {
                    infoBar = ":";
                    printMIVScreen(chars, pos, infoBar, editMode);
                    do
                    {
                        keyInfo = Kernel.Terminal.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Enter)
                        {
                            if (infoBar == ":wq")
                            {
                                String returnString = String.Empty;
                                for (int i = 0; i < pos; i++)
                                {
                                    returnString += chars[i];
                                }
                                return returnString;
                            }
                            else if (infoBar == ":q")
                            {
                                return null;

                            }
                            else if (infoBar == ":help")
                            {
                                printMIVStartScreen();
                                break;
                            }
                            else
                            {
                                infoBar = "ERROR: No such command";
                                printMIVScreen(chars, pos, infoBar, editMode);
                                break;
                            }
                        }
                        else if (keyInfo.Key == ConsoleKey.Backspace)
                        {
                            infoBar = stringCopy(infoBar);
                            printMIVScreen(chars, pos, infoBar, editMode);
                        }
                        else if (keyInfo.KeyChar == 'q')
                        {
                            infoBar += "q";
                        }
                        else if (keyInfo.KeyChar == ':')
                        {
                            infoBar += ":";
                        }
                        else if (keyInfo.KeyChar == 'w')
                        {
                            infoBar += "w";
                        }
                        else if (keyInfo.KeyChar == 'h')
                        {
                            infoBar += "h";
                        }
                        else if (keyInfo.KeyChar == 'e')
                        {
                            infoBar += "e";
                        }
                        else if (keyInfo.KeyChar == 'l')
                        {
                            infoBar += "l";
                        }
                        else if (keyInfo.KeyChar == 'p')
                        {
                            infoBar += "p";
                        }
                        else
                        {
                            continue;
                        }
                        printMIVScreen(chars, pos, infoBar, editMode);



                    } while (keyInfo.Key != ConsoleKey.Escape);
                }

                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    editMode = false;
                    infoBar = String.Empty;
                    printMIVScreen(chars, pos, infoBar, editMode);
                    continue;
                }

                else if (keyInfo.Key == ConsoleKey.I && !editMode)
                {
                    editMode = true;
                    infoBar = "-- INSERT --";
                    printMIVScreen(chars, pos, infoBar, editMode);
                    continue;
                }

                else if (keyInfo.Key == ConsoleKey.Enter && editMode && pos >= 0)
                {
                    chars[pos++] = '\n';
                    printMIVScreen(chars, pos, infoBar, editMode);
                    continue;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && editMode && pos >= 0)
                {
                    if (pos > 0) pos--;

                    chars[pos] = '\0';

                    printMIVScreen(chars, pos, infoBar, editMode);
                    continue;
                }

                if (editMode && pos >= 0)
                {
                    chars[pos++] = keyInfo.KeyChar;
                    printMIVScreen(chars, pos, infoBar, editMode);
                }

            } while (true);
        }

        public static bool isForbiddenKey(ConsoleKey key)
        {
            ConsoleKey[] forbiddenKeys = { ConsoleKey.Print, ConsoleKey.PrintScreen, ConsoleKey.Pause, ConsoleKey.Home, ConsoleKey.PageUp, ConsoleKey.PageDown, ConsoleKey.End, ConsoleKey.NumPad0, ConsoleKey.NumPad1, ConsoleKey.NumPad2, ConsoleKey.NumPad3, ConsoleKey.NumPad4, ConsoleKey.NumPad5, ConsoleKey.NumPad6, ConsoleKey.NumPad7, ConsoleKey.NumPad8, ConsoleKey.NumPad9, ConsoleKey.Insert, ConsoleKey.F1, ConsoleKey.F2, ConsoleKey.F3, ConsoleKey.F4, ConsoleKey.F5, ConsoleKey.F6, ConsoleKey.F7, ConsoleKey.F8, ConsoleKey.F9, ConsoleKey.F10, ConsoleKey.F11, ConsoleKey.F12, ConsoleKey.Add, ConsoleKey.Divide, ConsoleKey.Multiply, ConsoleKey.Subtract, ConsoleKey.LeftWindows, ConsoleKey.RightWindows };
            for (int i = 0; i < forbiddenKeys.Length; i++)
            {
                if (key == forbiddenKeys[i]) return true;
            }
            return false;
        }

        public static void delay(int time)
        {
            for (int i = 0; i < time; i++) ;
        }
        public static void StartMIV(string FilePath)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    ConsoleFunctions.PrintLogMSG($"Found file \"{FilePath}\".\n", ConsoleFunctions.LogType.INFO);
                }
                else if (!File.Exists(FilePath))
                {
                    ConsoleFunctions.PrintLogMSG($"Creating file \"{FilePath}\"...\n", ConsoleFunctions.LogType.INFO);
                    File.Create(FilePath);
                }

                Kernel.Terminal.Clear();
            }
            catch (Exception ex)
            {
                Kernel.Terminal.WriteLine(ex.Message);
            }

            String text = miv(File.ReadAllText(FilePath));

            Kernel.Terminal.Clear();

            if (text != null)
            {
                File.WriteAllText(FilePath, text);
                ConsoleFunctions.PrintLogMSG($"Content has been saved to \"{FilePath}\".\n\n", ConsoleFunctions.LogType.INFO);
            }
        }
    }
}
