using System.Collections.Generic;

namespace Defs
{
    public class DefConfigErrors
    {
        public Def CurrentDef { get; set; }

        public IReadOnlyList<LogItem> Errors => errors;
        public IReadOnlyList<LogItem> Warnings => warnings;

        private List<LogItem> errors = new List<LogItem>();
        private List<LogItem> warnings = new List<LogItem>();

        public void Error(string message, string fieldName = null)
        {
            errors.Add(new LogItem(message, fieldName, CurrentDef));
        }

        public void Warn(string message, string fieldName = null)
        {
            warnings.Add(new LogItem(message, fieldName, CurrentDef));
        }

        public struct LogItem
        {
            public string Message;
            public string FieldName;
            public Def Def;

            public LogItem(string msg, string field, Def def)
            {
                Message = msg;
                FieldName = field;
                Def = def;
            }

            public override string ToString()
            {
                if(FieldName != null)
                    return $"[{Def.DefID}] Field:<{FieldName}> {Message}";
                return $"[{Def.DefID}] {Message}";
            }
        }
    }
}
