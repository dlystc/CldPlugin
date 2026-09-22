using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Helpers.UI;
using ClassIsland.Platforms.Abstraction;
using MessageBox = System.Windows.MessageBox;

namespace UMP.DlyStc.Plugin.Cld.Components;

public partial class StcSettings : ComponentBase<StcConfig> {
    const long MaxImportFileSizeBytes = 1024 * 1024;
    const int MaxImportEntries = 5000;
    const int MaxEntryLength = 200;
    
    public StcSettings() {
        InitializeComponent();
    }

    public List<StcConfig.AttributesDisplayRule> AttributesRules { get; } = [
        StcConfig.AttributesDisplayRule.Sametime,
        StcConfig.AttributesDisplayRule.Separate
    ];

    async void ImportIgnoreListButton_OnClick(object sender,RoutedEventArgs e) {
        try {
            PopupHelper.DisableAllPopups();
            List<string> files = await PlatformServices.FilePickerService.OpenFilesPickerAsync(new FilePickerOpenOptions {
                Title = "导入排除列表",
                FileTypeFilter = [FilePickerFileTypes.TextPlain]
            },TopLevel.GetTopLevel(this) ?? AppBase.Current.GetRootWindow());
            PopupHelper.RestoreAllPopups();

            if (files.Count == 0) return;

            string path = files[0];
            if (new FileInfo(path).Length > MaxImportFileSizeBytes) {
                MessageBox.Show($"导入失败: 文件大小超过 {MaxImportFileSizeBytes / 1024} KB 限制","导入排除列表");
                return;
            }

            List<string> entries = Settings.IgnoreListString
                .Split("\r\n",StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            foreach (string line in await File.ReadAllLinesAsync(path)) {
                string entry = line.Trim();
                if (entry.Length == 0) continue;
                if (entry.Length > MaxEntryLength) entry = entry[..MaxEntryLength];
                if (!entries.Contains(entry)) entries.Add(entry);
            }

            if (entries.Count > MaxImportEntries) {
                MessageBox.Show($"导入失败: 排除列表条目数超过 {MaxImportEntries} 条限制","导入排除列表");
                return;
            }

            Settings.IgnoreListString = string.Join("\r\n",entries);
        }
        catch (Exception exception) {
            PopupHelper.RestoreAllPopups();
            Console.WriteLine(exception);
        }
    }
}