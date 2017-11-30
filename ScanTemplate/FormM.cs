using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ARTemplate;
using System.Text.RegularExpressions;
using System.Threading;
namespace ScanTemplate
{
	public delegate void MyInvoke( );
    public partial class FormM : Form
    {
        private string _workpath;
        private AutoAngle _angle;
        private Template _artemplate;
        private MyDetectFeatureRectAngle _rundr;
        private List<string> _runnameList;
        private String _runmsg;
        private DataTable _rundt;
        public FormM()
        {
            InitializeComponent();
            _workpath = textBoxWorkPath.Text;
            _angle = null;
            _artemplate = null;
            _rundr = null;
            _runnameList=null;
            _runmsg = "";
        }
        private void FormM_Load(object sender, EventArgs e)
        {
            //AutoLoadLatestImg(_workpath);
            foreach (string s in GetLastestSubDirectorys(_workpath))
            {
                //TODO: 使用类，显示相关信息
                listBox1.Items.Add(s);
            }
            string templatepath = _workpath.Substring( 0,_workpath.LastIndexOf("\\"))+"\\Template";
            foreach (string s in NameListFromDir(templatepath,".xml"))
            {
                //TODO: 使用类，显示相关信息
                
                listBoxTemplate.Items.Add( new TInfo(s));
            }
            panel3.AutoScroll = true;
        }
        private void buttonLeftHide_Click(object sender, EventArgs e)
        {
            if (TLP.ColumnStyles[0].Width > 10)
                TLP.ColumnStyles[0].Width =2;
            else
                TLP.ColumnStyles[0].Width =20.0F;

        }
        private void buttonRightHide_Click(object sender, EventArgs e)
        {
            if (TLP.ColumnStyles[2].Width >10)
                TLP.ColumnStyles[2].Width =2;
            else
                TLP.ColumnStyles[2].Width =35.0F;

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
            List<string> nameList = NameListFromDir(path);
            if (nameList.Count == 0) return;
            string filename = nameList[0];
            CreateTemplate(filename);
        }        
        private void ButtonUseTemplateClick(object sender, System.EventArgs e)
        {
        	if (listBox1.SelectedIndex == -1) return;
        	if(listBoxTemplate.SelectedIndex==-1){
        		MessageBox.Show("还未选择模板");
        		return ;
        	}
        	
        	string path = listBox1.SelectedItem.ToString();
            List<string> nameList = NameListFromDir(path);
            if (nameList.Count == 0) return;
            string filename = nameList[0];
            
            string templatefile =((TInfo) listBoxTemplate.SelectedItem).Str;            
            CreateTemplate(filename,templatefile);
        }
        private void ButtonScanClick(object sender, EventArgs e)
        {
        	if (listBox1.SelectedIndex == -1) return;
        	if(listBoxTemplate.SelectedIndex==-1){
        		MessageBox.Show("还未选择模板");
        		return ;
        	}
        	string path = listBox1.SelectedItem.ToString();
            List<string> nameList = NameListFromDir(path);
            if (nameList.Count == 0) return;
        	Bitmap bmp = (Bitmap)Bitmap.FromFile(_artemplate.Filename);
            MyDetectFeatureRectAngle dr = new MyDetectFeatureRectAngle(bmp);
            DetectAllImgs(dr, nameList);
        	
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
			string templatename = listBox1.SelectedItem.ToString();
            string txt = templatename.Substring(templatename.LastIndexOf("\\") + 1);
			bool unselected = true;
			for(int i=0; i<listBoxTemplate.Items.Count; i++){
				if(listBoxTemplate.Items[i].ToString().StartsWith(txt)){
					listBoxTemplate.SelectedIndex = i;
                    string templatefilename = ((TInfo)listBoxTemplate.SelectedItem).Str;
                    string dataname = templatefilename .Replace(".xml", ".txt");

                    if (_artemplate == null)
                    {
                        Template t = new Template(templatefilename);
                        if (t.Image != null)
                        {
                            _artemplate = t;
                            List<Point> ListPoint = new List<Point>();
                            foreach (Area I in t.Dic["特征点"])
                            {
                                ListPoint.Add(I.ImgArea.Location);
                            }
                            if(ListPoint.Count==3)
                                _angle = new AutoAngle( ListPoint);
                        }
                    }

                    if (File.Exists(dataname))
                    {
                        listBoxData.Items.Clear();
                        listBoxData.Items.Add(listBox1.SelectedItem);
                    }
					unselected = false;
					break;
				}
			}
			if(unselected)
				listBoxTemplate.SelectedIndex = -1;
        }
        private void listBoxData_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            string dataname = ((TInfo)listBoxTemplate.SelectedItem).Str.Replace(".xml", ".txt");
            //TODO： 检测是否导入已有数据
            _rundt = Tools.DataTableTools.ConstructDataTable(new string[]{
                    "序号","文件名","校验","姓名","考号","选择题"  });
            dgv.DataSource = _rundt;

