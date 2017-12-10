using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    public class LibRSAHelper
    {
        public static string Decrypt(string pwd)
        {
            string filePath = Path.Combine(EnvProvider.Default.MainPath, "pri.key");
            if (File.Exists(filePath))
            {
                return new LibRSACrypto(File.ReadAllText(filePath)).Decrypt(pwd);
            }
            return pwd;
        }

        public static string Encrypt(string pwd)
        {
            string filePath = Path.Combine(EnvProvider.Default.MainPath, "pub.key");
            if (File.Exists(filePath))
            {
                return new LibRSACrypto(null, File.ReadAllText(filePath)).Encrypt(pwd);
            }
            return pwd;
        }
    }
}
