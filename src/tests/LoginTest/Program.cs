using System;
using Craftitude.Authentication;

namespace LoginTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = Authentication.Authenticate(new AuthenticationRequest()
            {
                Agent = new AuthenticationAgent()
                {
                    Name = "Minecraft",
                    Version = 1
                },
                Username = "test",
                Password = "test"
            }, "http://login.modernminas.de");

            if (response.Success)
                Console.WriteLine("This client authenticated as {0}.", response.Data.SelectedProfile.Name);
            else
                Console.WriteLine("Could not authenticate.");

            Console.ReadKey();
        }
    }
}
