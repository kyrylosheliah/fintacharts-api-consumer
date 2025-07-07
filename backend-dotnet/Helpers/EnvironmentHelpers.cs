namespace BackendDotnet.Helpers;

public static class EnvironmentHelpers
{
    public static void ApplyEnvironmentVariablesToConfiguration(
        WebApplicationBuilder builder,
        List<EnvironmentVariableMapping> mappings
    )
    {
        var configDict = new Dictionary<string, string?>();
        foreach (var mapping in mappings)
        {
            var environmentValue = Environment.GetEnvironmentVariable(mapping.name);
            if (string.IsNullOrEmpty(environmentValue))
                throw new InvalidOperationException($"Missing required environment variable '{mapping.name}'");
            configDict.Add(mapping.configName, environmentValue);
        }
        builder.Configuration.AddInMemoryCollection(configDict);
    }
}

public record EnvironmentVariableMapping {
    public string name = "";
    public string configName = "";
}