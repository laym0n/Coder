using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    //Класс, представляет собой кодер
    internal class Coder
    {
        public Coder()
        {

        }
        //Событие, для оповещения о прогрессе кодирования
        public event Action<double> progresCompleted;
        public Task<CoderOutput> codе(byte[] input, int lengthCodeInBits)
        {
            return new Task<CoderOutput>(() =>
            {
                //Подсчитывается частота, каждого байта
                Dictionary<byte, int> characterFrequency = new Dictionary<byte, int>();
                input.ToList().ForEach(c => 
                {
                    if (!characterFrequency.ContainsKey(c))
                        characterFrequency.Add(c, 0);
                    characterFrequency[c]++;
                });
                //Байты сортируются по увеличению частоты
                List<KeyValuePair<byte, int>> sortedCharacterFrequency = characterFrequency.ToList().OrderBy(j => j.Value).ToList();
                //Составляется массив сум префиксов
                List<int> prefix = new List<int>(characterFrequency.Count + 1);
                prefix.Add(0);
                for (int i = 0; i < characterFrequency.Count; i++)
                    prefix.Add(prefix[i] + sortedCharacterFrequency[i].Value);
                //Словарь закодированных байтов
                Dictionary<byte, List<bool>> encodedSymbols = new Dictionary<byte, List<bool>>();
                sortedCharacterFrequency.ForEach(c => { encodedSymbols.Add(c.Key, new List<bool>()); });
                //Кодируем символы
                if (sortedCharacterFrequency.Count == 1)
                    encodedSymbols[sortedCharacterFrequency.First().Key].Add(true);
                else
                    codeCharacters(sortedCharacterFrequency, prefix, 0, sortedCharacterFrequency.Count, 0, lengthCodeInBits, encodedSymbols);
                //Создаем объект CoderOutput для возврата из метода
                CoderOutput result = new CoderOutput()
                {
                    countBytes = sortedCharacterFrequency.ToArray(),
                    lengthOriginal = input.Length
                };
                int allSize = input.Length;
                int num = 1;
                //Закодированные байты
                List<byte> bytes = new List<byte>();
                int bits_left = -1;
                //Кодируем каждый байт в тексте
                foreach (byte b in input)
                {
                    encodedSymbols[b].ForEach(bit =>
                    {
                        //Проверка на то, закончилось ли место в текущем байте
                        //Если да, то создаем новый
                        if (bits_left == -1)
                        {
                            bits_left = 7;
                            bytes.Add(0);
                        }
                        //Кодируем бит в байте в соответствии с кодом изначального байта
                        bytes[bytes.Count - 1] |= (byte)((bit ? 1 : 0) << (bits_left--));
                    });
                    num++;
                    if (allSize > 100 && num % (allSize / 100) < (num - 1) % (allSize / 100))
                        progresCompleted?.Invoke((((double)num) / allSize) * 100.0);
                }
                progresCompleted?.Invoke(100.0);
                //Возвращаем результат
                result.output = bytes.ToArray();
                return result;
            });
        }
        //Метод для кодирования символов
        private void codeCharacters(List<KeyValuePair<byte, int>> characterFrequency, List<int> prefix, int l, int r, int deep, int lengthCodeInBits, Dictionary<byte, List<bool>> encodedSymbols)
        {
            //Если в диапазоне остался 1 элемент, то можно возращаться из метода
            if (r - l == 1)
                return;
            if (deep >= lengthCodeInBits)
                throw new Exception("Колличества бит не хватает закодировать эту информацию");
            //Бин поиск индекса разделения входного диапазона по сумме в диапазонах
            int lb = l, rb = r;
            while(lb + 1 < rb)
            {
                int m = (lb + rb) / 2;
                if (prefix[m + 1] - prefix[l] <= prefix[r] - prefix[m + 1])
                    lb = m;
                else
                    rb = m;
            }
            //Добавляем 0 в дереве кодирования всем элементам в левом диапазоне
            for (int i = l; i <= lb; i++)
                encodedSymbols[characterFrequency[i].Key].Add(false);
            //Добавляем 1 в дереве кодирования всем элементам в правом диапазоне
            for (int i = lb + 1; i < r; i++)
                encodedSymbols[characterFrequency[i].Key].Add(true);
            //Кодируем дальше левый диапазон
            codeCharacters(characterFrequency, prefix, l, lb + 1, deep + 1, lengthCodeInBits, encodedSymbols);
            //Кодируем дальше правый диапазон
            codeCharacters(characterFrequency, prefix, lb + 1, r, deep + 1, lengthCodeInBits, encodedSymbols);
        }

    }
}
