using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BioVM
{
    public partial class Form1 : Form
    {
        private const string Version = "BioVM version 2008.04.12";

        private string[] mnemonic = new[]
        {
            /* 0 */ "halt",
            /* 1 */ "inc r0",
            /* 2 */ "inc r1",
            /* 3 */ "inc r2",
            /* 4 */ "dec r0",
            /* 5 */ "dec r1",
            /* 6 */ "dec r2",
            /* 7 */ "mov r0, [r1]",
            /* 8 */ "mov r0, [r2]",
            /* 9 */ "mov [r1], r0",
            /* a */ "mov [r2], r0",
            /* b */ "while [r1]",
            /* c */ "while true",
            /* d */ "wend",
            /* e */ "rand r2",
            /* f */ "join r2",
        };

        private byte[] Template = new byte[]
        {
            0xc, // while true
            0xb, //   while [r1]
            0x7, //     mov r0, [r1]
            0x2, //     inc r1
            0xa, //     mov [r2], r0
            0x3, //     inc r2
            0xd, //   wend
            0x5, //   dec r1
            0xb, //   while [r1]
            0x5, //     dec r1
            0x6, //     dec r2
            0xd, //   wend
            0x2, //   inc r1
            0xf, //   join r2
            0xe, //   rand r2
            0xd, // wend
            0x0, // halt
        };

        private class Life
        {
            public Life(uint ip, uint r2, int left)
            {
                this.ip = this.r1 = ip;
                this.r2 = r2;
                this.left = left;
            }
            public Guid guid = Guid.NewGuid();
            private uint ip, r1, r2;
            public uint IP
            {
                get { return ip; }
                set
                {
                    ip = value;
                    if (ip >= length) ip &= length - 1;
                }
            }
            public uint R1
            {
                get { return r1; }
                set
                {
                    r1 = value;
                    if (r1 >= length) r1 &= length - 1;
                }
            }
            public uint R2
            {
                get { return r2; }
                set
                {
                    r2 = value;
                    if (r2 >= length) r2 &= length - 1;
                }
            }
            public byte r0;
            public long age;
            public int left;
        }

        private const int length = 512 * 512; // 256KB
        private byte[] data = new byte[length];

        private List<Life> lifes = new List<Life>();
        private int current;
        private Random rand = new Random();

        private uint range;
        private long time, step, born, died, ages, extinct;
        private int miss = 500, mutation = 500, left = 500;
        private long miss_count, mutation_count;
        private float averageAge;
        private DateTime nextshow;

        private void Init()
        {
            data = new byte[length];
            lifes = new List<Life>();
            current = 0;
            time = step = born = died = ages = extinct = 0;
            miss_count = mutation_count = 0;
            averageAge = 0;
            nextshow = DateTime.Now;
            Array.Copy(Template, data, Template.Length);
            lifes.Add(new Life(0, GetRand(), left));
            born++;
            scrollBox1.VScrollBar.Value = 0;
            Redraw();
        }

        public Form1()
        {
            InitializeComponent();
            Init();
            scrollBox1.VScrollBar.Max = data.Length - 1;
        }

        private void binaryBox1_Paint(object sender, PaintEventArgs e)
        {
            Func<uint, string> format = addr =>
            {
                var d = data[addr];
                var mn = mnemonic[d & 0xf];
                return string.Format("{0:X8} | {1:X2} | {2}", addr, d, mn);
            };
            var ad = (uint)scrollBox1.VScrollBar.Value;
            float y = 1;
            var f = scrollBox1.Font;
            var fh = f.GetHeight();
            var pw = scrollBox1.ClientSize.Width;
            var ph = scrollBox1.ClientSize.Height;
            var brush = SystemBrushes.WindowText;
            var target = lifes[current];
            while (ad < data.Length && y < ph)
            {
                if (ad == target.IP)
                    e.Graphics.FillRectangle(Brushes.Red, 0, y - 1, pw, fh);
                var text = format(ad);
                e.Graphics.DrawString(text, f, brush, 0, y);
                y += fh;
                ad++;
            }
            range = ad - (uint)scrollBox1.VScrollBar.Value;

            var x = 200;
            y = 30;
            Action<Font, string> print = (font, s) =>
            {
                if (!string.IsNullOrEmpty(s))
                    e.Graphics.DrawString(s, font, brush, x, y);
                y += fh;
            };

            e.Graphics.FillRectangle(SystemBrushes.Highlight, x - 10, 20, 266, 266);
            e.Graphics.FillRectangle(SystemBrushes.Window, x - 5, 25, 256, 256);
            foreach (var life in lifes)
            {
                var lx = (life.IP & 0x1ff) >> 1;
                var ly = life.IP >> 10;
                e.Graphics.FillRectangle(Brushes.Pink, x - 5 + lx, 25 + ly, 1, 1);
            }
            print(f, string.Format("経過: {0:#,0} ({1:#,0})", time, step));
            print(f, string.Format("絶滅: {0:#,0}", extinct));
            print(f, string.Format("誕生: {0:#,0}", born));
            print(f, string.Format("死亡: {0:#,0}", died));
            print(f, string.Format("失敗: {0:#,0} (1 / {1:#,0})", miss_count, miss));
            print(f, string.Format("変異: {0:#,0} (1 / {1:#,0})", mutation_count, mutation));
            print(f, string.Format("寿命: {0:#,0.0}", averageAge));
            print(f, "");
            var fmt = lifes.Count.ToString().Length;
            print(f, string.Format("個体ID: {0, " + fmt + ":#,0} / {1:#,0}", current + 1, lifes.Count));
            print(Font, target.guid.ToString());
            print(f, "");
            print(f, "ip  = " + format(target.IP));
            print(f, string.Format("r0  = {0:X2}", target.r0));
            print(f, "r1  = " + format(target.R1));
            print(f, "r2  = " + format(target.R2));
            print(f, string.Format("age = {0:#,0} ({1:#,0})", target.age, target.left));
        }

        private void miStep_Click(object sender, EventArgs e)
        {
            Step();
            Redraw();
        }

        private byte Error(byte v)
        {
            var bit = rand.Next(4);
            return (byte)(v ^ (1 << bit));
        }

        private void Step()
        {
            var target = lifes[current];
            target.age++;
            target.left--;
            if (target.left == 0)
            {
                Step(target, "halt");
            }
            else
            {
                var ad = target.IP++;
                var d = data[ad];
                if (Mutated)
                {
                    d = Error(d);
                    data[ad] = d;
                }
                Step(target, mnemonic[d & 0xf]);
            }
            current++;
            step++;
            if (current == lifes.Count)
            {
                time++;
                current = 0;
            }
            if (!easyMode || DateTime.Now >= nextshow)
            {
                Redraw();
                nextshow = DateTime.Now.AddSeconds(1);
            }
        }

        private void Redraw()
        {
            var target2 = lifes[current];
            var vsv = scrollBox1.VScrollBar.Value;
            if (target2.IP < vsv || target2.IP >= vsv + range)
                scrollBox1.VScrollBar.Value = (int)Math.Max(target2.IP - range / 2, 0);
            scrollBox1.Invalidate();
            toolStripStatusLabel1.Text = string.Format(
                "経過: {0:#,0}, 個体数: {1:#,0}, 絶滅: {2:#,0}",
                time, lifes.Count, extinct);
        }

        private uint GetRand()
        {
            return (uint)rand.Next(data.Length);
        }

        private void Step(Life target, string cmd)
        {
            switch (cmd)
            {
                case "halt":
                    ages += target.age;
                    lifes.RemoveAt(current);
                    current--;
                    died++;
                    averageAge = ((float)(ages * 10 / died)) / 10;
                    if (lifes.Count == 0)
                    {
                        var p = (uint)rand.Next(data.Length - Template.Length - 2) + 1;
                        data[p - 1] = 0;
                        Array.Copy(Template, 0, data, p, Template.Length);
                        lifes.Add(new Life(p, GetRand(), left));
                        born++;
                        extinct++;
                    }
                    break;
                case "join r2":
                    {
                        foreach (var l in lifes)
                        {
                            var lip = l.IP;
                            if (l == target) lip--;
                            if (lip == target.R2)
                            {
                                Step(target, "halt");
                                return;
                            }
                        }
                        if (data[target.R2] == 0)
                        {
                            Step(target, "halt");
                            return;
                        }
                        lifes.Add(new Life(target.R2, GetRand(), left));
                        born++;
                        target.left = left;
                        break;
                    }
                case "rand r2":
                    target.R2 = GetRand();
                    break;
                case "while [r1]":
                    if (data[target.R1] == 0)
                    {
                        var start = target.IP;
                        for (var nest = 0; ; )
                        {
                            var d = data[target.IP++];
                            if (target.IP == start || d == 0)
                            {
                                Step(target, "halt");
                                return;
                            }
                            else if (d == 0xb || d == 0xc)
                                nest++;
                            else if (d == 0xd)
                            {
                                if (nest == 0) break;
                                nest--;
                            }
                        }
                    }
                    break;
                case "while true":
                    break;
                case "wend":
                    {
                        var start = target.IP;
                        target.IP--;
                        for (var nest = 0; ; )
                        {
                            target.IP--;
                            var d = data[target.IP];
                            if (target.IP == start || d == 0)
                            {
                                Step(target, "halt");
                                return;
                            }
                            else if (d == 0xd)
                                nest++;
                            else if (d == 0xb || d == 0xc)
                            {
                                if (nest == 0) break;
                                nest--;
                            }
                        }
                        break;
                    }
                case "inc r0":
                    target.r0++;
                    if (Failed) target.r0++;
                    break;
                case "inc r1":
                    target.R1++;
                    if (Failed) target.R1++;
                    break;
                case "inc r2":
                    target.R2++;
                    if (Failed) target.R2++;
                    break;
                case "dec r0":
                    target.r0--;
                    if (Failed) target.r0--;
                    break;
                case "dec r1":
                    target.R1--;
                    if (Failed) target.R1--;
                    break;
                case "dec r2":
                    target.R2--;
                    if (Failed) target.R2--;
                    break;
                case "mov r0, [r1]":
                    target.r0 = data[target.R1];
                    if (Failed)
                        target.r0 = Error((byte)target.r0);
                    break;
                case "mov r0, [r2]":
                    target.r0 = data[target.R2];
                    if (Failed)
                        target.r0 = Error((byte)target.r0);
                    break;
                case "mov [r1], r0":
                    {
                        var r0 = (byte)target.r0;
                        if (Failed) r0 = Error(r0);
                        data[target.R1] = r0;
                        break;
                    }
                case "mov [r2], r0":
                    {
                        var r0 = (byte)target.r0;
                        if (Failed) r0 = Error(r0);
                        data[target.R2] = r0;
                        break;
                    }
            }
        }

        private bool running = false;

        private void miRun_Click(object sender, EventArgs e)
        {
            if (running)
            {
                running = false;
                return;
            }

            miRun.Text = "中断";
            miStep.Enabled = false;
            var c = scrollBox1.BackColor;
            scrollBox1.BackColor = SystemColors.Window;

            running = true;
            while (Visible && running)
            {
                Step();
                Application.DoEvents();
            }

            miRun.Text = "実行";
            miStep.Enabled = true;
            scrollBox1.BackColor = c;
            Redraw();
        }

        private bool Failed
        {
            get
            {
                if (rand.Next(miss) == 0)
                {
                    miss_count++;
                    return true;
                }
                return false;
            }
        }

        private bool Mutated
        {
            get
            {
                if (rand.Next(mutation) == 0)
                {
                    mutation_count++;
                    return true;
                }
                return false;
            }
        }

        private bool easyMode = true;

        private void miVisible_Click(object sender, EventArgs e)
        {
            if (easyMode)
            {
                easyMode = false;
                miVisible.Text = "簡略";
            }
            else
            {
                easyMode = true;
                miVisible.Text = "詳細";
            }
        }

        private void fileNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            running = false;
            Init();
            scrollBox1.Invalidate();
        }

        private void fileExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            running = false;
            Close();
        }

        private void toolOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionDialog dlg = new OptionDialog();
            dlg.numericUpDown1.Value = miss;
            dlg.numericUpDown2.Value = mutation;
            dlg.numericUpDown3.Value = left;
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            miss = (int)dlg.numericUpDown1.Value;
            mutation = (int)dlg.numericUpDown2.Value;
            left = (int)dlg.numericUpDown3.Value;
            scrollBox1.Invalidate();
        }

        private void helpAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, Version, "バージョン情報");
        }
    }
}
