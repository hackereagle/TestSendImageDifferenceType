using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

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
                //byte[] tempData = new byte[bytesRead];
                //Array.Copy(state.buffer, tempData, bytesRead);
                //mRawData.Add(tempData); // 20210703

                //if (server.Available == 0)
                if (true)
                {
                    //content = state.sb.ToString();
                    //RjProtocolPackageDecoder packageDecoder = new RjProtocolPackageDecoder(content);
                    //Console.WriteLine($"receive command {packageDecoder.Command}, total package length = {content.Length}");

                    //if (ReplyRjCommandEvent != null)
                    //    ReplyRjCommandEvent(packageDecoder.Command, packageDecoder.Data);
                    //state.sb.Clear();

                    //if (ReplyWithRawDataEvent != null)
                    //{
                    //    byte[] rawData = MarshalRawDataList(mRawData);
                    //    ReplyWithRawDataEvent(rawData);
                    //}
                    //mRawData.Clear();

                    // There  might be more data, so store the data received so far.  
                    //string receiveData = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    byte[] tempData = new byte[bytesRead];
                    Array.Copy(state.buffer, tempData, bytesRead);
                    state.RawData.Add(tempData);
                    state.CurrentLength += bytesRead;

                    //if (handler.Available == 0)
                    if (true)
                    {
                        //string receiveData = state.sb.ToString();
                        //if (this.mIsGrabbing)
                        if (true)
                        {
                            int totalLen = BitConverter.ToInt32(state.RawData[0], 0);
                            state.TargetLength = totalLen;
                            if (state.TargetLength <= state.CurrentLength)
                            {
                                IPEndPoint point = server.RemoteEndPoint as IPEndPoint;
                                string clientIPAndPort = point.Address.ToString() + "/" + point.Port.ToString();
                                //將IP與接收到的資料傳給present
                                byte[] rawData = MarshalRawDataList(state.RawData);
                                while (rawData.Length >= state.TargetLength)
                                {
                                    byte[] dataSp0 = new byte[state.TargetLength];
                                    Array.Copy(rawData, 0, dataSp0, 0, state.TargetLength);
                                    //BackgroundLogger.AsyncWrite(LogType.Socket, $"=> {clientIPAndPort}, data length = {receiveData.Length}, raw data = {rawData.Length}, package num = {state.RawData.Count}, data = {receiveData}");
                                    //BackgroundLogger.AsyncWrite(LogType.Socket, $"image data => {clientIPAndPort}, data length = {receiveData.Length}, raw data = {rawData.Length}, package num = {state.RawData.Count}, data = ");
                                    BackgroundLogger.AsyncWrite(LogType.Socket, $"image data => {clientIPAndPort}, raw data = {rawData.Length}, package num = {state.RawData.Count}, data = ");
                                    if (ReplyWithRawDataEvent != null)
                                        ReplyWithRawDataEvent.Invoke(rawData);
                                    state.sb.Clear();
                                    state.RawData.Clear();

                                    int remainBytes = rawData.Length - state.TargetLength;
                                    if (remainBytes > 0)
                                    {
                                        byte[] dataSp1 = new byte[remainBytes];
                                        Array.Copy(rawData, state.TargetLength, dataSp1, 0, remainBytes);

                                        if (dataSp1.Length < 4)
                                        {
                                            state.RawData.Add(dataSp1);
                                            state.CurrentLength = dataSp1.Length;
                                            break;
                                        }
                                        else
                                        {
                                            totalLen = BitConverter.ToInt32(dataSp1, 0);
                                            if (dataSp1.Length >= totalLen)
                                            {
                                                //continue for decoding next image...
                                                rawData = new byte[dataSp1.Length];
                                                Array.Copy(dataSp1, 0, rawData, 0, dataSp1.Length);
                                            }
                                            else
                                            {
                                                state.RawData.Add(dataSp1);
                                                state.CurrentLength = dataSp1.Length;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        state.TargetLength = 0;
                                        state.CurrentLength = 0;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            string receiveData = state.sb.ToString();
                            receiveData = state.sb.ToString();
                            IPEndPoint point = server.RemoteEndPoint as IPEndPoint;
                            string clientIPAndPort = point.Address.ToString() + "/" + point.Port.ToString();

                            //將IP與接收到的資料傳給present
                            byte[] rawData = MarshalRawDataList(state.RawData);
                            BackgroundLogger.AsyncWrite(LogType.Socket, $"=> {clientIPAndPort}, data length = {receiveData.Length}, raw data = {rawData.Length}, package num = {state.RawData.Count}, data = {receiveData}");
                            if (ReplyWithRawDataEvent != null)
                                ReplyWithRawDataEvent.Invoke(rawData);

                            state.sb.Clear();
                            state.RawData.Clear();
                            state.TargetLength = 0;
                            state.CurrentLength = 0;
                        }
                    }

                    //循環接收資料
                    server.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }

            //server.BeginReceive(state.buffer, 0,
            //                    StateObject.BufferSize, 0,
            //                    new AsyncCallback(ReceiveCallback),
            //                    state);
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
