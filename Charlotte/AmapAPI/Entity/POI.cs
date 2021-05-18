/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using System.Collections.Generic;

namespace AmapAPITool.AmapAPI.Entity
{
    public class POI
    {
        public string id { get; set; }
        //public string? parent { get; set; }
        //public string? childtype { get; set; }
        public string name { get; set; }
        public string typecode { get; set; }
        public string type { get; set; }
        public string address { get; set; }
        public string citycode { get; set; }
        public string cityname { get; set; }
        public string postcode { get; set; }
        public string tel { get; set; }
        public string website { get; set; }
        public string email { get; set; }
        public string pname { get; set; }
        public string pcode { get; set; }
        public string adcode { get; set; }
        public string adname { get; set; }
        public string location { get; set; }
        public string tag { get; set; }
        public string biz_type { get; set; }
        public double? lat { get
            {
                string[] split = location.Split(',');
                if (split.Length == 2)
                {
                    return double.Parse(split[0]);
                } else
                {
                    return null;
                }
            }
        }
        public double? lon { get
            {
                string[] split = location.Split(',');
                if (split.Length == 2)
                {
                    return double.Parse(split[1]);
                }
                else
                {
                    return null;
                }
            }
        }
        //public List<POIPhoto> photos { get; set; }

        /// <summary>
        /// Get the list of all properties of the POI entry
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetPropertyList()
        {
            List<KeyValuePair<string, string>> propDict = new List<KeyValuePair<string, string>>();
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    propDict.Add(new KeyValuePair<string, string>(prop.Name, (string)prop.GetValue(this)));
                }
            }
            return propDict;
        }

        /// <summary>
        /// Match keyword
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns>true if match</returns>
        public bool Match(string[] keywords)
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    int matchCount = 0;
                    string value = (string)prop.GetValue(this);

                    if (!string.IsNullOrEmpty(value))
                    {
                        foreach (string s in keywords)
                        {
                            if (value.Contains(s))
                            {
                                matchCount++;
                            }
                        }
                    }

                    if (matchCount >= keywords.Length) // if all keywords match
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}