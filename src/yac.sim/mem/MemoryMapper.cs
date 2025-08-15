
/* Unmerged change from project 'yac.sim (net471)'
Before:
using System;
After:
// <copyright file="MemoryMapper.cs" company="yac Contributors">
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
// <copyright file="MemoryMapper.cs" company="yac Contributors">
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
using System.Collections.ObjectModel;

namespace yac.sim.mem
{
    public sealed class MemoryMapper : IMemory
    {
        private sealed class Range
        {
            public readonly ushort start;
            public readonly ushort end;
            public readonly IMemory memory;

            public Range(ushort start, ushort end, IMemory memory)
            {
                this.start = start;
                this.end = end;
                this.memory = memory;
            }
        }

        private readonly Collection<Range> ranges = new Collection<Range>();

        public void Map(ushort start, ushort length, IMemory memory)
        {
            Range newRange = new Range(start, (ushort)(start + length), memory);
            foreach (Range range in ranges)
            {
                if (Overlaps(newRange, range))
                {
                    throw new Exception("overlapping ranges");
                }
            }

            ranges.Add(newRange);
        }

        private static bool Overlaps(Range x, Range y)
        {
            return Math.Max(x.start, y.start) < Math.Min(x.end, y.end);
        }

        private static bool Contains(Range range, ushort addr)
        {
            if (range is null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            return addr >= range.start && addr < range.end;
        }

        public byte Read(ushort addr)
        {
            foreach (Range range in ranges)
            {
                if (Contains(range, addr))
                {
                    return range.memory.Read((ushort)(addr - range.start));
                }
            }

            throw new Exception("memory unmapped");
        }

        public void Write(ushort addr, byte data)
        {
            foreach (Range range in ranges)
            {
                if (Contains(range, addr))
                {
                    range.memory.Write((ushort)(addr - range.start), data);
                    return;
                }
            }

            throw new Exception("memory unmapped");
        }
    }
}
