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
        //Ключи символов, для расшифровки
        public KeyValuePair<char, bool[]>[] encodedCharacters;
    }
}
