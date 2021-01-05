/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using AmapAPITool.AmapAPI.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AmapAPITool.AmapAPI
{
    public class AmapAPIService
    {
        private string _appKey;

        static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public AmapAPIService(string amapAppKey)
        {
            _appKey = amapAppKey;
        }

        public void SetAppKey(string key)
        {
            _appKey = key;
        }

        public List<POI> SearchPoi(string keywords, string city, int poiPerPage, int page, out int resultCount)
        {
            string responseContent = PreprocessNull(HttpGet(AmapRestfulAPIUrlBuilder.GetQueryPoiUrl(_appKey, keywords, city, poiPerPage, page)));

            POIQueryResponse responseObject = JsonConvert.DeserializeObject<POIQueryResponse>(responseContent);
            if (responseObject == null)
            {
                throw new System.Exception("Failed parsing response");
            }
            if (responseObject.status == "0")
            {
                throw new System.Exception($"Server responded with an error: {responseObject.info}");
            }

            resultCount = Convert.ToInt32(responseObject.count);

            List<POI> pois = new List<POI>();
            responseObject.pois?.ForEach(poi =>
            {
                pois.Add(poi);
            });

            return pois;
        }

        public List<POI> SearchAroundPoi(string location, int radius, string type, int poiPerPage, int page, out int resultCount)
        {
            string responseContent = PreprocessNull(HttpGet(AmapRestfulAPIUrlBuilder.GetQueryAroundUrl(_appKey, location, radius, type, poiPerPage, page)));
            POIQueryResponse responseObject = JsonConvert.DeserializeObject<POIQueryResponse>(responseContent, _jsonSerializerSettings);
            if (responseObject == null)
            {
                throw new System.Exception("Failed parsing response");
            }
            if (responseObject.status == "0")
            {
                throw new System.Exception($"Server responded with an error: {responseObject.info}");
            }

            resultCount = Convert.ToInt32(responseObject.count);

            List<POI> pois = new List<POI>();
            responseObject.pois?.ForEach(poi =>
            {
                pois.Add(poi);
            });

            return pois;
        }

        private string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;
            WebResponse response = null;
            string responseStr = null;
            response = request.GetResponse();
            if (response != null)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                responseStr = reader.ReadLine();
            }
            return responseStr;
        }

        private string PreprocessNull(string json)
        {
            return json.Replace("[]", "null");
        }
    }
}