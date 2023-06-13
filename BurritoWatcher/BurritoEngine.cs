using IronOcr;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;
using TwitterSharp.Rule;

namespace BurritoWatcher
{
    public class BurritoEngine : IDisposable
    {
        public string TwitterBearerToken { get; set; }
        public class Recipient
        {
            public enum OS
            {
                IOS,
                Android
            }
            public string Name { get; set; }
            public long Number { get; set; }
            public OS Type { get; set; } = OS.IOS;
        }

        //by default monitor @ChipotleTweets as author
        public string MonitoredAccount { get; set; } = "ChipotleTweets";

        //by default monitor for hashtag
        public string MonitoredHashTag { get; set; } = "ChipotleFreePointer";

        //configurable list of recipients to receive forwarded codes
        public List<Recipient> Contacts { get; set;} = new List<Recipient>();
        private ADBSMS _smsManager = new ADBSMS();
        private TwitterSharp.Client.TwitterClient _twitterClient;
        private bool _watching = false;
        //default regex pattern to detect chipotle codes
        private Regex chipotlePattern = new Regex(@"Text (\S+) to", RegexOptions.Compiled);
        //download client for attached media on tweets
        private WebClient _webClient = new WebClient();
        //ocr engine to extract text from media
        private IronTesseract ocr = new IronTesseract();
        //task which runs monitoring on a new tweet matched by subscription
        private Task watchTask;

        //begin twitter monitoring for selected keyword by author
        public void Watch()
        {
            Console.WriteLine("Monitoring twitter for Author:{0} mentioning:{1}", MonitoredAccount, MonitoredHashTag);
            if (_watching) throw new Exception("Already watching");
            _watching = true;
            if (string.IsNullOrEmpty(TwitterBearerToken)) throw new Exception("Null Bearer token");
            if (_twitterClient == null) _twitterClient = new TwitterSharp.Client.TwitterClient(TwitterBearerToken);

            //subscribe to feed
            handleSubscriptions();

            //create background task to detect+send codes on post
            watchTask = Task.Run(async () =>
            {
                //create action when tweet caught by subscription rules
                await _twitterClient.NextTweetStreamAsync(async (tweet) =>
                {
                    
                    string code = await Detect(tweet);
                    if (string.IsNullOrEmpty(code) || code.Length < 4)
                    {
                        Console.WriteLine($"Tweet posted by {tweet.Author.Name}: No code found. {tweet.Text} (Rules: {string.Join(',', tweet.MatchingRules.Select(x => x.Tag))})");
                        return;
                    }
                    SendCode(code);
                    Console.WriteLine("Code detected:{0}", code);
                },
                new TweetSearchOptions
                {
                    UserOptions = Array.Empty<UserOption>()
                });
            });
        }

        //ensure subscription stream exists on the twitter account or create new one
        private async void handleSubscriptions()
        {
            Console.WriteLine("Checking twitter feed subscription status...");
            //find all existing subscriptions
            var subs = await _twitterClient.GetInfoTweetStreamAsync();
            //foreach (var sub in subs)
            //    await _twitterClient.DeleteTweetStreamAsync(new string[] { sub.Id });
            if (subs==null||!subs.Any(a=>a.Value.ToString().Contains(MonitoredAccount))){
                Console.WriteLine("No subscription found to tweets from:{0}", MonitoredAccount);
                //add subscription
                var request = new TwitterSharp.Request.StreamRequest(
                    Expression.Author(MonitoredAccount) // using TwitterSharp.Rule;
                , MonitoredHashTag);
                await _twitterClient.AddTweetStreamAsync(request); // Add them to the stream
            }
            Console.WriteLine("Subscriptions verified.");
            Console.WriteLine("BurritoWatcher is monitoring twitter {ESC} to quit.");
        }

        //logic to send code via sms to all involved parties
        private void SendCode(string code)
        {
            //automatically claim burrito via adb phone
            _smsManager.SendSMS("888222", code);

            //share code with contacts
            foreach(var recipient in Contacts)
            {
                Console.WriteLine("Sending code:{0} to {1} with phone type:{2}", code, recipient.Name, recipient.Type);
                string curNumber = recipient.Number.ToString();
                if (recipient.Type == Recipient.OS.Android)
                {
                    _smsManager.SendSMS(curNumber, code);
                    _smsManager.SendSMS(curNumber, "Send to sms:+888222 for a free burrito. -BurritoBot");
                }
                else _smsManager.SendSMS(curNumber, "sms://888222?body=" + code);
            }
        }

        //uses Regex to find any codes in tweet body or images
        //returns code if matched by pattern or null if no code found
        public async Task<string> Detect(Tweet tweet)
        {
            //check tweet body
            var codeMatch = chipotlePattern.Match(tweet.Text);
            if (codeMatch == null || !codeMatch.Success)
            {
                //download full tweet with media
                var fullTweet = await _twitterClient.GetTweetAsync(tweet.Id, new TweetSearchOptions() { TweetOptions = new[] { TweetOption.Attachments }, MediaOptions = new[] { MediaOption.Url } });
                if (fullTweet == null || fullTweet.Attachments==null ||fullTweet.Attachments.Media==null || fullTweet.Attachments?.Media?.Count() == 0) return null;
                
                //get first mediaUrl to download
                var url = fullTweet.Attachments.Media.Select(a => a.Url).First();
                
                //download media to byte[]
                var data = _webClient.DownloadData(url);

                //pass byte[] to tesseract
                using (OcrInput input = new OcrInput())
                {
                    input.AddImage(data);
                    OcrResult result = ocr.Read(input);
                    Console.WriteLine("Read {0} in attached image.",result.Text);
                    codeMatch = chipotlePattern.Match(result.Text);
                    if (codeMatch == null || !codeMatch.Success) return null;
                    return codeMatch.Value.Replace("Text ", "").Replace(" to", "");
                }
            }
            else return codeMatch.Value.Replace("Text ", "").Replace(" to", "");
        }
        //disposes objects properly and stops monitoring
        public void Stop() {
            if (!_watching) return;
            _watching = false;
            watchTask.Dispose();
        }

        //section: IDisposable
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            Console.WriteLine("Shutting down...");
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_twitterClient != null) _twitterClient.Dispose();
                    _smsManager.Dispose();
                    Stop();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
    }
}
