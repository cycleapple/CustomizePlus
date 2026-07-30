using CustomizePlus.Core.Services.Dalamud;
using System.Numerics;

namespace CustomizePlus.UI.Windows;

public partial class PopupSystem
{
    public static class Messages
    {
        public const string ActionError = "action_error";
        public const string ActionDone = "action_done";

        public const string FantasiaPlusDetected = "fantasia_detected_warn";

        public const string IPCProfileRemembered = "ipc_profile_remembered";
        public const string IPCGetProfileByIdRemembered = "ipc_get_profile_by_id_remembered";
        public const string IPCSetProfileToChrDone = "ipc_set_profile_to_character_done";
        public const string IPCRevertDone = "ipc_revert_done";
        public const string IPCCopiedToClipboard = "ipc_copied_to clipboard";
        public const string IPCSuccessfullyExecuted = "ipc_successfully_executed";
        public const string IPCEnableProfileByIdDone = "ipc_enable_profile_by_id_done";
        public const string IPCDisableProfileByIdDone = "ipc_disable_profile_by_id_done";

        public const string TemplateEditorActiveWarning = "template_editor_active_warn";
        public const string ClipboardDataUnsupported = "clipboard_data_unsupported_version";

        public const string ClipboardDataNotLongTerm = "clipboard_data_not_longterm";

        public const string PluginDisabledNonReleaseDalamud = "non_release_dalamud";
    }

    private void RegisterMessages()
    {
        RegisterPopup(Messages.ActionError, "執行所選操作時發生錯誤。\n詳細資訊已寫入 Dalamud 日誌（聊天欄輸入 /xllog）。");
        RegisterPopup(Messages.ActionDone, "操作已成功執行。");

        RegisterPopup(Messages.FantasiaPlusDetected, "Customize+ 偵測到已安裝 Fantasia+。\n請移除或停用 Fantasia+，並重新啟動遊戲後再使用 Customize+。");

        RegisterPopup(Messages.IPCProfileRemembered, "目前設定檔已複製到記憶體");
        RegisterPopup(Messages.IPCGetProfileByIdRemembered, "GetProfileByUniqueId 的結果已複製到記憶體");
        RegisterPopup(Messages.IPCSetProfileToChrDone, "已使用記憶體中的資料呼叫 SetProfileToCharacter，設定檔 ID 已寫入日誌");
        RegisterPopup(Messages.IPCRevertDone, "已呼叫 DeleteTemporaryProfileByUniqueId");
        RegisterPopup(Messages.IPCCopiedToClipboard, "已複製到剪貼簿");
        RegisterPopup(Messages.IPCSuccessfullyExecuted, "執行成功");
        RegisterPopup(Messages.IPCEnableProfileByIdDone, "已呼叫依 ID 啟用設定檔");
        RegisterPopup(Messages.IPCDisableProfileByIdDone, "已呼叫依 ID 停用設定檔");

        RegisterPopup(Messages.TemplateEditorActiveWarning, "必須先停止骨骼編輯，才能執行此操作");
        RegisterPopup(Messages.ClipboardDataUnsupported, "目前版本的 Customize+ 無法使用此剪貼簿資料。");

        RegisterPopup(Messages.ClipboardDataNotLongTerm, "警告：剪貼簿資料並非用於長期保存範本。\n無法保證不同 Customize+ 版本之間的剪貼簿資料相容性。", true, new Vector2(5, 10));
    }
}
