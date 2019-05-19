using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace CaspianTradex.Helpers
{
    class GoogleAPI
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        //static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "CaspianTrade";

        // https://docs.google.com/spreadsheets/d/1-1IkOOxGOK2TItBwocGR78k3tg0yqi2KDLV_RXuAyT4
        static string spreadsheetId = Program.configuration["GoogleConfig:spreadsheetId"];
        static string sheetTrades = Program.configuration["GoogleConfig:sheetTrades"];
        static SheetsService sheetService;

        static UserCredential credential;


        public static bool UpdateTrade(DateTime dateTime, double amount, string buyExchange, double buyPrice, 
            string sellExchange, double sellPrice, double gainAmount, double gainPerc)
        {
            try
            {
                ValueRange valueRangeObj = new ValueRange();
                valueRangeObj.MajorDimension = "ROWS";
                var valuesList = new List<object>() {
                    dateTime.ToString("MM/dd/yyyy HH:mm:ss"),
                    amount,
                    buyExchange,
                    buyPrice,
                    sellExchange,
                    sellPrice,
                    gainAmount,
                    gainPerc
                };
                valueRangeObj.Values = new List<IList<object>> { valuesList };


                // Create Google Sheets API service.
                sheetService = new SheetsService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = GetCredential(),
                    ApplicationName = ApplicationName,
                });

                String range = string.Empty;
                range = string.Format("{0}!{1}", sheetTrades, "A2:N");

                //APPEND
                //https://developers.google.com/sheets/api/reference/rest/v4/spreadsheets.values/append
                SpreadsheetsResource.ValuesResource.AppendRequest requestAppend = sheetService.Spreadsheets.Values.Append(valueRangeObj, spreadsheetId, range);
                requestAppend.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                requestAppend.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                AppendValuesResponse responseAppend = requestAppend.Execute();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERRO UpdateTradeExecuted: {0}\n{1}", ex.Message, ex.InnerException);
                return false;
            }
        }


        public static UserCredential GetCredential()
        {
            try
            {
                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    //string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    string credPath = "token.json";

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None
                        , new FileDataStore(credPath, true)
                        ).Result;
                    //if (Program.debug)
                    //    Console.WriteLine("Credential file saved to: " + credPath);
                }

                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERRO GetCredential: {0}\n{1}", ex.Message, ex.InnerException);
                return null;
            }
        }


    }
}
