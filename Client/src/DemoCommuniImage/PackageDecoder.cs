using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RORZE
{
    class PackageDecoder
    {
        private byte[] mOrgPackage = null;
        private string mCmd;
        private byte[] mData = null;
        public PackageDecoder(byte[] package)
        {
            int len = package.Length;
            this.mOrgPackage = new byte[len];
            Array.Copy(package, mOrgPackage, len);

            int index = 4;
            byte[] cmd = new byte[4];
            Array.Copy(mOrgPackage, index, cmd, 0, cmd.Length);
            mCmd = System.Text.Encoding.Default.GetString(cmd);
            index += 4;

            mData = new byte[len - index];
            Array.Copy(mOrgPackage, index, mData, 0, mData.Length);
        }

        public byte[] CompletePackage
        {
            get { return mOrgPackage; }
        }

        public string Command
        {
            get { return mCmd; }
        }

        public byte[] Data
        {
            get { return mData; }
        }
    }
}