            string[] ls = File.ReadAllLines(dataname);
            for (int i = 0; i < ls.Length; i++)
            {
                string[] ss = ls[i].Split(',');
                DataRow dr = _rundt.NewRow();
                dr["序号"] = _rundt.Rows.Count + 1;
                if (ss.Length > 3)
                {
                    dr["文件名"] = ss[0];
                    dr["校验"] = Convert.ToDouble(ss[2]);
                    dr["选择题"] = ss[3];
                }
                _rundt.Rows.Add(dr);
            }

        }
        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 || _rundt == null || e.ColumnIndex == -1 || _angle==null || _artemplate==null )
                return;
            string fn = _rundt.Rows[e.RowIndex]["文件名"].ToString().Replace("LJH\\","LJH\\Correct\\");
            if (File.Exists(fn))
            {
                double angle = (double)(_rundt.Rows[e.RowIndex]["校验"]);
                Bitmap bmp =(Bitmap) Bitmap.FromFile(fn);
                DrawInfoBmp(bmp,angle);
                //pictureBox1.Image = bmp;
            }
        }
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            //if (m_act == Act.ZoomMouse)
            pictureBox1.Focus();
        }
        private void pictureBox1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (pictureBox1.Image == null) return;
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            double f = 0.0;
            if (numberOfTextLinesToMove > 0)
            {
                for (int i = 0; i < numberOfTextLinesToMove; i++)
                {
                    f += 0.05;
                }
                Zoomrat(f + 1, e.Location);
            }
            else if (numberOfTextLinesToMove < 0)
            {
                for (int i = 0; i > numberOfTextLinesToMove; i--)
                {
                    f -= 0.05;
                }
                Zoomrat(f + 1, e.Location);
            }
        }
        private void Zoomrat(double rat, Point e)
        {
            Bitmap bitmap_show = (Bitmap)pictureBox1.Image;
            Point L = pictureBox1.Location;
            Point S = panel1.AutoScrollPosition;
            int w = (int)(pictureBox1.Width * rat);
            int h = w * bitmap_show.Height / bitmap_show.Width;
            L.Offset((int)(e.X * (rat - 1)), (int)(e.Y * (rat - 1)));
            pictureBox1.SetBounds(S.X, S.Y, w, h);
            //zoombox.UpdateBoxScale(pictureBox1);
            S.Offset((int)(e.X * (1 - rat)), (int)(e.Y * (1 - rat)));
            panel1.Invalidate();
            panel1.AutoScrollPosition = new Point(-S.X, -S.Y);
        }
        private void DrawInfoBmp(Bitmap bmp, double angle)
        {
            if(_angle!=null)
                _angle.SetPaper(angle);           
            bmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Pen pen = Pens.Red;
                Brush dark = Brushes.Black;
                Brush white = Brushes.White;
                Brush Red = Brushes.Red;
                Font font = DefaultFont;
              
                foreach (string s in new string[] { "特征点", "考号","姓名", "选择题", "非选择题"})
                    if (_artemplate.Dic.ContainsKey(s))
                    {
                        int cnt = 0;
                        foreach (Area I in _artemplate.Dic[s])
                        {
                            g.DrawRectangle(pen, I.ImgArea );
                            if (I.HasSubArea())
                            {
                                foreach (Rectangle r in I.ImgSubArea())
                                {
                                    r.Offset(I.ImgArea.Location);
                                    g.DrawRectangle(pen,  r );
                                    r.Offset(-1, -1);
                                    g.DrawRectangle(pen,  r );
                                    r.Offset(2, 2);
                                    g.DrawRectangle(pen,  r );
                                }
                            }
                            if (I.NeedFill())
                            {
                                g.FillRectangle(I.FillPen(), I.ImgArea );
                                g.DrawString(cnt.ToString(), font, Red,  I.ImgArea .Location);
                            }
                        }
                    }
            }
            pictureBox1.Image = bmp;
        }       
        private void CreateTemplate(string filename, string templatefilename = ""){
        	Bitmap bmp = (Bitmap)Bitmap.FromFile(filename);
            MyDetectFeatureRectAngle dr = new MyDetectFeatureRectAngle(bmp);
            if (dr.Detected())
            {
            	if(_artemplate!=null)
            		_artemplate.Clear();
            	
                _artemplate = new ARTemplate.Template(filename, bmp, dr.CorrectRect);
                _angle = new AutoAngle(dr.ListPoint); //或者导入时 设置
                if(templatefilename!="" && File.Exists(templatefilename)){
                	_artemplate.Load(templatefilename);
                }
                
            	this.Hide();
                _artemplate.SetFeaturePoint(dr.ListFeatureRectangle, dr.CorrectRect);
                ARTemplate.FormTemplate f = new ARTemplate.FormTemplate(_artemplate);
                f.ShowDialog();
                this.Show();
            }
        }
       
        private void DetectAllImgs(MyDetectFeatureRectAngle dr, List<string> nameList)
        {
            FileInfo fi = new FileInfo(nameList[0]);
            string dir = fi.Directory.FullName.Replace("LJH\\", "LJH\\Correct\\");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
			
            _rundt = Tools.DataTableTools.ConstructDataTable(new string[]{
                    "序号","文件名","校验","姓名","考号","选择题"  });
            dgv.DataSource = _rundt;
            _rundr=dr;
            _runnameList = nameList;
            
            Thread thread=new Thread(new ThreadStart(RunDetectAllImg)); 
			thread.Start(); 
        }
        public void RunDetectAllImg(){
        	StringBuilder sb = new StringBuilder();
            foreach (string s in _runnameList)
            {
            	_runmsg = DetectAllImg(_rundr, s).ToString();
                sb.Append(_runmsg);
                this.Invoke( new MyInvoke(ShowMsg));
                Thread.Sleep(100);
            }
            File.WriteAllText("allimport.txt", sb.ToString());
        }
        public void ShowMsg(){
            string[] ss = _runmsg.Split(',');

            DataRow dr = _rundt.NewRow();
            dr["文件名"] = ss[0];
            dr["校验"] = Convert.ToDouble( ss[2] );
    		dr["选择题"] = ss[3];
    		dr["序号"]=_rundt.Rows.Count+1;
    		_rundt.Rows.Add(dr);
        }
        private StringBuilder DetectAllImg(MyDetectFeatureRectAngle dr, string s)
        {
            Bitmap bmp = (Bitmap)Bitmap.FromFile(s);
            string str = s.Substring(s.Length - 7, 3);
            //MyDetectFeatureRectAngle dr = new MyDetectFeatureRectAngle(bmp);
            Rectangle CorrectRect = dr.Detected(bmp);
            StringBuilder sb = new StringBuilder();
            sb.Append(s + "," +  CorrectRect.ToString("-") + ",");
            if (CorrectRect.Width > 0)
            {
                //TODO: debug r1 in 001
                Rectangle cr1 = new Rectangle(CorrectRect.Right - 60, CorrectRect.Top - 20, 80, 80);
                Rectangle r1 = dr.Detected(cr1, bmp);
                Rectangle cr2 = new Rectangle(CorrectRect.Left - 20, CorrectRect.Bottom - 60, 80, 80);
                Rectangle r2 = dr.Detected(cr2, bmp);

                
                sb.Append( _angle.SetPaper(CorrectRect.Location, r1.Location, r2.Location)+"," );
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

        public static List<string> NameListFromDir(string fidir,string ext =".tif" )
        {
            List<string> namelist = new List<string>();
            DirectoryInfo dirinfo = new DirectoryInfo(fidir);
            //string ext = fi.Extension;
            foreach (FileInfo f in dirinfo.GetFiles())
                if (f.Extension.ToLower() == ext)
                    namelist.Add(f.FullName);
            return namelist;
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

    }
    public class TInfo{
    	public TInfo(string s){
    		this.Str  = s;
    	}
    	public string Str;
    	public override string ToString()
		{
    		if(Str.Contains("\\"))
    		return Str.Substring( Str.LastIndexOf("\\")+1);
    		return Str;
		}
 
    }
}
