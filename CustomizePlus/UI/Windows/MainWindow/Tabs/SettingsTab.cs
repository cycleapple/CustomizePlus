using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using OtterGui.Classes;
using OtterGui;
using OtterGui.Raii;
using OtterGui.Widgets;
using System.Diagnostics;
using System.Numerics;
using CustomizePlus.Core.Services;
using CustomizePlus.Configuration.Data;
using CustomizePlus.Profiles;
using CustomizePlus.Templates;
using CustomizePlus.Core.Helpers;
using CustomizePlus.Armatures.Services;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin;

namespace CustomizePlus.UI.Windows.MainWindow.Tabs;

public class SettingsTab
{
    private const uint DiscordColor = 0xFFDA8972;
    private const uint DonateColor = 0xFF5B5EFF;

    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly PluginConfiguration _configuration;
    private readonly ArmatureManager _armatureManager;
    private readonly HookingService _hookingService;
    private readonly TemplateEditorManager _templateEditorManager;
    private readonly CPlusChangeLog _changeLog;
    private readonly MessageService _messageService;
    private readonly SupportLogBuilderService _supportLogBuilderService;

    public SettingsTab(
        IDalamudPluginInterface pluginInterface,
        PluginConfiguration configuration,
        ArmatureManager armatureManager,
        HookingService hookingService,
        TemplateEditorManager templateEditorManager,
        CPlusChangeLog changeLog,
        MessageService messageService,
        SupportLogBuilderService supportLogBuilderService)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
        _armatureManager = armatureManager;
        _hookingService = hookingService;
        _templateEditorManager = templateEditorManager;
        _changeLog = changeLog;
        _messageService = messageService;
        _supportLogBuilderService = supportLogBuilderService;
    }

    public void Draw()
    {
        UiHelpers.SetupCommonSizes();
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;

        DrawGeneralSettings();

        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();

        using (var child2 = ImRaii.Child("SettingsChild"))
        {
            DrawProfileApplicationSettings();
            DrawInterface();
            DrawCommands();
            DrawAdvancedSettings();
        }

        DrawSupportButtons();
    }

    #region General Settings
    // General Settings
    private void DrawGeneralSettings()
    {
        DrawPluginEnabledCheckbox();
    }

    private void DrawPluginEnabledCheckbox()
    {
        using (var disabled = ImRaii.Disabled(_templateEditorManager.IsEditorActive))
        {
            var isChecked = _configuration.PluginEnabled;

            //users doesn't really need to know what exactly this checkbox does so we just tell them it toggles all profiles
        if (CtrlHelper.CheckboxWithTextAndHelp("##pluginenabled", "啟用 Customize+",
                "全域啟用或停用插件的所有功能。", ref isChecked))
            {
                _configuration.PluginEnabled = isChecked;
                _configuration.Save();
                _hookingService.ReloadHooks();
            }
        }
    }
    #endregion

    #region Profile application settings
    private void DrawProfileApplicationSettings()
    {
            var isShouldDraw = ImGui.CollapsingHeader("設定檔套用");

        if (!isShouldDraw)
            return;

        DrawApplyInCharacterWindowCheckbox();
        DrawApplyInTryOnCheckbox();
        DrawApplyInCardsCheckbox();
        DrawApplyInInspectCheckbox();
        DrawApplyInLobbyCheckbox();
    }

    private void DrawApplyInCharacterWindowCheckbox()
    {
        var isChecked = _configuration.ProfileApplicationSettings.ApplyInCharacterWindow;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##applyincharwindow", "在角色視窗套用設定檔",
                            "若已設定，便在主要角色視窗中套用自己角色的設定檔。", ref isChecked))
        {
            _configuration.ProfileApplicationSettings.ApplyInCharacterWindow = isChecked;
            _configuration.Save();
            _armatureManager.RebindAllArmatures();
        }
    }

    private void DrawApplyInTryOnCheckbox()
    {
        var isChecked = _configuration.ProfileApplicationSettings.ApplyInTryOn;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##applyintryon", "在試穿視窗套用設定檔",
                            "若已設定，便在試穿、染色預覽或投影台視窗中套用自己角色的設定檔。", ref isChecked))
        {
            _configuration.ProfileApplicationSettings.ApplyInTryOn = isChecked;
            _configuration.Save();
            _armatureManager.RebindAllArmatures();
        }
    }

    private void DrawApplyInCardsCheckbox()
    {
        var isChecked = _configuration.ProfileApplicationSettings.ApplyInCards;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##applyincards", "在冒險者名片套用設定檔",
                            "為目前查看的冒險者名片套用對應的設定檔。", ref isChecked))
        {
            _configuration.ProfileApplicationSettings.ApplyInCards = isChecked;
            _configuration.Save();
            _armatureManager.RebindAllArmatures();
        }
    }

    private void DrawApplyInInspectCheckbox()
    {
        var isChecked = _configuration.ProfileApplicationSettings.ApplyInInspect;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##applyininspect", "在調查視窗套用設定檔",
                            "為目前調查的角色套用對應的設定檔。", ref isChecked))
        {
            _configuration.ProfileApplicationSettings.ApplyInInspect = isChecked;
            _configuration.Save();
            _armatureManager.RebindAllArmatures();
        }
    }

    private void DrawApplyInLobbyCheckbox()
    {
        var isChecked = _configuration.ProfileApplicationSettings.ApplyInLobby;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##applyinlobby", "在角色選擇畫面套用設定檔",
                            "登入時，為角色選擇畫面上目前選取的角色套用對應的設定檔。", ref isChecked))
        {
            _configuration.ProfileApplicationSettings.ApplyInLobby = isChecked;
            _configuration.Save();
            _armatureManager.RebindAllArmatures();
        }
    }
    #endregion

    #region Chat Commands Settings
    private void DrawCommands()
    {
            var isShouldDraw = ImGui.CollapsingHeader("聊天指令");

        if (!isShouldDraw)
            return;

        DrawPrintSuccessMessages();
    }

    private void DrawPrintSuccessMessages()
    {
        var isChecked = _configuration.CommandSettings.PrintSuccessMessages;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##displaychatcommandconfirms", "在聊天欄顯示指令執行成功訊息",
                            "控制聊天指令成功執行時，是否另外顯示確認訊息。", ref isChecked))
        {
            _configuration.CommandSettings.PrintSuccessMessages = isChecked;
            _configuration.Save();
        }
    }
    #endregion

    #region Interface Settings

    private void DrawInterface()
    {
            var isShouldDraw = ImGui.CollapsingHeader("介面");

        if (!isShouldDraw)
            return;

        DrawOpenWindowAtStart();
        DrawHideWindowInCutscene();
        DrawHideWindowWhenUiHidden();
        DrawHideWindowInGPose();

        UiHelpers.DefaultLineSpace();

        DrawFoldersDefaultOpen();

        UiHelpers.DefaultLineSpace();

        DrawSetPreviewToCurrentCharacterOnLogin();

        UiHelpers.DefaultLineSpace();

                    if (Widget.DoubleModifierSelector("刪除範本的輔助鍵",
                            "按下「刪除範本」按鈕時，必須同時按住此輔助鍵才會生效。", 100 * ImGuiHelpers.GlobalScale,
            _configuration.UISettings.DeleteTemplateModifier, v => _configuration.UISettings.DeleteTemplateModifier = v))
            _configuration.Save();
    }

    private void DrawOpenWindowAtStart()
    {
        var isChecked = _configuration.UISettings.OpenWindowAtStart;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##openwindowatstart", "遊戲啟動時開啟 Customize+ 視窗",
                            "控制遊戲啟動時是否自動開啟 Customize+ 主視窗。", ref isChecked))
        {
            _configuration.UISettings.OpenWindowAtStart = isChecked;

            _configuration.Save();
        }
    }

    private void DrawHideWindowInCutscene()
    {
        var isChecked = _configuration.UISettings.HideWindowInCutscene;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##hidewindowincutscene", "過場動畫期間隱藏插件視窗",
                            "控制過場動畫期間是否隱藏所有 Customize+ 視窗。", ref isChecked))
        {
            _pluginInterface.UiBuilder.DisableCutsceneUiHide = !isChecked;
            _configuration.UISettings.HideWindowInCutscene = isChecked;

            _configuration.Save();
        }
    }

    private void DrawHideWindowWhenUiHidden()
    {
        var isChecked = _configuration.UISettings.HideWindowWhenUiHidden;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##hidewindowwhenuihidden", "遊戲介面隱藏時隱藏插件視窗",
                            "控制手動隱藏遊戲介面時，是否同時隱藏所有 Customize+ 視窗。", ref isChecked))
        {
            _pluginInterface.UiBuilder.DisableUserUiHide = !isChecked;
            _configuration.UISettings.HideWindowWhenUiHidden = isChecked;
            _configuration.Save();
        }
    }

    private void DrawHideWindowInGPose()
    {
        var isChecked = _configuration.UISettings.HideWindowInGPose;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##hidewindowingpose", "進入團體姿勢時隱藏插件視窗",
                            "控制進入團體姿勢時是否隱藏所有 Customize+ 視窗。", ref isChecked))
        {
            _pluginInterface.UiBuilder.DisableGposeUiHide = !isChecked;
            _configuration.UISettings.HideWindowInGPose = isChecked;
            _configuration.Save();
        }
    }

    private void DrawFoldersDefaultOpen()
    {
        var isChecked = _configuration.UISettings.FoldersDefaultOpen;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##foldersdefaultopen", "預設展開所有資料夾",
                            "控制範本與設定檔清單中的資料夾是否預設展開。", ref isChecked))
        {
            _configuration.UISettings.FoldersDefaultOpen = isChecked;
            _configuration.Save();
        }
    }

    private void DrawSetPreviewToCurrentCharacterOnLogin()
    {
        var isChecked = _configuration.EditorConfiguration.SetPreviewToCurrentCharacterOnLogin;

                    if (CtrlHelper.CheckboxWithTextAndHelp("##setpreviewcharaonlogin", "自動將目前角色設為編輯器預覽角色",
                            "控制登入時是否自動將編輯器角色設為目前角色。", ref isChecked))
        {
            _configuration.EditorConfiguration.SetPreviewToCurrentCharacterOnLogin = isChecked;
            _configuration.Save();
        }
    }

    #endregion

    #region Advanced Settings
    // Advanced Settings
    private void DrawAdvancedSettings()
    {
            var isShouldDraw = ImGui.CollapsingHeader("進階");

        if (!isShouldDraw)
            return;

        ImGui.NewLine();
        CtrlHelper.LabelWithIcon(FontAwesomeIcon.ExclamationTriangle,
                        "以下為進階設定，請自行承擔啟用風險。");
        ImGui.NewLine();

        DrawEnableRootPositionCheckbox();
        DrawDebugModeCheckbox();
    }

    private void DrawEnableRootPositionCheckbox()
    {
        var isChecked = _configuration.EditorConfiguration.RootPositionEditingEnabled;
                    if (CtrlHelper.CheckboxWithTextAndHelp("##rootpos", "根骨骼編輯",
                            "允許編輯根骨骼。", ref isChecked))
        {
            _configuration.EditorConfiguration.RootPositionEditingEnabled = isChecked;
            _configuration.Save();
        }
    }

    private void DrawDebugModeCheckbox()
    {
        var isChecked = _configuration.DebuggingModeEnabled;
                    if (CtrlHelper.CheckboxWithTextAndHelp("##debugmode", "偵錯模式",
                            "啟用偵錯模式。必須重新啟動插件，所有功能才能正確初始化。", ref isChecked))
        {
            _configuration.DebuggingModeEnabled = isChecked;
            _configuration.Save();
        }
    }

    #endregion

    #region Support Area
    private void DrawSupportButtons()
    {
        var width = ImGui.CalcTextSize("將支援資訊複製到剪貼簿").X + ImGui.GetStyle().FramePadding.X * 2;
        var xPos = ImGui.GetWindowWidth() - width;
        // Respect the scroll bar width.
        if (ImGui.GetScrollMaxY() > 0)
            xPos -= ImGui.GetStyle().ScrollbarSize + ImGui.GetStyle().FramePadding.X;

        ImGui.SetCursorPos(new Vector2(xPos, 0));
        DrawUrlButton("加入 Discord 取得支援", "https://discord.gg/KvGJCCnG8t", DiscordColor, width,
            "加入由社群志工管理的 Discord 伺服器以取得協助。將在瀏覽器中開啟 https://discord.gg/KvGJCCnG8t。");

        ImGui.SetCursorPos(new Vector2(xPos, ImGui.GetFrameHeightWithSpacing()));
        DrawUrlButton("透過 Ko-fi 支持開發者", "https://ko-fi.com/risadev", DonateColor, width,
            "所有贊助皆為自願，代表對 Customize+ 開發工作的感謝。將在瀏覽器中開啟 https://ko-fi.com/risadev。");

        ImGui.SetCursorPos(new Vector2(xPos, 2 * ImGui.GetFrameHeightWithSpacing()));
        if (ImGui.Button("將支援資訊複製到剪貼簿"))
        {
            var text = _supportLogBuilderService.BuildSupportLog();
            ImGui.SetClipboardText(text);
            _messageService.NotificationMessage("已將支援資訊複製到剪貼簿。", NotificationType.Success, false);
        }

        ImGui.SetCursorPos(new Vector2(xPos, 3 * ImGui.GetFrameHeightWithSpacing()));
        if (ImGui.Button("顯示更新記錄", new Vector2(width, 0)))
            _changeLog.Changelog.ForceOpen = true;
    }

    /// <summary> Draw a button to open some url. </summary>
    private void DrawUrlButton(string text, string url, uint buttonColor, float width, string? description = null)
    {
        using var color = ImRaii.PushColor(ImGuiCol.Button, buttonColor);
        if (ImGui.Button(text, new Vector2(width, 0)))
            try
            {
                var process = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                };
                Process.Start(process);
            }
            catch
            {
            _messageService.NotificationMessage($"無法開啟網址 {url}。", NotificationType.Error, false);
            }

        ImGuiUtil.HoverTooltip(description ?? $"開啟 {url}");
    }
    #endregion
}
