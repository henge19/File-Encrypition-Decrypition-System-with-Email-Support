using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace File_Encryption_Security_System
{
    public partial class Encryption_Form : Form
    {
        byte[] abc;
        byte[,] table;
        SendEmail Encrypt = new SendEmail();
        private TextBox txtEmailForEncryption;

        public void GetEmailForEncryption(TextBox txtEmail)
        {
            txtEmailForEncryption = txtEmail;
        }
        public Encryption_Form()
        {
            InitializeComponent();
        }

        private void Encryption_Form_Load(object sender, EventArgs e)
        {

            
            rbEncrypt.Checked = true;

            //init abc  and table 
            abc = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                abc[i] = Convert.ToByte(i);
            }

            table = new byte[256, 256];
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                {
                    table[i, j] = abc[(i + j) % 256];
                }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Multiselect = false;
            if (od.ShowDialog() == DialogResult.OK)
            {
                tbPath.Text = od.FileName;
            }       

        }

        private void btStart_Click(object sender, EventArgs e)
        {
            //Check input value 
            if (!File.Exists(tbPath.Text))
            {
                MessageBox.Show("File doesnt found");
                return;
            }
            if (String.IsNullOrEmpty(tbPath.Text))
            {
                MessageBox.Show("Password empty. Please enter your password");
                return;
            }

            try
            {
                byte[] fileContent = File.ReadAllBytes(tbPath.Text);
                byte[] passwordTmp = Encoding.ASCII.GetBytes(tbPassword.Text);
                byte[] keys = new byte[fileContent.Length];
                for (int i = 0; i < fileContent.Length; i++)
                {
                    keys[i] = passwordTmp[i % passwordTmp.Length];
                }

                //Encrypt
                byte[] result = new byte[fileContent.Length];
                if (rbEncrypt.Checked)
                {

                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        byte value = fileContent[i];
                        byte key = keys[i];
                        int valueIndex = -1, keyIndex = -1;

                        for (int j = 0; j < 256; j++)
                        {
                            if (abc[j] == value)
                            {
                                valueIndex = j;
                                break;
                            }
                        }
                        for (int j = 0; j < 256; j++)
                        {
                            if (abc[j] == key)
                            {
                                keyIndex = j;
                                break;
                            }

                        }
                        result[i] = table[keyIndex, valueIndex];
                    }
                }
                
                //save result to new file with the same extention
                string fileExt = Path.GetExtension(tbPath.Text);
                SaveFileDialog sd = new SaveFileDialog();
                sd.Filter = "Files (*" + fileExt + ") | *" + fileExt;
                if (sd.ShowDialog() == DialogResult.OK)
                {
                   File.WriteAllBytes(sd.FileName, result);
                }
                
                
            }
            catch
            {
                MessageBox.Show("File is in use");
                return;

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileExt1 = tbPath.Text;
            if (File.Exists(fileExt1)){

                File.Delete(fileExt1);
                MessageBox.Show("Your file deleted.");

            }
   
        }

        private void Browseforencryption_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Multiselect = false;
            if (od.ShowDialog() == DialogResult.OK)
            {
               tbPathforEncryption.Text = od.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Encrypt.GetPath(tbPathforEncryption);
            Encrypt.sendemailforgivingfile(txtEmailForEncryption);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
