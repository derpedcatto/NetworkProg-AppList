using System;

namespace NetworkProg_AppList._2_HTTP.Model
{
    public class RateJSON
    {
        public Int16 r030 { get; set; }  // short
        public String txt { get; set; }
        public Single rate { get; set; }  // float
        public String cc { get; set; }
        public String exchangedate { get; set; }
    }
}
