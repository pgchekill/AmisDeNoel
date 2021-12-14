using AmisDeNoel;

public class ChristmasMatch
{
    public Ami Giver { get; set; }
    public Ami Receiver { get; set; }

    public new string ToString() => $"{Giver.Name} gives to {Receiver.Name}";
    public bool IsEqual(ChristmasMatch match) => match.Giver.Name == Giver.Name && match.Receiver.Name == Receiver.Name;

    public ChristmasMatch()
    {

    }

    public ChristmasMatch(string giver, string receiver)
    {
        Giver = new Ami() { Name = giver };
        Receiver = new Ami() { Name = receiver };
    }
}
