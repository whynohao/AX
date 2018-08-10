using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Runtime
{
    public class ProgIdConfigListingManager
    {
        public static readonly string BcfFileName = "BcfConfig.bin";

        public static void BuildListing(string mainPath, string extendPath, Dictionary<string, Assembly> assemblyDic)
        {
            string bcfPath = Path.Combine(mainPath, "Bcf");
            string extendBcfPath = Path.Combine(extendPath, "Bcf");
            BuildProgIdListing(mainPath, bcfPath, extendBcfPath, BcfFileName, assemblyDic);
        }

        private static void BuildProgIdListing(string mainPath, string bcfPath, string extendBcfPath, string fileName, Dictionary<string, Assembly> assemblyDic)
        {
            LibBinaryFormatter formatter = new LibBinaryFormatter();
            if (Directory.Exists(bcfPath))
            {
                ProgIdConfigListing progIdListing = BuildProgId(bcfPath, extendBcfPath, assemblyDic);
                using (FileStream fs = new FileStream(Path.Combine(mainPath, "Runtime", fileName), FileMode.Create))
                {
                    formatter.Serialize(fs, progIdListing);
                }
            }
        }

        public static ProgIdConfigListing GetProgIdListing(string mainPath)
        {
            ProgIdConfigListing progIdListing = null;
            LibBinaryFormatter formatter = new LibBinaryFormatter();
            string filePath = Path.Combine(mainPath, "Runtime", BcfFileName);
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,FileShare.Read))
                {
                    progIdListing = (ProgIdConfigListing)formatter.Deserialize(fs);
                }
            }
            return progIdListing;
        }

        public static Dictionary<string, ProgIdConfigListing> BrowseListing(string mainPath)
        {
            Dictionary<string, ProgIdConfigListing> listingDic = new Dictionary<string, ProgIdConfigListing>();
            ProgIdConfigListing progIdBcfListing = GetProgIdListing(mainPath);
            listingDic.Add("bcf", progIdBcfListing);
            return listingDic;
        }

        private static ProgIdConfigListing BuildProgId(string bcfPath, string extendBcfPath, Dictionary<string, Assembly> assemblyDic)
        {
            ProgIdConfigListing progIdListing = new ProgIdConfigListing();
            //处理标准业务模块
            IEnumerator<string> enumerator = Directory.EnumerateFiles(bcfPath, "*.dll", SearchOption.AllDirectories).GetEnumerator();
            List<string> standardList = new List<string>();
            while (enumerator.MoveNext())
            {
                string filePath = enumerator.Current;
                if (filePath.Contains("_Axce"))
                    continue;
                standardList.Add(filePath);
            }
            BuildProgIdCore(progIdListing, standardList, assemblyDic);
            //处理二开扩展业务模块
            DirectoryInfo dirInfo = new DirectoryInfo(extendBcfPath);
            FileInfo[] fileInfo = dirInfo.GetFiles("*.dll", SearchOption.AllDirectories);
            List<string> extendList = new List<string>();
            if (fileInfo != null && fileInfo.Length > 0)
            {
                foreach (FileInfo file in fileInfo)
                {
                    string destPath = Path.Combine(bcfPath, file.Name);
                    extendList.Add(destPath);
                    File.Copy(file.FullName, destPath, true);
                }
                BuildProgIdCore(progIdListing, extendList, assemblyDic);
            }
            progIdListing.Version = LibDateUtils.DateTimeToLibDateTime(DateTime.Now);
            return progIdListing;
        }

        private static void BuildProgIdCore(ProgIdConfigListing progIdListing, List<string> files, Dictionary<string, Assembly> assemblyDic)
        {
            foreach (string file in files)
            {
                Assembly assembly = Assembly.LoadFrom(file);
                string dllName = assembly.ManifestModule.Name;
                if (!assemblyDic.ContainsKey(dllName))
                    assemblyDic.Add(dllName, assembly);
                DateTime fileDateTime = File.GetCreationTime(file);
                long version = LibDateUtils.DateTimeToLibDateTime(fileDateTime);
                if (!progIdListing.DllVersions.ContainsKey(dllName))
                {
                    progIdListing.DllVersions.Add(dllName, version);
                }
                Type[] types = assembly.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsDefined(typeof(ProgIdAttribute)))
                    {
                        ProgIdAttribute attr = (ProgIdAttribute)t.GetCustomAttribute(typeof(ProgIdAttribute));
                        ProgIdRelationDll relationDll = new ProgIdRelationDll(dllName, t.FullName);
                        if (progIdListing.RelationDlls.ContainsKey(attr.ProgId))
                            progIdListing.RelationDlls[attr.ProgId] = relationDll;
                        else
                            progIdListing.RelationDlls.Add(attr.ProgId, relationDll);
                        if (!string.IsNullOrEmpty(attr.VclPath))
                        {
                            if (progIdListing.VclMap.ContainsKey(attr.VclClass))
                                progIdListing.VclMap[attr.VclClass] = attr.VclPath;
                            else
                                progIdListing.VclMap.Add(attr.VclClass, attr.VclPath);
                        }
                        if (!string.IsNullOrEmpty(attr.ViewPath))
                        {
                            if (progIdListing.ViewMap.ContainsKey(attr.ViewClass))
                                progIdListing.ViewMap[attr.ViewClass] = attr.ViewPath;
                            else
                                progIdListing.ViewMap.Add(attr.ViewClass, attr.ViewPath);
                        }
                    }
                }
            }
        }
    }
}
