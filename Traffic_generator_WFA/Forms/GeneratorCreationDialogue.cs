using CsvHelper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Traffic_generator_WFA.Control;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Forms
{
    public partial class GeneratorCreationDialogue : Form
    {
        public GeneratorCreationDialogue()
        {
            var connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            var db = client.GetDatabase("transaction_data");
            var tokenRecords = db.GetCollection<Token>("tokens").Find(_ => true).ToList();

            Program.init.tc = new TransactionController();
            foreach (var token in tokenRecords)
            {
                Program.init.tc.tokenList.Add(token);
            }
            InitializeComponent();
            comboBox1.DataSource = Program.init.tc.tokenList;
        }

        private async void confirm_ClickAsync(object sender, EventArgs e)
        {

            if (noAccount.Text != "")
            {
                Program.init.CreateAccountsAsync(Int32.Parse(noAccount.Text), comboBox1.SelectedValue.ToString());
                Hide();
            }
            else
                MessageBox.Show("One or more parameters are not filled!\n" +
                    "Fill all parameters and repeat your request.", "Generator missing parameters", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {

        }

        private void volume_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void noTrasactions_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void noAccount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
