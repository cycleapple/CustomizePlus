using Dalamud.Interface.Utility;
using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using CustomizePlus.Core.Services;
using CustomizePlus.Game.Services;
using CustomizePlus.Configuration.Data;
using CustomizePlus.UI.Windows.MainWindow.Tabs.Templates;
using CustomizePlus.Core.Helpers;
using CustomizePlus.Api;
using CustomizePlus.Core.Data;
using CustomizePlus.Core.Services.Dalamud;

namespace CustomizePlus.UI.Windows.Controls;

public class PluginStateBlock
{
    private readonly BoneEditorPanel _boneEditorPanel;
    private readonly PluginConfiguration _configuration;
    private readonly GameStateService _gameStateService;
    private readonly HookingService _hookingService;
    private readonly CustomizePlusIpc _ipcService;

    public PluginStateBlock(
        BoneEditorPanel boneEditorPanel,
        PluginConfiguration configuration,
        GameStateService gameStateService,
        HookingService hookingService,
        CustomizePlusIpc ipcService)
    {
        _boneEditorPanel = boneEditorPanel;
        _configuration = configuration;
        _gameStateService = gameStateService;
        _hookingService = hookingService;
        _ipcService = ipcService;
    }

    public void Draw(float yPos)
    {
        var severity = PluginStateSeverity.Normal;
        string? message = null;
        string? hoverInfo = null;

        if(_hookingService.RenderHookFailed || _hookingService.MovementHookFailed)
        {
            severity = PluginStateSeverity.Error;
            message = "偵測到遊戲 Hook 發生錯誤，Customize+ 已停用。";
        }
        else if (!_configuration.PluginEnabled)
        {
            severity = PluginStateSeverity.Warning;
            message = "插件目前已停用，無法使用範本骨骼編輯。";
        }
        else if (_boneEditorPanel.IsEditorActive)
        {
            if (!_boneEditorPanel.IsCharacterFound)
            {
                severity = PluginStateSeverity.Error;
            message = "找不到選取的預覽角色。";
            }
            else
            {
                if (_boneEditorPanel.HasChanges)
                    severity = PluginStateSeverity.Warning;

            message = $"編輯器使用中。{(_boneEditorPanel.HasChanges ? " 尚有未儲存的變更；結束範本骨骼編輯即可開啟儲存／還原視窗。" : "")}";
            }
        }
        else if (_gameStateService.GameInPosingMode())
        {
            severity = PluginStateSeverity.Warning;
            message = "團體姿勢使用中，與其他擺姿勢工具的相容性有限。";
        }
        else if (_ipcService.IPCFailed) //this is a low priority error
        {
            severity = PluginStateSeverity.Error;
            message = "偵測到 IPC 發生錯誤，與其他插件的整合功能將無法運作。";
        }
        else if(VersionHelper.IsTesting)
        {
            severity = PluginStateSeverity.Warning;
            message = "目前使用的是 Customize+ 測試版本，將滑鼠移至此處可查看詳情。";
            hoverInfo = "這是 Customize+ 的測試組建，與其他插件整合等部分功能可能無法正常運作。";
        }

        if (message != null)
        {
            ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - ImGui.CalcTextSize(message).X - 30, yPos - ImGuiHelpers.GlobalScale));

            var icon = FontAwesomeIcon.InfoCircle;
            var color = Constants.Colors.Normal;
            switch (severity)
            {
                case PluginStateSeverity.Warning:
                    icon = FontAwesomeIcon.ExclamationTriangle;
                    color = Constants.Colors.Warning;
                    break;
                case PluginStateSeverity.Error:
                    icon = FontAwesomeIcon.ExclamationTriangle;
                    color = Constants.Colors.Error;
                    break;
            }

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            CtrlHelper.LabelWithIcon(icon, message, false);
            ImGui.PopStyleColor();
            if (hoverInfo != null)
                CtrlHelper.AddHoverText(hoverInfo);
        }
    }

    private enum PluginStateSeverity
    {
        Normal,
        Warning,
        Error
    }
}
