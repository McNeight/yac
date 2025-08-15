# yac
Yet Another COSMAC (Assembler|Disassembler|Simulator)

## Assembler
yac.a: CDP1802 Level I Assembler

Based on [Asm1802 by Daniel Quadros](https://github.com/dquadros/Asm1802)

Assembles code for the RCA CDP1802 microprocessor written
in Level I Assembly Language as defined in [MPM-216 Operator 
Manual for the RCA COSMAC Development System II](https://archive.org/details/bitsavers_rcacosmacMentSystemIIOperatingManualOct1977_13596670).

UT20.asm in the asm directory is the Monitor Software for
the COSMAC Development System II, as listed in RCA's
Operator Manual. To fully test the assembler, I tried to
type it exactly as listed. For now, the load routine is
not included.

Known issues:
* Does not truncate symbol in datalist to 8 bit
* Error messages not very helpful

## Disassembler
yac.d: CDP1802 Disassembler

Based on [Disasm1802 by Daniel Quadros](https://github.com/dquadros/Disasm1802)

This is a very crude disassembler for the RCA CDP1802 microprocessor.

It accepts as input an Intel Hex file (checksums are ignored and may be
omitted). An optional .def file can specify what areas should be treated
as code or data (by default all bytes are treated as code) and label for
addresses. More details (for now) are in the code.

The main objective is to disassemble binary code from old examples. Testing
was done with the Monitor PROM for the
[Netronics ELF II with a Giant Board](https://en.wikipedia.org/wiki/ELF_II)
as listed on [page 64 of the March 1978 issue of Popular Electronics](https://archive.org/details/197803PopularElectronics/page/62/mode/1up).

## Simulator
yac.sim: CDP1802 Simulator

Based on [cdp1802-simulator by Rasmus Svensson](https://github.com/raek/cdp1802-simulator)

The CDP1802 microprocessor was the basis of many early hobbyist
microcomputers, including the COSMAC ELF and the "EFA4" computer
described in the Swedish book "Elektronik för alla, del 4". More
recently in 2010, a new hobbyist microcomputer kit called [The 1802
Membership Card](https://www.sunrise-ev.com/1802.htm) was introduced by Lee Hart.

This repository contains a simulator for the CDP1802 microprocessor
written in ~~Java~~ (translated to C# with
[Paul Irwin's JavaToCSharp utility](https://github.com/paulirwin/JavaToCSharp)).
So far it is not complete, but merely a hobby project.

The code can be compiled using ~~the included Eclipse project~~ Visual Studio
 2022. The Startup Project should be set to yac.sim.
