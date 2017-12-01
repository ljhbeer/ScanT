using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ARTemplate;
using System.IO;

namespace ScanTemplate
{
    public partial class FormVerify : Form
    {
        private ARTemplate.Template _artemplate;
        private DataTable _rundt;
        private AutoAngle _angle;
        public FormVerify(ARTemplate.Template _artemplate, DataTable _rundt, AutoAngle _angle)
        {
            this._artemplate = _artemplate;
            this._rundt = _rundt;
            this._angle = _angle;
            InitializeComponent();
        }
        private void buttonImportData_Click(object sender, EventArgs e)
        {
            listBoxXH.Items.Clear();
            foreach (DataRow dr in _rundt.Rows)
                if (dr["考号"].ToString() == "")
                {
                    string value = dr["序号"] + "-" + dr["考号"];
                    listBoxXH.Items.Add(new ValueTag(value, dr));
                }

            listBoxXZT.Items.Clear();

            int xztcnt = 0;
            if (_artemplate.Dic.ContainsKey("选择题"))
            {
                foreach (Area I in _artemplate.Dic["选择题"])
                {
                    xztcnt += ((SingleChoiceArea)I).Count;
                }
            }
            foreach (DataRow dr in _rundt.Rows)
            {
                bool b = false;
                for (int i = 0; i < xztcnt; i++)
                {
                    if (!"ABCD".Contains(dr["x" + (i + 1)].ToString()) ||
                        dr["x" + (i + 1)].ToString().Length > 1)
                    {
                        string value = dr["序号"] + "-" + dr["考号"];
                        int index = listBoxXZT.Items.Add(new ValueTag(value,dr));
                        //MessageBox.Show(listBoxXZT.Items[index].ToString());
                        break;
                    }
                }
                if (b)
                    listBoxXH.Items.Add(dr);
            }
        }
        private void listBoxXH_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ListBox)sender).SelectedIndex == -1) return;

        }
        private void listBoxXZT_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.SelectedIndex == -1) return;
            DataRow dr = (DataRow)((ValueTag)(lb.SelectedItem)).Tag;
            string fn = dr["文件名"].ToString().Replace("LJH\\", "LJH\\Correct\\");
            if (File.Exists(fn))
            {
                double angle = (double)( dr["校验"]);
                Bitmap bmp = (Bitmap)Bitmap.FromFile(fn);
                DrawInfoBmp(bmp, angle);
                //pictureBox1.Image = bmp;
            }
        }
        private void DrawInfoBmp(Bitmap bmp, double angle)
        {
            if (_angle != null)
                _angle.SetPaper(angle);
            bmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            string keyname = "选择题";
            if (_artemplate.Dic.ContainsKey(keyname))
            {
                foreach (Area I in _artemplate.Dic[keyname])
                {
                    bmp = bmp.Clone(I.ImgArea,bmp.PixelFormat);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        if (I.HasSubArea())
                        {
                            foreach (Rectangle r in I.ImgSubArea())
                            {
                                //r.Offset(I.ImgArea.Location);
                                g.DrawRectangle(Pens.Red, r);
                                //r.Offset(-1, -1);
                                //g.DrawRectangle(pen, r);
                                //r.Offset(2, 2);
                                //g.DrawRectangle(pen, r);
                            }
                        }
                    }

                    break;
                }
                pictureBox1.Image = bmp;
            }
        }
    }
}
