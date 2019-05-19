using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

using CaspianTradex.Helpers;
using CaspianTradex.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Text;
using MimeKit;
using System.Net.Http;

namespace CaspianTradex
{
    class Program
    {
        static public IConfigurationRoot configuration;
        public static bool debug = false;
        public static CultureInfo cultureInfoUS = new CultureInfo("en-US");
        public static CultureInfo cultureInfoBR = new CultureInfo("pt-BR");

        public static string diretorioBase = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public static StringBuilder erro = new StringBuilder();

        private static Int32 execPause = 0;
        private static int nRun = 0;
        public static string passo = string.Empty;
        public static string asset = string.Empty;
        public static string market = string.Empty;
        public static double amountDefault = 0;


        static int Main(string[] args)
        {
            SetEnvironment();

            try
            {
                DateTime inicio = DateTime.Now;
                Console.WriteLine("\nInicio: {0}\n", inicio);

                RunArbitragem();

                //Só ativar quando executado manualmente  
                Thread.Sleep(execPause);
                RunArbitragemLoop();

                DateTime fim = DateTime.Now;
                Console.WriteLine("\nInicio:{0}\nFim:   {1}\nDuracao: {2}", inicio.ToString(), fim.ToString(), fim.Subtract(inicio).ToString());
                Console.ReadKey();          // Keep the console window open in debug mode.
                return 0;
            }
            catch (Exception ex)
            {
                erro = new StringBuilder();
                erro.AppendLine(ex.Message);
                if (ex.InnerException != null)
                    erro.AppendLine(ex.InnerException.ToString());
                Console.WriteLine("\nERRO: \n{0}", erro.ToString());
                return 99;
            }

        }


