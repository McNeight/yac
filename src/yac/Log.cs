// <copyright file="Log.cs" company="yac Contributors">
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
using System.IO;

namespace yac
{
    public static class Log
    {
        private static readonly TextWriter _output = Console.Out;
        private static int _warnings = 0;

        public static int ErrorCount { get; private set; }

        public static void Warning(SourcePosition position, string message)
        {
            _warnings++;
            _output.WriteLine($"{position.Describe()}: warning: {message}");
        }

        public static void Error(SourcePosition position, string message)
        {
            ErrorCount++;
            _output.WriteLine($"{position.Describe()}: {message}");

            if (ErrorCount > 100)
            {
                throw new InvalidDataException("Error limit exceeded, aborting");
            }
        }


        public static void Error(string message)
        {
            _output.WriteLine($"{message}");
        }

        public static void Error(CodeException exception)
        {
            if (exception is null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            Error(exception.Position, exception.Message);
        }

        public static void DumpSummary()
        {
            _output.WriteLine($"Errors: {ErrorCount} Warnings: {_warnings}");
        }
    }
}
