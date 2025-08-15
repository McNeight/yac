
/* Unmerged change from project 'yac.sim (net471)'
Before:
namespace yac.sim.cpu
After:
// <copyright file="Cdp1802.State.cs" company="yac Contributors">
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

namespace yac.sim.cpu
*/
// <copyright file="Cdp1802.State.cs" company="yac Contributors">
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

namespace yac.sim.cpu
{
    public partial class Cdp1802
    {
        public class State
        {
            /// <summary>
            /// I: Holds High-Order instruction digit (4 bits).
            /// </summary>
            public byte i;

            /// <summary>
            /// N: Holds Low-Order instruction digit (4 bits).
            /// </summary>
            public byte n;

            /// <summary>
            /// P: Designates which register is Program Counter (4 bits).
            /// </summary>
            public byte p;

            /// <summary>
            /// X: Designates which register is Data Pointer (4 bits).
            /// </summary>
            public byte x;

            /// <summary>
            /// D: Data Register (Accumulator).
            /// </summary>
            public byte d;

            /// <summary>
            /// T: Holds old X, P after Interrupt (temporary register; X is high byte).
            /// </summary>
            public byte t;

            /// <summary>
            /// R[0-F]: Scratchpad Registers.
            /// </summary>
            public readonly ushort[] r = new ushort[16];

            /// <summary>
            /// DF: Data Flag (ALU Carry).
            /// </summary>
            public bool df;

            /// <summary>
            /// IE: Master Interrupt Enable Flip-Flop.
            /// </summary>
            public bool ie = true;

            /// <summary>
            /// Q: Output Flip-Flop.
            /// </summary>
            public bool q;

            public bool idle;
        }
    }
}
