// <copyright file="Tiny.cs" company="yac Contributors">
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
using System.Drawing;
using System.Windows.Forms;

using yac.sim.cpu;
using yac.sim.io;
using yac.sim.mem;

namespace yac.sim.gui
{
    public partial class Tiny : Form
    {
        private const string TestProgram = "EDF801BCBDF800ACADF8D05DDD2DF85A7360F800F0F800722DF8A55CF8000CF8004C2CC43000";

        private Cdp1802.State s;
        private IMemory m;
        private SimpleIO io;
        private Cdp1802 cpu;

        private Label[] registerLabel;
        private Label[] outputLabels;
        private TextBox[] inputFields;
        private Panel[] inputPanels;
        private Label dLabel;
        private Label qLabel;
        private Label idleLabel;
        private Label pLabel;
        private Label xLabel;
        private Dictionary<Label, string> currentLabelTexts;

        public Tiny()
        {
            InitializeComponent();
        }

        public void LayoutLabels()
        {
            registerLabel = new Label[16];
            for (int i = 0; i < registerLabel.Length; i++)
            {
                registerLabel[i] = new Label()
                {
                    Dock = DockStyle.Fill,
                    Text = "R" + i.ToString(),
                };

                tlp1.Controls.Add(registerLabel[i], 0, i);
            }

            outputLabels = new Label[7];
            for (int i = 0; i < outputLabels.Length; i++)
            {
                outputLabels[i] = new Label()
                {
                    Dock = DockStyle.Fill,
                    Text = "O" + i.ToString(),
                };

                tlp2.Controls.Add(outputLabels[i], 0, i);
            }

            dLabel = new Label();
            tlp2.Controls.Add(dLabel, 0, 7);

            qLabel = new Label();
            tlp2.Controls.Add(qLabel, 0, 8);

            idleLabel = new Label();
            tlp2.Controls.Add(idleLabel, 0, 9);

            inputFields = new TextBox[7];
            inputPanels = new Panel[7];
            for (int i = 0; i < inputFields.Length; i++)
            {
                inputFields[i] = new TextBox()
                {
                    Dock = DockStyle.Fill,
                    Text = "I" + i.ToString(),
                };

                inputPanels[i] = new Panel()
                {
                    Dock = DockStyle.Fill,
                };

                inputPanels[i].Controls.Add(new Label() { Text = $"INP{i + 1}=" });
                inputPanels[i].Controls.Add(inputFields[i]);

                tlp3.Controls.Add(inputPanels[i], 0, i);
            }

            pLabel = new Label();
            tlp3.Controls.Add(pLabel, 0, 7);

            xLabel = new Label();
            tlp3.Controls.Add(xLabel, 0, 8);

            currentLabelTexts = new Dictionary<Label, string>();

            UpdateGui();
        }

        private void SetLabelText(Label label, string newText)
        {
            if (label is null)
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (string.IsNullOrEmpty(newText))
            {
                throw new ArgumentException($"'{nameof(newText)}' cannot be null or empty.", nameof(newText));
            }

            currentLabelTexts.TryGetValue(label, out string oldText);
            if (oldText == null || oldText.Equals(newText))
            {
                label.ForeColor = Color.Black;
            }
            else
            {
                label.ForeColor = Color.Red;
            }

            label.Text = newText;
            currentLabelTexts[label] = newText;
        }

        private void UpdateGui()
        {
            for (int i = 0; i < 16; i++)
            {
                SetLabelText(registerLabel[i], $"R({i})={s.r[i]:X4}");
            }

            for (int i = 0; i < 7; i++)
            {
                SetLabelText(outputLabels[i], $"OUT{i + 1:X1}={io.outputValues[i]:X2}");
            }

            SetLabelText(dLabel, $"D={s.d:X2}");
            SetLabelText(qLabel, s.q ? "Q=1" : "Q=0");
            SetLabelText(idleLabel, s.idle ? "idle" : "run");
            SetLabelText(pLabel, $"P={s.p:X2}");
            SetLabelText(xLabel, $"X={s.x:X2}");
        }

        private void TinyLoad(object sender, System.EventArgs e)
        {
            s = new Cdp1802.State();
            {
                IMemory rom = new ArrayMemory(0x100);
                util.Utils.LoadHex(rom, TestProgram);
                IMemory ram = new ArrayMemory(0x100);
                MemoryMapper mm = new MemoryMapper();
                mm.Map(0x0000, 0x100, new MemoryWriteProtector(rom));
                mm.Map(0x0100, 0x100, ram);
                m = mm;
            }

            io = new SimpleIO();
            cpu = new Cdp1802(s, m, io);

            LayoutLabels();
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            cpu.Tick();

            UpdateGui();
        }
    }
}
