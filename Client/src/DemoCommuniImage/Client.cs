using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RORZE
{
    class Client
    {
        public delegate void delReplyCommand(string cmd, byte[] data);
        public event delReplyCommand ReplyCommandEvent; 
        // 20210701
        public delegate void delReplyRjCommad(string cmd, string data);
        public event delReplyRjCommad ReplyRjCommandEvent; 

        private System.Net.Sockets.Socket mSocket;
        public Client()
        {
            mSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
        }

        public bool ConnectServer(string ip, string port)
        {
            bool connectResult = false;
            try
            {
                mSocket.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), Convert.ToInt32(port)));
                Console.WriteLine("Socket connected to -> {0}", mSocket.RemoteEndPoint.ToString());

                //System.Threading.Thread receiveDataThread = new System.Threading.Thread(ReceiveData);
                //receiveDataThread.Start();
                System.Threading.Thread receiveDataThread = new System.Threading.Thread(ReceiveDataWithRjProtocol);
                receiveDataThread.Start();

                connectResult = true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Occuring problem when connect server: " + ex.Message + "\n" + ex.StackTrace);
                connectResult = false;
            }

            return connectResult;
        }

        // 20210701
        //private void ReceiveData()
        //{
        //    try
        //    {
        //        while (mSocket.Connected)
        //        {
        //            int DataLen = 0;
        //            byte[] buffer = new byte[4];

        //            DataLen = mSocket.Receive(buffer, System.Net.Sockets.SocketFlags.Peek);
        //            int len = BitConverter.ToInt32(buffer, 0);
        //            if (len > 0)
        //            {
        //                byte[] package = new byte[len];

        //                int size = len;
        //                var total = 0;
        //                var dataleft = size;
        //                while (total < size)
        //                { 
        //                    var recv = mSocket.Receive(package, total, dataleft, System.Net.Sockets.SocketFlags.None);
        //                    if (recv == 0)
        //                    {
        //                        break;
        //                    }
        //                    total += recv;
        //                    dataleft -= recv;
        //                }

        //                PackageDecoder packageDecoder = new PackageDecoder(package);
        //                Console.WriteLine($"receive command {packageDecoder.Command}");

        //                // The function ReplyCommandEvent include send data after doing action from server command.
        //                if (ReplyCommandEvent != null)
        //                    ReplyCommandEvent(packageDecoder.Command, packageDecoder.Data);
        //            }
        //        }

        //        System.Windows.Forms.MessageBox.Show("Connection was broken.");
        //        //string ip = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
        //        //string port = System.Configuration.ConfigurationManager.AppSettings["ServerPort"];
        //        //while (!ConnectServer(ip, port) && !mSocket.Connected)
        //        //    System.Threading.Thread.Sleep(1000);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Occuring problem when ReceiveData: " + ex.Message + "\n" + ex.StackTrace);
        //        System.Windows.Forms.MessageBox.Show("Connection was broken.");

        //    }
        //}

        private void RecordDifferenceBetweenByteArrAndString(byte[] rawData)
        {
            string str = System.Text.Encoding.ASCII.GetString(rawData);
            byte[] afterConverted = System.Text.Encoding.ASCII.GetBytes(str);

            System.IO.StreamWriter sw = new System.IO.StreamWriter("CompareRawDataAndConvertedData.csv", false);
            int len = rawData.Length;
            if (afterConverted.Length != len)
            {
                sw.WriteLine("Data do not have same length!");
                if (afterConverted.Length > len)
                {
                    sw.WriteLine("Used rawData length! afterTrans is too long.");
                }
                else
                { 
                    sw.WriteLine("Used afterTrans length! rawData is too long.");
                    len = afterConverted.Length;
                }
            }

            sw.WriteLine($"raw data, converted data, difference");
            for (int i = 0; i < len; i++)
            {
                sw.WriteLine($"{rawData[i]},{afterConverted[i]},{afterConverted[i] - rawData[i]}");
            }
        }

        private void ReceiveDataWithRjProtocol()
        {
            try
            {
                while (mSocket.Connected)
                {
                    if (HaveMsg())
                    { 
                        var package = new List<byte>();

                        while (mSocket.Available > 0)
                        { 
                            byte[] buffer = new byte[1];
                            var recv = mSocket.Receive(buffer, buffer.Length, System.Net.Sockets.SocketFlags.None);

                            if(recv == 1)
                                package.Add(buffer[0]);
                        }
                        Console.WriteLine($"\nPackage length = {package.Count}");
                        //RecordDifferenceBetweenByteArrAndString(package.ToArray());

                        //var msg = System.Text.Encoding.Default.GetString(package.ToArray());
                        var msg = System.Text.Encoding.ASCII.GetString(package.ToArray());
                        RjProtocolPackageDecoder packageDecoder = new RjProtocolPackageDecoder(msg);
                        Console.WriteLine($"receive command {packageDecoder.Command}, total package length = {msg.Length}");

                        if (ReplyRjCommandEvent != null)
                            ReplyRjCommandEvent(packageDecoder.Command, packageDecoder.Data);
                        package.Clear();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Occuring problem when ReceiveData: " + ex.Message + "\n" + ex.StackTrace);
                System.Windows.Forms.MessageBox.Show("Connection was broken.");

            }
        }
        private bool HaveMsg()
        {
            byte[] buffer = new byte[10];
            var len = mSocket.Receive(buffer, buffer.Length, System.Net.Sockets.SocketFlags.Peek);
            if (len > 0)
                return true;
            else
                return false;
        }

        public void SendData(byte[] msg)
        {
            mSocket.Send(msg);
        }
    }
}
