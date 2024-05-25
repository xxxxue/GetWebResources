using System;

namespace GetWebResources.Model;

public class ResourcesModel
{
    public string Url { get; set; }

    public string Ext { get; set; }

    public string Host { get; set; }

    public string Name { get; set; }

    public Uri uri { get; set; }
}