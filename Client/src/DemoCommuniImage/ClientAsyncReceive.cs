using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RORZE
{
    class ClientAysncReceive
    {
        public delegate void delReplyCommand(string cmd, byte[] data);
        public event delReplyCommand ReplyCommandEvent; 
        // 20210701
        public delegate void delReplyRjCommad(string cmd, string data);
        public event delReplyRjCommad ReplyRjCommandEvent;
        // 20210703
        public delegate void delReplyWithRawData(byte[] rawData);
        public event delReplyWithRawData ReplyWithRawDataEvent;

        private System.Net.Sockets.Socket mSocket;
        private List<byte[]> mRawData; // 20210703
        public ClientAysncReceive()
        {
            mSocket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            mRawData = new List<byte[]>(); // 20210703
        }

        public bool ConnectServer(string ip, string port)
        {
            bool connectResult = false;
            try
            {
                mSocket.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ip), Convert.ToInt32(port)));
                Console.WriteLine("Socket connected to -> {0}", mSocket.RemoteEndPoint.ToString());

                // Begin receiveing data
                StateObject state = new StateObject();
                state.workSocket = mSocket;
                mSocket.BeginReceive(state.buffer, 0,
                                     StateObject.BufferSize, 0,
                                     new AsyncCallback(ReceiveCallback),
                                     state);
                connectResult = true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Occuring problem when connect server: " + ex.Message + "\n" + ex.StackTrace);
                connectResult = false;
            }

            return connectResult;
        }

        protected virtual void ReceiveCallback(IAsyncResult ar)
        {
            string content = string.Empty;
            StateObject state = ar.AsyncState as StateObject;
            System.Net.Sockets.Socket server = state.workSocket;

            int bytesRead = server.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer,
                                                         0,
                                                         bytesRead));
                byte[] tempData = new byte[bytesRead];
                Array.Copy(state.buffer, tempData, bytesRead);
                mRawData.Add(tempData); // 20210703
                if (server.Available == 0)
                {
                    content = state.sb.ToString();
                    RjProtocolPackageDecoder packageDecoder = new RjProtocolPackageDecoder(content);
                    Console.WriteLine($"receive command {packageDecoder.Command}, total package length = {content.Length}");

                    if (ReplyRjCommandEvent != null)
                        ReplyRjCommandEvent(packageDecoder.Command, packageDecoder.Data);
                    state.sb.Clear();

                    if (ReplyWithRawDataEvent != null)
                    {
                        byte[] rawData = MarshalRawDataList(mRawData);
                        ReplyWithRawDataEvent(rawData);
                    }
                    mRawData.Clear();
                }
            }

            server.BeginReceive(state.buffer, 0,
                                StateObject.BufferSize, 0,
                                new AsyncCallback(ReceiveCallback),
                                state);
        }

        private byte[] MarshalRawDataList(List<byte[]> rawDataList)
        {
            int len = CountRawDataTotalLength(rawDataList);
            byte[] rawData = new byte[len];

            int index = 0;
            foreach (var raw in rawDataList)
            {
                int rawLen = raw.Length;
                Array.Copy(raw, 0, rawData, index, rawLen);
                index = index + rawLen;
            }

            return rawData;
        }

        private int CountRawDataTotalLength(List<byte[]> rawDataList)
        {
            int len = 0;
            foreach (var raw in rawDataList)
            {
                len = len + raw.Length;
            }
            return len;
        }

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

        public void SendData(byte[] msg)
        {
            mSocket.Send(msg);
        }
    }
}
