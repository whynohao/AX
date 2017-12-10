using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Utils
{
    public static class LibStringBuilder
    {
        public const char Quto = '\'';
        public static string GetQuotString(string str)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Quto);
            if (str != null)
            {
                int length = str.Length;
                for (int i = 0; i < length; i++)
                {
                    if (str[i] == Quto)
                        builder.Append(Quto);
                    builder.Append(str[i]);
                }
            }
            builder.Append(Quto);
            return builder.ToString();
        }

        public static string GetQuotObject(object value)
        {
            string str = LibSysUtils.ToString(value);
            StringBuilder builder = new StringBuilder();
            builder.Append(Quto);
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                if (str[i] == Quto)
                    builder.Append(Quto);
                builder.Append(str[i]);
            }
            builder.Append(Quto);
            return builder.ToString();
        }

        public static string JoinStringList(List<string> list, string joinStr)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (i == 0)
                    builder.AppendFormat("{0}", item);
                else
                    builder.AppendFormat(" {0} {1}", joinStr, item);
                i++;
            }
            return builder.ToString();
        }

    }
}
