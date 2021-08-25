using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Filmoteka_WPF
{
    interface IFilesable
    {
        void New();
        void Save(Filmoteka filmoteka);
        void Load(Filmoteka filmoteka);
    }
    class XMLFile : IFilesable
    {
        private string filename;
        public XMLFile(string filename)
        {
            this.filename = filename;
        }
        public void New()
        {
            XmlTextWriter newFile = new XmlTextWriter(filename, Encoding.UTF8);
            newFile.Formatting = Formatting.Indented;
            newFile.WriteStartDocument();
            newFile.WriteStartElement("data");
            newFile.WriteEndElement();
            newFile.Flush();
            newFile.Close();
        }

        public void Save(Filmoteka filmoteka)
        {
            XmlDocument doxument = new XmlDocument();
            doxument.Load(filename);
            XmlElement root = doxument.DocumentElement;
            root.RemoveAll();
            foreach(Film film in filmoteka.GetFilms())
            {
                XmlElement filmElement = doxument.CreateElement("film");
                XmlElement nameElement = doxument.CreateElement("name");
                nameElement.InnerText = film.Nazev;
                filmElement.AppendChild(nameElement);
                XmlElement filenameElement = doxument.CreateElement("filename");
                filenameElement.InnerText = film.Filename;
                filmElement.AppendChild(filenameElement);
                XmlElement yearElement = doxument.CreateElement("year");
                yearElement.InnerText = film.Rok.ToString();
                filmElement.AppendChild(yearElement);
                XmlElement genres = doxument.CreateElement("genres");
                foreach(string zanr in film.Zanr)
                {
                    XmlElement genre = doxument.CreateElement("genre");
                    genre.InnerText = zanr;
                    genres.AppendChild(genre);
                }
                filmElement.AppendChild(genres);
                XmlElement describtionElement = doxument.CreateElement("describtion");
                describtionElement.InnerText = film.Popis;
                filmElement.AppendChild(describtionElement);
                root.AppendChild(filmElement);
            }
            doxument.Save(filename);
        }

        public void Load(Filmoteka filmoteka)
        {
            filmoteka.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNodeList nodes = doc.SelectNodes("/data/film");
            foreach (XmlNode node in nodes)
            {
                Film film = new Film();
                film.SetNazev(node.SelectSingleNode("name").InnerText);
                film.SetFilename(node.SelectSingleNode("filename").InnerText);
                film.SetRok(Convert.ToInt32(node.SelectSingleNode("year").InnerText));
                XmlNodeList genres = node.SelectNodes("genres/genre");
                List<string> zanry = new List<string>();
                foreach(XmlNode genre in genres)
                {
                    zanry.Add(genre.InnerText);
                }
                film.SetZanr(zanry);
                film.SetPopis(node.SelectSingleNode("describtion").InnerText);
                filmoteka.AddFilm(film);
            }
        }
    }

    class LocalFiles
    {
        private string[] _genres;
        private int[] _years;
        public LocalFiles(bool useResources = false)
        {
            if (useResources)
            {
                _genres = Properties.Resources.Genres.Split(',');
                string[] years = Properties.Resources.Years.Split(',');
                _years = new int[years.Length];
                for(int index = 0; index < years.Length;index++)
                {
                    _years[index] = Convert.ToInt32(years[index]);
                }
            }
        }

        public string[] GetGenres()
        {
            return _genres;
        }

        public int[] GetYears()
        {
            return _years;
        }
        private string PrintGenres()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string genre in _genres)
            {
                sb.AppendLine($"Žánr: {genre} ({genre.Length})");
            }
            return sb.ToString();
        }
    }
}
