// <copyright file="Cdp1802.cs" company="yac Contributors">
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

using yac.sim.io;
using yac.sim.mem;

namespace yac.sim.cpu
{
    /**********************************************************************

        RCA COSMAC CPU emulation

    **********************************************************************
                                _____   _____
                       Vcc   1 |*    \_/     | 40  Vdd
                    _BUS 3   2 |             | 39  _BUS 4
                    _BUS 2   3 |             | 38  _BUS 5
                    _BUS 1   4 |             | 37  _BUS 6
                    _BUS 0   5 |             | 36  _BUS 7
                       _N0   6 |             | 35  Vss
                       _N1   7 |             | 34  _EF1
                       _N2   8 |             | 33  _EF2
                       _N3   9 |             | 32  _EF3
                         *  10 |   CDP1801U  | 31  _EF4
                         *  11 |             | 30  _DMA OUT
                         *  12 |             | 29  _INTERRUPT
                         *  13 |             | 28  _DMA IN
                         *  14 |             | 27  _CLEAR
                    _CLOCK  15 |             | 26  _LOAD
                      _TPB  16 |             | 25  _SC2
                      _TPA  17 |             | 24  _SC1
                         *  18 |             | 23  _SC0
                       MWR  19 |             | 22  _M READ
                       Vss  20 |_____________| 21  *

                                _____   _____
                       Vcc   1 |*    \_/     | 28  Vdd
                    _BUS 4   2 |             | 27  _BUS 3
                    _BUS 5   3 |             | 26  _BUS 2
                    _BUS 6   4 |             | 25  _BUS 1
                    _BUS 7   5 |             | 24  _BUS 0
                      _MA0   6 |             | 23  *
                      _MA1   7 |   CDP1801C  | 22  _TPB
                      _MA2   8 |             | 21  *
                      _MA3   9 |             | 20  *
                      _MA4  10 |             | 19  *
                      _MA5  11 |             | 18  *
                      _MA6  12 |             | 17  *
                      _MA7  13 |             | 16  *
                       Vss  14 |_____________| 15  _CLEAR

                                _____   _____
                     CLOCK   1 |*    \_/     | 40  Vdd
                     _WAIT   2 |             | 39  _XTAL
                    _CLEAR   3 |             | 38  _DMA IN
                         Q   4 |             | 37  _DMA OUT
                       SC1   5 |             | 36  _INTERRUPT
                       SC0   6 |             | 35  _MWR
                      _MRD   7 |             | 34  TPA
                     BUS 7   8 |             | 33  TPB
                     BUS 6   9 |   CDP1802   | 32  MA7
                     BUS 5  10 |   CDP1804   | 31  MA6
                     BUS 4  11 |   CDP1805   | 30  MA5
                     BUS 3  12 |   CDP1806   | 29  MA4
                     BUS 2  13 |             | 28  MA3
                     BUS 1  14 |             | 27  MA2
                     BUS 0  15 |             | 26  MA1
                         *  16 |             | 25  MA0
                        N2  17 |             | 24  _EF1
                        N1  18 |             | 23  _EF2
                        N0  19 |             | 22  _EF3
                       Vss  20 |_____________| 21  _EF4

        Type            Internal ROM    Internal RAM    Timer   Pin 16 (*)
        ------------------------------------------------------------------
        CDP1801         none            none            no          Vcc
        CDP1802         none            none            no          Vcc
        CDP1804         2 KB            64 bytes        yes         _EMS
        CDP1805         none            64 bytes        yes         _ME
        CDP1806         none            none            yes         Vdd

    **********************************************************************/

    public partial class Cdp1802
    {
        private readonly State _s;
        private readonly IMemory _m;
        private readonly IPort _io;

        public Cdp1802(State s, IMemory m, IPort io)
        {
            _s = s;
            _m = m;
            _io = io;
        }

        public void Tick()
        {
            if (_s.idle)
            {
                return;
            }

            byte ini = Fetch();
            byte i = HighNibble(ini);
            byte n = LowNibble(ini);
            Execute(i, n);
        }

        private byte Fetch()
        {
            byte ini = _m.Read(_s.r[_s.p]++);
            _s.r[_s.p] &= 0xFFFF;
            return ini;
        }

        private static byte LowNibble(byte ini)
        {
            return (byte)(ini & 0xF);
        }

        private static byte HighNibble(byte ini)
        {
            return (byte)(ini >> 4);
        }

        private static byte LowByte(ushort w)
        {
            return (byte)(w & 0x00FF);
        }

        private static byte HighByte(ushort w)
        {
            return (byte)(w >> 8);
        }

