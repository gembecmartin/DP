using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nethereum.Generators;
using MongoDB.Driver;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Forms
{
    public partial class AddIcoDIalogue : Form
    {
        public string selectedAbi = "";
        private List<Token> tokenList = new List<Token>();
        private ComboBox parentPicker;
        public AddIcoDIalogue(ComboBox originalComboBox)
        {
            parentPicker = originalComboBox;

            InitializeComponent();
            labelControl1.Visible = false;
            labelControl2.Visible = false;

            var connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            var db = client.GetDatabase("DP");
            var tokenRecords = db.GetCollection<Token>("tokens").Find(_ => true).ToList();

            foreach (var token in tokenRecords)
            {
                tokenList.Add(token);
            }

            comboBox1.DataSource = tokenList;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            xtraOpenFileDialog1.Filter = "Text Files (.txt)|*.txt";
            xtraOpenFileDialog1.FilterIndex = 1;
            xtraOpenFileDialog1.Multiselect = false;
            xtraOpenFileDialog1.CheckFileExists = true;
            xtraOpenFileDialog1.FilterIndex = 1;
            xtraOpenFileDialog1.RestoreDirectory = true;

            if (xtraOpenFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream fileStream = xtraOpenFileDialog1.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    try
                    {
                        string abi = reader.ReadToEnd();
                        string temp = ClearAbiString(abi);
                        JArray.Parse(temp);
                        selectedAbi = string.Copy(temp);
                        memoEdit1.Text = string.Copy(abi);

                        memoEdit1.Properties.Appearance.BorderColor = Color.Transparent;
                        labelControl2.Visible = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ABI not in JSON format.");
                    }
                }
                fileStream.Close();
            }
        }

        private string ClearAbiString(string abi)
        {
            string newAbi = string.Copy(abi);
            newAbi = newAbi.Replace("\r", "").Replace("\n", "").Replace("\t", "");
            return newAbi;
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void confirm_Click(object sender, EventArgs e)
        {
            bool errName = false;   
            bool errAbi = false;
            bool errBytecode = false;
            bool errCode = false;

            if (textEdit1.Text == "")
            {
                textEdit1.Properties.Appearance.BorderColor = Color.Red;
                errName = true;
                labelControl1.Visible = true;
            }

            if (memoEdit1.Text == "")
            {
                memoEdit1.Properties.Appearance.BorderColor = Color.Red;
                errAbi = true;
                labelControl2.Visible = true;
            }

            if(textEdit2.Text == "")
            {
                textEdit2.Properties.Appearance.BorderColor = Color.Red;
                errBytecode = true;
                labelControl3.Visible = true;
            }

            if (textEdit3.Text == "")
            {
                textEdit3.Properties.Appearance.BorderColor = Color.Red;
                errCode = true;
                labelControl4.Visible = true;
            }

            if (!errAbi && !errName && !errBytecode && !errCode) //
            {
                try {
                    string abi = ClearAbiString(memoEdit1.Text);
                    JArray.Parse(abi);

                    Program.init.CreateNewToken(abi, textEdit2.Text, textEdit3.Text, textEdit1.Text, (TokenProperties)comboBox1.SelectedValue);
                    parentPicker.Refresh();
                    Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("ABI not in JSON format.");
                    return;
                }
            }
        }

        private void panelControl1_Validating(object sender, CancelEventArgs e)
        {
            
        }

        private void textEdit1_Validating(object sender, CancelEventArgs e)
        {
            if (textEdit1.Text == "")
            {
                textEdit1.Properties.Appearance.BorderColor = Color.Red;
                labelControl1.Visible = true;
            }
            else
            {
                textEdit1.Properties.Appearance.BorderColor = Color.Transparent;
                labelControl1.Visible = false;
            }
        }

        private void memoEdit1_Validating(object sender, CancelEventArgs e)
        {
            if (memoEdit1.Text == "")
            {
                memoEdit1.Properties.Appearance.BorderColor = Color.Red;
                labelControl2.Visible = true;
            }
            else
            {
                memoEdit1.Properties.Appearance.BorderColor = Color.Transparent;
                labelControl2.Visible = false;
            }
        }

        private void textEdit1_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void textEdit1_TextChanged(object sender, EventArgs e)
        {
            textEdit1.DoValidate();
        }

        private void memoEdit1_TextChanged(object sender, EventArgs e)
        {
            memoEdit1.DoValidate();
        }

        private void textEdit2_Validating(object sender, CancelEventArgs e)
        {
            if (textEdit2.Text == "")
            {
                textEdit2.Properties.Appearance.BorderColor = Color.Red;
                labelControl3.Visible = true;
            }
            else
            {
                textEdit2.Properties.Appearance.BorderColor = Color.Transparent;
                labelControl3.Visible = false;
            }
        }

        private void textEdit2_TextChanged(object sender, EventArgs e)
        {
            textEdit2.DoValidate();
        }

        private void textEdit3_TextChanged(object sender, EventArgs e)
        {
            textEdit3.DoValidate();
        }

        private void textEdit3_Validating(object sender, CancelEventArgs e)
        {
            if (textEdit3.Text == "")
            {
                textEdit3.Properties.Appearance.BorderColor = Color.Red;
                labelControl4.Visible = true;
            }
            else
            {
                textEdit3.Properties.Appearance.BorderColor = Color.Transparent;
                labelControl4.Visible = false;
            }
        }
    }
}
