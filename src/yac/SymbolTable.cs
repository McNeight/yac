// <copyright file="SymbolTable.cs" company="yac Contributors">
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

namespace yac
{
    /// <summary>
    /// Implements the Symbol Table.
    /// </summary>
    public class SymbolTable
    {
        // The table is stored in a dictionary
        private readonly Dictionary<string, Symbol> symtable;

        // Constructor
        public SymbolTable()
        {
            symtable = new Dictionary<string, Symbol>();
        }

        // Lookup a symbol
        // return null if not found
        public Symbol Lookup(string name)
        {
            if (symtable.ContainsKey(name))
            {
                return symtable[name];
            }
            else
            {
                return null;
            }
        }

        // Add a Symbol whithout a value
        public void Add(string name)
        {
            symtable.Add(name, new Symbol(name));
        }

        // Add a Symbol whith a value
        public void Add(string name, ushort value)
        {
            symtable.Add(name, new Symbol(name, value));
        }

        // Prints the symbol table to the console
        public void Print()
        {
            Console.Out.WriteLine("Symbol Table");
            Console.Out.WriteLine();
            Console.Out.WriteLine("Symbol".PadRight(Symbol.NameLen, ' ') +
                " Hex    Dec");
            foreach (KeyValuePair<string, Symbol> kvp in symtable)
            {
                Console.Out.WriteLine(kvp.Value.ToString());
            }
            Console.Out.WriteLine();
        }
    }
}
