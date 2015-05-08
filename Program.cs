using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using RestSharp;

namespace BrainAPI
{
    internal class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [STAThread()]
        private static void Main(string[] args)
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            var parameters = Clipboard.GetText().Split(Environment.NewLine.ToCharArray());
            var path = parameters[0];
            var command = path;
            if (path.Contains("/"))
            {
                var commands = path.Split('/');
                command = commands[0];
            }

            try
            {
                var client = new RestClient(Properties.Settings.Default.BaseURL);
                client.Timeout = 10000;
                client.ReadWriteTimeout = 10000;
                RestRequest request = null;

                if (command == "auth")
                {
                    using (MD5 md5Hash = MD5.Create())
                    {
                        request = new RestRequest(path, Method.POST);
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
                }
                else if (command == "logout")
                {
                    request = new RestRequest(path, Method.POST); // example "logout/{sid}"
                    var logoutResponse = client.Execute(request);
                    Clipboard.SetText(logoutResponse.Content);
                }
                else
                {
                    if (parameters.Length > 1)
                    {
                        switch (parameters[1])
                        {
                            case "GET":
                                request = new RestRequest(path, Method.GET); // examples "products/{category_id}/{sid}" "product/product_code/{product_code}/{sid}"
                                break;
                            case "POST":
                                request = new RestRequest(path, Method.POST); // examples "order/{sid}"
                                break;
                        }
                    }
                    if (request != null)
                    {
                        if (parameters.Length > 2)
                        {
                            request.AddParameter("data", parameters[2]); // examples "[{"productID":"15949","quantity":"12","comment":"thank for service"}, {"productID":"23267","quantity":"1"}]"
                        }
                        var productResponse = client.Execute(request);
                        Clipboard.SetText(productResponse.Content);
                    }
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
