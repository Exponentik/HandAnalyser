using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandAnalyzer.Structures;

namespace HandAnalyzer
{
    internal class BlobPoint
    {
        public String Name;
        public FortuneSite site;

        public BlobPoint(String name, FortuneSite site)
        {
            this.site = site;
            this.Name = name;
        }
    }
}
