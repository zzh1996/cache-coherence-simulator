using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CacheCoherenceSimulator
{
    public partial class Form1 : Form
    {
        private Processor[] proc;
        public Form1()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private string StateToStr(State state)
        {
            switch (state)
            {
                case State.Invalid:
                    return "无效";
                case State.Shared:
                    return "共享";
                case State.Own:
                    return "独占";
            }
            return "";
        }

        private Color StateToColor(State state)
        {
            switch (state)
            {
                case State.Invalid:
                    return Color.Gray;
                case State.Shared:
                    return Color.DarkGreen;
                case State.Own:
                    return Color.OrangeRed;
            }
            return Color.White;
        }

        private void DrawProc(Processor p, DataGridView v, DataGridView v2)
        {
            v.Rows.Clear();
            for (int i = 0; i < 4; i++)
            {
                v.Rows.Add(i, StateToStr(p.states[i]), p.states[i] > 0 ? p.addrs[i].ToString() : "");
                v.Rows[i].Cells[1].Style.BackColor = StateToColor(p.states[i]);
            }
            v2.Rows.Clear();
            for (int i = 0; i < 8; i++)
            {
                v2.Rows.Add(p.no * 8 + i, p.dirs[i].ToString());
            }
        }

        private void Draw()
        {
            DrawProc(proc[0], dataGridView1, dataGridView2);
            DrawProc(proc[1], dataGridView4, dataGridView3);
            DrawProc(proc[2], dataGridView6, dataGridView5);
            DrawProc(proc[3], dataGridView8, dataGridView7);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
            comboBox6.SelectedIndex = 0;
            Reset();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            proc[0].Run((int)numericUpDown1.Value, (Oper)comboBox2.SelectedIndex);
            Draw();
        }

        private void Reset()
        {
            proc = new Processor[4];
            for (int i = 0; i < 4; i++)
            {
                proc[i] = new Processor(comboBox3.SelectedIndex == 1, (Assoc)comboBox1.SelectedIndex, proc, textBox1, i);
            }
            Draw();
            textBox1.Text = "历史记录:" + Environment.NewLine;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Reset();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            Reset();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            proc[1].Run((int)numericUpDown2.Value, (Oper)comboBox4.SelectedIndex);
            Draw();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            proc[2].Run((int)numericUpDown3.Value, (Oper)comboBox5.SelectedIndex);
            Draw();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            proc[3].Run((int)numericUpDown4.Value, (Oper)comboBox6.SelectedIndex);
            Draw();
        }
    }
}
