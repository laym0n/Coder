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
                    node head = node.makeTree(coderOutput.encodedCharacters);
                    int leftBits = -1;
                    int index = 0;
                    byte curByte = 0;
                    StringBuilder result = new StringBuilder();
                    node cur = head;
                    while (true)
                    {
                        if (leftBits == -1)
                        {
                            if (index == coderOutput.output.Length)
                                break;
                            curByte = coderOutput.output[index++];
                            if (allSize > 100 && index % (allSize / 100) < (index - 1) % (allSize / 100))
                                progresCompleted?.Invoke((((double)index) / allSize) * 100.0);
                            leftBits = 7;
                        }
                        if ((curByte & (1 << (leftBits--))) != 0)
                        {
                            if (cur.right == null)
                            {
                                result.Append(cur.myChar);
                                cur = head;
                            }
                            cur = cur.right;
                        }
                        else
                        {
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
            private static void makeBranch(KeyValuePair<char, bool[]> code, node head, int deep)
            {
                if(deep == code.Value.Length)
                {
                    head.myChar = code.Key;
                    return;
                }
                if (code.Value[deep])
                {
                    if (head.right == null)
                        head.right = new node();
                    makeBranch(code, head.right, deep + 1);
                }
                else
                {
                    if (head.left == null)
                        head.left = new node();
                    makeBranch(code, head.left, deep + 1);
                }

            }
            public static node makeTree(KeyValuePair<char, bool[]>[] encodedCharacters)
            {
                node head = new node();
                foreach(var code in encodedCharacters)
                {
                    makeBranch(code, head, 0);
                }
                return head;
            }

        }
    }
}
