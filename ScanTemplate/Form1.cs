﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace ScanTemplate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _workpath = textBoxWorkPath.Text;
            _angle = null;
            _artemplate = null;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //AutoLoadLatestImg(_workpath);
            foreach (string s in GetLastestSubDirectorys(_workpath))
            {
                //TODO: 使用类，显示相关信息
                listBox1.Items.Add(s); 
            }
        }

        private void buttonworkpath_Click(object sender, EventArgs e)
        {
            string str = textBoxWorkPath.Text;
            if (Directory.Exists(str))
                _workpath = textBoxWorkPath.Text;
        }
        private void buttonGo_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            string path = listBox1.SelectedItem.ToString();

            List<string> nameList = NameListFromDir( path);
            if(nameList.Count==0) return;

            //string msg = "共有文件" + nameList.Count + "个" + string.Join("\r\n", nameList);
            //MessageBox.Show(msg);
            //TODO：检测是否存在模板文件
            string filename = nameList[0];
            TestAndCreateTemplate(filename, nameList);
        }
        private void TestAndCreateTemplate( string filename, List<string> namelist=null)
        {
            Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
            MyDetectFeatureRectAngle dr = new MyDetectFeatureRectAngle(bmp);

            if (dr.Detected())
            {
                if (_artemplate == null)
                {
                    _artemplate = new ARTemplate.Template(filename, bmp, dr.CorrectRect);
                    _angle = new AutoAngle(dr.ListPoint); //或者导入时 设置
                }
                _angle.SetPaper(dr.ListPoint[0],dr.ListPoint[1],dr.ListPoint[2]);

                _artemplate.ResetBitMap(filename, bmp, dr.CorrectRect);//,_angle

                _artemplate.SetFeaturePoint(dr.ListFeatureRectangle,dr.CorrectRect);

                this.Hide();
                ARTemplate.FormTemplate f = new ARTemplate.FormTemplate(_artemplate);
                f.ShowDialog();
                this.Show();

                //if(namelist!=null)
                //DetectAllImgs(dr, namelist);
            }
        }
        private void DetectAllImgs(MyDetectFeatureRectAngle dr,List<string> nameList)
        {
            FileInfo fi = new FileInfo(nameList[0]);
            string dir = fi.Directory.FullName.Replace("LJH\\", "LJH\\Correct\\");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            StringBuilder sb = new StringBuilder();
            foreach (string s in nameList)
            {
                sb.Append(DetectAllImg(dr, s));
            }
            File.WriteAllText("allimport.txt", sb.ToString());
        }

        private StringBuilder DetectAllImg(MyDetectFeatureRectAngle dr, string s)
        {
            Bitmap bmp = (Bitmap)Bitmap.FromFile(s);
            string str = s.Substring(s.Length - 7, 3);
            //MyDetectFeatureRectAngle dr = new MyDetectFeatureRectAngle(bmp);
            Rectangle CorrectRect = dr.Detected(bmp);
            StringBuilder sb = new StringBuilder();
            sb.Append(s + "," + Recttostring(CorrectRect) + ",");
            if (CorrectRect.Width > 0)
            {
            	//TODO: debug r1 in 001
                Rectangle cr1 = new Rectangle(CorrectRect.Right-60,CorrectRect.Top-20,80,80);           
                Rectangle r1 = dr.Detected(cr1, bmp);
                Rectangle cr2 = new Rectangle(CorrectRect.Left - 20, CorrectRect.Bottom - 60, 80, 80);
                Rectangle r2 = dr.Detected(cr2, bmp);

                _angle.SetPaper(CorrectRect.Location, r1.Location, r2.Location);

                Bitmap nbmp = (Bitmap)bmp.Clone(CorrectRect, bmp.PixelFormat);
                nbmp.Save(s.Replace("LJH\\", "LJH\\Correct\\"));

                //计算选择题
                AutoComputeXZT acx = new AutoComputeXZT(_artemplate, _angle, nbmp);

                sb.Append(acx.ComputeXZT(str));


            }
            else
            {
                //检测失败
            }
            sb.AppendLine();
            return sb;
            //MessageBox.Show(sb.ToString());
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            string path = listBox1.SelectedItem.ToString();
            listBox2.Items.Clear();
            listBox2.Items.AddRange(NameListFromDir(path) .ToArray());
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listBox2.SelectedIndex == -1) return;
            string filename = listBox2.SelectedItem.ToString();
            if (checkBoxDetectImg.Checked)
            {
                Bitmap bmp1 = (Bitmap)Bitmap.FromFile(listBox2.Items[0].ToString());
                MyDetectFeatureRectAngle dr = new MyDetectFeatureRectAngle(bmp1);
                if (dr.Detected())
                {
                    DetectAllImg(dr, listBox2.SelectedItem.ToString());
                }
            }
            else
            {
                TestAndCreateTemplate(filename);
            }
        }

        private string Recttostring(Rectangle r)
        {
            return "(" + r.X + "," + r.Y + "," + r.Width + "," + r.Height + ")";
        }
        private static string GetLastestSubDirectory(string path)
        {
            List<string> sudirects = GetLastestSubDirectorys(path);               
            if (sudirects.Count > 0)
                return sudirects.Max();
            return "";
        }
        private static List<string> GetLastestSubDirectorys(string path)
        {
            Regex r = new Regex("[0-9]{8}-[0-9]+");
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                List<string> sudirects = new List<string>();
                foreach (DirectoryInfo d in dir.GetDirectories())
                {
                    if (r.IsMatch(d.Name))
                    {
                        sudirects.Add(d.FullName);
                    }
                }
                return sudirects;
            };
            return new List<string>();
        }

        private List<string> NameListFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                FileInfo fi = new FileInfo(filename);
                string fidir = fi.Directory.FullName;
                return NameListFromDir(fidir);
            }
            return new List<string>();
        }
        public static List<string> NameListFromDir(string fidir)
        {
            List<string> namelist = new List<string>();
            DirectoryInfo dirinfo = new DirectoryInfo(fidir);
            //string ext = fi.Extension;
            foreach (FileInfo f in dirinfo.GetFiles())
                if (f.Extension.ToLower() == ".tif")
                    namelist.Add(f.FullName);
            return namelist;
        }
        private string _workpath;
        private AutoAngle _angle;
        private ARTemplate.Template _artemplate;
    }
}
