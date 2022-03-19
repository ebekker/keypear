// Keypear Security Tool.
// Copyright (C) Eugene Bekker.

using System.Reflection;
using System.Text.RegularExpressions;
using Keypear.CliClient.Templates;

[assembly: Template("record", "basic record with all possible input fields")]
[assembly: Template("password", "password generator input parameters")]
[assembly: Template("totp", "custom fields for Time-based One-Time Password (TOTP) generation")]


namespace Keypear.CliClient.Templates;

[AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class TemplateAttribute : Attribute
{
    public TemplateAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }

    public string? Description { get; set; }
}

public static class Templates
{
    private static readonly string ResPrefix = $"{typeof(TemplateAttribute).Namespace}.";
    private static readonly string ResSuffix = ".json";

    private static readonly Regex ResPrefixRegex = new Regex($"^{ResPrefix}".Replace(".", "\\."));
    private static readonly Regex ResSuffixRegex = new Regex($"{ResSuffix}$".Replace(".", "\\."));

    public static IEnumerable<KeyValuePair<string, TemplateAttribute?>> GetTemplates()
    {
        var resNames = typeof(Program).Assembly.GetManifestResourceNames().Where(
            x => ResPrefixRegex.IsMatch(x) && ResSuffixRegex.IsMatch(x));

        foreach (var resName in resNames)
        {
            var templateName = ResSuffixRegex.Replace(ResPrefixRegex.Replace(resName, ""), "");

            var meta = typeof(Program).Assembly.GetCustomAttributes<TemplateAttribute>()
                .FirstOrDefault(x => x.Name == templateName);

            yield return new(templateName, meta);
        }
    }

    public static Stream? GetTemplateStream(string name)
    {
        var asm = typeof(Templates).Assembly;
        var resName = $"{ResPrefix}{name}{ResSuffix}";
        var resInfo = asm.GetManifestResourceInfo(resName);

        return resInfo == null
            ? null
            : asm.GetManifestResourceStream(resName);
    }

    public static string? GetTemplate(string name)
    {
        var stream = GetTemplateStream(name);
        if (stream == null)
        {
            return null;
        }

        using (stream)
        using (var str = new MemoryStream())
        {
            stream.CopyTo(str);
            return Encoding.UTF8.GetString(str.ToArray());
        }
    }
}