        static void RunArbitragem()
        {
            nRun++;
            Console.WriteLine("\nExecução: {0} - {1}\n", nRun, DateTime.Now);
            passo = "RunArbitragem";

            try
            {
                #region GetPriceData
                //Cotações
                double quantidade = amountDefault;
                string amount = quantidade.ToString(cultureInfoUS);

                //Console.WriteLine("\nBUY");
                List <Ticker> tickerBuy = AirSwapAPI.GetTickers("buy", asset, amount);
                //Console.WriteLine("\nSELL");
                List<Ticker> tickerSell = AirSwapAPI.GetTickers("sell", asset, amount);
                #endregion

                //Encontrar a menor compra a mercado e a maior venda a mercado
                Ticker menorCompra = new Ticker();
                Ticker maiorVenda = new Ticker();

                double spreadGain = Convert.ToDouble(configuration["spreadGain"], cultureInfoUS);
                if (Program.debug)
                    Console.WriteLine("Spread lucro: {0:0.00} %", spreadGain*100);

                //Calcula menor compra
                Console.WriteLine("\nBUY");                
                foreach (Ticker p in tickerBuy)
                {
                    //Lista todas as cotações encontradas
                    if (Program.debug)
                        Console.WriteLine("{0:0.000000000000000000}\t {1}", p.avgPrice, p.exchangeName);

                    if ((p.avgPrice < menorCompra.avgPrice) || (menorCompra.avgPrice == 0))
                    {
                        menorCompra.exchangeName = p.exchangeName;
                        menorCompra.totalPrice = p.totalPrice;
                        menorCompra.tokenAmount = p.tokenAmount;
                        menorCompra.tokenSymbol = p.tokenSymbol;
                        menorCompra.avgPrice = p.avgPrice;
                        menorCompra.timestamp = p.timestamp;
                        menorCompra.error = p.error; 
                    }
                }
                if (menorCompra.exchangeName == null)
                {
                    Console.WriteLine("Exchange de compra não encontrada");
                    return;
                }

                //Calcula maior Venda
                Console.WriteLine("\nSELL");
                foreach (Ticker p in tickerSell)
                {
                    if (Program.debug)
                        Console.WriteLine("{0:0.000000000000000000}\t {1}", p.avgPrice, p.exchangeName);

                    if ((p.avgPrice > maiorVenda.avgPrice) && (p.exchangeName != menorCompra.exchangeName))
                    {
                        maiorVenda.exchangeName = p.exchangeName;
                        maiorVenda.totalPrice = p.totalPrice;
                        maiorVenda.tokenAmount = p.tokenAmount;
                        maiorVenda.tokenSymbol = p.tokenSymbol;
                        maiorVenda.avgPrice = p.avgPrice;
                        maiorVenda.timestamp = p.timestamp;
                        maiorVenda.error = p.error;
                    }
                }
                if (maiorVenda.exchangeName == null)
                {
                    Console.WriteLine("Exchange de venda não encontrada");
                    return;
                }

                //Lucro
                double lucro = maiorVenda.avgPrice - menorCompra.avgPrice;
                if (Program.debug)
                    Console.WriteLine("\nlucro: {0:0.00000000}", lucro);
                double gainPerc = (maiorVenda.avgPrice / menorCompra.avgPrice) - 1;

                Console.WriteLine("Compra:\t{0:0.000000000000000000}\t{1}", menorCompra.avgPrice, menorCompra.exchangeName);
                Console.WriteLine("Venda:\t{0:0.000000000000000000}\t{1}", maiorVenda.avgPrice, maiorVenda.exchangeName);
                Console.WriteLine("Calculo - Compra em {0} por {1:0.000000000000000000} - Venda em {2} por {3:0.000000000000000000}\nLucro: {4:0.000000000000000000} ({5:0.0000}%)"
                    , menorCompra.exchangeName, menorCompra.avgPrice, maiorVenda.exchangeName, maiorVenda.avgPrice, lucro, gainPerc * 100);
                Console.WriteLine();

                DateTime dateTime = DateTime.Now;

                if (gainPerc > spreadGain)
                {
                    Console.WriteLine("PROFIT FOUND");
                    double gainAmount = lucro * quantidade;

                    string alert = string.Format("{0}\tCompra em\t{1}\t{2:0.00000000}\tpor\t{3:0.000000000000000000}\nVenda em\t{4}\t{5:0.00000000}\tpor\t{6:0.000000000000000000}\nLucro:\t{7:0.000000000000000000}\t-\t{8:0.0000}%"
                        , dateTime, menorCompra.exchangeName, quantidade, menorCompra.avgPrice,
                        maiorVenda.exchangeName, quantidade, maiorVenda.avgPrice, gainAmount, gainPerc * 100);

                    Console.WriteLine("{0}\tCompra:\t{1}\tpor\t{2:0.000000000000000000}", menorCompra.exchangeName, quantidade, menorCompra.avgPrice);
                    Console.WriteLine("{0}\tVenda:\t{1}\tpor\t{2:0.000000000000000000}", maiorVenda.exchangeName, quantidade, maiorVenda.avgPrice);
                    Console.WriteLine("Lucro lote: {0:0.000000000000000000}\n", gainAmount);

                    GoogleAPI.UpdateTrade(dateTime, quantidade, menorCompra.exchangeName, menorCompra.avgPrice,
                        maiorVenda.exchangeName, maiorVenda.avgPrice, gainAmount, gainPerc);

                }   //lucro > spreadGain

                return;
            }
            catch (Exception ex)
            {
                erro = new StringBuilder();
                erro.AppendLine(ex.Message);
                if (ex.InnerException != null)
                    erro.AppendLine(ex.InnerException.ToString());
                Console.WriteLine("\nERRO {0}\n{1}", passo, erro.ToString());
            }
        }


        static void RunArbitragemLoop()
        {
            Int32 numeroExecucoes = Convert.ToInt32(configuration["numeroExecucoes"]);

            try
            {
                for (int i = 1; i < numeroExecucoes; i++)
                {
                    RunArbitragem();
                    Thread.Sleep(execPause);
                }

                return;
            }
            catch (Exception)
            {
                throw;
            }

        }


        static void SetEnvironment()
        {
            passo = "SetEnvironment";

            try
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
                configuration = builder.Build();

                debug = Convert.ToBoolean(configuration["debug"]);
                execPause = Convert.ToInt32(configuration["execPause"], cultureInfoUS);
                asset = configuration["asset"].ToUpper();
                market = configuration["market"].ToUpper();
                amountDefault = Convert.ToDouble(configuration["amountDefault"], cultureInfoUS);

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERRO {0}\n{1}\n{2}", passo, ex.Message, ex.InnerException);
                throw;
            }
        }


    }
}
