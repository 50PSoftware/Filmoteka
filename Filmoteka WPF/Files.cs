using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace Filmoteka_WPF
{
    internal interface IFilesable
    {
        void Load(Filmoteka filmoteka);

        void Save(Filmoteka filmoteka);
    }

    internal class JSONFile : IFilesable
    {
        private string _filename;

        public JSONFile(string filename)
        {
            this._filename = filename;
        }

        public void Load(Filmoteka filmoteka)
        {
            filmoteka.Clear();
            StreamReader file = new StreamReader(File.OpenRead(_filename));
            Film[] films = JsonSerializer.Deserialize<Film[]>(file.ReadToEnd());
            filmoteka.AddRange(films);
        }

        public void Save(Filmoteka filmoteka)
        {
            Film[] films = filmoteka.GetFilms();
            var jsonObj = JsonSerializer.Serialize<Film[]>(films);
            StreamWriter file = new StreamWriter(File.OpenWrite(_filename));
            file.Write(jsonObj);
            file.Close();
        }
    }

    internal class LocalFiles
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
                for (int index = 0; index < years.Length; index++)
                {
                    _years[index] = Convert.ToInt32(years[index]);
                }
            }
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

        public string[] GetGenres()
        {
            return _genres;
        }

        public int[] GetYears()
        {
            return _years;
        }
    }

    internal class XMLFile : IFilesable
    {
        private Stream _file;
        private string _filename;

        public XMLFile(string filename)
        {
            _filename = filename;
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

        public void Save(Filmoteka filmoteka)
        {
            Film[] films = filmoteka.GetFilms();
            _file = File.Create(_filename);
            XmlSerializer writer = new XmlSerializer(typeof(Film[]));
            writer.Serialize(_file, films);
            _file.Close();
        }
    }
}
