﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
using System.Resources;
using System.Xml.Linq; //xml
using System.Diagnostics;//file
using AForge.Video;
using Accord.Video.FFMPEG;
using System.Runtime.InteropServices;//DllImport

namespace MTS_Recorder
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
        }
        public void Setup()
        {
            // Tạo một ImageList để lưu trữ ảnh đại diện
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(100, 100); // Kích thước của ảnh đại diện

            // Tạo một ListView
            ListView listView = new ListView();
            listView.View = View.LargeIcon;
            listView.LargeImageList = imageList;

            // Đường dẫn đến video
            string videoPath = AdrVideo;

            // Tạo một đối tượng đại diện cho video
            VideoFileReader videoReader = new VideoFileReader();
            videoReader.Open(videoPath);

            // Lấy khung hình đầu tiên của video
            Bitmap firstFrame = videoReader.ReadVideoFrame();

            // Thêm ảnh đại diện vào ImageList
            imageList.Images.Add(firstFrame);

            // Tạo một ListViewItem với đường dẫn video
            ListViewItem item = new ListViewItem(videoPath, 0); // 0 là chỉ số ảnh đại diện trong ImageList

            // Thêm ListViewItem vào ListView
            lvVideo.Items.Add(item);

            // Đóng đối tượng đọc video
            videoReader.Close();
        }
        #region Khai báo thư viện
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        const int MYACTION_HOTKEY_ID = 1;
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        public int[] Key = new int[] { 2, 3 };
        #endregion


        #region Khởi tạo và hủy Hotkey
        private void _KhoiTaoPhimNong()
        {
            try
            {
                RegisterHotKey(this.Handle, Key[0], (int)KeyModifier.None, Keys.F8.GetHashCode());
                RegisterHotKey(this.Handle, Key[1], (int)KeyModifier.None, Keys.F9.GetHashCode());

            }
            catch (Exception)
            {

            }
        }

        private void _HuyPhimNong()
        {
            try
            {
                UnregisterHotKey(this.Handle, Key[0]);
                UnregisterHotKey(this.Handle, Key[1]);

            }
            catch (Exception)
            {

            }
        }
        #endregion


        #region Sử dụng Hotkey
        protected override void WndProc(ref Message m)
        {
            try
            {
                base.WndProc(ref m);

                if (m.Msg == 0x0312)
                {

                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);
                    int id = m.WParam.ToInt32();

                    switch (id)
                    {
                        case 2:
                            {
                                Recorder();
                                break;
                            }
                        case 3:
                            {
                                camerapic_Click(this, new EventArgs());
                                break;
                            }

                    }
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion


        #region vùng biến



        private Size s;
        Bitmap bt;

        VideoFileWriter writer = new VideoFileWriter();
        string AdrVideo;
        string AdrImg;


        private string NameApp = "MTS Recorder";
        private string NameVideo;
        private string NameImg;
        const byte FPS = 10;
        private bool flagRecorder = false;

        private string path;

        #endregion


        #region Xử lý quay và chụp
        private void ptrRec_Click(object sender, EventArgs e)//Nhấn nút rec
        {
            Recorder();

        }
        //Sự kiện nhất nút Stop
        private void ptrStop_Click(object sender, EventArgs e)
        {
            Recorder();
        }
        //nút Pause quay
        private void picPause_Click(object sender, EventArgs e)
        {
            picPause.Visible = false; //ẩn nút pause
            picContinue.Visible = true; //hiện nút tiếp tục
            timer1.Stop();
            timer3.Stop();
        }
        //Nút tiếp tục quay
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            picPause.Visible = true; //hiện nút pause
            picContinue.Visible = false; //ẩn nút tiếp tục
            timer1.Start();
            timer3.Start();

        }

        //Chụp ảnh màn
        private void TakeImage(string adr)
        {
            AllScreen().Save(adr, System.Drawing.Imaging.ImageFormat.Png);
        }
        //Sự kiện chụp màn hình
        private void camerapic_Click(object sender, EventArgs e)
        {
            //Tên Image
            NameImg = NameApp + "-Image-" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "-"
                 + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond
                 + @".jpg";

            try
            {
                if (Directory.Exists(AdrImg))//Kiểm tra thư mục lưu file tồn tại không
                {
                    AddImageXml(AdrImg, NameImg);

                    TakeImage(AdrImg + @"/" + NameImg);

                    loadXml(false);//load lại ds ảnh

                    DialogResult rs = MessageBox.Show("Screenshot successfully, see the image now?", "MTS Recorder", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (rs == DialogResult.Yes)
                    {
                        OpenFile(AdrImg + @"/" + NameImg);
                    }
                }
                else
                {
                    MessageBox.Show("The folder you want to save the photo does not exist, please select another folder!");
                }


            }
            catch (Exception ec)
            {

            }
        }
        //Sự kiện nhất nút Rec
        private void Recorder()
        {
            //Tên video ban đầu
            NameVideo = NameApp + "-Video-" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "-"
                 + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond
                 + @".mp4";
            string filename = AdrVideo + @"\" + NameVideo;

            if (flagRecorder == false) // Nếu chưa quay thì bắt đầu quay
            {


                if (Directory.Exists(AdrVideo))//Kiểm tra thư mục lưu file tồn tại không
                {
                    if (File.Exists(filename)) //File không tồn tại thì thêm mới
                    {
                        MessageBox.Show("The file already exists, please rename it differently in Settings!");
                    }
                    else
                    {


                        if (System.IO.File.Exists(filename))
                            System.IO.File.Delete(filename);


                        ptrRec.Visible = false; //Ẩn nút rec
                        ptrStop.Visible = true; //Hiện nút stop
                        picPause.Visible = true; //Hiện nút pause
                        flagRecorder = true;//đang quay

                        AddVideoXml(AdrVideo, NameVideo); // thêm video vào danh sách



                        writer.Open(filename, s.Width, s.Height, FPS, VideoCodec.MPEG4, 5000000);
                        timer1.Interval = 90;

                        timer1.Start();//ghi
                        timer3.Start(); //đếm


                    }

                }
                else
                {
                    MessageBox.Show("The folder you want to save the video does not exist, please select another folder!");
                }
            }
            else // nếu đang quay thì xử lý dừng

            {

                lbTime.Text = "00:00:00";
                ptrRec.Visible = true; //Hiện nút rec
                ptrStop.Visible = false; //Ẩn nút stop
                flagRecorder = flag;//dừng quay

                //đóng quay video
                writer.Close();


                picPause.Visible = false; //ẩn nút pause
                timer1.Stop();//ghi
                timer3.Stop(); //đếm

                loadXml(true);//load lại ds video

                //cập nhật thông số đếm
                i = 0; j = 0; k = 0; m = 0; n = 0; l = 0;
                labe1 = "0";
                labe2 = "0";
                labe3 = "0";
                labe4 = "0";
                labe5 = "0";
                labe6 = "0";


                DialogResult rs = MessageBox.Show("Video saved successfully, see the video now?", "MTS Recorder", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (rs == DialogResult.Yes)
                {
                    OpenFile(lvVideo.Items[lvVideo.Items.Count - 1].SubItems[1].Text + "\\" + lvVideo.Items[lvVideo.Items.Count - 1].Text);
                }
            }
        }
        //Timer1
        private void timer1_Tick(object sender, EventArgs e)
        {
            writer.WriteVideoFrame(AllScreen());

        }
        #endregion


        #region Khởi động
        //Hàm trả về 1 ảnh 
        private Bitmap AllScreen()
        {

            Graphics g = Graphics.FromImage(bt);

            g.CopyFromScreen(0, 0, 0, 0, s);

            return bt;
        }
        //Timer ghi hình vào home
        private void timer2_Tick(object sender, EventArgs e)
        {
            Screen.Image = AllScreen();
        }
        //Khởi động

        private void Form1_Load(object sender, EventArgs e)
        {
            // Đặt thuộc tính Dock và Anchor cho tất cả các control
            

            AdrVideo = Properties.Settings.Default.AdrVideo;
            AdrImg = Properties.Settings.Default.AdrImg;

            if (AdrVideo == "NULL" || AdrImg == "NULL") //mới cài <MTS Recorder>
            {
                Properties.Settings.Default.AdrVideo = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                Properties.Settings.Default.AdrImg = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                Properties.Settings.Default.Save();

                AdrVideo = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                AdrImg = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
            txtAdrVideo.Text = AdrVideo;
            txtAdrImg.Text = AdrImg;

            _KhoiTaoPhimNong();

            timer2.Start();

            float i = (int)System.Windows.SystemParameters.PrimaryScreenWidth;

            s = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);

            bt = new Bitmap(
              s.Width,
              s.Height,
              PixelFormat.Format32bppRgb);

            loadXml(true);//Load  list video

            loadXml(false);//Load list image



        }
        //Giao diện Chỉnh sửa nút tabcontrol
        private void tabControl1_DrawItem_2(object sender, DrawItemEventArgs e)
        {


            Graphics g = e.Graphics;

            Brush _textBrush;

            // Lấy vật phẩm từ bộ sưu tập.

            TabPage _tabPage = tabControl1.TabPages[e.Index];

            // Nhận giới hạn thực sự cho hình chữ nhật tab.


            Rectangle _tabBounds = tabControl1.GetTabRect(e.Index);

            SolidBrush color = new SolidBrush(Color.FromArgb(202, 229, 232));


            g.FillRectangle(color, e.Bounds);


            if (e.State == DrawItemState.Selected)

            {


                // Vẽ một màu nền khác và không vẽ hình chữ nhật tiêu điểm.

                _textBrush = new SolidBrush(Color.Black);

                color = new SolidBrush(Color.FromArgb(153, 209, 211));

                g.FillRectangle(color, e.Bounds);

            }

            else

            {

                _textBrush = new System.Drawing.SolidBrush(Color.White);


            }



            //Sử dụng phông chữ của riêng chúng tôi.

            Font _tabFont = new Font("Arial", (float)13.0);



            // Vẽ chuỗi. Căn giữa văn bản.

            StringFormat _stringFlags = new StringFormat();

            _stringFlags.Alignment = StringAlignment.Center;

            _stringFlags.LineAlignment = StringAlignment.Center;

            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));


            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            int radius = 10; // Điều chỉnh độ cong của góc
            int x = _tabBounds.X;
            int y = _tabBounds.Y;
            int width = _tabBounds.Width;
            int height = _tabBounds.Height;

            path.AddArc(x, y, radius, radius, 180, 90);
            path.AddArc(x + width - radius, y, radius, radius, 270, 90);
            path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
            path.AddArc(x, y + height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            // Vẽ hình chữ nhật với góc bo
            g.DrawPath(new Pen(color), path);

        }
        #endregion


        #region File và Folder

        //Nút chọn folder video khác
        private void btnSelectAdrVideo_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtAdrVideo.Text;

            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                AdrVideo = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.AdrVideo = AdrVideo;
                Properties.Settings.Default.Save();
                txtAdrVideo.Text = AdrVideo;
            }

        }

        //Nút chọn folder ảnh khác
        private void btnSelectAdrImg_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtAdrImg.Text;

            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                AdrImg = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.AdrImg = AdrImg;
                Properties.Settings.Default.Save();
                txtAdrImg.Text = AdrImg;


            }

        }
        //Nút Open folder video
        private void btnOpenFolderVideo_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AdrVideo);
            }
            catch
            {
                MessageBox.Show("The video folder does not exist or has been deleted, please select another folder again!");
            }
        }

        //Nút Open folder ảnh
        private void btnOpenFolderImage_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(AdrImg);
            }
            catch
            {
                MessageBox.Show("The image folder does not exist or has been deleted, please select another folder again!");
            }
        }

        //Thêm video vào XML
        private void AddVideoXml(string path, string name)
        {
            try
            {
                string pathvideo = "ListVideo.xml";
                XDocument XML = XDocument.Load(pathvideo);
                XElement newx =

                new XElement("Data",
                new XElement("Path", path)

                );

                newx.SetAttributeValue("ID", name);

                XML.Element("Datas").Add(newx);
                XML.Save(pathvideo);

            }
            catch (Exception err)
            {

            }
        }

        //Thêm ảnh vào XML
        private void AddImageXml(string path, string name)
        {
            try
            {
                string pathimage = "ListImage.xml";
                //if (!File.Exists(pathimage)) {
                    XDocument XML = XDocument.Load(pathimage);
                    XElement newx =
                         new XElement("Data",
                         new XElement("Path", path)
                    );
                    newx.SetAttributeValue("ID", name);

                    XML.Element("Datas").Add(newx);
                    XML.Save(pathimage);
                
               

            }
            catch (Exception err)
            {

            }
        }
        //Xóa list xml ảnh
        private void btnDeleteA_Click(object sender, EventArgs e)
        {
            if (lvImage.SelectedItems.Count > 0)
            {

                DeleteFile(lvImage.SelectedItems[0].SubItems[1].Text + "\\" + lvImage.SelectedItems[0].Text);
                DeleteXml(lvImage.SelectedItems[0].Text, false);
            }
            else
            {
                MessageBox.Show("Select a file to delete!");
            }


        }
        //Hoàm xóa file và xóa list xml
        private void DeleteXml(string ID, bool k) //k=true=> Video
        {

            if (k == true)
                path = "ListVideo.xml";
            else
                path = "ListImage.xml";

            try
            {

                XDocument testXML = XDocument.Load(path);
                XElement cStudent =
                    testXML.Descendants("Data").Where(c => c.Attribute("ID").Value.Equals(ID)).FirstOrDefault();

                cStudent.Remove();
                testXML.Save(path);
                loadXml(true); //load ds video
                loadXml(false); // load ds anh

            }
            catch (Exception err)
            {

            }
        }
        //Mo
        private void btnOpenA_Click(object sender, EventArgs e)
        {
            if (lvImage.SelectedItems.Count > 0)
            {


                OpenFile(lvImage.SelectedItems[0].SubItems[1].Text + "\\" + lvImage.SelectedItems[0].Text);
            }
            else
            {
                MessageBox.Show("Select a file to open!");
            }
        }

        private void btnDeleteVD_Click(object sender, EventArgs e)
        {

            if (lvVideo.SelectedItems.Count > 0)
            {
                DeleteFile(lvVideo.SelectedItems[0].SubItems[1].Text + "\\" + lvVideo.SelectedItems[0].Text);
                DeleteXml(lvVideo.SelectedItems[0].Text, true);
            }
            else
            {
                MessageBox.Show("Select a file to delete!");
            }


        }
        //Hàm mở file
        private void OpenFile(string file)
        {
            try
            {
                Process.Start(file);
            }
            catch
            {
                MessageBox.Show("File deleted!");
            }
        }
        //Hàm xóa file
        private void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    MessageBox.Show("File deleted!");
                }
            }
            else
            {
                MessageBox.Show("File deleted before!");
            }
        }
        //Load data
        private void loadXml(bool k) //k=true->video
        {
            if (k == true)
            {
                path = "ListVideo.xml";

                try
                {
                    lvVideo.Items.Clear();
                    DataSet dataSet = new DataSet(); //khởi tạo data xml 
                    dataSet.ReadXml(path);

                    DataTable dt = new DataTable();
                    dt = dataSet.Tables["Data"];

                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        lvVideo.Items.Add(dr["ID"].ToString());
                        lvVideo.Items[i].SubItems.Add(dr["Path"].ToString());
                        i++;
                    }

                }
                catch (Exception)
                {

                }

            }
            else
            {
                path = "ListImage.xml";

                try
                {
                    lvImage.Items.Clear();
                    DataSet dataSet = new DataSet();
                    dataSet.ReadXml(path);
                    DataTable dt = new DataTable();
                    dt = dataSet.Tables["Data"];
                    int i = 0;
                    foreach (DataRow dr in dt.Rows)
                    {
                        lvImage.Items.Add(dr["ID"].ToString());
                        lvImage.Items[i].SubItems.Add(dr["Path"].ToString());
                        i++;
                    }
                }
                catch (Exception)
                {

                }
            }


        }

        //Open File
        private void button5_Click(object sender, EventArgs e)
        {


            if (lvVideo.SelectedItems.Count > 0)
            {


                OpenFile(lvVideo.SelectedItems[0].SubItems[1].Text + "\\" + lvVideo.SelectedItems[0].Text);
            }
            else
            {
                MessageBox.Show("Select a file to open!");
            }


        }
        #endregion


        #region Chức năng phụ
        //Ẩn ứng dụng xuống icon thông báo
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(500, "Hidden MTS Recorder!", "F8 Start/Stop recording\r\nF9 Screenshot", ToolTipIcon.Info);
            this.Hide();
        }
        //Thoát ứng dụng
        private void button1_Click(object sender, EventArgs e)
        {
            _HuyPhimNong();
            Application.Exit();
        }
               private void pictureBox3_Click(object sender, EventArgs e)
        {
            _HuyPhimNong();
            Application.Exit();
        }
        //Ẩn xuống Minimized
        private void button2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        //Icon ẩn
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) //double vào icon ẩn
        {
            notifyIcon1.Visible = false;
            this.Show();
            WindowState = FormWindowState.Normal;
        }


        //Thanh công cụ cho icon ẩn
        private void openToolStripMenuItem_Click(object sender, EventArgs e) //nút mở trong icon
        {
            notifyIcon1.Visible = false;
            this.Show();
            WindowState = FormWindowState.Normal;
        }
        //chọn Thoát
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //Chọn quay
        private void recordToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Recorder();

            contextMenuStrip1.Items[1].Visible = false;
            contextMenuStrip1.Items[2].Visible = true;
        }
        //Chọn dừng quay
        private void StopRecordingVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Recorder();

            contextMenuStrip1.Items[1].Visible = true;
            contextMenuStrip1.Items[2].Visible = false;
        }
        //Chọn chụp màn hình
        private void captureImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            camerapic_Click(this, new EventArgs());
        }

        //Kéo khung
        //x y vị trí nhấn chuột. flag kiểm tra đang nhấn chuột
        bool flag = false;
        int x, y;
        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (flag == true)
            {
                this.SetDesktopLocation(Cursor.Position.X - x, Cursor.Position.Y - y);
            }
        }
        //Nhấn chuột vào khung kéo
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            flag = true;
            x = e.X;
            y = e.Y;

        }
        //Nhấc chuột
        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            flag = false;
        }
        //Chạy bộ đếm thời gian 
        int i = 0, j = 0, k = 0, m = 0, n = 0, l = 0;
        string labe1 = "0";
        string labe2 = "0";
        string labe3 = "0";

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void lbTime_Click(object sender, EventArgs e)
        {

        }

        private void pnrectop_Paint(object sender, PaintEventArgs e)
        {

        }

        string labe4 = "0";
        string labe5 = "0";
        string labe6 = "0";

        private void tgian(ref int t1, ref int t2, ref string lb1, ref string lb2)
        {
            t1 = 0;
            t2++;
            lb1 = t2.ToString();
            lb2 = t1.ToString();
        }
        private string tgian1()
        {
            string x = "";
            i = i + 1;
            labe6 = i.ToString();
            if (i == 10)
            {
                tgian(ref i, ref j, ref labe5, ref labe6);

            }
            if (j == 6)
            {
                tgian(ref j, ref k, ref labe4, ref labe5);

            }

            if (k == 10)
            {
                tgian(ref k, ref m, ref labe3, ref labe4);

            }
            if (m == 6)
            {
                tgian(ref m, ref n, ref labe2, ref labe3);

            }
            if (n == 10)
            {

                tgian(ref n, ref l, ref labe1, ref labe2);

            }
            x = labe1 + labe2 + ":" + labe3 + labe4 + ":" + labe5 + labe6;
            return x;
        }
        //Timer3 chạy bộ đếm
        private void timer3_Tick(object sender, EventArgs e)
        {
            lbTime.Text = tgian1();
        }
        #endregion


    }
}
