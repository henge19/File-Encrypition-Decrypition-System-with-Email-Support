using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Net;

namespace File_Encryption_Security_System
{


    public partial class Form1 : Form
    {
        FilterInfoCollection fico;                                               // bilgisara bağlı kameraları tutan dizim.
        VideoCaptureDevice vcd;
        User_Form k = new User_Form();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fico = new FilterInfoCollection(FilterCategory.VideoInputDevice);    // Find your cameras that connected to the your computer          
            vcd = new VideoCaptureDevice(fico[1].MonikerString);                 //.MonikerString takma isim oluşturma . My main camera is in the second index.
            vcd.NewFrame += Vcd_NewFrame;
            vcd.Start();
            k.GetVideoCaptureDevice(vcd);
            k.GetPictureBox(pictureBox1);


        }
        private void Vcd_NewFrame(object sender, NewFrameEventArgs eventArgs)    // kameradan aldığımızı çerçeveyi picterebox a akatarıyoruz aktarmak için oluşturduk
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }



        private void btnSign_Click(object sender, EventArgs e)
        {
            k.user(txtEmail, txtPassword);
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            k.newuser(NameSurnametxt, Emailtxt, Passwordtxt, ConfirmPasswordtxt, groupBox2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
            vcd.Stop();
        }

        private void howToUseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("First you have to create an account for accessing features of this application. \n\nThen when you sign in to your account, you can encrypt your files and send it to your gmail adress. \n\nYou will receive  your encrypted file with the decryption program.");
        }

        private void tToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Ali ECEMİŞ  2016513021 \n\nMehmet ALTIPARMAK  2016513005 ");
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
            vcd.Stop();
        }
    }

    class User_Form
    {

        SqlConnection DataBaseConnection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\source\repos\File_Encryption_Security_System\File_Encryption_Security_System\Database.mdf;Integrated Security=True");
        SqlCommand Command;
        SqlDataReader Read;
        Encryption_Form NewForm = new Encryption_Form();
        private PictureBox PictureBoxClass;
        private VideoCaptureDevice vcd;

        public void GetPictureBox(PictureBox pictureBox1)
        {
            PictureBoxClass = pictureBox1;
        }
        public void GetVideoCaptureDevice(VideoCaptureDevice vcd)
        {
            this.vcd = vcd;
        }

        public SqlDataReader user(TextBox Email, TextBox Password)  // Buraya useremal
        {
            SendEmail s = new SendEmail();                          // Email class created.

            DataBaseConnection.Open();
            Command = new SqlCommand();
            Command.Connection = DataBaseConnection;
            Command.CommandText = "select *from tbl_User where Email='" + Email.Text + "'";  // string part of the code is a commend for database
            Read = Command.ExecuteReader();
            s.GetEmail(Email);                                      // txtEmail has taken here from the Email parameter
            NewForm.GetEmailForEncryption(Email);

            if (Read.Read() == true)
            {
                if (Password.Text == Read["Password"].ToString())            // "Password" is coming from datebase column
                {
                    
                    vcd.Stop();
                    NewForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Check your password ", "Error1");    // ********Email olayı burda olacak*******
                    string path = @"D:\Desktop\test_of_Prj";
                    PictureBoxClass.Image.Save(path + @"\" + "Wanted!" + ".jpg", ImageFormat.Jpeg);
                    vcd.Stop();
                    s.sendemailforwrongpassword();          // We sended our email in here.
                }
            }
            else
            {
                MessageBox.Show("Check your informations", "Error2");    // if its username is diffrent 
            }
            DataBaseConnection.Close();
            return Read;
        }



        public void newuser(TextBox NameSurname, TextBox Email, TextBox Password, TextBox ConfirmPassword, GroupBox group)
        {

            if (String.IsNullOrEmpty(Email.Text) || String.IsNullOrEmpty(NameSurname.Text))
            {
                Email.Text = "";
                MessageBox.Show("Fill information boxs.");
            }

            else
            {
                DataBaseConnection.Open();
                Command = new SqlCommand();
                Command.Connection = DataBaseConnection;
                Command.CommandText = "select *from tbl_User where Email='" + Email.Text + "'";  // string part of the code is a commend for database
                Read = Command.ExecuteReader();

                if (Read.Read() == true)
                {
                    if (Email.Text == Read["Email"].ToString())
                    {
                        MessageBox.Show("This email address has already has used. Please change your email address.");
                        DataBaseConnection.Close();                    
                    }                                  
                }
                else
                {
                    if (Password.Text == ConfirmPassword.Text)
                    {
                        DataBaseConnection.Close();
                        DataBaseConnection.Open();
                        Command = new SqlCommand();
                        Command.Connection = DataBaseConnection;
                        Command.CommandText = "insert into tbl_User values('" + NameSurname.Text + "', '" + Email.Text + "', '" + Password.Text + "')";
                        Command.ExecuteNonQuery();
                        DataBaseConnection.Close();
                        MessageBox.Show("New member added.");
                        foreach (Control item in group.Controls) if (item is TextBox) item.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("It didnt match with your password.", "Error");
                        DataBaseConnection.Close();
                    }
                }               
            }
        }
    }
  
    class SendEmail
    {

        private TextBox txtEmail;
        private TextBox path;


        public void GetPath(TextBox path)
        {
            this.path =path;
        }
        public void GetEmail(TextBox txtEmail)
        {
            this.txtEmail = txtEmail;
        }

        public void sendemailforwrongpassword()
        {

            SmtpClient Client = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,


                Credentials = new NetworkCredential()
                {
                    UserName = "programingLanguage.demo@gmail.com",
                    Password = "Programing2021"
                }
            };

            MailAddress FromEmail = new MailAddress("programingLanguage.demo@gmail.com", "Security");
            MailAddress ToEmail = new MailAddress(txtEmail.Text, "User");
            Attachment attachment = new Attachment(@"D:\Desktop\test_of_Prj\Wanted!.jpg");
            MailMessage Message = new MailMessage()
            {
                From = FromEmail,
                Subject = "Unauthorized User",
                Body = "Somebody tried to enter your account.",
            };

            Message.Attachments.Add(attachment);
            Message.To.Add(ToEmail);

            try
            {
                Client.Send(Message);
                MessageBox.Show(" Your mail succesfully send it.", "Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something wrong\n" + ex.Message, "Error");
            }

        }

        public void sendemailforgivingfile(TextBox EmailHolder)
        {


            SmtpClient Client = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,


                Credentials = new NetworkCredential()
                {
                    UserName = "programingLanguage.demo@gmail.com",
                    Password = "Programing2021"
                }
            };

            MailAddress FromEmail = new MailAddress("programingLanguage.demo@gmail.com", "Security");
            MailAddress ToEmail = new MailAddress(EmailHolder.Text, "User");
            Attachment attachment = new Attachment(path.Text);  // ******tbPath uzantısı  eklenecek******            
            MailMessage Message = new MailMessage()
            {
                From = FromEmail,
                Subject = "Encrypted File",
                Body = "You can decrypt your file with the program in this link https://1drv.ms/u/s!AlR2g090TvY5jS2PHZVN8pr-rdln?e=lOhzcE using your password ",
            };
            Message.Attachments.Add(attachment);          
            Message.To.Add(ToEmail);

            try
            {
                Client.Send(Message);
                MessageBox.Show(" Your mail succesfully send it.", "Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something wrong\n" + ex.Message, "Error");
            }

        }

    }

}