        private static ushort WithLowByte(ushort w, byte b)
        {
            return (ushort)((w & 0xFF00) | b);
        }

        private static ushort WithHighByte(ushort w, byte b)
        {
            return (ushort)((b << 8) | (w & 0x00FF));
        }

        private void Execute(byte i, byte n)
        {
            switch (i)
            {
                case 0x0:
                    if (n == 0)
                    {
                        // Idle (IDL)
                        _s.idle = true;
                    }
                    else
                    {
                        // Load Via N (LDN)
                        _s.d = _m.Read(_s.r[n]);
                    }
                    break;

                case 0x1:
                    // Increment Register N (INC)
                    _s.r[n]++;
                    // With ushort registers, this is redundant
                    // _s.r[n] &= 0xFFFF;
                    break;

                case 0x2:
                    // Decrement Register N (DEC)
                    _s.r[n]--;
                    // With ushort registers, this is redundant
                    // _s.r[n] &= 0xFFFF;
                    break;

                case 0x3:
                    ExecuteShortBranch(n);
                    break;

                case 0x4:
                    // Load Advance (LDA)
                    _s.d = _m.Read(_s.r[n]++);
                    // With ushort registers, this is redundant
                    // _s.r[n] &= 0xFFFF;
                    break;

                case 0x5:
                    // Store Via N (STR)
                    _m.Write(_s.r[n], _s.d);
                    break;

                case 0x6:
                    ExecuteInputOutput(n);
                    break;

                case 0x7:
                    ExecuteControl(n);
                    break;

                case 0x8:
                    // Get Low Register N (GLO)
                    _s.d = LowByte(_s.r[n]);
                    break;

                case 0x9:
                    // Get High Register N (GHI)
                    _s.d = HighByte(_s.r[n]);
                    break;

                case 0xA:
                    // Put Low Register N (PLO)
                    _s.r[n] = WithLowByte(_s.r[n], _s.d);
                    break;

                case 0xB:
                    // Put High Register N (PHI)
                    _s.r[n] = WithHighByte(_s.r[n], _s.d);
                    break;

                case 0xC:
                    ExecuteLongBranch(n);
                    break;

                case 0xD:
                    // Set P (SEP)
                    _s.p = n;
                    break;

                case 0xE:
                    // Set X (SEX)
                    _s.x = n;
                    break;

                case 0xF:
                    ExecuteArithmeticLogic(n);
                    break;
            }
        }

        /// <summary>
        /// 0x3N
        /// </summary>
        /// <param name="n"></param>
        /// <exception cref="InstructionNotImplementedException"></exception>
        private void ExecuteShortBranch(byte n)
        {
            bool satisfied;
            switch (n)
            {
                case 0x0:
                    // Unconditional Short Branch (BR)
                    satisfied = true;
                    break;

                case 0x1:
                    // Short Branch if Q=1 (BQ)
                    satisfied = _s.q;
                    break;

                case 0x2:
                    // Short Branch if D=0 (BZ)
                    satisfied = (_s.d == 0);
                    break;

                case 0x3:
                    // Short Branch if DF=1 (BDF) (following a shift)
                    // Short Branch if Positive or Zero (BPZ) (following subtraction)
                    // Short Branch if Equal or Greater (BGE) (following comparison by subtraction)
                    satisfied = _s.df;
                    break;

                case 0x8:
                    // No Short Branch (NBR)
                    // Short Skip (SKP)
                    satisfied = false;
                    break;

                case 0x9:
                    // Short Branch if Q=0 (BNQ)
                    satisfied = !_s.q;
                    break;

                case 0xA:
                    // Short Branch if D NOT 0 (BNZ)
                    satisfied = (_s.d != 0);
                    break;

                case 0xB:
                    // Short Branch if DF=0 (BNF) (following a shift)
                    // Short Branch if Minus (BM) (following subtraction)
                    // Short Branch if Less (BL) (following comparison by subtraction)
                    satisfied = !_s.df;
                    break;

                case 0x4:
                // B1
                case 0x5:
                // B2
                case 0x6:
                // B3
                case 0x7:
                // B4
                case 0xC:
                // BN1
                case 0xD:
                // BN2
                case 0xE:
                // BN3
                case 0xF:
                // BN4
                default:
                    throw new InstructionNotImplementedException(0x3, n);
            }

            if (satisfied)
            {
                _s.r[_s.p] = WithLowByte(_s.r[_s.p], _m.Read(_s.r[_s.p]));
            }
            else
            {
                _s.r[_s.p]++;
                // With ushort registers, this is redundant
                // _s.r[_s.p] &= 0xFFFF;
            }
        }

