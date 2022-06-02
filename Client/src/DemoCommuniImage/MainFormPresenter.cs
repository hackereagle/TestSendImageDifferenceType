using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RORZE
{
    class MainFormPresenter
    {
        public bool IsInfiniteTest { set; get; }
        private MainForm mMainForm = null;
        //private Client mClient = null;
        private ClientAysncReceive mClient = null;
        private RecordTestingResult mRecorder = null;
        public MainFormPresenter(MainForm ui)
        {
            mMainForm = ui;
            mMainForm.Connect = new MainForm.delConnect(ConnectServer);
            mMainForm.SendGrabImage = new MainForm.delSendGrabImage(SendGrabImgCmd);

            mClient = new ClientAysncReceive();
            //mClient.ReplyRjCommandEvent += new ClientAysncReceive.delReplyRjCommad(ReceiveServerCommand);
            mClient.ReplyWithRawDataEvent += new ClientAysncReceive.delReplyWithRawData(ReceiveServerRawData);

            IsInfiniteTest = false;
            mRecorder = new RecordTestingResult();
        }

        public bool Initialize()
        {
            ConnectServer();

            return true;
        }

        private void ConnectServer()
        {
            string ip = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
            string port = System.Configuration.ConfigurationManager.AppSettings["ServerPort"];
            mClient.ConnectServer(ip, port);
        }

        private void SendGrabImgCmd()
        {
            byte[] cmd = System.Text.Encoding.Default.GetBytes("GRAB");
            int len = 4 + cmd.Length;
            byte[] msg = new byte[len];
            Array.Copy(BitConverter.GetBytes(len), 0, msg, 0, 4);
            Array.Copy(cmd, 0, msg, 4, 4);
            mRecorder.SetStartRecordPoint();
            mClient.SendData(msg);
        }

        private System.Drawing.Bitmap SaveByteArr2Bitmap(byte[] data, int width, int height)
        {
            System.Drawing.Bitmap img = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            System.Drawing.Imaging.ColorPalette pal = img.Palette;
            for (int i = 0; i < 256; i++)
                pal.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);

            img.Palette = pal;
            System.Drawing.Imaging.BitmapData bitmapData = img.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            unsafe
            {
                fixed (byte* dataPtr = &data[0])
                {
                    byte* imgPtr = (byte*)bitmapData.Scan0;
                    int size = width * height;
                    for (int i = 0; i < size; i++)
                        *(imgPtr + i) = *(dataPtr + i);
                }
            }
            img.UnlockBits(bitmapData);

            return img;
        }

        private void ReceiveServerCommand(string cmd, string data)
        {
            //Console.WriteLine($"command:{cmd}, data length = {data.Length}");
            //int firstSplitter = data.IndexOf(',');
            //int secondSplitter = data.Substring(firstSplitter + 1).IndexOf(',');

            //int width = Convert.ToInt32(data.Substring(0, firstSplitter));
            //int height = Convert.ToInt32(data.Substring(firstSplitter + 1, secondSplitter));
            //byte[] imgData = System.Text.Encoding.ASCII.GetBytes(data.Substring(firstSplitter + 1 + secondSplitter + 1));
            //Console.WriteLine($"Image data length = {imgData.Length}");

            //System.Drawing.Bitmap img = SaveByteArr2Bitmap(imgData, width, height);
            //img.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
        }

        private void ReceiveServerRawData(byte[] rawData)
        {
            Console.WriteLine($"Receive {rawData.Length} byte raw data.");

            int total = BitConverter.ToInt32(rawData, 0);
            byte type = rawData[4];
            int width = BitConverter.ToInt32(rawData, 5);
            int height = BitConverter.ToInt32(rawData, 9);
            int channels = BitConverter.ToInt32(rawData, 13);
            // get image data
            byte[] imgData = new byte[width * height];
            Array.Copy(rawData, 17, imgData, 0, width * height);

            System.Drawing.Bitmap img = SaveByteArr2Bitmap(imgData, width, height);
            img.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
            mMainForm.DisplayImg(img);

            HalconDotNet.HImage himg = ConvertByteArr2HImage(imgData, width, height);
            mMainForm.DisplayImgInHalconSmartWindow(himg);
        }

        private HalconDotNet.HImage ConvertByteArr2HImage(byte[] data, int width, int height)
        {
            HalconDotNet.HImage img;

            System.Runtime.InteropServices.GCHandle mngImgData = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
            IntPtr imgData = mngImgData.AddrOfPinnedObject();
            img = new HalconDotNet.HImage("byte", width, height, imgData);

            return img;
        }

        public void Send2Server(byte[] msg)
        {
            mClient.SendData(msg);
        }
    }
}
