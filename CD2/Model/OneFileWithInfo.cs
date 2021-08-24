using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvmArchive.Model
{
    public class OneFileWithInfo
    {
        public OneFileWithInfo(List<int> values, string path, string fileName)
        {
            Values = values;
            Path = path;
            FileName = fileName;
        }

        public List<int> Values { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
    }
}

