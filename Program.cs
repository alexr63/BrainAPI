using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BrainAPI
{
    class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            if (Clipboard.GetText() == "a")
            {
                Clipboard.SetText("b");
            }
        }
    }
}
