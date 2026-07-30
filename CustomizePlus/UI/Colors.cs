using System.Collections.Generic;

namespace CustomizePlus.UI;

public enum ColorId
{
    UsedTemplate,
    UnusedTemplate,
    EnabledProfile,
    DisabledProfile,
    LocalCharacterEnabledProfile,
    LocalCharacterDisabledProfile,
    FolderExpanded,
    FolderCollapsed,
    FolderLine,
    HeaderButtons,
}

public static class Colors
{
    public const uint SelectedRed = 0xFF2020D0;

    public static (uint DefaultColor, string Name, string Description) Data(this ColorId color)
        => color switch
        {
            // @formatter:off
            ColorId.UsedTemplate => (0xFFFFFFFF, "使用中的範本", "至少由一個設定檔使用的範本。"),
            //ColorId.EnabledTemplate => (0xFFA0F0A0, "Enabled Automation Set", "An automation set that is currently enabled. Only one set can be enabled for each identifier at once."),
            ColorId.UnusedTemplate => (0xFF808080, "未使用的範本", "目前未由任何設定檔使用的範本。"),
            ColorId.EnabledProfile => (0xFFFFFFFF, "已啟用的設定檔", "目前已啟用的設定檔。"),
            ColorId.DisabledProfile => (0xFF808080, "已停用的設定檔", "目前已停用的設定檔。"),
            ColorId.LocalCharacterEnabledProfile => (0xFF18C018, "目前角色的設定檔（已啟用）", "目前已啟用且與自己角色關聯的設定檔。"),
            ColorId.LocalCharacterDisabledProfile => (0xFF808080, "目前角色的設定檔（已停用）", "目前已停用且與自己角色關聯的設定檔。"),
            ColorId.FolderExpanded => (0xFFFFF0C0, "展開的資料夾", "目前已展開的資料夾。"),
            ColorId.FolderCollapsed => (0xFFFFF0C0, "收合的資料夾", "目前已收合的資料夾。"),
            ColorId.FolderLine => (0xFFFFF0C0, "展開資料夾的連線", "用來標示哪些子項目屬於已展開資料夾的連線。"),
            ColorId.HeaderButtons => (0xFFFFF0C0, "標題列按鈕", "標題列按鈕（如寫入保護切換）的文字與邊框顏色。"),
            _ => (0x00000000, string.Empty, string.Empty),
            // @formatter:on
        };

    private static IReadOnlyDictionary<ColorId, uint> _colors = new Dictionary<ColorId, uint>();

    /// <summary> Obtain the configured value for a color. </summary>
    public static uint Value(this ColorId color)
        => _colors.TryGetValue(color, out var value) ? value : color.Data().DefaultColor;

    /// <summary> Set the configurable colors dictionary to a value. </summary>
    /*public static void SetColors(Configuration config)
        => _colors = config.Colors;*/
}
