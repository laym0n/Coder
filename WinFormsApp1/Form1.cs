using System.Runtime.Serialization.Formatters.Binary;
using System;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Coder coder = new Coder();
            coder.progresCompleted += showProgress;
            string[] input = File.ReadAllLines(openFileDialog1.FileName);
            Task<CoderOutput> res = coder.codе(input, (int)numericUpDown1.Value);
            res.Start();
            await res;
            CoderOutput result = res.Result;
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(Path.ChangeExtension(openFileDialog1.FileName, "dat"), FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, result);
            }
            sw.Stop();
            double originalSize = new System.IO.FileInfo(openFileDialog1.FileName).Length;
            double newSize = new System.IO.FileInfo(Path.ChangeExtension(openFileDialog1.FileName, "dat")).Length;
            label5.Text = "Степень сжатия: " + ((originalSize - newSize) / originalSize * 100.0) + "%";
            label6.Text = "Время выполнения: " + sw.Elapsed;
            coder.progresCompleted -= showProgress;
        }
        private void showProgress(double progress)
        {
            label3.Text = "Сжатие выполнено на " + progress.ToString() + "%";
        }
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog(this);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Decoder decoder = new Decoder();
            decoder.progresCompleted += showProgressDecode;
            BinaryFormatter formatter = new BinaryFormatter();
            CoderOutput input = (CoderOutput)formatter.Deserialize(openFileDialog2.OpenFile());
            Task<string> task = decoder.decode(input);
            task.Start();
            await task;
            string output = task.Result;
            string path = openFileDialog2.FileName;
            path = Path.ChangeExtension(path, "txt");
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                byte[] buffer = Encoding.Default.GetBytes(output);
                fs.Write(buffer, 0, buffer.Length);
            }
            sw.Stop();
            label10.Text = "Время выполнения: " + sw.Elapsed;
            decoder.progresCompleted -= showProgressDecode;
        }
        private void showProgressDecode(double progress)
        {
            label9.Text = "Декодирование выполнено на " + progress.ToString() + "%";
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            label1.Text = "Входной файл: " + openFileDialog1.FileName;
            label2.Text = "Выходной файл: " + Path.ChangeExtension(openFileDialog1.FileName, "dat");
        }

        private void openFileDialog2_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            label7.Text = "Входной файл: " + openFileDialog2.FileName;
            label8.Text = "Выходной файл: " + Path.ChangeExtension(openFileDialog2.FileName, "txt");
        }
    }
}