using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;


// ReSharper disable CheckNamespace
namespace Craftitude.Authentication
{
    public static class Authentication
    {
        public static AuthenticationResponse Authenticate(AuthenticationRequest request, string server = "https://authserver.mojang.com")
        {
            var jsonreq = JsonConvert.SerializeObject(request);

            HttpWebRequest httpRequest = null;
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(server + "/authenticate");
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json";
                httpRequest.ContentLength = jsonreq.Length;


                using (var stream = httpRequest.GetRequestStream())
                {
                    stream.Write(new ASCIIEncoding().GetBytes(jsonreq), 0, jsonreq.Length);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                var errorresponse = new AuthenticationResponse {Error = e.Message, Success = false};
                return errorresponse;
            }
            //Response Handling
            try
            {
                var resp = (HttpWebResponse)(httpRequest.GetResponse());
                using (var responseStream = resp.GetResponseStream())
                {
                    //Console.WriteLine(resp.StatusCode.ToString());
                    if (responseStream != null)
                        using (var respReader = new StreamReader(responseStream))
                        {
                            var successresponse = new AuthenticationResponse
                            {
                                Success = true,
                                Data = JsonConvert.DeserializeObject<AuthenticationData>(respReader.ReadToEnd())
                            };

                            return successresponse;
                        }
                    else
                    {
                        throw new Exception("Internal error");
                    }
                }

            }
            catch (Exception exx)
            {
                var errorresponse = new AuthenticationResponse {Error = exx.Message, Success = false};
                return errorresponse;

            }


        }
    }

}
