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
            //Засекаем время
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Coder coder = new Coder();
            coder.progresCompleted += showProgress;
            //Считываем входные данные
            byte[] input = File.ReadAllBytes(openFileDialog1.FileName);
            //Кодируем данные
            Task<CoderOutput> res = coder.codе(input, (int)numericUpDown1.Value);
            res.Start();
            await res;
            CoderOutput result = res.Result;
            result.extension = Path.GetExtension(openFileDialog1.FileName);
            //Записываем в файл результат
            BinaryFormatter formatter = new BinaryFormatter();
            long sizeResultFile;
            using (FileStream fs = new FileStream(Path.ChangeExtension(openFileDialog1.FileName, "dat"), FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, result);
                sizeResultFile = fs.Length;
            }
            //Выводим результат
            sw.Stop();
            double originalSize = new System.IO.FileInfo(openFileDialog1.FileName).Length;
            double newSize = new System.IO.FileInfo(Path.ChangeExtension(openFileDialog1.FileName, "dat")).Length;
            label5.Text = "Степень сжатия: " + ((originalSize - newSize) / originalSize * 100.0) + "%";
            label6.Text = "Время выполнения: " + sw.Elapsed;
            label13.Text = result.output.Length.ToString();
            label14.Text = sizeResultFile.ToString();
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
            //Засекаем время
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Decoder decoder = new Decoder();
            decoder.progresCompleted += showProgressDecode;
            //Считываем входные данные
            BinaryFormatter formatter = new BinaryFormatter();
            CoderOutput input = (CoderOutput)formatter.Deserialize(openFileDialog2.OpenFile());
            showFileName(input.extension);
            //Декодируем данные
            Task<byte[]> task = decoder.decode(input);
            task.Start();
            await task;
            byte[] output = task.Result;
            //Записываем в файл результат
            string path = openFileDialog2.FileName;
            path = Path.ChangeExtension(path, input.extension);
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                fs.Write(output, 0, output.Length);
            }
            //Выводим результат
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

        private void showFileName(String extension)
        {
            label7.Text = "Входной файл: " + openFileDialog2.FileName;
            label8.Text = "Выходной файл: " + Path.ChangeExtension(openFileDialog2.FileName, extension);
        }
    }
}