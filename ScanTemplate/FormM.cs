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
                TLP.ColumnStyles[2].Width =20.0F;

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

			string txt = listBox1.SelectedItem.ToString();
			txt = txt.Substring( txt.LastIndexOf("\\")+1);
			bool unselected = true;
			for(int i=0; i<listBoxTemplate.Items.Count; i++){
				if(listBoxTemplate.Items[i].ToString().StartsWith(txt)){
					listBoxTemplate.SelectedIndex = i;
					unselected = false;
					break;
				}
			}
			if(unselected)
				listBoxTemplate.SelectedIndex = -1;
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
                    "序号","文件名","姓名","考号","选择题"  });
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
        	string s = _runmsg.Substring(0,_runmsg.IndexOf(","));
        	string ns = _runmsg.Substring( _runmsg.IndexOf("},")+1);

    		DataRow dr = _rundt.NewRow();
    		dr["文件名"]  = s;
    		dr["选择题"] = ns;
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
            sb.Append(s + "," +  CorrectRect.ToString() + ",");
            if (CorrectRect.Width > 0)
            {
                //TODO: debug r1 in 001
                Rectangle cr1 = new Rectangle(CorrectRect.Right - 60, CorrectRect.Top - 20, 80, 80);
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
