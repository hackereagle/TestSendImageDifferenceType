using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace RORZE
{
    class StateObject
    {
        // Size of receive buffer.  
        public static readonly int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
        // Received data string.  
        public List<byte[]> RawData = new List<byte[]>();
        public int TargetLength = 0;
        public int CurrentLength = 0;
    }
}
