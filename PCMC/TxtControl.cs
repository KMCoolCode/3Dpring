using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
namespace sun
{
    static class TxtControl
    {
        public static void WriteNewTxt(string txtname, string txttext)
        {
            string logPath1 = Environment.CurrentDirectory;
            string logPath = logPath1 + @"\" + txtname + ".txt";

            FileStream fs = new FileStream(logPath, FileMode.Create);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(txttext);
            }
            fs.Close();
        }
        public static void WriteOldTxt(string txtname, string txttext)
        {
            string logPath1 = Environment.CurrentDirectory;
            string logPath = logPath1 + @"\" + txtname + ".txt";

            FileStream fs = new FileStream(logPath, FileMode.Append);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(txttext);
            }
            fs.Close();
        }
        public static List<string> ReadLineTxt1(string txtname)
        {
            List<string> Strlist = new List<string>();
            string logPath1 = Environment.CurrentDirectory;
            string logPath = logPath1 + @"\" + txtname + ".txt";
            FileStream fs1 = new FileStream(logPath, FileMode.Open);
            StreamReader sr = new StreamReader(fs1);
            while (sr.Peek() >= 0)
            {

                Strlist.Add(sr.ReadLine());
            }
            fs1.Close();
            sr.Close();
            return Strlist;
        }
        public static List<string> ReadLineTxt(string txtname)
        {
            List<string> Strlist = new List<string>();
 
            string logPath = txtname ;
            FileStream fs1 = new FileStream(logPath, FileMode.Open);
            StreamReader sr = new StreamReader(fs1);
            while (sr.Peek() >= 0)
            {

                Strlist.Add(sr.ReadLine());
            }
            fs1.Close();
            sr.Close();
            return Strlist;
        }
        public static List<string> ReadAllTxt(string txtname)
        {
            List<string> Strlist = new List<string>();
            string logPath1 = Environment.CurrentDirectory;
            string logPath = logPath1 + @"\" + txtname + ".txt";
            FileStream fs1 = new FileStream(logPath, FileMode.Open);
            StreamReader sr = new StreamReader(fs1);
            while (sr.Peek() >= 0)
            {

                Strlist.Add(sr.ReadToEnd());
            }
            fs1.Close();
            sr.Close();
            return Strlist;
        }
        //public void write_product()
        //{
        //    string logPath1 = Environment.CurrentDirectory;
        //    string logPath = "";
        //    if (TBB1.Text != "")
        //    {
        //        logPath = logPath1 + @"\production" + @"\" + TBB1.Text + ".txt";
        //    }
        //    else
        //    {
        //        logPath = logPath1 + @"\production" + @"\" + comboBox1.SelectedItem.ToString() + ".txt";
        //    }

        //    FileStream fs = new FileStream(logPath, FileMode.Create);
        //    using (StreamWriter sw = new StreamWriter(fs))
        //    {

        //        foreach (var item in groupBox7.Controls)
        //        {
        //            if (item is TextBox) //TextBox可以换成其他控件
        //            {
        //                var comm = item as TextBox;
        //                if (comm.Text != "")
        //                {
        //                    sw.Write("\r\n" + comm.Name + ":" + comm.Text);
        //                }
        //                else
        //                {
        //                    sw.Write("\r\n" + comm.Name + ":" + "##");
        //                }
        //            }
        //        }
        //    }

        //    fs.Close();
        //}

        //public void read_product()
        //{
        //    try
        //    {
        //        string logPath1 = Environment.CurrentDirectory;
        //        List<string> b = new List<string>();
        //        if (TXTname != "")
        //        {
        //            string logPath = logPath1 + @"\production" + @"\" + TXTname + ".txt";
        //            FileStream fs1 = new FileStream(logPath, FileMode.Open);
        //            StreamReader sr = new StreamReader(fs1);
        //            string Str = "";
        //            int inde = 0;
        //            while (sr.Peek() >= 0)
        //            {

        //                Str = sr.ReadLine();
        //                if (Str.Length > 2)
        //                {
        //                    b.Add(Str.Remove(0, Str.IndexOf(":") + 1));
        //                }
        //            }

        //            fs1.Close();
        //            sr.Close();
        //            foreach (var item in groupBox7.Controls)
        //            {
        //                if (item is TextBox) //TextBox可以换成其他控件
        //                {
        //                    var comm = item as TextBox;
        //                    comm.Text = b[inde];
        //                    inde = inde + 1;
        //                }
        //            }

        //            comboBox1.SelectedIndex = int.Parse(TXTindex);
        //        }
        //    }
        //    catch (Exception EX)
        //    {
        //        message(EX.ToString());
        //    }
        //}
        public static List<string> Readlist(string listName)
        {
            List<string> Listvalue = new List<string>();
            if (!Directory.Exists(Environment.CurrentDirectory + @"\list"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\list");
            }

            string logPath1 = Environment.CurrentDirectory + @"\list";
            DirectoryInfo folder = new DirectoryInfo(logPath1);
            string logPath = logPath1 + @"\" + listName + ".txt";
            if (folder.GetFiles(listName + ".txt").Count() == 0)
            {
                FileStream fs = new FileStream(logPath, FileMode.Create);
                fs.Close();
            }
            else
            {
                Listvalue = ReadLineTxt(logPath);
            }
            return Listvalue;
        }
    }
}
