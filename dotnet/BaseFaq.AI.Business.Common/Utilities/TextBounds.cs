namespace BaseFaq.AI.Business.Common.Utilities;

public static class TextBounds
{
    public static string Truncate(string value, int maxLength)
    {
        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength));
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
