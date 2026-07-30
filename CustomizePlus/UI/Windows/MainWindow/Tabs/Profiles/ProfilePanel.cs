using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using OtterGui;
using OtterGui.Raii;
using System;
using System.Linq;
using System.Numerics;
using CustomizePlus.Profiles;
using CustomizePlus.Configuration.Data;
using CustomizePlus.Profiles.Data;
using CustomizePlus.UI.Windows.Controls;
using CustomizePlus.Templates;
using CustomizePlus.Core.Data;
using CustomizePlus.Templates.Events;
using Penumbra.GameData.Actors;
using Penumbra.String;
using static FFXIVClientStructs.FFXIV.Client.LayoutEngine.ILayoutInstance;
using CustomizePlus.GameData.Extensions;
using CustomizePlus.Core.Extensions;
using Dalamud.Interface.Components;
using OtterGui.Extensions;

namespace CustomizePlus.UI.Windows.MainWindow.Tabs.Profiles;

public class ProfilePanel
{
    private readonly ProfileFileSystemSelector _selector;
    private readonly ProfileManager _manager;
    private readonly PluginConfiguration _configuration;
    private readonly TemplateCombo _templateCombo;
    private readonly TemplateEditorManager _templateEditorManager;
    private readonly ActorAssignmentUi _actorAssignmentUi;
    private readonly ActorManager _actorManager;
    private readonly TemplateEditorEvent _templateEditorEvent;

    private string? _newName;
    private int? _newPriority;
    private Profile? _changedProfile;

    private Action? _endAction;

    private int _dragIndex = -1;

    private string SelectionName
        => _selector.Selected == null ? "未選取" : _selector.IncognitoMode ? _selector.Selected.Incognito : _selector.Selected.Name.Text;

    public ProfilePanel(
        ProfileFileSystemSelector selector,
        ProfileManager manager,
        PluginConfiguration configuration,
        TemplateCombo templateCombo,
        TemplateEditorManager templateEditorManager,
        ActorAssignmentUi actorAssignmentUi,
        ActorManager actorManager,
        TemplateEditorEvent templateEditorEvent)
    {
        _selector = selector;
        _manager = manager;
        _configuration = configuration;
        _templateCombo = templateCombo;
        _templateEditorManager = templateEditorManager;
        _actorAssignmentUi = actorAssignmentUi;
        _actorManager = actorManager;
        _templateEditorEvent = templateEditorEvent;
    }

    public void Draw()
    {
        using var group = ImRaii.Group();
        if (_selector.SelectedPaths.Count > 1)
        {
            DrawMultiSelection();
        }
        else
        {
            DrawHeader();
            DrawPanel();
        }
    }

    private HeaderDrawer.Button LockButton()
        => _selector.Selected == null
            ? HeaderDrawer.Button.Invisible
            : _selector.Selected.IsWriteProtected
                ? new HeaderDrawer.Button
                {
                Description = "允許編輯此設定檔。",
                    Icon = FontAwesomeIcon.Lock,
                    OnClick = () => _manager.SetWriteProtection(_selector.Selected!, false)
                }
                : new HeaderDrawer.Button
                {
                Description = "將此設定檔設為寫入保護。",
                    Icon = FontAwesomeIcon.LockOpen,
                    OnClick = () => _manager.SetWriteProtection(_selector.Selected!, true)
                };

    private void DrawHeader()
        => HeaderDrawer.Draw(SelectionName, 0, ImGui.GetColorU32(ImGuiCol.FrameBg),
            0, LockButton(),
            HeaderDrawer.Button.IncognitoButton(_selector.IncognitoMode, v => _selector.IncognitoMode = v));

    private void DrawMultiSelection()
    {
        if (_selector.SelectedPaths.Count == 0)
            return;

        var sizeType = ImGui.GetFrameHeight();
        var availableSizePercent = (ImGui.GetContentRegionAvail().X - sizeType - 4 * ImGui.GetStyle().CellPadding.X) / 100;
        var sizeMods = availableSizePercent * 35;
        var sizeFolders = availableSizePercent * 65;

        ImGui.NewLine();
        ImGui.TextUnformatted("目前選取的設定檔");
        ImGui.Separator();
        using var table = ImRaii.Table("profile", 3, ImGuiTableFlags.RowBg);
        ImGui.TableSetupColumn("btn", ImGuiTableColumnFlags.WidthFixed, sizeType);
        ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthFixed, sizeMods);
        ImGui.TableSetupColumn("path", ImGuiTableColumnFlags.WidthFixed, sizeFolders);

