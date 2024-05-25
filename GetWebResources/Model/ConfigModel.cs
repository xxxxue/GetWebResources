using System.Collections.Generic;

using Newtonsoft.Json;

namespace GetWebResources.Model;

public class ConfigModel
{
    [JsonProperty("SavedPath")]
    public string BasePath { get; set; }

    [JsonProperty("ExcludeKeywordsfromExtensions")]
    public List<string> ExcludeKeyWordList { get; set; }

    [JsonProperty("ContainsHostList")]
    public List<string> ContainsHostList { get; set; }
}