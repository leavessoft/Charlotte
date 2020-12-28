/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */
using System.Collections.Generic;

namespace AmapAPITool.AmapAPI
{
    public class PoiType
    {
        private Dictionary<string, PoiCategory> _typeCategoryDict = new Dictionary<string, PoiCategory>();

        public PoiType(string csvContent)
        {
            string[] lines = csvContent.Replace("\r", "").Split('\n');
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }

                string[] categories = line.Split(',');
                if (categories.Length != 4)
                {
                    continue;
                }

                if (!_typeCategoryDict.ContainsKey(categories[0]))
                {
                    PoiCategory category = new PoiCategory();
                    category.BigCategory = categories[1];
                    category.MidCategory = categories[2];
                    category.SubCategory = categories[3];
                    _typeCategoryDict.Add(categories[0], category);
                }
            }
        }

        public Dictionary<string, PoiCategory> GetTypeCategoryDictionary()
        {
            return _typeCategoryDict;
        }

        public Dictionary<string, PoiCategory> GetTypeByCategoryKeyword(string keyword)
        {
            Dictionary<string, PoiCategory> result = new Dictionary<string, PoiCategory>();
            
            foreach (KeyValuePair<string,PoiCategory> keyValuePair in _typeCategoryDict)
            {
                PoiCategory category = keyValuePair.Value;
                if (category.BigCategory.Contains(keyword) || category.MidCategory.Contains(keyword) ||
                    category.SubCategory.Contains(keyword))
                {
                    result.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return result;
        }
    }

    public struct PoiCategory
    {
        public string BigCategory { get; set; }
        public string MidCategory { get; set; }
        public string SubCategory { get; set; }
    }
}