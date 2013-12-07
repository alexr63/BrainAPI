using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using RestSharp;

namespace BrainAPI
{
    internal class Program
    {
        [STAThread()]
        private static void Main(string[] args)
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            var path = Clipboard.GetText();
            var command = path;
            if (path.Contains("/"))
            {
                var commands = path.Split('/');
                command = commands[0];
            }

            try
            {
                var client = new RestClient("http://api.brain.com.ua");
                client.Timeout = 1000;
                RestRequest request;

                if (command == "auth")
                {
                    using (MD5 md5Hash = MD5.Create())
                    {
                        request = new RestRequest(path, Method.POST);
                        request.AddParameter("login", "victor@it.lg.ua");
                        request.AddParameter("password", GetMd5Hash(md5Hash, "123456"));
                        IRestResponse<AuthStatus> authResponse = client.Execute<AuthStatus>(request);
                        Clipboard.SetText(authResponse.Data.Result);
                    }
                }
                else if (command == "logout")
                {
                    request = new RestRequest(path, Method.POST); // example "logout/{sid}"
                    var logoutResponse = client.Execute(request);
                }
                else
                {
                    request = new RestRequest(path, Method.GET); // examples "products/{category_id}/{sid}" "product/product_code/{product_code}/{sid}"
                    var productResponse = client.Execute(request);
                    Clipboard.SetText(productResponse.Content);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
