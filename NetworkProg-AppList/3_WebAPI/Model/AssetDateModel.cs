using System;
using System.Collections.Generic;
using System.Globalization;

namespace NetworkProg_AppList._3_WebAPI.Model
{
    /* Модель-оболочка от сайта https://api.coincap.io/v2/assets/bitcoin/history?interval=d1
    * {
    *      data: [...HistoryModel...],
    *      timestamp:	1669852800000
    * }*/

    public class AssetDateModelList
    {
        public List<AssetDateModel> data { get; set; }
        public Int64 timestamp { get; set; }
    }


    /* Модель для данных 
    * {
    *    "priceUsd": "17068.7558958508773389",
    *    "time": 1669852800000,
    *    "date": "2022-12-01T00:00:00.000Z"
    *  }*/

    public class AssetDateModel
    {
        public String priceUsd { get; set; }
        public Int64 time { get; set; }
        public String date { get; set; }
        public double price => Convert.ToDouble(priceUsd, CultureInfo.InvariantCulture);    // На ОС вместо '.' -> ','
    }
}