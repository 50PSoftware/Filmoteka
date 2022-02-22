using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace Filmoteka_WPF
{
    interface IFilesable
    {
        void Save(Filmoteka filmoteka);
        void Load(Filmoteka filmoteka);
    }
    class XMLFile : IFilesable
    {
        private string _filename;
        private Stream _file;
        public XMLFile(string filename)
        {
            _filename = filename;
        }

        public void Save(Filmoteka filmoteka)
        {
            Film[] films = filmoteka.GetFilms();
            _file = File.Create(_filename);
            XmlSerializer writer = new XmlSerializer(typeof(Film[]));
            writer.Serialize(_file, films);
            _file.Close();
        }

        public void Load(Filmoteka filmoteka)
        {
            filmoteka.Clear();
            _file = File.OpenRead(_filename);
            XmlSerializer reader = new XmlSerializer(typeof(Film[]));
            Film[] films = (Film[])reader.Deserialize(_file);
            _file.Close();
            filmoteka.AddRange(films);
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
