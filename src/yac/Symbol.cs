// <copyright file="Symbol.cs" company="yac Contributors">
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

namespace yac
{
    /// <summary>
    /// This class stores a symbol.
    /// </summary>
    public class Symbol
    {
        // Name Length for printing
        public const int NameLen = 8;

        public string Name { get; }

        public bool HasValue { get; private set; }

        // Symbol Value
        private ushort _value = 0;

        public ushort Value
        {
            get { return _value; }
            set
            {
                _value = value;
                HasValue = true;
            }
        }

        public bool Duplicate { get; private set; }

        // Constructors
        public Symbol(string name)
        {
            Name = name;
        }

        public Symbol(string name, ushort value)
        {
            Name = name;
            Value = value;
        }

        // Mark as duplicate
        public void MarkDup()
        {
            Duplicate = true;
        }

        // Convert to string for printing
        public override string ToString()
        {
            if (HasValue)
            {
                return Name.PadRight(NameLen, ' ').Substring(0, NameLen) + " " +
                    Value.ToString("X4") + ' ' + Value.ToString().PadLeft(5);
            }
            else
            {
                return Name.PadRight(NameLen, ' ').Substring(0, NameLen) +
                    " ????";
            }
        }
    }
}
