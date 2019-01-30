using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Forms;

namespace Traffic_generator_WFA
{
    static class Program
    {
        public static Initializer init;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            init = new Initializer();
            init.tc = new TransactionController();
            init.mw = new MainWindow();
            Application.Run(init.mw);
        }
    }
}
