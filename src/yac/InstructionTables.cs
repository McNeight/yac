// <copyright file="InstructionTables.cs" company="yac Contributors">
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
    /// Instruction table.
    /// </summary>
    public static class InstructionTables
    {
        private static readonly Dictionary<string, Instruction> s_mnemonicToInstruction = InitializeMnemonicToInstruction();
        private static readonly Dictionary<byte, string> s_byteToMnemonic = InitializeByteToMnemonic();

        /// <summary>
        /// Defined by Class of Operation as listed in Appendix A, Table I.
        /// </summary>
        private static Dictionary<string, Instruction> InitializeMnemonicToInstruction()
        {
            Dictionary<string, Instruction> d = new Dictionary<string, Instruction>
            {
                // Register Operations Instructions
                { "INC", new Instruction("INC", "INCREMENT REG N", 0x10, OperandType.REG0, "R(N) +1", 2, 1) },
                { "DEC", new Instruction("DEC", "DECREMENT REG N", 0x20, OperandType.REG0, "R(N) -1", 2, 1) },
                { "IRX", new Instruction("IRX", "INCREMENT REG X", 0x60, OperandType.NONE, "R(X) +1", 2, 1) },
                { "GLO", new Instruction("GLO", "GET LOW REG N", 0x80, OperandType.REG0, "R(N).0→D", 2, 1) },
                { "PLO", new Instruction("PLO", "PUT LOW REG N", 0xA0, OperandType.REG0, "D→R(N).0", 2, 1) },
                { "GHI", new Instruction("GHI", "GET HIGH REG N", 0x90, OperandType.REG0, "R(N).1→D", 2, 1) },
                { "PHI", new Instruction("PHI", "PUT HIGH REG N", 0xB0, OperandType.REG0, "D→R(N).1", 2, 1) },
                // Memory Reference Instructions
                { "LDN", new Instruction("LDN", "LOAD VIA N", 0x00, OperandType.REG1, "M(R(N))→D; FOR N NOT 0", 2, 1) },
                { "LDA", new Instruction("LDA", "LOAD ADVANCE", 0x40, OperandType.REG0, "M(R(N))→D; R(N) +1", 2, 1) },
                { "LDX", new Instruction("LDX", "LOAD VIA X", 0xF0, OperandType.NONE, "M(R(X))→D", 2, 1) },
                { "LDXA", new Instruction("LDXA", "LOAD VIA X AND ADVANCE", 0x72, OperandType.NONE, "M(R(X))→D; R(X) +1", 2, 1) },
                { "LDI", new Instruction("LDI", "LOAD IMMEDIATE", 0xF8, OperandType.EXPR, "M(R(P))→D; R(P) +1", 2, 2) },
                { "STR", new Instruction("STR", "STORE VIA N", 0x50, OperandType.REG0, "D→M(R(N))", 2, 1) },
                { "STXD", new Instruction("STXD", "STORE VIA X AND DECREMENT", 0x73, OperandType.NONE, "D→M(R(X)); R(X) -1", 2, 1) },
                // Logic Operations Instructions
                { "OR", new Instruction("OR",  "OR", 0xF1, OperandType.NONE, "M(R(X)) OR D→D", 2, 1) },
                { "ORI", new Instruction("ORI", "OR IMMEDIATE", 0xF9, OperandType.EXPR, "M(R(P)) OR D→D; R(P) +1", 2, 2) },
                { "XOR", new Instruction("XOR", "EXCLUSIVE OR", 0xF3, OperandType.NONE, "M(R(X)) XOR D→D", 2, 1) },
                { "XRI", new Instruction("XRI", "EXCLUSIVE OR IMMEDIATE", 0xFB, OperandType.EXPR, "M(R(P)) XOR D→D; R(P) +1", 2, 2) },
                { "AND", new Instruction("AND", "AND", 0xF2, OperandType.NONE, "M(R(X)) AND D→D", 2, 1) },
                { "ANI", new Instruction("ANI", "AND IMMEDIATE", 0xFA, OperandType.EXPR, "M(R(P)) AND D→D; R(P) +1", 2, 2) },
                { "SHR", new Instruction("SHR", "SHIFT RIGHT", 0xF6, OperandType.NONE, "SHIFT D RIGHT, LSB(D)→DF, 0→MSB(D)", 2, 1) },
                { "SHRC", new Instruction("SHRC", "SHIFT RIGHT WITH CARRY", 0x76, OperandType.NONE, "SHIFT D RIGHT, LSB(D)→DF, DF→MSB(D)", 2, 1) },
                { "RSHR", new Instruction("RSHR", "RING SHIFT RIGHT", 0x76, OperandType.NONE, "SHIFT D RIGHT, LSB(D)→DF, DF→MSB(D)", 2, 1) },
                { "SHL", new Instruction("SHL", "SHIFT LEFT", 0xFE, OperandType.NONE, "SHIFT D LEFT, MSB(D)→DF, 0→LSB(D)", 2, 1) },
                { "SHLC", new Instruction("SHLC", "SHIFT LEFT WITH CARRY", 0x7E, OperandType.NONE, "SHIFT D LEFT, MSB(D)→DF, DF→LSB(D)", 2, 1) },
                { "RSHL", new Instruction("RSHL", "RING SHIFT LEFT", 0x7E, OperandType.NONE, "SHIFT D LEFT, MSB(D)→DF, DF→LSB(D)", 2, 1) },
                // Arithmetic Operations Instructions
                { "ADD", new Instruction("ADD", "ADD", 0xF4, OperandType.NONE, "M(R(X)) +D→DF, D", 2, 1) },
                { "ADI", new Instruction("ADI", "ADD IMMEDIATE", 0xFC, OperandType.EXPR, "M(R(P)) +D→DF, D; R(P) +1", 2, 2) },
                { "ADC", new Instruction("ADC", "ADD WITH CARRY", 0x74, OperandType.NONE, "M(R(X)) +D+DF→DF, D", 2, 1) },
                { "ADCI", new Instruction("ADCI", "ADD WITH CARRY, IMMEDIATE", 0x7C, OperandType.EXPR, "M(R(P)) +D+DF→DF, D; R(P) +1", 2, 2) },
                { "SD", new Instruction("SD",  "SUBTRACT D", 0xF5, OperandType.NONE, "M(R(X)) -D→DF, D", 2, 1) },
                { "SDI", new Instruction("SDI", "SUBTRACT D IMMEDIATE", 0xFD, OperandType.EXPR, "M(R(P)) -D→DF, D; R(P) +1", 2, 2) },
                { "SDB", new Instruction("SDB", "SUBTRACT D WITH BORROW", 0x75, OperandType.NONE, "M(R(X)) -D-(NOT DF)→DF, D", 2, 1) },
                { "SDBI", new Instruction("SDBI", "SUBTRACT D WITH BORROW, IMMEDIATE", 0x7D, OperandType.EXPR, "M(R(P)) -D-(NOT DF)→DF, D; R(P) +1", 2, 2) },
                { "SM", new Instruction("SM",  "SUBTRACT MEMORY", 0xF7, OperandType.NONE, "D-M(R(X))→DF, D", 2, 1) },
                { "SMI", new Instruction("SMI", "SUBTRACT MEMORY IMMEDIATE", 0xFF, OperandType.EXPR, "D-M(R(P))→DF, D; R(P) +1", 2, 2) },
                { "SMB", new Instruction("SMB", "SUBTRACT MEMORY WITH BORROW", 0x77, OperandType.NONE, "D-M(R(X))-(NOT DF)→DF, D", 2, 1) },
                { "SMBI", new Instruction("SMBI", "SUBTRACT MEMORY WITH BORROW, IMMEDIATE", 0x7F, OperandType.EXPR, "D-M(R(P))-(NOT DF)→DF, D; R(P) +1", 2, 2) },
                // Branch Instructions - Short Branch
                { "BR", new Instruction("BR", "SHORT BRANCH", 0x30, OperandType.ADDR8, "M(R(P))→R(P).0", 2, 2) },
                { "NBR", new Instruction("NBR", "NO SHORT BRANCH (SEE SKP)", 0x38, OperandType.NONE, "R(P) +1", 2, 2) },
                { "BZ", new Instruction("BZ", "SHORT BRANCH IF D=0", 0x32, OperandType.ADDR8, "IF D=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BNZ", new Instruction("BNZ", "SHORT BRANCH IF D NOT 0", 0x3A, OperandType.ADDR8, "IF D NOT 0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BDF", new Instruction("BDF", "SHORT BRANCH IF DF=1", 0x33, OperandType.ADDR8, "IF DF=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BPZ", new Instruction("BPZ", "SHORT BRANCH IF POS OR ZERO", 0x33, OperandType.ADDR8, "IF DF=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BGE", new Instruction("BGE", "SHORT BRANCH IF EOUAL OR GREATER", 0x33, OperandType.ADDR8, "IF DF=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BNF", new Instruction("BNF", "SHORT BRANCH IF DF=0", 0x3B, OperandType.ADDR8, "IF DF=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BM", new Instruction("BM", "SHORT BRANCH IF MINUS", 0x3B, OperandType.ADDR8, "IF DF=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BL", new Instruction("BL", "SHORT BRANCH IF LESS", 0x3B, OperandType.ADDR8, "IF DF=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BQ", new Instruction("BQ", "SHORT BRANCH IF Q=1", 0x31, OperandType.ADDR8, "IF Q=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BNQ", new Instruction("BNQ", "SHORT BRANCH IF Q=0", 0x39, OperandType.ADDR8, "IF Q=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "B1", new Instruction("B1", "SHORT BRANCH IF EF1=1", 0x34, OperandType.ADDR8, "IF EF1=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BN1", new Instruction("BN1", "SHORT BRANCH IF EF1=0", 0x3C, OperandType.ADDR8, "IF EF1=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "B2", new Instruction("B2", "SHORT BRANCH IF EF2=1", 0x35, OperandType.ADDR8, "IF EF2=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BN2", new Instruction("BN2", "SHORT BRANCH IF EF2=0", 0x3D, OperandType.ADDR8, "IF EF2=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "B3", new Instruction("B3", "SHORT BRANCH IF EF3=1", 0x36, OperandType.ADDR8, "IF EF3=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BN3", new Instruction("BN3", "SHORT BRANCH IF EF3=0", 0x3E, OperandType.ADDR8, "IF EF3=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "B4", new Instruction("B4", "SHORT BRANCH IF EF4=1", 0x37, OperandType.ADDR8, "IF EF4=1, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                { "BN4", new Instruction("BN4", "SHORT BRANCH IF EF4=0", 0x3F, OperandType.ADDR8, "IF EF4=0, M(R(P))→R(P).0; ELSE R(P) +1", 2, 2) },
                // Branch Instructions - Long Branch
                { "LBR", new Instruction("LBR", "LONG BRANCH", 0xC0, OperandType.ADDR16, "M(R(P))→R(P).1; M(R(P) +1)→R(P).0", 3, 3) },
                { "NLBR", new Instruction("NLBR", "NO LONG BRANCH (SEE LSKP)", 0xC8, OperandType.ADDR16, "R(P) +2", 3, 3) },
                { "LBZ", new Instruction("LBZ", "LONG BRANCH IF D=0", 0xC2, OperandType.ADDR16, "IF D=0, M(R(P))→R(P).1; M(R(P) +1)→R(P).0; ELSE R(P) +2", 3, 3) },
                { "LBNZ", new Instruction("LBNZ", "LONG BRANCH IF D NOT 0", 0xCA, OperandType.ADDR16, "IF D NOT 0, M(R(P))→R(P).1; M(R(P) +1)→R(P).0; ELSE R(P) +2", 3, 3) },
                { "LBDF", new Instruction("LBDF", "LONG BRANCH IF DF=1", 0xC3, OperandType.ADDR16, "IF DF=1, M(R(P))→R(P).1; M(R(P) +1)→R(P).0; ELSE R(P) +2", 3, 3) },
                { "LBNF", new Instruction("LBNF", "LONG BRANCH IF DF=0", 0xCB, OperandType.ADDR16, "IF DF=0, M(R(P))→R(P).1; M(R(P) +1)→R(P).0; ELSE R(P) +2", 3, 3) },
                { "LBQ", new Instruction("LBQ", "LONG BRANCH IF Q=1", 0xC1, OperandType.ADDR16, "IF Q=1, M(R(P))→R(P).1; M(R(P) +1)→R(P).0; ELSE R(P) +2", 3, 3) },
                { "LBNQ", new Instruction("LBNQ", "LONG BRANCH IF Q=0", 0xC9, OperandType.ADDR16, "IF Q=0, M(R(P))→R(P).1; M(R(P) +1)→R(P).0; ELSE R(P) +2", 3, 3) },
                // Skip Instructions
                { "SKP", new Instruction("SKP", "SHORT SKIP (SEE NBR)", 0x38, OperandType.NONE, "R(P) +1", 2, 1) },
                { "LSKP", new Instruction("LSKP", "LONG SKIP (SEE NLBR)", 0xC8, OperandType.NONE, "R(P) +2", 3, 1) },
                { "LSZ", new Instruction("LSZ", "LONG SKIP IF D=0", 0xCE, OperandType.NONE, "IF D=0, R(P) +2; ELSE CONTINUE", 3, 1) },
                { "LSNZ", new Instruction("LSNZ", "LONG SKIP IF D NOT 0", 0xC6, OperandType.NONE, "IF D NOT 0, R(P) +2; ELSE CONTINUE", 3, 1) },
                { "LSDF", new Instruction("LSDF", "LONG SKIP IF DF=1", 0xCF, OperandType.NONE, "IF DF=1, R(P) +2; ELSE CONTINUE", 3, 1) },
                { "LSNF", new Instruction("LSNF", "LONG SKIP IF DF=0", 0xC7, OperandType.NONE, "IF DF=0, R(P) +2; ELSE CONTINUE", 3, 1) },
                { "LSQ", new Instruction("LSQ", "LONG SKIP IF Q=1", 0xCD, OperandType.NONE, "IF Q=1, R(P) +2; ELSE CONTINUE", 3, 1) },
                { "LSNQ", new Instruction("LSNQ", "LONG SKIP IF Q=O", 0xC5, OperandType.NONE, "IF Q=0, R(P) +2; ELSE CONTINUE", 3, 1) },
                { "LSIE", new Instruction("LSIE", "LONG SKIP IF IE=1", 0xCC, OperandType.NONE, "IF IE=1, R(P) +2; ELSE CONTINUE", 3, 1) },
                // Control Instructions
                { "IDL", new Instruction("IDL", "IDLE",0x00,OperandType.NONE,"WAIT FOR DMA OR INTERRUPT; M(R(0))→BUS",2,1) },
                { "NOP", new Instruction("NOP", "NO OPERATION",0xC4,OperandType.NONE,"CONTINUE",3,1) },
                { "SEP", new Instruction("SEP", "SET P", 0xD0, OperandType.REG0, "N→P", 2, 1) },
                { "SEX", new Instruction("SEX", "SET X", 0xE0, OperandType.REG0, "N→X", 2, 1) },
                { "SEQ", new Instruction("SEQ", "SET Q", 0x7B, OperandType.NONE, "1→Q", 2, 1) },
                { "REQ", new Instruction("REQ", "RESET Q", 0x7A, OperandType.NONE, "0→Q", 2, 1) },
                { "SAV", new Instruction("SAV", "SAVE", 0x78, OperandType.NONE, "T→M(R(X))", 2, 1) },
                { "MARK", new Instruction("MARK", "PUSH X,P TO STACK", 0x79, OperandType.NONE, "(X,P)→T; (X,P)→M(R(2)) THEN P→X; R(2)-1", 2, 1) },
                { "RET", new Instruction("RET", "RETURN", 0x70, OperandType.NONE, "M(R(X))→(X,P); R(X) +1; 1→IE", 2, 1) },
                { "DIS", new Instruction("DIS", "DISABLE", 0x71, OperandType.NONE, "M(R(X))→(X,P); R(X) +1; 0→IE", 2, 1) },
                // Input-Output Byte Transfer
                { "OUT", new Instruction("OUT", "OUTPUT N",0x60,OperandType.IODEV,"M(R(X))→BUS; R(X) +1; N LINES = N",2,1) },
                { "INP", new Instruction("INP", "INPUT N",0x68,OperandType.IODEV,"BUS→M(R(X)); BUS→D; N LINES = N",2,1) }
            };

            return d;
        }

        /// <summary>
        /// Defined by Numerical Order as listed in Appendix A, Table II.
        /// </summary>
        private static Dictionary<byte, string> InitializeByteToMnemonic()
        {
            Dictionary<byte, string> d = new Dictionary<byte, string>
            {
                { 0x00, "IDL" },
                { 0x01, "LDN" },
                { 0x02, "LDN" },
                { 0x03, "LDN" },
                { 0x04, "LDN" },
                { 0x05, "LDN" },
                { 0x06, "LDN" },
                { 0x07, "LDN" },
                { 0x08, "LDN" },
                { 0x09, "LDN" },
                { 0x0A, "LDN" },
                { 0x0B, "LDN" },
                { 0x0C, "LDN" },
                { 0x0D, "LDN" },
                { 0x0E, "LDN" },
                { 0x0F, "LDN" },
                { 0x10, "INC" },
                { 0x11, "INC" },
                { 0x12, "INC" },
                { 0x13, "INC" },
                { 0x14, "INC" },
                { 0x15, "INC" },
                { 0x16, "INC" },
                { 0x17, "INC" },
                { 0x18, "INC" },
                { 0x19, "INC" },
                { 0x1A, "INC" },
                { 0x1B, "INC" },
                { 0x1C, "INC" },
                { 0x1D, "INC" },
                { 0x1E, "INC" },
                { 0x1F, "INC" },
                { 0x20, "DEC" },
                { 0x21, "DEC" },
                { 0x22, "DEC" },
                { 0x23, "DEC" },
                { 0x24, "DEC" },
                { 0x25, "DEC" },
                { 0x26, "DEC" },
                { 0x27, "DEC" },
                { 0x28, "DEC" },
                { 0x29, "DEC" },
                { 0x2A, "DEC" },
                { 0x2B, "DEC" },
                { 0x2C, "DEC" },
                { 0x2D, "DEC" },
                { 0x2E, "DEC" },
                { 0x2F, "DEC" },
                { 0x30, "BR" },
                { 0x31, "BQ" },
                { 0x32, "BZ" },
                { 0x33, "BDF" },
                //{ 0x33, "BPZ" },
                //{ 0x33, "BGE" },
                { 0x34, "B1" },
                { 0x35, "B2" },
                { 0x36, "B3" },
                { 0x37, "B4" },
                { 0x38, "NBR" },
                //{ 0x38, "SKP" },
                { 0x39, "BNQ" },
                { 0x3A, "BNZ" },
                { 0x3B, "BNF" },
                //{ 0x3B, "BM" },
                //{ 0x3B, "BL" },
                { 0x3C, "BN1" },
                { 0x3D, "BN2" },
                { 0x3E, "BN3" },
                { 0x3F, "BN4" },
                { 0x40, "LDA" },
                { 0x41, "LDA" },
                { 0x42, "LDA" },
                { 0x43, "LDA" },
                { 0x44, "LDA" },
                { 0x45, "LDA" },
                { 0x46, "LDA" },
                { 0x47, "LDA" },
                { 0x48, "LDA" },
                { 0x49, "LDA" },
                { 0x4A, "LDA" },
                { 0x4B, "LDA" },
                { 0x4C, "LDA" },
                { 0x4D, "LDA" },
                { 0x4E, "LDA" },
                { 0x4F, "LDA" },
                { 0x50, "STR" },
                { 0x51, "STR" },
                { 0x52, "STR" },
                { 0x53, "STR" },
                { 0x54, "STR" },
                { 0x55, "STR" },
                { 0x56, "STR" },
                { 0x57, "STR" },
                { 0x58, "STR" },
                { 0x59, "STR" },
                { 0x5A, "STR" },
                { 0x5B, "STR" },
                { 0x5C, "STR" },
                { 0x5D, "STR" },
                { 0x5E, "STR" },
                { 0x5F, "STR" },
                { 0x60, "IRX" },
                { 0x61, "OUT" },
                { 0x62, "OUT" },
                { 0x63, "OUT" },
                { 0x64, "OUT" },
                { 0x65, "OUT" },
                { 0x66, "OUT" },
                { 0x67, "OUT" },
                { 0x68, "XXX" },
                { 0x69, "INP" },
                { 0x6A, "INP" },
                { 0x6B, "INP" },
                { 0x6C, "INP" },
                { 0x6D, "INP" },
                { 0x6E, "INP" },
                { 0x6F, "INP" },
                { 0x70, "RET" },
                { 0x71, "DIS" },
                { 0x72, "LDXA" },
                { 0x73, "STXD" },
                { 0x74, "ADC" },
                { 0x75, "SDB" },
                { 0x76, "SHRC" },
                //{ 0x76, "RSHR" },
                { 0x77, "SMB" },
                { 0x78, "SAV" },
                { 0x79, "MARK" },
                { 0x7A, "SEQ" },
                { 0x7B, "REQ" },
                { 0x7C, "ADDI" },
                { 0x7D, "SDBI" },
                { 0x7E, "SHLC" },
                //{ 0x7E, "RSHL" },
                { 0x7F, "SMBI" },
                { 0x80, "GLO" },
                { 0x81, "GLO" },
                { 0x82, "GLO" },
                { 0x83, "GLO" },
                { 0x84, "GLO" },
                { 0x85, "GLO" },
                { 0x86, "GLO" },
                { 0x87, "GLO" },
                { 0x88, "GLO" },
                { 0x89, "GLO" },
                { 0x8A, "GLO" },
                { 0x8B, "GLO" },
                { 0x8C, "GLO" },
                { 0x8D, "GLO" },
                { 0x8E, "GLO" },
                { 0x8F, "GLO" },
                { 0x90, "GHI" },
                { 0x91, "GHI" },
                { 0x92, "GHI" },
                { 0x93, "GHI" },
                { 0x94, "GHI" },
                { 0x95, "GHI" },
                { 0x96, "GHI" },
                { 0x97, "GHI" },
                { 0x98, "GHI" },
                { 0x99, "GHI" },
                { 0x9A, "GHI" },
                { 0x9B, "GHI" },
                { 0x9C, "GHI" },
                { 0x9D, "GHI" },
                { 0x9E, "GHI" },
                { 0x9F, "GHI" },
                { 0xA0, "PLO" },
                { 0xA1, "PLO" },
                { 0xA2, "PLO" },
                { 0xA3, "PLO" },
                { 0xA4, "PLO" },
                { 0xA5, "PLO" },
                { 0xA6, "PLO" },
                { 0xA7, "PLO" },
                { 0xA8, "PLO" },
                { 0xA9, "PLO" },
                { 0xAA, "PLO" },
                { 0xAB, "PLO" },
                { 0xAC, "PLO" },
                { 0xAD, "PLO" },
                { 0xAE, "PLO" },
                { 0xAF, "PLO" },
                { 0xB0, "PHI" },
                { 0xB1, "PHI" },
                { 0xB2, "PHI" },
                { 0xB3, "PHI" },
                { 0xB4, "PHI" },
                { 0xB5, "PHI" },
                { 0xB6, "PHI" },
                { 0xB7, "PHI" },
                { 0xB8, "PHI" },
                { 0xB9, "PHI" },
                { 0xBA, "PHI" },
                { 0xBB, "PHI" },
                { 0xBC, "PHI" },
                { 0xBD, "PHI" },
                { 0xBE, "PHI" },
                { 0xBF, "PHI" },
                { 0xC0, "LBR" },
                { 0xC1, "LBQ" },
                { 0xC2, "LBZ" },
                { 0xC3, "LBDF" },
                { 0xC4, "NOP" },
                { 0xC5, "LSNQ" },
                { 0xC6, "LSNZ" },
                { 0xC7, "LSNF" },
                { 0xC8, "LSKP" },
                //{ 0xC8, "NLBR" },
                { 0xC9, "LBNQ" },
                { 0xCA, "LBNZ" },
                { 0xCB, "LBNF" },
                { 0xCC, "LSIE" },
                { 0xCD, "LSQ" },
                { 0xCE, "LSZ" },
                { 0xCF, "LSDF" },
                { 0xD0, "SEP" },
                { 0xD1, "SEP" },
                { 0xD2, "SEP" },
                { 0xD3, "SEP" },
                { 0xD4, "SEP" },
                { 0xD5, "SEP" },
                { 0xD6, "SEP" },
                { 0xD7, "SEP" },
                { 0xD8, "SEP" },
                { 0xD9, "SEP" },
                { 0xDA, "SEP" },
                { 0xDB, "SEP" },
                { 0xDC, "SEP" },
                { 0xDD, "SEP" },
                { 0xDE, "SEP" },
                { 0xDF, "SEP" },
                { 0xE0, "SEX" },
                { 0xE1, "SEX" },
                { 0xE2, "SEX" },
                { 0xE3, "SEX" },
                { 0xE4, "SEX" },
                { 0xE5, "SEX" },
                { 0xE6, "SEX" },
                { 0xE7, "SEX" },
                { 0xE8, "SEX" },
                { 0xE9, "SEX" },
                { 0xEA, "SEX" },
                { 0xEB, "SEX" },
                { 0xEC, "SEX" },
                { 0xED, "SEX" },
                { 0xEE, "SEX" },
                { 0xEF, "SEX" },
                { 0xF0, "LDX" },
                { 0xF1, "OR" },
                { 0xF2, "AND" },
                { 0xF3, "XOR" },
                { 0xF4, "ADD" },
                { 0xF5, "SD" },
                { 0xF6, "SHR" },
                { 0xF7, "SM" },
                { 0xF8, "LDI" },
                { 0xF9, "ORI" },
                { 0xFA, "ANI" },
                { 0xFB, "XRI" },
                { 0xFC, "ADI" },
                { 0xFD, "SDI" },
                { 0xFE, "SHL" },
                { 0xFF, "SMI" }
            };

            return d;
        }

        // Lookup an instruction
        // return null if not found
        public static Instruction Lookup(string mnemonic)
        {
            if (s_mnemonicToInstruction.TryGetValue(mnemonic, out Instruction value))
            {
                return value;
            }
            else
            {
                return new Instruction("???", mnemonic, 0x00, OperandType.NONE, "???", 2, 0);
            }
        }

        public static Instruction Lookup(byte opcode)
        {
            string mnemonic = s_byteToMnemonic[opcode];

            if (s_mnemonicToInstruction.TryGetValue(mnemonic, out Instruction value))
            {
                return value;
            }
            else
            {
                return new Instruction("???", mnemonic, opcode, OperandType.NONE, "???", opcode, 0);
            }
        }

        public static string Mnemonic(byte opcode)
        {
            return s_byteToMnemonic[opcode];
        }

        public static void Dump()
        {
            for (byte lo = 0; lo < 16; lo++)
            {
                for (byte hi = 0; hi < 16; hi++)
                {
                    string i = s_byteToMnemonic[(byte)(hi * 16 + lo)];
                    string mne = i ?? "???";
                    Console.Out.Write(mne.PadRight(5));
                }
                Console.Out.WriteLine();
            }
        }
    }
}
