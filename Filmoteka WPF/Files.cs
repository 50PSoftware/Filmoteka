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
        private string filename;

        public JSONFile(string filename)
        {
            this.filename = filename;
        }

        public void Load(Filmoteka filmoteka)
        {
            filmoteka.Clear();
            using (var file = new StreamReader(File.OpenRead(filename)))
            {
                var films = JsonSerializer.Deserialize<Film[]>(file.ReadToEnd());
                filmoteka.AddRange(films);
                file.Flush();
            }
        }

        public void Save(Filmoteka filmoteka)
        {
            var films = filmoteka.GetFilms();
            var jsonObj = JsonSerializer.Serialize<Film[]>(films);
            using (var file = new StreamWriter(File.OpenWrite(filename)))
            {
                file.Write(jsonObj);
                file.Flush();
            }
        }
    }

    internal class LocalFiles
    {
        private string[] genres;
        private int[] years;

        public LocalFiles(bool useResources = false)
        {
            if (useResources)
            {
                genres = Properties.Resources.Genres.Split(',');
                var years = Properties.Resources.Years.Split(',');
                years = new int[years.Length];
                for (int index = 0; index < years.Length; index++)
                {
                    years[index] = Convert.ToInt32(years[index]);
                }
            }
        }

        private string PrintGenres()
        {
            var sb = new StringBuilder();
            foreach (string genre in genres)
            {
                sb.AppendLine($"Žánr: {genre} ({genre.Length})");
            }
            return sb.ToString();
        }

        public string[] GetGenres()
        {
            return genres;
        }

        public int[] GetYears()
        {
            return years;
        }
    }

    internal class XMLFile : IFilesable
    {
        private Stream file;
        private string filename;

        public XMLFile(string filename)
        {
            this.filename = filename;
        }

        public void Load(Filmoteka filmoteka)
        {
            filmoteka.Clear();
            using (file = File.OpenRead(filename))
            {
                var reader = new XmlSerializer(typeof(Film[]));
                var films = (Film[])reader.Deserialize(_file);
                file.Flush();
                filmoteka.AddRange(films);
            }
        }

        public void Save(Filmoteka filmoteka)
        {
            var films = filmoteka.GetFilms();
            using (file = File.Create(filename))
            {
                var writer = new XmlSerializer(typeof(Film[]));
                writer.Serialize(file, films);
                file.Flush();
            }
        }
    }
}
