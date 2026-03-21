namespace WebApp.ViewModels;

public sealed class CodeStatsVm
{
    public List<FileTypeRow> Rows { get; init; } = new();
    public int TotalFiles { get; init; }
    public int TotalLines { get; init; }

    public sealed class FileTypeRow
    {
        public string Extension { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public int FileCount { get; init; }
        public int LineCount { get; init; }
    }
}
