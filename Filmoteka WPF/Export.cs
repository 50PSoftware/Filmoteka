namespace Filmoteka_WPF
{
    internal class Export
    {
        private readonly Filmoteka source;

        public Export(Filmoteka source)
        {
            this.source = source;
        }

        public void ToJSON(string exportFilename)
        {
            JSONFile json = new JSONFile(exportFilename);
            json.Save(source);
        }

        public void ToXML(string exportFilename)
        {
            XMLFile xml = new XMLFile(exportFilename);
            xml.Save(source);
        }
    }
}
