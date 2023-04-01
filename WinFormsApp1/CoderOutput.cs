using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    //Класс представляет собой результат работы кодера
    //Объект этого класса, серилизуется в итогом файле
    [Serializable]
    internal class CoderOutput
    {
        //Закодированный массив байт
        public byte[] output;
        //Расширение оригинального файла
        public string extension;
        //Количество байт оригинального байта
        public int lengthOriginal;
        //Массив пар типа (байт; его частота)
        public KeyValuePair<byte, int>[] countBytes;
    }
}