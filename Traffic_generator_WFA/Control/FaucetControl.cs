using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Traffic_generator_WFA.Models;

namespace Traffic_generator_WFA.Control
{
    public class FaucetControl
    {
        public string faucetUri = "https://faucet.ropsten.be/donate/{0}";
        public bool cancel = false;

        public void Donate(MongoAccount address)
        {
            cancel = false;

            while (!cancel)
           {
                try
                {
                   
                    var request = (HttpWebRequest)WebRequest.Create(string.Format(faucetUri, address.Address));
                    var response = (HttpWebResponse)request.GetResponse();
                    string responseString;
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            responseString = reader.ReadToEnd();
                        }
                    }
                    
                }
                catch (Exception e) {
                    Console.WriteLine(e.InnerException);
                };

                Thread.Sleep(3600000);
            }
        }

        public void CancelDonate()
        {
            cancel = true;
        }
    }
}
