using Querify.Tools.Seed.Application;

namespace Querify.Tools.Seed;

public static class Program
{
    public static int Main()
    {
        return SeedApplication.Build().Run();
    }
}