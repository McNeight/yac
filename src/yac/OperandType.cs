// <copyright file="OperandType.cs" company="yac Contributors">
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
    /// Type of operand.
    /// </summary>
    /// <remarks>
    /// Does not include value passed through a datalist.
    /// </remarks>
    public enum OperandType
    {
        /// <summary>
        /// No operand.
        /// </summary>
        NONE,

        /// <summary>
        /// Register N (0 to F).
        /// </summary>
        REG0,

        /// <summary>
        /// Register N (1 to F).
        /// </summary>
        REG1,

        /// <summary>
        /// I/O Device (1 to 7).
        /// </summary>
        IODEV,

        /// <summary>
        /// expression
        /// </summary>
        EXPR,

        /// <summary>
        /// Short (8-bit) address.
        /// </summary>
        ADDR8,

        /// <summary>
        /// Long (16-bit) address.
        /// </summary>
        ADDR16,
    };
}
