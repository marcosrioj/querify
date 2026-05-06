namespace Querify.Tools.Seed.Abstractions;

public interface IConsoleAdapter
{
    void Write(string value);
    void WriteLine(string value);
    string? ReadLine();
}