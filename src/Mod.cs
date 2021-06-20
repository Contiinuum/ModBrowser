using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBrowser
{
    public class Mod
    {
        public string repoName;
        public string displayRepoName;
        public string userName;
        public string displayUserName;
        public string description;
        public string fileName;
        public Version version;
        public string downloadLink;
        public bool isDownloaded = false;
        public bool isUpdated = false;
    }
}
