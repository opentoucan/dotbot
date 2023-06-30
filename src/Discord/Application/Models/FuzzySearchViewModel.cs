namespace Discord.Application.Models;

public class FuzzySearchViewModel
{
    public string Value { get; private set; }
    public decimal Score { get; private set; }

    public FuzzySearchViewModel(string value, decimal score)
    {
        Value = value;
        Score = score;
    }
}