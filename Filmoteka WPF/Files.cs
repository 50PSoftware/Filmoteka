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
            using (var file = new StreamReader(File.OpenRead(_filename)))
            {
                var films = JsonSerializer.Deserialize<Film[]>(file.ReadToEnd());
                filmoteka.AddRange(films);
            }
        }

        public void Save(Filmoteka filmoteka)
        {
            var films = filmoteka.GetFilms();
            var jsonObj = JsonSerializer.Serialize<Film[]>(films);
            using (var file = new StreamWriter(File.OpenWrite(_filename)))
            {
                file.Write(jsonObj);
                file.Flush();
            }
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
                var years = Properties.Resources.Years.Split(',');
                _years = new int[years.Length];
                for (int index = 0; index < years.Length; index++)
                {
                    _years[index] = Convert.ToInt32(years[index]);
                }
            }
        }

        private string PrintGenres()
        {
            var sb = new StringBuilder();
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
            this._filename = filename;
        }

        public void Load(Filmoteka filmoteka)
        {
            filmoteka.Clear();
            using (_file = File.OpenRead(_filename))
            {
                var reader = new XmlSerializer(typeof(Film[]));
                var films = (Film[])reader.Deserialize(_file);
                _file.Flush();
                filmoteka.AddRange(films);
            }
        }

        public void Save(Filmoteka filmoteka)
        {
            var films = filmoteka.GetFilms();
            using (_file = File.Create(_filename))
            {
                var writer = new XmlSerializer(typeof(Film[]));
                writer.Serialize(_file, films);
                _file.Flush();
            }
        }
    }
}
