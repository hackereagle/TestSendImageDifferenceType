using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RORZE
{
    public partial class MainForm : Form
    {
        #region Delegate Or Events Set From Presenter
        public delegate void delConnect();
        public delConnect Connect;
        public delegate void delSendGrabImage();
        public delSendGrabImage SendGrabImage;
        #endregion Delegate Or Events Set From Presenter
        private MainFormPresenter mPresenter = null;
        public MainForm()
        {
            InitializeComponent();

            this.hswDisplay.MouseWheel += HalconSmartWindowMouseWheel;

            mPresenter = new MainFormPresenter(this);
            mPresenter.Initialize();
        }

        private void HalconSmartWindowMouseWheel(object sender, MouseEventArgs e)
        {
            System.Drawing.Point pt = this.Location;
            int leftBorder = hswDisplay.Location.X;
            int rightBorder = hswDisplay.Location.X + hswDisplay.Size.Width;
            int topBorder = hswDisplay.Location.Y;
            int bottomBorder = hswDisplay.Location.Y + hswDisplay.Size.Height;
            if (e.X > leftBorder && e.X < rightBorder && e.Y > topBorder && e.Y < bottomBorder)
            {
                MouseEventArgs newe = new MouseEventArgs(e.Button, e.Clicks,
                                                     e.X - pt.X, e.Y - pt.Y, e.Delta);
                hswDisplay.HSmartWindowControl_MouseWheel(sender, newe);
            }
        }


        private System.Drawing.Bitmap DeepCopyBitmap(System.Drawing.Bitmap srcImg)
        { 
            try
            {
                System.Drawing.Bitmap dstBitmap = null; 
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    bf.Serialize(ms, srcImg);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    dstBitmap = (System.Drawing.Bitmap)bf.Deserialize(ms);
                }

                return dstBitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: Functions.FunctionTools.DeepCopyBitmap, \r\n" + ex.ToString());

                return null;
            }
        }
        public void DisplayImg(System.Drawing.Bitmap img)
        {
            MethodInvoker dispImg = delegate {
                this.picDisp.Image = DeepCopyBitmap(img);
            };

            if (this.picDisp.InvokeRequired)
            {
                this.picDisp.Invoke(dispImg);
            }
            else
            {
                dispImg();
            }
        }

        public void DisplayImgInHalconSmartWindow(HalconDotNet.HImage img)
        { 
            MethodInvoker dispImg = delegate {
                this.hswDisplay.HalconWindow.DispObj(img);
            };

            if (this.picDisp.InvokeRequired)
            {
                this.picDisp.Invoke(dispImg);
            }
            else
            {
                dispImg();
            }
        }

        public void ShowElapseTime(long elapseTime)
        { 
            MethodInvoker showElapseTime = delegate
            {
                this.lblTime.Text = "Elapse Time: " + elapseTime.ToString() + " ms";
            };

            if (this.lblTime.InvokeRequired)
            {
                this.lblTime.Invoke(showElapseTime);
            }
            else
            {
                showElapseTime();
            }
        }

        public void ShowInformation(string information)
        { 
            MethodInvoker showElapseTime = delegate
            {
                this.lblTime.Text = "Information: " + information;
            };

            if (this.lblTime.InvokeRequired)
            {
                this.lblTime.Invoke(showElapseTime);
            }
            else
            {
                showElapseTime();
            }
        }

        private void btnGrabImg_Click(object sender, EventArgs e)
        {
            SendGrabImage();
        }

        private void btnInfinitelyLoop_Click(object sender, EventArgs e)
        {
            if (mPresenter.IsInfiniteTest == false)
            {
                mPresenter.IsInfiniteTest = true;
                SendGrabImage();
            }
            else
            {
                mPresenter.IsInfiniteTest = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect();
        }

    }
}
