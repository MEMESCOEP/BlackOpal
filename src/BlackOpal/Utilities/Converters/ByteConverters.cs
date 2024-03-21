using System;
using System.Text;

namespace BlackOpal.Utilities.Converters
{
    internal class ByteConverters
    {
        public static string ByteArrayToString(byte[] arrInput)
        {
            StringBuilder sOutput = new StringBuilder(arrInput.Length);

            for (int ByteIndex = 0; ByteIndex < arrInput.Length - 1; ByteIndex++)
            {
                sOutput.Append(arrInput[ByteIndex].ToString("X2"));
            }

            return sOutput.ToString();
        }
    }
}
