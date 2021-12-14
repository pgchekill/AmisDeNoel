using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;

namespace AmisDeNoel
{
    public class Program
    {
        static void Main(string[] args)
        {
            var configPath = @"C:\Users\gerber\Documents\git-p\amis-de-noel\amis.json";
            var crendentialsPaths = @"C:\Users\gerber\Documents\git-p\amis-de-noel\app-pass.txt";

            var json = File.ReadAllText(configPath);
            var friends = JsonConvert.DeserializeObject<List<Ami>>(json);
            var forbidenMatches = new List<ChristmasMatch>()
            {
                new ChristmasMatch("Véro", "Fabien"),
                new ChristmasMatch("Fabien", "Véro"),
                new ChristmasMatch("Diane", "Philippe"),
                new ChristmasMatch("Philippe", "Diane"),
                new ChristmasMatch("Carmen", "Fernand"),
                new ChristmasMatch("Fernand", "Carmen")
            };

            var seed = new Random();
            var matches = GetMatches(friends, forbidenMatches, seed);


            var credentials = File.ReadAllLines(crendentialsPaths);
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential()
                    {
                        UserName = credentials[0],
                        Password = credentials[1],
                    };
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;

                    smtpClient.Send("chekill.pg@gmail.com", "philippe.e.gerber@gmail.com", "Test amis", "this is a body");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("{0}: {1}", e.ToString(), e.Message);
            }

        }

        private static List<ChristmasMatch> GetMatches(List<Ami> friends, List<ChristmasMatch> forbidenMatches, Random seed)
        {
            var givers = friends.Select(f => f.Name).ToList();
            var receivers = friends.Select(f => f.Name).ToList();
            var matchCount = friends.Count();
            var matches = new List<ChristmasMatch>();
            for (int i = 0; i < matchCount; i++)
            {
                var match = GetMatch(forbidenMatches, seed, givers, receivers, matches, friends);

                _ = givers.Remove(match.Giver.Name);
                _ = receivers.Remove(match.Receiver.Name);

                matches.Add(match);
            }

            return matches;
        }

        private static ChristmasMatch GetMatch(
            List<ChristmasMatch> forbidenMatches,
            Random seed,
            List<string> givers,
            List<string> receivers,
            List<ChristmasMatch> matches,
            List<Ami> friends)
        {
            var isValid = false;
            var match = default(ChristmasMatch);
            while (!isValid && match == null)
            {
                match = DrawMatch(receivers, givers, seed, friends);

                isValid = true;
                var inverseMatch =
                        matches
                            .Where(
                                m => m.Giver.Name == match.Receiver.Name
                                    && m.Receiver.Name == match.Giver.Name)
                            .FirstOrDefault();

                var isFobidenMatch =
                        forbidenMatches
                            .Where(m => m.IsEqual(match))
                            .Any();

                if (match.Giver == match.Receiver
                    || inverseMatch != null
                    || givers.Contains(match.Receiver.Name)
                    || isFobidenMatch)
                {
                    isValid = false;
                }
            }

            return match;
        }

        private static ChristmasMatch DrawMatch(List<string> receivers, List<string> givers, Random seed, List<Ami> friends)
        {
            var g = seed.Next(0, givers.Count);
            var r = seed.Next(0, receivers.Count);

            var giver = friends.Where(f => f.Name == givers[g]).Single();
            var receiver = friends.Where(f => f.Name == receivers[r]).Single();

            var match = new ChristmasMatch() { Giver = giver, Receiver = receiver };
            return match;
        }
    }
}