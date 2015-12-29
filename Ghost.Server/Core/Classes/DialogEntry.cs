using Ghost.Server.Utilities;

namespace Ghost.Server.Core.Classes
{
    public class DialogEntry
    {
        public byte Npc;
        public int Message;
        public DialogType Type;
        public int CommandData01;
        public int CommandData02;
        public int ConditionData01;
        public int ConditionData02;
        public DialogCommand Command;
        public DialogCondition Condition;
        public bool IsEnd
        {
            get { return Command == DialogCommand.DialogEnd; }
        }
        public bool IsGoTo
        {
            get { return Command == DialogCommand.GoTo; }
        }
        public DialogEntry(byte npc, byte type, int message, byte cnd, int cndData01, int cndData02, byte cmd, int cmdData01, int cmdData02)
        {
            Npc = npc;
            Message = message;
            Type = (DialogType)type;
            CommandData01 = cmdData01;
            CommandData02 = cmdData02;
            ConditionData01 = cndData01;
            ConditionData02 = cndData02;
            Command = (DialogCommand)cmd;
            Condition = (DialogCondition)cnd;
        }
    }
}