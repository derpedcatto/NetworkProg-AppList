using System;
using System.Collections.Generic;

namespace NetworkProg_AppList._3_WebAPI.Model
{
    /* Модель-оболочка от сайта https://api.coincap.io/v2/assets/
    * {
    *      data: [...AssetModel...],
    *      timestamp:	1669852800000
    * }*/

    public class AssetModelList
    {
        public List<AssetModel> data { get; set; }
        public Int64 timestamp { get; set; }
    }


    public class AssetModel
    {
        public string id { get; set; }
        public string rank { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string supply { get; set; }
        public string maxSupply { get; set; }
        public string marketCapUsd { get; set; }
        public string volumeUsd24Hr { get; set; }
        public string priceUsd { get; set; }
        public string changePercent24Hr { get; set; }
        public string vwap24Hr { get; set; }
        public string explorer { get; set; }
    }
}
