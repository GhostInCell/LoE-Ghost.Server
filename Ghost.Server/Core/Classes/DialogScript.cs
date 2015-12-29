using Ghost.Server.Core.Objects;
using Ghost.Server.Core.Players;
using Ghost.Server.Core.Servers;
using Ghost.Server.Core.Structs;
using Ghost.Server.Mgrs;
using Ghost.Server.Utilities;
using Ghost.Server.Utilities.Interfaces.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ghost.Server.Core.Classes
{
    public class DialogScript: IScriptedDialog
    {
        private readonly ushort _id;
        private WO_NPC[] _npcs;
        private MapServer _server;
        private SortedDictionary<short, DialogEntry> _entries;
        public ushort ID
        {
            get { return _id; }
        }
        public WO_NPC[] NPC
        {
            get { return _npcs; }
        }
        public MapServer Server
        {
            get
            {
                return _server;
            }
        }
        public DialogScript(DB_Dialog data, MapServer server)
        {
            _id = data.ID;
            _server = server;
            _npcs = new WO_NPC[2];
            _entries = data.Entries;
        }
        public void Destroy()
        {
            _npcs = null;
            _server = null;
            _entries = null;
        }
        public void OnDialogEnd(MapPlayer player)
        {

        }
        public void OnDialogNext(MapPlayer player)
        {
            DialogEntry entry;
            short dState = player.Data.GetDialogState(_id);
            if (_entries.TryGetValue(dState, out entry))
            {
                ExecuteCommand(ref dState, false, player, entry);
                if (entry.IsEnd) return;
                if (!entry.IsGoTo) dState++;
                Execute(ref dState, false, player);
            }
            else player.DialogEnd();
            player.Data.Dialogs[_id] = dState;
        }
        public void OnDialogStart(MapPlayer player)
        {
            short dState = player.Data.GetDialogState(_id);
            foreach (var item in _entries.TakeWhile(x => x.Key < 0))
            {
                if (item.Value.Type != DialogType.Command) continue;
                if (item.Value.Condition == DialogCondition.Always || CheckCondition(dState, true, player, item.Value))
                {
                    ExecuteCommand(ref dState, true, player, item.Value);
                    if (item.Value.IsEnd) return;
                    if (item.Value.IsGoTo) break;
                }
            }
            Execute(ref dState, true, player);
            player.Data.Dialogs[_id] = dState;
        }
        public void OnDialogChoice(MapPlayer player, int choice)
        {
            short dState = player.Data.GetDialogState(_id);
            if (SelectChoice(ref dState, false, player, choice))
                Execute(ref dState, false, player);
            player.Data.Dialogs[_id] = dState;
        }
        private void Execute(ref short dState, bool isStart, MapPlayer player)
        {
            DialogEntry entry;
            while (_entries.TryGetValue(dState, out entry))
            {
                if (entry.Type == DialogType.Greetings && (!isStart || _npcs[entry.Npc] != player.Dialog))
                {
                    dState++;
                    continue;
                }
                if (entry.Condition == DialogCondition.Always || CheckCondition(dState, isStart, player, entry))
                {
                    switch (entry.Type)
                    {
                        case DialogType.Choice:
                            SetChoices(dState, isStart, player, entry);
                            return;
                        case DialogType.Command:
                            ExecuteCommand(ref dState, isStart, player, entry);
                            if (entry.IsEnd) return;
                            if (!entry.IsGoTo) dState++;
                            continue;
                        case DialogType.Say:
                        case DialogType.Greetings:
                            player.DialogSetMessage(_npcs[entry.Npc], entry.Message);
                            return;
                    }
                }
                else dState++;
            }
            player.DialogEnd();
        }
        private void SetChoices(short dState, bool isStart, MapPlayer player, DialogEntry entry)
        {
            List<string> _choices = new List<string>();
            do
            {
                if (entry.Condition == DialogCondition.Always || 
                    CheckCondition(dState, isStart, player, entry))
                    _choices.Add(DataMgr.SelectMessage(entry.Message).Item2.GetMessage(player));
                dState++;
            }
            while (_entries.TryGetValue(dState, out entry) && entry.Type == DialogType.Choice);
            player.DialogSetOptions(_choices.ToArray());
            _choices.Clear(); _choices = null;
        }
        private bool SelectChoice(ref short dState, bool isStart, MapPlayer player, int choice)
        {
            DialogEntry entry; int iChoice = choice;
            while (_entries.TryGetValue(dState, out entry))
            {
                if (entry.Type != DialogType.Choice) return true;
                if (iChoice != -1 && (entry.Condition == DialogCondition.Always || CheckCondition(dState, isStart, player, entry)))
                {
                    if (iChoice == 0)
                    {
                        iChoice = -1;
                        player.LastDialogChoice = dState;
                        if (entry.Command != DialogCommand.None)
                            ExecuteCommand(ref dState, isStart, player, entry);
                        if (entry.IsEnd) return false;
                        if (entry.IsGoTo) return true;
                    }
                    else iChoice--;
                }
                dState++;
            }
            player.DialogEnd();
            return false;
        }
        private bool CheckCondition(short dState, bool isStart, MapPlayer player, DialogEntry entry)
        {
            switch (entry.Condition)
            {
                case DialogCondition.Always:
                    return true;
                case DialogCondition.Quest_NotStarted:
                case DialogCondition.Quest_InProgress:
                case DialogCondition.Quest_Failed:
                case DialogCondition.Quest_Completed:
                case DialogCondition.Quest_StateInProgress:
                case DialogCondition.Quest_StateFailed:
                case DialogCondition.Quest_StateCompleted:
                    return false;
                case DialogCondition.NpcIndex_Equal:
                    return _npcs[entry.Npc] == player.Dialog;
                case DialogCondition.NpcIndex_NotEqual:
                    return _npcs[entry.Npc] != player.Dialog;
                case DialogCondition.State_Equal:
                    return dState == entry.ConditionData01;
                case DialogCondition.State_NotEqual:
                    return dState != entry.ConditionData01;
                case DialogCondition.State_Lower:
                    return dState < entry.ConditionData01;
                case DialogCondition.State_Greater:
                    return dState > entry.ConditionData01;
                case DialogCondition.State_LowerOrEqual:
                    return dState <= entry.ConditionData01;
                case DialogCondition.State_GreaterOrEqual:
                    return dState >= entry.ConditionData01;
                case DialogCondition.Level_Equal:
                    return player.Stats.Level == entry.ConditionData01;
                case DialogCondition.Level_NotEqual:
                    return player.Stats.Level != entry.ConditionData01;
                case DialogCondition.Level_Lower:
                    return player.Stats.Level < entry.ConditionData01;
                case DialogCondition.Level_Greater:
                    return player.Stats.Level > entry.ConditionData01;
                case DialogCondition.Level_LowerOrEqual:
                    return player.Stats.Level <= entry.ConditionData01;
                case DialogCondition.Level_GreaterOrEqual:
                    return player.Stats.Level >= entry.ConditionData01;
                case DialogCondition.LastChoice_Equal:
                    return player.LastDialogChoice == entry.ConditionData01;
                case DialogCondition.LastChoice_NotEqual:
                    return player.LastDialogChoice != entry.ConditionData01;
                case DialogCondition.TalentLevel_Equal:
                    return player.Data.GetTalentLevel(entry.ConditionData02) == entry.ConditionData01;
                case DialogCondition.TalentLevel_NotEqual:
                    return player.Data.GetTalentLevel(entry.ConditionData02) != entry.ConditionData01;
                case DialogCondition.TalentLevel_Lower:
                    return player.Data.GetTalentLevel(entry.ConditionData02) < entry.ConditionData01;
                case DialogCondition.TalentLevel_Greater:
                    return player.Data.GetTalentLevel(entry.ConditionData02) > entry.ConditionData01;
                case DialogCondition.TalentLevel_LowerOrEqual:
                    return player.Data.GetTalentLevel(entry.ConditionData02) <= entry.ConditionData01;
                case DialogCondition.TalentLevel_GreaterOrEqual:
                    return player.Data.GetTalentLevel(entry.ConditionData02) >= entry.ConditionData01;
                case DialogCondition.Item_HasCount:
                    return player.Items.HasItems(entry.ConditionData01, entry.ConditionData02);
                default: return false;
            }
        }
        private void ExecuteCommand(ref short state, bool isStart, MapPlayer player, DialogEntry entry)
        {
            switch (entry.Command)
            {
                case DialogCommand.DialogEnd:
                    if (entry.CommandData01 > 0)
                        player.Data.Dialogs[_id] = (short)entry.CommandData01;
                    player.DialogEnd();
                    return;
                case DialogCommand.GoTo:
                    state = (short)entry.CommandData01;
                    return;
                case DialogCommand.AddXP:
                    if (entry.CommandData02 == -1)
                        player.Stats.AddExpAll((uint)entry.CommandData01);
                    else
                        player.Stats.AddExp((Talent)entry.CommandData02, (uint)entry.CommandData01);
                    break;
                case DialogCommand.AddBits:
                    player.Items.AddBits(entry.CommandData01);
                    break;
                case DialogCommand.AddItem:
                    player.Items.AddItems(entry.CommandData01, entry.CommandData02);
                    break;
                case DialogCommand.RemoveBits:
                    player.Items.AddBits(-entry.CommandData01);
                    break;
                case DialogCommand.RemoveItem:
                    player.Items.RemoveItems(entry.CommandData01, entry.CommandData02);
                    break;
                case DialogCommand.AddQuest:
                case DialogCommand.RemoveQuest:
                case DialogCommand.SetQuestState:
                    break;
            }
        }
    }
}