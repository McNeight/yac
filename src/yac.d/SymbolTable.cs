// <copyright file="SymbolTable.cs" company="yac Contributors">
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

using System.Collections.Generic;

namespace yac.d
{
    // The Symbol table
    internal class SymbolTable
    {
        // The table is stored in a dictionary
        private readonly Dictionary<int, string> symtable;

        // Constructor
        public SymbolTable()
        {
            symtable = new Dictionary<int, string>();
        }

        // Check if there is a name for an address
        public bool HasName(int addr)
        {
            return symtable.ContainsKey(addr);
        }

        // Add a symbol
        public void Add(string name, int addr)
        {
            symtable.Add(addr, name);
        }

        // Get a name for an address
        public string GetName(int addr)
        {
            if (HasName(addr))
            {
                return symtable[addr];
            }
            else
            {
                return "L" + addr.ToString("X4");
            }
        }

        public int Count
        {
            get { return symtable.Count; }
        }
    }
}
