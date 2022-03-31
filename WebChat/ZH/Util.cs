using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebChat.ZH.Util
{
    public class zhFile
    {
        public static void removeAllChild(string dirPath)
        {
            DirectoryInfo di = new DirectoryInfo(dirPath);
            foreach (FileInfo f in di.GetFiles())
            {
                f.Delete();
            }
            foreach (DirectoryInfo d in di.GetDirectories())
            {
                d.Delete();
            }

        }
        public static void SaveFormFile(IFormFile file, string path)
        {
            var stream = File.Create(path);
            //file.CopyToAsync(stream); 
            file.CopyTo(stream); 
            stream.Flush();
            stream.Close();
        }
    }
}
