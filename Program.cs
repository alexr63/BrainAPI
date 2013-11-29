using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

            using (MD5 md5Hash = MD5.Create())
            {
                var client = new RestClient("http://api.brain.com.ua");
                var request = new RestRequest("auth", Method.POST);
                request.AddParameter("login", "victor@it.lg.ua");
                request.AddParameter("password", GetMd5Hash(md5Hash, "123456"));
                IRestResponse<AuthStatus> authResponse = client.Execute<AuthStatus>(request);
                var sid = authResponse.Data.Result;

                request = new RestRequest("product/product_code/{product_code}/{sid}", Method.GET);
                var productCode = Clipboard.GetText();
                request.AddUrlSegment("product_code", productCode); // example U0002149
                request.AddUrlSegment("sid", sid);
                var productResponse = client.Execute(request);
                Clipboard.SetText(productResponse.Content);

                Console.WriteLine(productResponse.Content);

                request = new RestRequest("logout/{sid}", Method.POST);
                request.AddUrlSegment("sid", sid);
                var logoutResponse = client.Execute(request);
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
