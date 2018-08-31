

using System.ComponentModel;

namespace GutterLines
{
    public enum CoordsAccessMods
    {
        Memory,
        File
    }
    public class ChatLogConfig
    {
        [Description("Name.")]
        public string Name { get; set; }
        [Description("Path to the directory where chat logs saved.")]
        public string Path { get; set; }
        [Description("Pattern for filter chat log files.")]
        public string FileNamePattern { get; set; }
        [Description("Pattern for filter coords.")]
        public string CoordsPattern { get; set; }
        [Description("1 - remove files every time after read, 0 - do nothing.")]
        public bool RemoveFilesAfterRead { get; set; }
    }
    
    public class Settings
    {
        [Description("0 - memory access mode, 1 - file access mode.")]
        public CoordsAccessMods CoordsAccessMod { get; set; }
        [Description("List of configs in file mode.")]
        public ChatLogConfig[] ChatLogConfigs { get; set; }
        [Description("Current file config index, starts with 0.")]
        public int CurrentChatLogConfigIndex { get; set; }        
    }
}
