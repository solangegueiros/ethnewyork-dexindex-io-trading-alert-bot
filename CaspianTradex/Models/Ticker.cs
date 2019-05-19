using System;
using System.Collections.Generic;
using System.Text;

namespace CaspianTradex.Models
{
    class Ticker
    {
        public string exchangeName { get; set; }
        public double totalPrice { get; set; }
        public int tokenAmount { get; set; }
        public string tokenSymbol { get; set; }
        public double avgPrice { get; set; }
        public long timestamp { get; set; }
        //public object error { get; set; }
        public string error { get; set; }
    }

}
