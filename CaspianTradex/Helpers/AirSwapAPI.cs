using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using CaspianTradex.Models;


namespace CaspianTradex.Helpers
{
    class AirSwapAPI
    {
        static string url_base = @"http://localhost:1337/";
        static HttpClient client = new HttpClient();
        public static string passo = string.Empty;

        public static List<Ticker> GetTickers(string side, string symbol, string amount)
        {
            passo = "AirSwap-GetTickers";

            try
            {
                HttpClient client = new HttpClient();
                //http://localhost:1337/sell?symbol=DAI&amount=500
                string uri = string.Format(@"{0}{1}?symbol={2}&amount={3}", url_base, side, symbol, amount);
                //Console.WriteLine(uri);                
                HttpResponseMessage response = client.GetAsync(uri).Result;

                string jsonString = string.Empty;
                using (HttpContent content = response.Content)
                {
                    var responseTask = content.ReadAsStringAsync();
                    responseTask.Wait();
                    jsonString = responseTask.Result;
                    //Console.WriteLine(jsonString);
                }

                List<Ticker> tickers = new List<Ticker>();
                Dictionary<string, Ticker> tickerAux = new Dictionary<string, Ticker>();

                if (response.IsSuccessStatusCode)
                {
                    dynamic obj = JsonConvert.DeserializeObject(jsonString);                    

                    foreach (var item in obj)
                    {
                        //Console.WriteLine(item.ToString());
                        tickerAux = JsonConvert.DeserializeObject<Dictionary<string, Ticker>>(item.ToString());

                        foreach (var itemTickerAux in tickerAux)
                        {
                            if (string.IsNullOrEmpty(itemTickerAux.Value.error))
                            {
                                tickers.Add(itemTickerAux.Value);
                                //Console.WriteLine("{0}\t{1}\t tokenAmount: {2}\t avgPrice: {3}", itemTickerAux.Value.tokenSymbol, itemTickerAux.Value.exchangeName, itemTickerAux.Value.tokenAmount, itemTickerAux.Value.avgPrice);
                            }
                            else
                            {
                                //Console.WriteLine("{0}\t{1}\t ERROR: {2}", itemTickerAux.Value.tokenSymbol, itemTickerAux.Value.exchangeName, itemTickerAux.Value.error);
                            }

                        }                        
                    }

                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }

                return tickers;

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERRO {0}\n{1}\n{2}", passo, ex.Message, ex.InnerException);
                return null;
            }
        }


    }
}
