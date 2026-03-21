using Microsoft.AspNetCore.Hosting;
using WebApp.ViewModels;

namespace WebApp.Services;

/// <summary>
/// Scans source files and counts lines of code per file type.
/// The result is computed once and cached for the lifetime of the application.
/// </summary>
public sealed class CodeStatsService
{
    private static readonly (string Extension, string Label)[] TrackedTypes =
    [
        (".cs",     "C#"),
        (".cshtml", "Razor"),
        (".css",    "CSS"),
        (".js",     "JavaScript"),
    ];

    private static readonly HashSet<string> ExcludedDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj", "node_modules", "Migrations", ".git",
    };

    private readonly Lazy<CodeStatsVm> _stats;

    public CodeStatsService(IWebHostEnvironment env)
    {
        _stats = new Lazy<CodeStatsVm>(() => ComputeStats(env.ContentRootPath));
    }

    public CodeStatsVm GetStats() => _stats.Value;

    private static CodeStatsVm ComputeStats(string contentRootPath)
    {
        var searchRoot = FindSolutionRoot(contentRootPath) ?? contentRootPath;

        var rows = new List<CodeStatsVm.FileTypeRow>();
        var totalFiles = 0;
        var totalLines = 0;

        foreach (var (ext, label) in TrackedTypes)
        {
            var files = EnumerateSourceFiles(searchRoot, ext);
            var lineCount = files.Sum(CountLines);

            rows.Add(new CodeStatsVm.FileTypeRow
            {
                Extension = ext,
                Label     = label,
                FileCount = files.Count,
                LineCount = lineCount,
            });

            totalFiles += files.Count;
            totalLines += lineCount;
        }

        return new CodeStatsVm
        {
            Rows       = rows,
            TotalFiles = totalFiles,
            TotalLines = totalLines,
        };
    }

    /// <summary>Walk up from <paramref name="startPath"/> until a *.sln file is found.</summary>
    private static string? FindSolutionRoot(string startPath)
    {
        var dir = startPath;
        for (var i = 0; i < 5; i++)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0)
                return dir;

            var parent = Directory.GetParent(dir)?.FullName;
            if (parent is null || parent == dir)
                break;

            dir = parent;
        }
        return null;
    }

    private static List<string> EnumerateSourceFiles(string root, string extension)
    {
        var result = new List<string>();
        CollectFiles(new DirectoryInfo(root), extension, result);
        return result;
    }

    private static void CollectFiles(DirectoryInfo dir, string extension, List<string> result)
    {
        foreach (var file in dir.EnumerateFiles("*" + extension, SearchOption.TopDirectoryOnly))
            result.Add(file.FullName);

        foreach (var sub in dir.EnumerateDirectories())
        {
            if (ExcludedDirs.Contains(sub.Name))
                continue;

            CollectFiles(sub, extension, result);
        }
    }

    private static int CountLines(string filePath)
    {
        try
        {
            var lines = 0;
            using var reader = new StreamReader(filePath);
            while (reader.ReadLine() is not null)
                lines++;
            return lines;
        }
        catch
        {
            return 0;
        }
    }
}
