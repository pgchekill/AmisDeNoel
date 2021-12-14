using Newtonsoft.Json;
using System.Diagnostics;

namespace AmisDeNoel
{
    public class Program
    {
        static void Main(string[] args)
        {
            var configPath = @"C:\Users\gerber\Documents\git-p\amis-de-noel\amis.json";
            var crendentialsPaths = @"C:\Users\gerber\Documents\git-p\amis-de-noel\app-pass.txt";
            var htmlTemplatePath = @"C:\Users\gerber\Documents\git-p\amis-de-noel\src\AmisDeNoel\email-template.html";
            
            var json = File.ReadAllText(configPath);
            var friends = JsonConvert.DeserializeObject<List<Ami>>(json);
            var forbidenFriends =
                    friends
                        .Where(f => f.ForbidenFriends != null)
                        .SelectMany(
                            f => f.ForbidenFriends
                                    .Select(ff => new ChristmasMatch(f.Name, ff)))
                        .ToList();

            var seed = new Random();
            var matches = GetMatches(friends, forbidenFriends, seed);

            var htmlTemplate = File.ReadAllText(htmlTemplatePath);
            var credentials = File.ReadAllLines(crendentialsPaths);
            var user = credentials[0];
            var pass = credentials[1];

            FriendEmailSender.SendEmails(matches, htmlTemplatePath, user, pass);
        }

        private static List<ChristmasMatch> GetMatches(List<Ami> friends, List<ChristmasMatch> forbidenMatches, Random seed)
        {
            var givers = friends.Select(f => f.Name).ToList();
            var receivers = friends.Select(f => f.Name).ToList();
            var matchCount = friends.Count();
            var matches = new List<ChristmasMatch>();
            var currentGiver = givers[seed.Next(0, givers.Count)];
            for (int i = 0; i < matchCount; i++)
            {
                var match = GetMatch(currentGiver, forbidenMatches, seed, givers, receivers, matches, friends);

                _ = givers.Remove(match.Giver.Name);
                _ = receivers.Remove(match.Receiver.Name);

                currentGiver = match.Receiver.Name;

                matches.Add(match);
            }

            return matches;
        }

        private static ChristmasMatch GetMatch(
            string currentGiver,
            List<ChristmasMatch> forbidenMatches,
            Random seed,
            List<string> givers,
            List<string> receivers,
            List<ChristmasMatch> matches,
            List<Ami> friends)
        {
            var isNotValid = true;
            var match = default(ChristmasMatch);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (isNotValid || match == null)
            {
                match = DrawReceiver(currentGiver, receivers, givers, seed, friends);

                isNotValid = false;
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

                if (match.Giver.Name == match.Receiver.Name
                    || inverseMatch != null
                    || isFobidenMatch)
                {
                    isNotValid = true;
                }

                if (stopWatch.Elapsed.TotalSeconds > 2)
                    throw new Exception("Couldn't find a suitable match line... Please retry.");
            }

            return match;
        }

        private static ChristmasMatch DrawReceiver(string currentGiver, List<string> receivers, List<string> givers, Random seed, List<Ami> friends)
        {
            var r = seed.Next(0, receivers.Count);

            var giver = friends.Where(f => f.Name == currentGiver).Single();
            var receiver = friends.Where(f => f.Name == receivers[r]).Single();

            var match = new ChristmasMatch() { Giver = giver, Receiver = receiver };

            return match;
        }
    }
}