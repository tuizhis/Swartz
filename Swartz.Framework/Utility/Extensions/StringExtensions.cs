using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Swartz.Utility.Extensions
{
    public static class StringExtensions
    {
        public static string CamelFriendly(this string camel)
        {
            if (string.IsNullOrWhiteSpace(camel))
                return "";

            var sb = new StringBuilder(camel);

            for (var i = camel.Length - 1; i > 0; i--)
            {
                var current = sb[i];
                if ('A' <= current && current <= 'Z')
                {
                    sb.Insert(i, ' ');
                }
            }

            return sb.ToString();
        }

        public static string ToSafeDirectoryName(this string name)
        {
            foreach (var c in Path.GetInvalidPathChars())
            {
                name = name.Replace(c, '_');
            }

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        }

        public static byte[] ToByteArray(this string hex)
        {
            return
                Enumerable.Range(0, hex.Length)
                    .Where(x => 0 == x%2)
                    .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                    .ToArray();
        }

        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static string Sha1(this string input)
        {
            return Hash(input, "sha1");
        }

        public static string Sha256(this string input)
        {
            return Hash(input);
        }

        public static string Md5(this string input)
        {
            return Hash(input, "md5");
        }

        public static string ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string FromBase64(this string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        private static string Hash(string input, string algorithm = "sha256")
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return Hash(Encoding.UTF8.GetBytes(input), algorithm);
        }

        private static string Hash(byte[] input, string algorithm = "sha256")
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using (var algorithm2 = HashAlgorithm.Create(algorithm))
            {
                if (algorithm2 == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "不支持的哈希散列算法：{0}",
                        algorithm));
                }

                return BinaryToHex(algorithm2.ComputeHash(input));
            }
        }

        private static string BinaryToHex(byte[] data)
        {
            var chArray = new char[data.Length*2];
            for (var i = 0; i < data.Length; i++)
            {
                var num2 = (byte) (data[i] >> 4);
                chArray[i*2] = num2 > 9 ? (char) (num2 + 0x37) : (char) (num2 + 0x30);
                num2 = (byte) (data[i] & 15);
                chArray[i*2 + 1] = num2 > 9 ? (char) (num2 + 0x37) : (char) (num2 + 0x30);
            }
            return new string(chArray);
        }
    }
}