        /// <summary>
        /// 0x6N
        /// </summary>
        /// <param name="n"></param>
        /// <exception cref="InstructionNotImplementedException"></exception>
        private void ExecuteInputOutput(byte n)
        {
            if (n == 0)
            {
                // Increment Register X (IRX)
                _s.r[_s.x]++;
                // With ushort registers, this is redundant
                // _s.r[_s.x] &= 0xFFFF;
            }
            else if (n < 8)
            {
                // Output (OUT)
                _io.Output(n, _m.Read(_s.r[_s.x]++));
                // With ushort registers, this is redundant
                // _s.r[_s.x] &= 0xFFFF;
            }
            else if (n == 8)
            {
                throw new InstructionNotImplementedException(0x6, n);
            }
            else
            {
                // Input (INP)
                _s.d = _io.Input((byte)(n & 0x7));
                _m.Write(_s.r[_s.x], _s.d);
            }
        }

        /// <summary>
        /// 0x7N
        /// </summary>
        /// <param name="n"></param>
        /// <exception cref="InstructionNotImplementedException"></exception>
        private void ExecuteControl(byte n)
        {
            switch (n)
            {
                case 0x2:
                    // Load Via X and Advance (LDXA)
                    _s.d = _m.Read(_s.r[_s.x]++);
                    // With ushort registers, this is redundant
                    // _s.r[_s.x] &= 0xFFFF;
                    break;

                case 0x3:
                    // Store Via X and Decrement (STXD)
                    _m.Write(_s.r[_s.x]--, _s.d);
                    // With ushort registers, this is redundant
                    // _s.r[_s.x] &= 0xFFFF;
                    break;

                case 0x4:
                    {
                        // Add with Carry (ADC)
                        int carry = _s.df ? 1 : 0;
                        int sum = _m.Read(_s.r[_s.x]) + _s.d + carry;
                        _s.d = (byte)(sum & 0xFF);
                        _s.df = sum > 0xFF;
                        break;
                    }

                case 0x5:
                    {
                        // Subtract D with Borrow (SDB)
                        int borrow = _s.df ? 0 : 1;
                        int diff = _m.Read(_s.r[_s.x]) - _s.d - borrow;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0x6:
                    {
                        // Shift Right with Carry (SHRC)
                        // Ring Shift Right (RSHR)
                        bool oldDf = _s.df;
                        _s.df = (_s.d & 0x01) != 0x00;
                        _s.d = (byte)((_s.d >> 1) | (oldDf ? 0x80 : 0x00));
                        break;
                    }

                case 0x7:
                    {
                        // Subtract Memory with Borrow (SMB)
                        int borrow = _s.df ? 0 : 1;
                        int diff = _s.d - _m.Read(_s.r[_s.x]) - borrow;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0xA:
                    // Reset Q (REQ)
                    _s.q = false;
                    break;

                case 0xB:
                    // Set Q (SEQ)
                    _s.q = true;
                    break;

                case 0xC:
                    {
                        // Add with Carry, Immediate (ADI)
                        int carry = _s.df ? 1 : 0;
                        int sum = _m.Read(_s.r[_s.p]++) + _s.d + carry;
                        // With ushort registers, this is redundant
                        // _s.r[_s.p] &= 0xFFFF;
                        _s.d = (byte)(sum & 0xFF);
                        _s.df = sum > 0xFF;
                        break;
                    }

                case 0xD:
                    {
                        // Subtract D with Borrow, Immediate (SDBI)
                        int borrow = _s.df ? 0 : 1;
                        int diff = _m.Read(_s.r[_s.p]++) - _s.d - borrow;
                        // With ushort registers, this is redundant
                        // _s.r[_s.p] &= 0xFFFF;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0xE:
                    {
                        // Shift Left with Carry (SHLC)
                        // Ring Shift Left (RSHL)
                        bool oldDf = _s.df;
                        _s.df = (_s.d & 0x80) != 0x00;
                        _s.d = (byte)((_s.d << 1) | (oldDf ? 0x01 : 0x00));
                        break;
                    }

                case 0xF:
                    {
                        // Subtract Memory with Borrow, Immediate (SMBI)
                        int borrow = _s.df ? 0 : 1;
                        int diff = _s.d - _m.Read(_s.r[_s.p]++) - borrow;
                        // With ushort registers, this is redundant
                        // _s.r[_s.p] &= 0xFFFF;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0x0:
                // RET
                case 0x1:
                // DIS
                case 0x8:
                // SAV
                case 0x9:
                // MARK
                default:
                    throw new InstructionNotImplementedException(0x7, n);
            }
        }

        /// <summary>
        /// 0xCN
        /// </summary>
        /// <param name="n"></param>
        /// <exception cref="InstructionNotImplementedException"></exception>
        private void ExecuteLongBranch(byte n)
        {
            switch (n)
            {
                case 0x4:
                    // No Operation (NOP)
                    break;

                case 0x0:
                // LBR
                case 0x1:
                // LBQ
                case 0x2:
                // LBZ
                case 0x3:
                // LBDF
                case 0x5:
                // LSNQ
                case 0x6:
                // LSNZ
                case 0x7:
                // LSNF
                case 0x8:
                // NLBR
                // LSKP
                case 0x9:
                // LBNQ
                case 0xA:
                // LBNZ
                case 0xB:
                // LBNF
                case 0xC:
                // LSIE
                case 0xD:
                // LSQ
                case 0xE:
                // LSZ
                case 0xF:
                // LSDF
                default:
                    throw new InstructionNotImplementedException(0xC, n);
            }
        }

        /// <summary>
        /// 0xFN
        /// </summary>
        /// <param name="n"></param>
        private void ExecuteArithmeticLogic(byte n)
        {
            switch (n)
            {
                case 0x0:
                    // Load Via X (LDX)
                    _s.d = _m.Read(_s.r[_s.x]);
                    break;

                case 0x1:
                    // OR (OR)
                    _s.d = (byte)(_m.Read(_s.r[_s.x]) | _s.d);
                    break;

                case 0x2:
                    // AND (AND)
                    _s.d = (byte)(_m.Read(_s.r[_s.x]) & _s.d);
                    break;

                case 0x3:
                    // XOR (XOR)
                    _s.d = (byte)(_m.Read(_s.r[_s.x]) ^ _s.d);
                    break;

                case 0x4:
                    {
                        // Add (ADD)
                        int sum = _m.Read(_s.r[_s.x]) + _s.d;
                        _s.d = (byte)(sum & 0xFF);
                        _s.df = sum > 0xFF;
                        break;
                    }

                case 0x5:
                    {
                        // Subtract D (SD)
                        int diff = _m.Read(_s.r[_s.x]) - _s.d;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0x6:
                    // Shift Right (SHR)
                    _s.df = (_s.d & 0x01) != 0x00;
                    _s.d = (byte)(_s.d >> 1);
                    break;

                case 0x7:
                    {
                        // Subtract Memory (SM)
                        int diff = _s.d - _m.Read(_s.r[_s.x]);
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0x8:
                    // Load Immediate (LDI)
                    _s.d = _m.Read(_s.r[_s.p]++);
                    // With ushort registers, this is redundant
                    // _s.r[_s.p] &= 0xFFFF;
                    break;

                case 0x9:
                    // OR Immediate (ORI)
                    _s.d = (byte)(_m.Read(_s.r[_s.p]++) | _s.d);
                    // With ushort registers, this is redundant
                    // _s.r[_s.p] &= 0xFFFF;
                    break;

                case 0xA:
                    // AND Immediate (ANI)
                    _s.d = (byte)(_m.Read(_s.r[_s.p]++) & _s.d);
                    // With ushort registers, this is redundant
                    // _s.r[_s.p] &= 0xFFFF;
                    break;

                case 0xB:
                    // XOR Immediate (XRI)
                    _s.d = (byte)(_m.Read(_s.r[_s.p]++) ^ _s.d);
                    // With ushort registers, this is redundant
                    // _s.r[_s.p] &= 0xFFFF;
                    break;

                case 0xC:
                    {
                        // Add Immediate (ADI)
                        int sum = _m.Read(_s.r[_s.p]++) + _s.d;
                        // With ushort registers, this is redundant
                        // _s.r[_s.p] &= 0xFFFF;
                        _s.d = (byte)(sum & 0xFF);
                        _s.df = sum > 0xFF;
                        break;
                    }

                case 0xD:
                    {
                        // Subtract D Immediate (SDI)
                        int diff = _m.Read(_s.r[_s.p]++) - _s.d;
                        // With ushort registers, this is redundant
                        // _s.r[_s.p] &= 0xFFFF;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }

                case 0xE:
                    // Shift Left (SHL)
                    _s.df = (_s.d & 0x80) != 0x00;
                    _s.d = (byte)(_s.d << 1);
                    break;

                case 0xF:
                    {
                        // Subtract Memory Immediate (SMI)
                        int diff = _s.d - _m.Read(_s.r[_s.p]++);
                        // With ushort registers, this is redundant
                        // _s.r[_s.p] &= 0xFFFF;
                        _s.d = (byte)(diff & 0xFF);
                        _s.df = diff >= 0x00;
                        break;
                    }
            }
        }
    }
}
