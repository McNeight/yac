// <copyright file="Utils.cs" company="yac Contributors">
// MIT License
//
// Copyright © 2017, 2022 Daniel Quadros
// Copyright © 2025 Neil McNeight
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace yac
{
	public static partial class Utils
    {
        public static string Indent(int indent)
        {
            return new string(' ', indent * 2);
        }

        public static string TypeName(object o)
        {
            if (o == null)
            {
                return "null";
            }

            var name = o.GetType().Name;
            if (name.StartsWith("ExprNode"))
            {
                return name.Substring(8).ToLowerInvariant();
            }

            return name;
        }

        public static byte PackByte(SourcePosition pos, object value)
        {
            if (value is long v)
            {
                return PackByte(pos, v);
            }

            Log.Error(pos, $"Can't convert {Utils.TypeName(value)} to byte");
            return 0xFF;
        }

        public static ushort PackWord(SourcePosition pos, object value)
        {
            if (value is long v)
            {
                return PackWord(pos, v);
            }

            Log.Error(pos, $"Can't convert {Utils.TypeName(value)} to word");
            return 0xFF;
        }

        public static byte PackByte(SourcePosition pos, long value)
        {
            // Check range (yes, sbyte and byte)
            if (value < sbyte.MinValue || value > byte.MaxValue)
            {
                Log.Error(pos, $"value out of range: {value} (0x{value:X}) doesn't fit in 8-bits");
                return 0xFF;
            }
            else
            {
                return (byte)(value & 0xFF);
            }
        }

        public static ushort PackWord(SourcePosition pos, long value)
        {
            // Check range (yes, short and ushort)
            if (value < short.MinValue || value > ushort.MaxValue)
            {
                Log.Error(pos, $"value out of range: {value} (0x{value:X}) doesn't fit in 16-bits");
                return 0xFFFF;
            }
            else
            {
                return (ushort)(value & 0xFFFF);
            }
        }

        public static ushort ParseUShort(string str)
		{
			try
			{
				if (str.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
				{
					return Convert.ToUInt16(str.Substring(2), 16);
				}
				else
				{
					return ushort.Parse(str);
				}
			}
			catch (Exception)
			{
				throw new InvalidOperationException(string.Format("Invalid number: '{0}'", str));
			}
		}

		public static int[] ParseIntegers(string str, int Count)
		{
			var values = new List<int>();
			if (str != null)
			{
				foreach (var n in str.Split(','))
				{
					values.Add(int.Parse(n));
				}
			}

			if (Count != 0 && Count != values.Count)
			{
				throw new InvalidOperationException(string.Format("Invalid value - expected {0} comma separated values", Count));
			}


			return values.ToArray();
		}


		public static List<string> ParseCommandLine(string args)
		{
			var newargs = new List<string>();

			var temp = new StringBuilder();

			int i = 0;
			while (i < args.Length)
			{
				if (char.IsWhiteSpace(args[i]))
				{
					i++;
					continue;
				}

				bool bInQuotes = false;
				temp.Length = 0;
				while (i < args.Length && (!char.IsWhiteSpace(args[i]) || bInQuotes))
				{
					if (args[i] == '\"')
					{
						if (args[i + 1] == '\"')
						{
							temp.Append("\"");
							i++;
						}
						else
						{
							bInQuotes = !bInQuotes;
						}
					}
					else
					{
						temp.Append(args[i]);
					}

					i++;
				}

				if (temp.Length > 0)
				{
					newargs.Add(temp.ToString());
				}
			}

			return newargs;
		}

	}
}
