
/* Unmerged change from project 'yac.sim (net471)'
Before:
using System;
After:
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
*/
// <copyright file="Utils.cs" company="yac Contributors">
// MIT License
//
// Copyright © 2014 Rasmus Svensson
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

using yac.sim.mem;

namespace yac.sim.util
{
    public static class Utils
    {
        public static void LoadHex(IMemory m, string hexString)
        {
            if (m is null)
            {
                throw new ArgumentNullException(nameof(m));
            }

            if (string.IsNullOrEmpty(hexString))
            {
                throw new ArgumentException($"'{nameof(hexString)}' cannot be null or empty.", nameof(hexString));
            }

            if (hexString.Length % 2 != 0)
            {
                throw new FormatException("odd number of hex digits");
            }

            for (ushort i = 0; i < (hexString.Length / 2); i++)
            {
                char high = hexString[2 * i];
                char low = hexString[2 * i + 1];
                m.Write(i, HexByte(high, low));
            }
        }

        public static byte HexByte(char high, char low)
        {
            byte highInt = HexDigit(high);
            byte lowInt = HexDigit(low);
            return (byte)((highInt << 4) | lowInt);
        }

        public static byte HexDigit(char h)
        {
            if (h >= '0' && h <= '9')
            {
                return (byte)(h - '0');
            }
            else if (h >= 'a' && h <= 'f')
            {
                return (byte)(h - 'a' + 10);
            }
            else if (h >= 'A' && h <= 'F')
            {
                return (byte)(h - 'A' + 10);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public static int HexByteFromString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException($"'{nameof(s)}' cannot be null or empty.", nameof(s));
            }

            if (s.Length != 2)
            {
                throw new ArgumentException();
            }

            return HexByte(s[0], s[1]);
        }
    }
}
