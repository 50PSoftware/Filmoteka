namespace Filmoteka_WPF
{
    internal class Export
    {
        private readonly Filmoteka _source;

        public Export(Filmoteka source)
        {
            this._source = source;
        }

        public void ToJSON(string exportFilename)
        {
            var json = new JSONFile(exportFilename);
            json.Save(_source);
        }

        public void ToXML(string exportFilename)
        {
            var xml = new XMLFile(exportFilename);
            xml.Save(_source);
        }
    }
}
