using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    [Serializable]
    internal class CoderOutput
    {
        public byte[] output;
        public KeyValuePair<char, bool[]>[] encodedCharacters;
    }
}
