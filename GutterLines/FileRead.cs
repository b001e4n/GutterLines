using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace GutterLines
{
    public class FileRead
    {
        public GameInfo GetValues(ChatLogConfig conf)
        {
            var directory = new DirectoryInfo(conf.Path);
            var lastFile = directory.GetFiles(conf.FileNamePattern).OrderByDescending(x => x.LastWriteTime).FirstOrDefault();
            if (conf.RemoveFilesAfterRead)
            {
                try
                {
                    foreach (FileInfo file in directory.GetFiles().Where(x => x.Name != lastFile.Name))
                        file.Delete();
                }
                catch { }
            }           

            var chatRex = new Regex(conf.CoordsPattern);
            try
            {
                if (lastFile != null)
                {
                    var chatLines = File.ReadLines(lastFile.FullName).ToArray();
                    for (int i = chatLines.Count() - 1; i >= 0; i--)
                    {
                        var match = chatRex.Match(chatLines[i]);
                        if (match.Success)
                        {
                            return new GameInfo()
                            {
                                Name = match.Groups["city"].Value,
                                Lat = int.Parse(match.Groups["lat"].Value),
                                Lon = int.Parse(match.Groups["lon"].Value),
                            };
                        }
                    }
                }
            }
            catch { }
            
            return null;
        }
    }
}
