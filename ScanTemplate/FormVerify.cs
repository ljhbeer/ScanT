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
        private List<int> _ColState;
        public FormVerify(ARTemplate.Template _artemplate, DataTable _rundt, AutoAngle _angle)
        {
            this._artemplate = _artemplate;
            this._rundt = _rundt;
            this._angle = _angle;
            _ColState = new List<int>();
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
                double angle = (double)( dr["校验角度"]);
                Bitmap bmp = (Bitmap)Bitmap.FromFile(fn);
                 if (_angle != null)
                _angle.SetPaper(angle);
//                 DrawInfoBmp(bmp, angle);
                //pictureBox1.Image = bmp;
                
                DataTable dt = Tools.DataTableTools
                	.ConstructDataTable(new string[]{"学号","题号","图片","你的答案","A","B","C","D" ,"多选"   });
                _ColState = new List<int>(){ 0,0,0,0,1,2,3,4,-5};
                dgv.DataSource = dt;  
                dgv.RowTemplate.Height = 30;
                dgv.Columns["学号"].Width = 40;
                dgv.Columns["题号"].Width = 20;
                dgv.Columns["学号"].Width = 40;
                dgv.Columns["图片"].Width = 100;
                dgv.Columns["你的答案"].Width = 20;
                dgv.Columns["A"].Width = 15;
                dgv.Columns["B"].Width = 15;
                dgv.Columns["C"].Width = 15;
                dgv.Columns["D"].Width = 15;
                dgv.Columns["多选"].Width = 40;
                
                for(int i=0; i< _artemplate.XztRect.Count; i++){
                	string value = dr["x"+(i+1)].ToString();
                	if(value.Length!=1 || !"ABCD".Contains(value)){
                		DataRow ndr = dt.NewRow();
                		ndr["学号"] = dr["考号"];
                		ndr["题号"] = "x"+(i+1);
                		Rectangle r = _artemplate.XztRect[i];
//                		r.Location = _angle.GetCorrectPoint(r.X,r.Y);
                		Bitmap nbmp = bmp.Clone( r,bmp.PixelFormat);
                		ndr["图片"] = nbmp;
                		ndr["你的答案"] = value;
                		dt.Rows.Add(ndr);
                	}
                }                            
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
//                pictureBox1.Image = bmp;
            }
        }
        
        void DgvCellClick(object sender, DataGridViewCellEventArgs e)
        {
        	if ( e.ColumnIndex == -1) return;
			if (_ColState[e.ColumnIndex] > 0)
            {
//				int score = _ColState[e.ColumnIndex] - 1;        
//				int scoreindex = e.ColumnIndex - score -1;      
//                if (e.RowIndex == -1)
//                {
//                    for(int i=0; i<dgvs.Rows.Count; i++)
//                        dgvs.Rows[i].Cells[scoreindex].Value = score;
//                    dgvs.Invalidate();
//                }
//                else
//                {
//                    dgvs.Rows[e.RowIndex].Cells[scoreindex].Value = score;
//                    dgvs.InvalidateRow(e.RowIndex); 
//                }
            }
        }
        
        void DgvCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
        	if(e.ColumnIndex==-1 || e.RowIndex == -1) return;
			if (_ColState[e.ColumnIndex] > 0)
            {
				int score = _ColState[e.ColumnIndex] - 1;        
				int scoreindex = e.ColumnIndex - score -1; 
				if(dgv.Rows[e.RowIndex].Cells[scoreindex].Value is DBNull) return;                
                string str = dgv.Rows[e.RowIndex].Cells[scoreindex].Value.ToString();                
                Char value =Convert.ToChar( 'A'+score);
                if (str.Contains(value))
                {                    
        			e.CellStyle.BackColor =  Color.Red;
                }
			}
        }
    }
}
