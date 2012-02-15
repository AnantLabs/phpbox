
namespace phpBox
{
    public enum ScriptDataType
    {
        Output,
        Error
    }

    public class ScriptData
    {
        public ScriptData()
        {
            Type = ScriptDataType.Output;
            Message = null;
        }

        public ScriptData(string Message, ScriptDataType Type = ScriptDataType.Output)
        {
            this.Type = Type;
            this.Message = Message;
        }

        public ScriptDataType Type { get; set; }
        public string Message { get; set; }
    }
}
