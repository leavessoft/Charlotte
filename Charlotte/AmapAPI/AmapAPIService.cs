/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */
using System;
using System.Collections.Generic;
using System.Net;
using AmapAPITool.AmapAPI.Entity;
using Newtonsoft.Json;

namespace AmapAPITool.AmapAPI
{
    public class AmapAPIService
    {
        WebClient _webClient = new WebClient();
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
            string responseContent = PreprocessNull(_webClient.DownloadString(AmapRestfulAPIUrlBuilder.GetQueryPoiUrl(_appKey, keywords, city, poiPerPage, page)));
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
        
        public List<POI> SearchAroundPoi(string location, int radius, string type, int poiPerPage, int page, out int resultCount)
        {
            string responseContent = PreprocessNull(_webClient.DownloadString(AmapRestfulAPIUrlBuilder.GetQueryAroundUrl(_appKey, location, radius, type, poiPerPage, page)));
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

        private string PreprocessNull(string json)
        {
            return json.Replace("[]", "null");
        }
    }
}