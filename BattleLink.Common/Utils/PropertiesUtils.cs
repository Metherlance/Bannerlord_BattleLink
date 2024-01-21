using System.Collections.Generic;

namespace BattleLink.Common.Utils
{
    public class PropertiesUtils
    {
        private string filePath;
        private Dictionary<string, string> dic;

        public PropertiesUtils(string filePath)
        {
            this.filePath = filePath;
            dic = new Dictionary<string, string>();

            using (var reader = new System.IO.StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("=") && !line.StartsWith("#"))
                    {
                        var pair = line.Split('=');
                        dic.Add(pair[0], pair[1]);
                    }
                }
            }
        }

        public string Get(string key)
        {
            return dic[key];
        }

    }
}
