using System;
using System.Collections.Generic;
using System.Text;

namespace Util
{
    public class DataConv
    {
        public static string ToText(byte[] buffer)
        {
            return Encoding.Default.GetString(buffer);
        }

        public static string ToHexStr(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(string.Format("{0:X2} ", buffer[i]));
            }

            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        public static string ToHexStr(byte[] buffer, string split)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < buffer.Length; i++)
            {
                sb.Append(string.Format("{0:X2}", buffer[i]));

                if (!string.IsNullOrEmpty(split)) sb.Append(split);

            }

            if (!string.IsNullOrEmpty(split) && sb.Length > 0 )
            {
                sb.Remove(sb.Length - split.Length, split.Length);
            }

            return sb.ToString();
        }

    }
}
