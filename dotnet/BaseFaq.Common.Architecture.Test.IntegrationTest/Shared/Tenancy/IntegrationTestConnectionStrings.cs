namespace BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;

public static class IntegrationTestConnectionStrings
{
    public static string Tenant => CreateNamed("basefaq-tenant-tests");

    public static string QnA => CreateNamed("basefaq-qna-tests");

    public static string CreateNamed(string databaseName)
    {
        return $"Data Source=file:{databaseName}?mode=memory&cache=shared";
    }
}
