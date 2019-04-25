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
using Nethereum.Web3.Accounts;

namespace Traffic_generator_WFA.Forms
{
    public partial class AccountItem : DevExpress.XtraEditors.XtraUserControl
    {
        public string AccountName { get; set; }
        public string Balance { get; set; }
        public AccountItem(string address)
        {
            InitializeComponent();
            
        }

        public AccountItem(Account account)
        {
            InitializeComponent();
            //AccountName = account.
        }

        private void editFormUserControl2_Load(object sender, EventArgs e)
        {

        }

        private void labelControl1_Click(object sender, EventArgs e)
        {

        }
    }
}
