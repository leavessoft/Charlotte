/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
namespace AmapAPITool.AmapAPI
{
    public static class AmapRestfulAPIUrlBuilder
    {
        public static string BaseUrl = "https://restapi.amap.com/v3/";

        public static string GetQueryPoiUrl(string key, string keywords, string city, int offset, int page, 
            string extensions = "all", string output = "json")
        {
            string url = BaseUrl + "place/text?" +
                         $"keywords={keywords}&" +
                         $"city={city}&" +
                         $"offset={offset}&" +
                         $"page={page}&" +
                         $"extensions={extensions}&" +
                         $"output={output}&" + 
                         $"key={key}";
            return url;
        }

        public static string GetQueryAroundUrl(string key, string location, int radius, string type, int offset, int page, 
            string extensions = "all", string output = "json")
        {
            string url = BaseUrl + "place/around?" +
                         $"location={location}&" +
                         $"radius={radius}&" +
                         $"type={type}&" +
                         $"offset={offset}&" +
                         $"page={page}&" +
                         $"extensions={extensions}&" +
                         $"output={output}&" + 
                         $"key={key}";
            return url;
        }
    }
}