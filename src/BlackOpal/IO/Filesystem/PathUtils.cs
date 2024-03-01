using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BlackOpal.IO.Filesystem
{
    internal class PathUtils
    {
        public static string ListToPath(List<string> InputList, bool RemoveCommandFromList = false)
        {
            var ArgPath = "";

            // Remove the command from the list
            if (RemoveCommandFromList)
            {
                InputList.RemoveAt(0);
            }

            // Create a path from arguments
            foreach (var PathPart in InputList)
            {
                ArgPath += PathPart + " ";
            }

            return ArgPath;
        }

        // Returns a valid path from an (possibly invalid) input string
        public static string GetValidPath(string InputPath)
        {
            bool AppendQuotes = true;
            var CurrentDirectory = Directory.GetCurrentDirectory();
            var ValidPath = "";

            // Get a path in-between quotes
            if (InputPath.StartsWith("\""))
            {
                ValidPath = "";

                foreach (var Part in InputPath.Split(' '))
                {
                    ValidPath += Part + " ";
                }

                // Get the file name inside of a the quotes
                int PFrom = ValidPath.IndexOf("\"") + 1;
                int PTo = ValidPath.LastIndexOf("\"");
                ValidPath = ValidPath.Substring(PFrom, PTo - PFrom).TrimEnd();
                AppendQuotes = false;
            }
            else
            {
                ValidPath = InputPath;
            }

            // Get parent directories when the path contains "..\"
            if (InputPath.Contains(@"..\"))
            {
                var BackCount = InputPath.Split(@"..\").Length - 1;
                var RootPath = Path.GetPathRoot(CurrentDirectory);

                ValidPath = ValidPath.Replace(@"..\", "");

                for (int BackDir = 0; BackDir < BackCount; BackDir++)
                {
                    if (CurrentDirectory == RootPath)
                    {
                        break;
                    }

                    CurrentDirectory = Directory.GetParent(CurrentDirectory).FullName;
                }

                ValidPath = Path.Join(CurrentDirectory, ValidPath);
            }

            // Remove invalid path characters
            foreach (char InvalidCharacter in Path.GetInvalidPathChars())
            {
                ValidPath = ValidPath.Replace(InvalidCharacter, '-');
            }

            ValidPath = Path.GetFullPath(ValidPath).Replace(@"\\", @"\");

            /*if (AppendQuotes)
            {
                ValidPath = $"\"{ValidPath}\"";
            }*/

            return ValidPath;
        }
    }
}
