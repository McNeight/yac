// <copyright file="LineNumbers.cs" company="yac Contributors">
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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace yac
{
    public class LineNumbers
    {
        public LineNumbers(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new System.ArgumentException($"'{nameof(str)}' cannot be null or empty.", nameof(str));
            }

            _totalLength = str.Length;
            // Build a list of the offsets of the start of all lines
            _lineOffsets.Add(0);
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\r' && i + 1 < str.Length && str[i + 1] == '\n')
                {
                    i++;
                }

                if (str[i] == '\n' || str[i] == '\r')
                {
                    _lineOffsets.Add(i + 1);
                }
            }
        }

        /// <summary>
        /// Convert a file offset to a line number and character position.
        /// </summary>
        /// <param name="fileOffset"></param>
        /// <param name="lineNumber"></param>
        /// <param name="charPosition"></param>
        /// <returns></returns>
        public bool FromFileOffset(int fileOffset, out int lineNumber, out int charPosition)
        {
            // Look up line offset
            for (int i=1; i<_lineOffsets.Count; i++)
            {
                if (fileOffset < _lineOffsets[i])
                {
                    lineNumber = i - 1;
                    charPosition = fileOffset - _lineOffsets[i - 1];
                    return true;
                }
            }
            
            // Past the last line?
            lineNumber = _lineOffsets.Count - 1;
            charPosition = fileOffset - _lineOffsets[_lineOffsets.Count - 1];
            return false;
        }

        /// <summary>
        /// Convert a line number and character position to a file offset.
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <param name="characterPosition"></param>
        /// <returns></returns>
        public int ToFileOffset(int lineNumber, int characterPosition)
        {
            if (lineNumber > _lineOffsets.Count)
            {
                return _totalLength;
            }

            int offset = _lineOffsets[lineNumber] + characterPosition;
            if (offset > _totalLength)
            {
                offset = _totalLength;
            }

            return offset;
        }

        private readonly Collection<int> _lineOffsets = new Collection<int>();
        private readonly int _totalLength;
    }
}