        var i = 0;
        foreach (var (fullName, path) in _selector.SelectedPaths.Select(p => (p.FullName(), p))
                     .OrderBy(p => p.Item1, StringComparer.OrdinalIgnoreCase))
        {
            using var id = ImRaii.PushId(i++);
            ImGui.TableNextColumn();
            var icon = (path is ProfileFileSystem.Leaf ? FontAwesomeIcon.FileCircleMinus : FontAwesomeIcon.FolderMinus).ToIconString();
                if (ImGuiUtil.DrawDisabledButton(icon, new Vector2(sizeType), "從選取項目中移除。", false, true))
                _selector.RemovePathFromMultiSelection(path);

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(path is ProfileFileSystem.Leaf l ? _selector.IncognitoMode ? l.Value.Incognito : l.Value.Name.Text : string.Empty);

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(_selector.IncognitoMode ? "匿名模式使用中" : fullName);
        }
    }

    private void DrawPanel()
    {
        using var child = ImRaii.Child("##Panel", -Vector2.One, true);
        if (!child || _selector.Selected == null)
            return;

        DrawEnabledSetting();

        ImGui.Separator();

        using (var disabled = ImRaii.Disabled(_selector.Selected?.IsWriteProtected ?? true))
        {
            DrawBasicSettings();

            ImGui.Separator();

            var isShouldDraw = ImGui.CollapsingHeader("新增角色");

            if (isShouldDraw)
                DrawAddCharactersArea();

            ImGui.Separator();

            DrawCharacterListArea();

            ImGui.Separator();

            DrawTemplateArea();
        }
    }

    private void DrawEnabledSetting()
    {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { X = ImGui.GetStyle().ItemSpacing.X, Y = ImGui.GetStyle().ItemSpacing.Y };

        using (var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing))
        {
            var enabled = _selector.Selected?.Enabled ?? false;
            using (ImRaii.Disabled(_templateEditorManager.IsEditorActive || _templateEditorManager.IsEditorPaused))
            {
                if (ImGui.Checkbox("##Enabled", ref enabled))
                    _manager.SetEnabled(_selector.Selected!, enabled);
                    ImGuiUtil.LabeledHelpMarker("啟用",
                        "是否要套用此設定檔中的範本。");
            }
        }
    }

    private void DrawBasicSettings()
    {
        using (var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)))
        {
            using (var table = ImRaii.Table("BasicSettings", 2))
            {
                ImGui.TableSetupColumn("BasicCol1", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("lorem ipsum dolor").X);
                ImGui.TableSetupColumn("BasicCol2", ImGuiTableColumnFlags.WidthStretch);

                    ImGuiUtil.DrawFrameColumn("設定檔名稱");
                ImGui.TableNextColumn();
                var width = new Vector2(ImGui.GetContentRegionAvail().X, 0);
                var name = _newName ?? _selector.Selected!.Name;
                ImGui.SetNextItemWidth(width.X);

                if (!_selector.IncognitoMode)
                {
                    if (ImGui.InputText("##ProfileName", ref name, 128))
                    {
                        _newName = name;
                        _changedProfile = _selector.Selected;
                    }

                    if (ImGui.IsItemDeactivatedAfterEdit() && _changedProfile != null)
                    {
                        _manager.Rename(_changedProfile, name);
                        _newName = null;
                        _changedProfile = null;
                    }
                }
                else
                    ImGui.TextUnformatted(_selector.Selected!.Incognito);

                ImGui.TableNextRow();

                    ImGuiUtil.DrawFrameColumn("優先度");
                ImGui.TableNextColumn();

                var priority = _newPriority ?? _selector.Selected!.Priority;

                ImGui.SetNextItemWidth(50);
                if (ImGui.InputInt("##Priority", ref priority, 0, 0))
                {
                    _newPriority = priority;
                    _changedProfile = _selector.Selected;
                }

                if (ImGui.IsItemDeactivatedAfterEdit() && _changedProfile != null)
                {
                    _manager.SetPriority(_changedProfile, priority);
                    _newPriority = null;
                    _changedProfile = null;
                }

                    ImGuiComponents.HelpMarker("此處數值較高的設定檔會優先於數值較低者。\n" +
                                               "若兩個以上的設定檔影響同一角色，將套用優先度較高的設定檔。");
            }
        }
    }

    private void DrawAddCharactersArea()
    {
        using (var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)))
        {
            var width = new Vector2(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("Limit to my creatures").X - 68, 0);

            ImGui.SetNextItemWidth(width.X);

            bool appliesToMultiple = _manager.DefaultProfile == _selector.Selected || _manager.DefaultLocalPlayerProfile == _selector.Selected;
            using (ImRaii.Disabled(appliesToMultiple))
            {
                _actorAssignmentUi.DrawWorldCombo(width.X / 2);
                ImGui.SameLine();
                _actorAssignmentUi.DrawPlayerInput(width.X / 2);

                var buttonWidth = new Vector2(165 * ImGuiHelpers.GlobalScale - ImGui.GetStyle().ItemSpacing.X / 2, 0);

                        if (ImGuiUtil.DrawDisabledButton("套用至玩家角色", buttonWidth, string.Empty, !_actorAssignmentUi.CanSetPlayer))
                    _manager.AddCharacter(_selector.Selected!, _actorAssignmentUi.PlayerIdentifier);

                ImGui.SameLine();

                        if (ImGuiUtil.DrawDisabledButton("套用至雇員", buttonWidth, string.Empty, !_actorAssignmentUi.CanSetRetainer))
                    _manager.AddCharacter(_selector.Selected!, _actorAssignmentUi.RetainerIdentifier);

                ImGui.SameLine();

                        if (ImGuiUtil.DrawDisabledButton("套用至模特兒", buttonWidth, string.Empty, !_actorAssignmentUi.CanSetMannequin))
                    _manager.AddCharacter(_selector.Selected!, _actorAssignmentUi.MannequinIdentifier);

                var currentPlayer = _actorManager.GetCurrentPlayer().CreatePermanent();
                        if (ImGuiUtil.DrawDisabledButton("套用至目前角色", buttonWidth, string.Empty, !currentPlayer.IsValid))
                    _manager.AddCharacter(_selector.Selected!, currentPlayer);

                ImGui.Separator();

                _actorAssignmentUi.DrawObjectKindCombo(width.X / 2);
                ImGui.SameLine();
                _actorAssignmentUi.DrawNpcInput(width.X / 2);

                        if (ImGuiUtil.DrawDisabledButton("套用至選取的 NPC", buttonWidth, string.Empty, !_actorAssignmentUi.CanSetNpc))
                    _manager.AddCharacter(_selector.Selected!, _actorAssignmentUi.NpcIdentifier);
            }
        }
    }

    private void DrawCharacterListArea()
    {
        var isDefaultLP = _manager.DefaultLocalPlayerProfile == _selector.Selected;
        var isDefaultLPOrCurrentProfilesEnabled = (_manager.DefaultLocalPlayerProfile?.Enabled ?? false) || (_selector.Selected?.Enabled ?? false);
        using (ImRaii.Disabled(isDefaultLPOrCurrentProfilesEnabled))
        {
            if (ImGui.Checkbox("##DefaultLocalPlayerProfile", ref isDefaultLP))
                _manager.SetDefaultLocalPlayerProfile(isDefaultLP ? _selector.Selected! : null);
                    ImGuiUtil.LabeledHelpMarker("套用至自己登入的所有角色",
                        "是否將此設定檔中的範本套用至目前登入的任意角色。\r\n對該角色而言，此選項的優先度高於下一個選項。\r\n此設定不能同時套用於多個設定檔。");
        }
        if (isDefaultLPOrCurrentProfilesEnabled)
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Constants.Colors.Warning);
            ImGuiUtil.PrintIcon(FontAwesomeIcon.ExclamationTriangle);
            ImGui.PopStyleColor();
                        ImGuiUtil.HoverTooltip("只有目前選取的設定檔與已勾選此選項的設定檔皆停用時才能變更。");
        }

        ImGui.SameLine();
        using(ImRaii.Disabled(true))
            ImGui.Button("##splitter", new Vector2(1, ImGui.GetFrameHeight()));
        ImGui.SameLine();

        var isDefault = _manager.DefaultProfile == _selector.Selected;
        var isDefaultOrCurrentProfilesEnabled = (_manager.DefaultProfile?.Enabled ?? false) || (_selector.Selected?.Enabled ?? false);
        using (ImRaii.Disabled(isDefaultOrCurrentProfilesEnabled))
        {
            if (ImGui.Checkbox("##DefaultProfile", ref isDefault))
                _manager.SetDefaultProfile(isDefault ? _selector.Selected! : null);
                    ImGuiUtil.LabeledHelpMarker("套用至所有玩家與雇員",
                        "是否將此設定檔中的範本套用至沒有指定設定檔的所有玩家與雇員。\r\n此設定不能同時套用於多個設定檔。");
        }
        if (isDefaultOrCurrentProfilesEnabled)
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, Constants.Colors.Warning);
            ImGuiUtil.PrintIcon(FontAwesomeIcon.ExclamationTriangle);
            ImGui.PopStyleColor();
                        ImGuiUtil.HoverTooltip("只有目前選取的設定檔與已勾選此選項的設定檔皆停用時才能變更。");
        }
        bool appliesToMultiple = _manager.DefaultProfile == _selector.Selected || _manager.DefaultLocalPlayerProfile == _selector.Selected;

        ImGui.Separator();

        using var dis = ImRaii.Disabled(appliesToMultiple);
        using var table = ImRaii.Table("CharacterTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY, new Vector2(ImGui.GetContentRegionAvail().X, 200));
        if (!table)
            return;

        ImGui.TableSetupColumn("##charaDel", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
                ImGui.TableSetupColumn("角色", ImGuiTableColumnFlags.WidthFixed, 320 * ImGuiHelpers.GlobalScale);
        ImGui.TableHeadersRow();

        if (appliesToMultiple)
        {
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
                        ImGui.TextUnformatted("套用至多個目標");
            return;
        }

        //warn: .ToList() might be performance critical at some point
        //the copying via ToList is done because manipulations with .Templates list result in "Collection was modified" exception here
        var charas = _selector.Selected!.Characters.WithIndex().ToList();

        if (charas.Count == 0)
        {
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("此設定檔未關聯任何角色");
        }

        foreach (var (character, idx) in charas)
        {
            using var id = ImRaii.PushId(idx);
            ImGui.TableNextColumn();
            var keyValid = _configuration.UISettings.DeleteTemplateModifier.IsActive();
            var tt = keyValid
                        ? "從設定檔中移除此角色。"
                        : $"從設定檔中移除此角色。\n按住 {_configuration.UISettings.DeleteTemplateModifier} 以移除。";

            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()), tt, !keyValid, true))
                _endAction = () => _manager.DeleteCharacter(_selector.Selected!, character);
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(!_selector.IncognitoMode ? $"{character.ToNameWithoutOwnerName()}{character.TypeToString()}" : "匿名");

            var profiles = _manager.GetEnabledProfilesByActor(character).ToList();
            if (profiles.Count > 1)
            {
                //todo: make helper
                ImGui.SameLine();
                if (profiles.Any(x => x.IsTemporary))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Constants.Colors.Error);
                    ImGuiUtil.PrintIcon(FontAwesomeIcon.Lock);
                }
                else if (profiles[0] != _selector.Selected!)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Constants.Colors.Warning);
                    ImGuiUtil.PrintIcon(FontAwesomeIcon.ExclamationTriangle);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Constants.Colors.Info);
                    ImGuiUtil.PrintIcon(FontAwesomeIcon.Star);
                }

                ImGui.PopStyleColor();

                if (profiles.Any(x => x.IsTemporary))
                    ImGuiUtil.HoverTooltip("此角色正受到外部插件設定的臨時設定檔影響，因此不會套用此設定檔！");
                else
                    ImGuiUtil.HoverTooltip(profiles[0] != _selector.Selected! ? "有多個設定檔正嘗試影響此角色，因此不會套用此設定檔！" :
                        "有多個設定檔正嘗試影響此角色，目前正在套用此設定檔。");
            }
        }

        _endAction?.Invoke();
        _endAction = null;
    }

    private void DrawTemplateArea()
    {
        using var table = ImRaii.Table("TemplateTable", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollX | ImGuiTableFlags.ScrollY);
        if (!table)
            return;

        ImGui.TableSetupColumn("##del", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("##Index", ImGuiTableColumnFlags.WidthFixed, 30 * ImGuiHelpers.GlobalScale);

                ImGui.TableSetupColumn("範本", ImGuiTableColumnFlags.WidthFixed, 220 * ImGuiHelpers.GlobalScale);

        ImGui.TableSetupColumn("##editbtn", ImGuiTableColumnFlags.WidthFixed, 120 * ImGuiHelpers.GlobalScale);

        ImGui.TableHeadersRow();

        //warn: .ToList() might be performance critical at some point
        //the copying via ToList is done because manipulations with .Templates list result in "Collection was modified" exception here
        foreach (var (template, idx) in _selector.Selected!.Templates.WithIndex().ToList())
        {
            using var id = ImRaii.PushId(idx);
            ImGui.TableNextColumn();
            var keyValid = _configuration.UISettings.DeleteTemplateModifier.IsActive();
            var tt = keyValid
                        ? "從設定檔中移除此範本。"
                        : $"從設定檔中移除此範本。\n按住 {_configuration.UISettings.DeleteTemplateModifier} 以移除。";

            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()), tt, !keyValid, true))
                _endAction = () => _manager.DeleteTemplate(_selector.Selected!, idx);
            ImGui.TableNextColumn();
            ImGui.Selectable($"#{idx + 1:D2}");
            DrawDragDrop(_selector.Selected!, idx);
            ImGui.TableNextColumn();
            _templateCombo.Draw(_selector.Selected!, template, idx);
            DrawDragDrop(_selector.Selected!, idx);
            ImGui.TableNextColumn();

            var disabledCondition = _templateEditorManager.IsEditorActive || template.IsWriteProtected;

                    if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Edit.ToIconString(), new Vector2(ImGui.GetFrameHeight()), "在範本編輯器中開啟此範本", disabledCondition, true))
                _templateEditorEvent.Invoke(TemplateEditorEvent.Type.EditorEnableRequested, template);

            if (disabledCondition)
            {
                //todo: make helper
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, Constants.Colors.Warning);
                ImGuiUtil.PrintIcon(FontAwesomeIcon.ExclamationTriangle);
                ImGui.PopStyleColor();
                        ImGuiUtil.HoverTooltip("無法編輯此範本，因為它已受寫入保護，或你已在編輯其他範本。");
            }
        }

        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("新增");
        ImGui.TableNextColumn();
        _templateCombo.Draw(_selector.Selected!, null, -1);
        ImGui.TableNextRow();

        _endAction?.Invoke();
        _endAction = null;
    }

    private void DrawDragDrop(Profile profile, int index)
    {
        const string dragDropLabel = "TemplateDragDrop";
        using (var target = ImRaii.DragDropTarget())
        {
            if (target.Success && ImGuiUtil.IsDropping(dragDropLabel))
            {
                if (_dragIndex >= 0)
                {
                    var idx = _dragIndex;
                    _endAction = () => _manager.MoveTemplate(profile, idx, index);
                }

                _dragIndex = -1;
            }
        }

        using (var source = ImRaii.DragDropSource())
        {
            if (source)
            {
            ImGui.TextUnformatted($"正在移動範本 #{index + 1:D2}……");
                if (ImGui.SetDragDropPayload(dragDropLabel, null, 0))
                {
                    _dragIndex = index;
                }
            }
        }
    }

    private void UpdateIdentifiers()
    {

    }
}
