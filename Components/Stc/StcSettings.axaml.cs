using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Helpers.UI;
using ClassIsland.Platforms.Abstraction;
using UMP.DlyStc.Plugin.Cld.Components.Stc;

namespace UMP.DlyStc.Plugin.Cld.Components;

public partial class StcSettings : ComponentBase<StcConfig>
{
    const long MaxImportFileSizeBytes = 1024 * 1024;
    const int MaxImportEntries = 5000;
    const int MaxEntryLength = 200;

    public StcSettings()
    {
        InitializeComponent();
    }

    public List<StcConfig.AttributesDisplayRule> AttributesRules { get; } = [
        StcConfig.AttributesDisplayRule.Sametime,
        StcConfig.AttributesDisplayRule.Separate
    ];

    async void ImportIgnoreListButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            PopupHelper.DisableAllPopups();
            List<string> files = await PlatformServices.FilePickerService.OpenFilesPickerAsync(new FilePickerOpenOptions
            {
                Title = "导入排除列表",
                FileTypeFilter = [FilePickerFileTypes.TextPlain]
            }, TopLevel.GetTopLevel(this) ?? AppBase.Current.GetRootWindow());

            if (files.Count == 0) return;

            string path = files[0];
            if (new FileInfo(path).Length > MaxImportFileSizeBytes)
            {
                await ShowMessageBoxAsync(this, "导入排除列表", $"导入失败: 文件大小超过 {MaxImportFileSizeBytes / 1024} KB 限制");

                return;
            }

            List<string> entries = [.. Settings.IgnoreListString.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
            foreach (string line in await File.ReadAllLinesAsync(path))
            {
                string entry = line.Trim();

                if (entry.Length == 0) continue;

                if (entry.Length > MaxEntryLength) entry = entry[..MaxEntryLength];

                if (!entries.Contains(entry)) entries.Add(entry);
            }

            if (entries.Count > MaxImportEntries)
            {
                await ShowMessageBoxAsync(this, "导入排除列表", $"导入失败: 排除列表条目数超过 {MaxImportEntries} 条限制");

                return;
            }

            Settings.IgnoreListString = string.Join(Environment.NewLine, entries);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
        finally
        {
            PopupHelper.RestoreAllPopups();
        }
    }

    static async Task ShowMessageBoxAsync(Control owner, string title, string message)
    {
        var topLevel = TopLevel.GetTopLevel(owner);
        if (topLevel is Window parentWindow)
        {
            var dialog = new Window
            {
                Title = title,
                Content = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(20)
                },
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = 400,
                MaxHeight = 300
            };
            
            await dialog.ShowDialog(parentWindow);
        }
    }
}