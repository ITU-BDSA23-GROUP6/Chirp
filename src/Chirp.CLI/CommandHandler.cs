

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommandHandle
{
    public class CommandHandler
    {
        private readonly HttpClient client;

        public CommandHandler()
        {
            // Create the HttpClient with a base address.
            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:3000"); // Replace with your server's URL
        }

        public async Task Fondle(bool shouldReadOne, bool shouldReadAll, string msg)
        {
            if (shouldReadAll)
            {
                Console.WriteLine("1) Entered if-statement");
                try
                {
                    Console.WriteLine("2) Entered try-block; pre");
                    // Make the GET request to the /cheeps endpoint.
                    HttpResponseMessage response = await client.GetAsync("/cheeps");

                    // Check if the response is successful (status code 200).
                        // Read and print the response content.
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response: " + responseBody);
                        Console.WriteLine("3) Entered try-block; post");
                   
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            else
            {
                Console.WriteLine("ERROR IN FONDLE");
            }
        }
    }
}
