using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    internal class Decoder
    {
        public Decoder() { }
        public event Action<double> progresCompleted;
        public Task<byte[]> decode(CoderOutput coderOutput)
        {
            return new Task<byte[]>(() =>
                {
                    int allSize = coderOutput.output.Length;
                    //Создаем дерево для расшифровки
                    node head = node.makeTree(coderOutput.countBytes);
                    int leftBits = -1;
                    int index = 0;
                    byte curByte = 0;
                    List<byte> result = new List<byte>();
                    node cur = head;
                    for(int i = 0;i < coderOutput.lengthOriginal;)
                    {
                        //Если текущий байт полностью использован берем следующий
                        if (leftBits == -1)
                        {
                            //Если байты закончились, то заканчиваем
                            if (index == coderOutput.output.Length)
                                break;
                            curByte = coderOutput.output[index++];
                            if (allSize > 100 && index % (allSize / 100) < (index - 1) % (allSize / 100))
                                progresCompleted?.Invoke((((double)index) / allSize) * 100.0);
                            leftBits = 7;
                        }
                        //Если очередной бит 1, то идем вправо, иначе влево
                        if ((curByte & (1 << (leftBits--))) != 0)
                        {
                            //Если справа ничего нет, то в текущем элементе лежит нужный символ
                            if (cur.right == null)
                            {
                                if (cur.myByte == null)
                                    throw new Exception("cur.myByte = null");
                                result.Add(cur.myByte ?? 1);
                                i++;
                                cur = head;
                            }
                            cur = cur.right;
                        }
                        else
                        {
                            //Если слева ничего нет, то в текущем элементе лежит нужный символ
                            if (cur.left == null)
                            {
                                if (cur.myByte == null)
                                    throw new Exception("cur.myByte = null");
                                result.Add(cur.myByte ?? 1);
                                i++;
                                cur = head;
                            }
                            cur = cur.left;
                        }
                    }

                    return result.ToArray();
                });
        }

        //Элемент дерева для расшифровки
        class node
        {
            public byte? myByte;
            public node? left, right;
            private node(byte? myByte, node? left, node? right)
            {
                this.left = left;
                this.right = right;
                this.myByte = myByte;
            }
            public node()
            {
                this.right = null;
                this.left = null;
                this.myByte = null;
            }
            //Метод для создания дерева кодирования по кодам символов
            public static node makeTree(KeyValuePair<byte, int>[] countBytes)
            {
                List<int> prefix = new List<int>(countBytes.Length + 1);
                prefix.Add(0);
                for (int i = 0; i < countBytes.Length; i++)
                    prefix.Add(prefix[i] + countBytes[i].Value);
                return codeCharacters(countBytes, prefix, 0, countBytes.Length);
            }
            private static node codeCharacters(KeyValuePair<byte, int>[] countBytes, List<int> prefix, int l, int r)
            {
                //Если в диапазоне остался 1 элемент, то можно возращаться из метода
                if (r - l == 1)
                    return new node(countBytes[l].Key, null, null);
                int lb = l, rb = r;
                while (lb + 1 < rb)
                {
                    int m = (lb + rb) / 2;
                    if (prefix[m + 1] - prefix[l] <= prefix[r] - prefix[m + 1])
                        lb = m;
                    else
                        rb = m;
                }
                node cur = new node(null, null, null);
                //Кодируем дальше левый диапазон
                cur.left = codeCharacters(countBytes, prefix, l, lb + 1);
                //Кодируем дальше правый диапазон
                cur.right = codeCharacters(countBytes, prefix, lb + 1, r);
                return cur;
            }
            //Метод для дополнения дерева по коду символа
            private static void makeBranch(KeyValuePair<byte, bool[]> code, node head, int deep)
            {
                //Если дошли до конца кода символа, то записываем в текущий элемент символ
                if (deep == code.Value.Length)
                {
                    head.myByte = code.Key;
                    return;
                }
                //Если очередной код true, то надо идти вправо, иначе влево по дереву
                if (code.Value[deep])
                {
                    //Если правого элемента нет, создаем
                    if (head.right == null)
                        head.right = new node();
                    makeBranch(code, head.right, deep + 1);
                }
                else
                {
                    //Если левого элемента нет, создаем
                    if (head.left == null)
                        head.left = new node();
                    makeBranch(code, head.left, deep + 1);
                }

            }

        }
    }
}
