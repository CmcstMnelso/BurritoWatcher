using SixLabors.ImageSharp.ColorSpaces;

namespace BurritoWatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //check requirements
            new Setup();

            //load settings
            var secrets = new Settings("secrets.json");

            //debug Info
            Console.WriteLine("Settings loaded.");
            Console.WriteLine("Using Bearer Token:{0}", secrets.TwitterBearerToken);
            Console.WriteLine("Forwarding codes to:{0} recipients", secrets.Contacts.Count());
            Console.WriteLine("-------------------------------------------------------------------");
            Console.WriteLine("Starting BurritoEngine...");

            //create engine
            using (BurritoEngine engine = new BurritoEngine() { TwitterBearerToken = secrets.TwitterBearerToken, Contacts = secrets.Contacts})
            {
                engine.Watch();
                //wait for escape key before closing
                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            }
        }
    }
}