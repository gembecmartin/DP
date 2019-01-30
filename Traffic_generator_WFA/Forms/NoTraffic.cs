using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Traffic_generator_WFA.Forms
{
    public partial class NoTraffic : System.Windows.Forms.UserControl
    {
        public NoTraffic()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            var generatorForm = new GeneratorCreationDialogue();
            generatorForm.Show(this);
        }
    }
}
