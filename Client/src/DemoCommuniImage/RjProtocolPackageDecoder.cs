using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RORZE
{
    class RjProtocolPackageDecoder
    {
        private string mUnitId;
        private string mCmd;
        private string mData;
        public RjProtocolPackageDecoder(string package)
        {
            mUnitId = package.Substring(1, 4);
            mCmd = package.Substring(6, 4);

            int splitSymbolPos = package.IndexOf(':');
            mData = package.Substring(splitSymbolPos + 1);
        }

        public string UnitId
        {
            get { return mUnitId; }
        }

        public string Command
        {
            get { return mCmd; }
        }

        public string Data
        {
            get { return mData; }
        }
    }
}
