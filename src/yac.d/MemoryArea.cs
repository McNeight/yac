// <copyright file="MemoryArea.cs" company="yac Contributors">
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

namespace yac.d
{
    /// <summary>
    /// Definition of a memory area.
    /// </summary>
    public class MemoryArea : IComparable<MemoryArea>
    {
        public enum MemoryType
        { CODE, DATA };

        public MemoryType Type { get; }

        public int start, end;

        public MemoryArea(MemoryType type, int start, int end)
        {
            this.start = start;
            this.end = end;
            Type = type;
        }

        public int CompareTo(MemoryArea m)
        {
            return start.CompareTo(m.start);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(MemoryArea left, MemoryArea right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(MemoryArea left, MemoryArea right)
        {
            return !(left == right);
        }

        public static bool operator <(MemoryArea left, MemoryArea right)
        {
            /* Unmerged change from project 'COSMAC1802.Disassembler (net8.0)'
            Before:
                        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
            After:
                        return ReferenceEquals(left, null) ? !ReferenceEquals(right is not null : left.CompareTo(right) < 0;
            */

            /* Unmerged change from project 'COSMAC1802.Disassembler (net6.0)'
            Before:
                        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
            After:
                        return ReferenceEquals(left, null) ? !ReferenceEquals(right is not null : left.CompareTo(right) < 0;
            */
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(MemoryArea left, MemoryArea right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(MemoryArea left, MemoryArea right)
        {
            /* Unmerged change from project 'COSMAC1802.Disassembler (net8.0)'
            Before:
                        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
            After:
                        return !ReferenceEquals(left is not null && left.CompareTo(right) > 0;
            */

            /* Unmerged change from project 'COSMAC1802.Disassembler (net6.0)'
            Before:
                        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
            After:
                        return !ReferenceEquals(left is not null && left.CompareTo(right) > 0;
            */
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(MemoryArea left, MemoryArea right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
