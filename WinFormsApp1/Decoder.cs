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
        public Task<string> decode(CoderOutput coderOutput)
        {
            return new Task<string>(() =>
                {
                    int allSize = coderOutput.output.Length;
                    //Создаем дерево для расшифровки
                    node head = node.makeTree(coderOutput.encodedCharacters);
                    int leftBits = -1;
                    int index = 0;
                    byte curByte = 0;
                    StringBuilder result = new StringBuilder();
                    node cur = head;
                    while (true)
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
                                result.Append(cur.myChar);
                                cur = head;
                            }
                            cur = cur.right;
                        }
                        else
                        {
                            //Если слева ничего нет, то в текущем элементе лежит нужный символ
                            if (cur.left == null)
                            {
                                result.Append(cur.myChar);
                                cur = head;
                            }
                            cur = cur.left;
                        }
                    }

                    return result.ToString();
                });
        }

        //Элемент дерева для расшифровки
        class node
        {
            public char? myChar;
            public node? left, right;
            private node(char? myChar, node? left, node? right)
            {
                this.left = left;
                this.right = right;
                this.myChar = myChar;
            }
            public node()
            {
                this.right = null;
                this.left = null;
                this.myChar = null;
            }
            //Метод для создания дерева кодирования по кодам символов
            public static node makeTree(KeyValuePair<char, bool[]>[] encodedCharacters)
            {
                node head = new node();
                foreach(var code in encodedCharacters)
                    makeBranch(code, head, 0);
                return head;
            }
            //Метод для дополнения дерева по коду символа
            private static void makeBranch(KeyValuePair<char, bool[]> code, node head, int deep)
            {
                //Если дошли до конца кода символа, то записываем в текущий элемент символ
                if (deep == code.Value.Length)
                {
                    head.myChar = code.Key;
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
