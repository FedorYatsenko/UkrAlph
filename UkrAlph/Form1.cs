using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UkrAlph
{
    public partial class Form1 : Form
    {
        const int rowCount = 22;
        const int colCount = 50;
        const int colSize = 30;

        class TextStatistics
        {
            public Dictionary<char, double> letters = new Dictionary<char, double>();
            public Dictionary<string, double> bigrams = new Dictionary<string, double>();
            public Dictionary<string, double> trigrams = new Dictionary<string, double>();
            private string prev  = "";
            private string pprev = "";
            public int lettersCount = 0;
            public int bigramsCount = 0;
            public int trigramsCount = 0;

            public void addChar(char ch)
            {
                lettersCount++;
                if (letters.Keys.Contains(ch))
                {
                    letters[ch]++;
                }
                else
                {
                    letters.Add(ch, 1);
                }

                if (prev != "")
                {
                    bigramsCount++;

                    if (bigrams.Keys.Contains(prev + ch))
                        bigrams[prev + ch]++;
                    else
                        bigrams.Add(prev + ch, 1);
                }

                if (pprev != "")
                {
                    trigramsCount++;

                    if (trigrams.Keys.Contains(pprev + prev + ch))
                        trigrams[pprev + prev + ch]++;
                    else
                        trigrams.Add(pprev + prev + ch, 1);
                }

                pprev = prev;
                prev = ch.ToString();
            }

            public void setNewLine()
            {
                prev = "";
                pprev = "";

            }
        }

        private TextStatistics textStatistics = null;

        private void getTextStatistics(string filename)
        {
            textStatistics = new TextStatistics();

            using (StreamReader sr = new StreamReader(filename, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (char c in line)
                    {
                        textStatistics.addChar(c);
                    }

                    textStatistics.setNewLine();
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text File|*.txt";
            openFileDialog1.Title = "Select text File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName.Text = openFileDialog1.FileName;
                getTextStatistics(fileName.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string file = fileName.Text;

            if (File.Exists(file))
            {
                if (radioButton1.Checked) ShowLetters();
                else
                    if (radioButton2.Checked) ShowBigrams();
                else
                    ShowTrigrams();
            }
            else
            {
                textStatistics = null;
                MessageBox.Show("Файл не існує");
            }
        }

        private void ShowLetters()
        {
            diagram1.Visible = true;
            dataGridView1.Visible = false;
            diagram1.Series["Series1"].Points.Clear();

            if (radioButton6.Checked)
            {
                List<char> list = textStatistics.letters.Keys.ToList();
                list.Sort();

                foreach (char ch in list)
                    diagram1.Series["Series1"].Points.AddXY(ch.ToString(), textStatistics.letters[ch]);
            }
            else
            {
                foreach (var el in textStatistics.letters.OrderBy(i => -i.Value))
                    diagram1.Series["Series1"].Points.AddXY(el.Key.ToString(), el.Value);
            }

            label1.Text = "";
            foreach (var el in textStatistics.letters.OrderBy(i => -i.Value))
                label1.Text += el.Key;
        }

        private void ShowBigrams()
        {
            if (radioButton7.Checked)
            {
                diagram1.Visible = true;
                dataGridView1.Visible = false;

                diagram1.Series["Series1"].Points.Clear();
                foreach (var el in textStatistics.bigrams.OrderBy(i => -i.Value).Take(30))
                    diagram1.Series["Series1"].Points.AddXY("\"" + el.Key + "\"", el.Value);
            }
            else
            {
                diagram1.Visible = false;
                dataGridView1.Visible = true;
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();

                var bigrams = textStatistics.bigrams.OrderBy(i => -i.Value);
                double count = textStatistics.bigramsCount;

                dataGridView1.RowCount = rowCount;
                dataGridView1.ColumnCount = 2 * (int)Math.Ceiling((double)bigrams.Count() / rowCount);

                for (int i = 0; i < bigrams.Count(); i++)
                {
                    dataGridView1.Rows[i % rowCount].Cells[2 * (i / rowCount)].Value = bigrams.ElementAt(i).Key;
                    if(radioButton11.Checked)
                        dataGridView1.Rows[i % rowCount].Cells[2 * (i / rowCount) + 1].Value = bigrams.ElementAt(i).Value;
                    else
                        dataGridView1.Rows[i % rowCount].Cells[2 * (i / rowCount) + 1].Value = bigrams.ElementAt(i).Value / count;
                }

                foreach(DataGridViewColumn column in dataGridView1.Columns)
                    if (column.Index % 2 == 0)
                        column.Width = colSize;
                    else
                        column.Width = 2 * colSize;
            }

            label1.Text = "";
            foreach (var el in textStatistics.bigrams.OrderBy(i => -i.Value).Take(30))
                label1.Text += "\"" + el.Key + "\" ";
        }

        private void ShowTrigrams()
        {
            if (radioButton9.Checked)
            {
                diagram1.Visible = true;
                dataGridView1.Visible = false;

                diagram1.Series["Series1"].Points.Clear();
                foreach (var el in textStatistics.trigrams.OrderBy(i => -i.Value).Take(30))
                    diagram1.Series["Series1"].Points.AddXY("\"" + el.Key + "\"", el.Value);
            }
            else
            {
                diagram1.Visible = false;
                dataGridView1.Visible = true;
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();

                var trigrams = textStatistics.trigrams.OrderBy(i => -i.Value);
                double count = textStatistics.trigramsCount;

                dataGridView1.RowCount = rowCount;

                if (2 * trigrams.Count() > rowCount * colCount)
                    dataGridView1.ColumnCount = colCount;
                else
                    dataGridView1.ColumnCount = 2 * (int)Math.Ceiling((double)trigrams.Count() / rowCount);

                for (int i = 0; i < trigrams.Count() && (2 * i < rowCount * colCount); i++)
                {
                    dataGridView1.Rows[i % rowCount].Cells[2 * (i / rowCount)].Value = trigrams.ElementAt(i).Key;

                    if (radioButton11.Checked)
                        dataGridView1.Rows[i % rowCount].Cells[2 * (i / rowCount) + 1].Value = trigrams.ElementAt(i).Value;
                    else
                        dataGridView1.Rows[i % rowCount].Cells[2 * (i / rowCount) + 1].Value = trigrams.ElementAt(i).Value / count;
                }

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                    if(column.Index % 2 == 0)
                        column.Width = colSize;
                    else
                        column.Width = 2 * colSize;
            }

            label1.Text = "";
            int row = 0;
            foreach (var el in textStatistics.trigrams.OrderBy(i => -i.Value).Take(30))
            {
                label1.Text += "\"" + el.Key + "\" ";

                if (row == 14)
                {
                    label1.Text += "\n";
                    row = 0;
                }
                else
                    row++;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            diagram1.ChartAreas[0].AxisX.Interval = 1;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = true;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;
            ShowLetters();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = true;
            groupBox4.Enabled = false;
            ShowBigrams();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = true;
            ShowTrigrams();
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            ShowLetters();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            ShowLetters();
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            ShowBigrams();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ShowBigrams();
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            ShowTrigrams();
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            ShowTrigrams();
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }
    }
}
