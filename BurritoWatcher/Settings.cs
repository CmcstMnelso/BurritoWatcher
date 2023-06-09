using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BurritoWatcher
{
    public class Settings
    {
        //the bearer token for monitoring realtime tweet subscriptions
        public string TwitterBearerToken { get; set; }

        //the recipients to forward any found codes
        public List<BurritoEngine.Recipient> Contacts { get; set; } = new List<BurritoEngine.Recipient>();
        public class TSettings
        {
            public string TwitterBearerToken { get; set; }
            public TRecipient[] Contacts { get; set; }

            public class TRecipient
            {
                public string Name { get; set; }
                public long Number { get; set; }
                public string Type { get; set; }
            }
        }
        //reads {settingsfile}.json and parses data into this class
        public Settings(string settingsFile) 
        {
            if (string.IsNullOrWhiteSpace(settingsFile)) throw new ArgumentNullException(nameof(settingsFile));
            if (!File.Exists(settingsFile)) throw new FileNotFoundException("Settings file was not found. Please create a .json file at "+settingsFile+" which includes a twitterBearerKey");
            var deserialized = JsonSerializer.Deserialize<TSettings>(File.ReadAllText(settingsFile));
            this.TwitterBearerToken = deserialized.TwitterBearerToken;
            this.Contacts = deserialized.Contacts.Select(contact =>
            {
                object parsed;
                var ctact = new BurritoEngine.Recipient() { Name = contact.Name, Number = contact.Number };
                if (Enum.TryParse(typeof(BurritoEngine.Recipient.OS), contact.Type, out parsed))
                    ctact.Type = (BurritoEngine.Recipient.OS)parsed;
                return ctact;
            }).ToList();
        }
    }
}
