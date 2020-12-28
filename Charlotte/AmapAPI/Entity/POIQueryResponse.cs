/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE', which is part of this source code package.
 */
using System.Collections.Generic;

namespace AmapAPITool.AmapAPI.Entity
{
    public class POIQueryResponse
    {
        public string status { get; set; } // 0: success; 1: fail
        public string count { get; set; } // max = 1000
        public string info { get; set; }
        public string infocode { get; set; }
        //public suggestion
        public List<POI> pois { get; set; }
    }
}