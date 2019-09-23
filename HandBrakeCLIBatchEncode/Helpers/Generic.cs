using System;
using System.Collections.Generic;
using System.IO;

namespace HandBrakeCLIBatchEncode
{
    internal class GenericHelper
    { 
        internal static List<string> GetCompatibleFiles(string rootFileOrCombined)
        {
            var compatibleFileList = new List<string>();

            try
            {

                if (rootFileOrCombined.Contains(";"))
                {
                    string[] parts = rootFileOrCombined.Split(';');

                    foreach (var part in parts)
                    {
                        if (part.IsFile())
                        {
                            if (new FileInfo(part).Extension.ToLower().In(Global.CompatibleExtensions))
                                compatibleFileList.Add(part);
                        }
                        else
                        {
                            string[] filesList = Directory.GetFiles(part, "*.*", SearchOption.AllDirectories);
                            foreach (string file in filesList)
                            {
                                if (new FileInfo(file).Extension.ToLower().In(Global.CompatibleExtensions))
                                    compatibleFileList.Add(file);
                            }
                        }
                    }
                }

                else
                {
                    if (rootFileOrCombined.IsFile())
                    {
                        if (new FileInfo(rootFileOrCombined).Extension.ToLower().In(Global.CompatibleExtensions))
                            compatibleFileList.Add(rootFileOrCombined);
                    }
                    else
                    {
                        string[] filesList = Directory.GetFiles(rootFileOrCombined, "*.*", SearchOption.AllDirectories);
                        foreach (string file in filesList)
                        {
                            if (new FileInfo(file).Extension.ToLower().In(Global.CompatibleExtensions))
                                compatibleFileList.Add(file);
                        }
                    }
                }

            }
            catch { }

            return compatibleFileList;
        }
    }
}
