using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmoteka_WPF
{
    class Export
    {
        private readonly Filmoteka _source;
        public Export(Filmoteka source)
        {
            _source = source;
        }

        public void ToXML(string exportFilename)
        {
            XMLFile xml = new XMLFile(exportFilename);
            xml.Save(_source);
        }
    }
}
