using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using RestSharp;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace BrainAPI
{
    internal class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread()]
        private static void Main(string[] args)
        {
            log.Info("App start");

            if (!Clipboard.ContainsText())
            {
                log.Warn("Clipboard text missed");
                return;
            }

            var commandLines = Clipboard.GetText().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var command = commandLines[0];
            var method = commandLines[1];

            try
            {
                var client = new RestClient(Properties.Settings.Default.BaseURL);
                client.Timeout = 10000;
                client.ReadWriteTimeout = 10000;
                RestRequest request = null;

                if (command == "auth")
                {
                    log.Info("Auth start");
                    using (MD5 md5Hash = MD5.Create())
                    {
                        request = new RestRequest(command, Method.POST);
                        request.AddParameter("login", Properties.Settings.Default.Login);
                        request.AddParameter("password", GetMd5Hash(md5Hash, Properties.Settings.Default.Password));
                        IRestResponse<AuthStatus> authResponse = client.Execute<AuthStatus>(request);
                        if (authResponse.ErrorException != null)
                        {
                            log.Error(authResponse.ErrorMessage, authResponse.ErrorException);
                        }
                        else
                        {
                            Clipboard.SetText(authResponse.Data.Result);
                        }
                    }
                    log.Info("Auth start");
                }
                else if (command == "logout")
                {
                    log.Info("Logout start");
                    var parameters = commandLines[2];
                    request = new RestRequest(command + parameters, Method.POST); // example "logout/{sid}"
                    var logoutResponse = client.Execute(request);
                    Clipboard.SetText(logoutResponse.Content);
                    log.Info("Logout end");
                }
                else
                {
                    log.Info("Command start");
                    var parameters = commandLines[2];
                    switch (method)
                    {
                        case "GET":
                            request = new RestRequest(command + parameters, Method.GET); // examples "products/{category_id}/{sid}" "product/product_code/{product_code}/{sid}"
                            break;
                        case "POST":
                            request = new RestRequest(command + parameters, Method.POST); // examples "order/{sid}"
                            break;
                    }
                    log.Info(command + parameters);
                    if (commandLines.Length > 3)
                    {
                        var data = commandLines[3];
                        request.AddParameter("data", data); // examples "[{"productID":"15949","quantity":"12","comment":"thank for service"}, {"productID":"23267","quantity":"1"}]"
                        log.Info("Data: " + data);
                    }
                    var productResponse = client.Execute(request);
                    Clipboard.SetText(productResponse.Content);
                    log.Info("Command end");
                }
            }
            catch (Exception exception)
            {
                log.Error(exception.Message, exception);
            }
        }

        private static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        private class AuthStatus
        {
            public string Status { get; set; }
            public string Result { get; set; }
        }
    }
}
