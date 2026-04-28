using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Filmoteka_WPF
{
    internal class Filmoteka
    {
        private List<Film> _films = null;
        private int[] _years;
        private string[] _genres;

        public Filmoteka(string FilenameOrConnectionString, bool NewFile = false)
        {
            _films = new List<Film>();
            var localFiles = new LocalFiles(true);
            if (NewFile)
            {
                switch (new FileInfo(FilenameOrConnectionString).Extension)
                {
                    case ".xml":
                        new XMLFile(FilenameOrConnectionString).Save(this);
                        break;

                    case ".json":
                        new JSONFile(FilenameOrConnectionString).Save(this);
                        break;
                }
            }

            _genres = localFiles.GetGenres();
            _years = localFiles.GetYears();
        }

        public event EventHandler<FilmotekaEventArgs> FilmsAutoAdded;

        public string Folder { get; set; }

        protected virtual void OnFilmsAdded(FilmotekaEventArgs e)
        {
            EventHandler<FilmotekaEventArgs> handler = FilmsAutoAdded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void AddFilm(Film film)
        {
            _films.Add(film);
        }

        public void AddFilm(string name, string filename, string description, string[] genres, int year)
        {
            var film = new Film(name, filename, genres, year, description);
            _films.Add(film);
        }

        public void AddRange(Film[] films)
        {
            if (this._films.Count == 0)
                this._films.AddRange(films);
            else
                throw new FilmException($"Films collection is not empty! Count: {this._films.Count}");
        }

        public void AutoAddFilms()
        {
            var films = AutoAdd.GetFilms(Folder);
            foreach (Film film in films)
            {
                bool isAlreadyInFileOrDatabase = false;
                var query = from _film in this._films where _film.Filename == film.Filename select _film;
                foreach (Film filmQuery in query)
                {
                    isAlreadyInFileOrDatabase = true;
                }
                if (!isAlreadyInFileOrDatabase)
                    this._films.Add(film);
            }

            OnFilmsAdded(new FilmotekaEventArgs(films));
        }

        public void Clear()
        {
            _films.Clear();
        }

        public Film[] GetFilms()
        {
            return _films.ToArray();
        }

        public Film[] GetFilmsByGenre(string[] zanr)
        {
            var result = new List<Film>();
            for (int i = 0; i < zanr.Length; i++)
            {
                var query = from film in _films where film.Genres.Contains(zanr[i]) select film;
                foreach (Film f in query)
                {
                    if (!result.Contains(f))
                    {
                        result.Add(f);
                    }
                }
            }

            return result.ToArray();
        }

        public Film[] GetFilmsByYear(int rok)
        {
            var query = from film in _films where film.Year == rok select film;
            var result = new List<Film>();
            foreach (Film film1 in query)
            {
                result.Add(film1);
            }

            return result.ToArray();
        }

        public int[] GetYears()
        {
            return _years;
        }

        public string[] GetGenres()
        {
            return _genres;
        }

        public void RemoveFilm(Film film)
        {
            _films.Remove(film);
        }

        public void RemoveFilm(int index)
        {
            var filmToDelete = _films[index];
            _films.RemoveAt(index);
        }

        public void UpdateFilm(Film film, string name, string filename, string description, string[] genres, int year)
        {
            var query = from _film in _films where _film.Filename == film.Filename select _film;
            foreach (var filmInQuery in query)
            {
                film.Name = name;
                film.Description = description;
                film.Filename = filename;
                film.Year = year;
                film.Genres = new List<string>(genres);
            }
        }
    }

    public class Film
    {
        public Film(string name, string filename, List<string> genres, int year, string description)
        {
            this.Name = name;
            this.Filename = filename;
            this.Year = year;
            this.Genres = genres;
            this.Description = description;
        }

        public Film(string name, string filename, string[] genres, int year, string description)
        {
            this.Name = name;
            this.Filename = filename;
            this.Year = year;
            this.Description = description;
            this.Genres = new List<string>();
            this.Genres.AddRange(genres);
        }

        public Film()
        { }

        public string Filename { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public List<string> Genres { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public class FilmotekaEventArgs : EventArgs
    {
        public FilmotekaEventArgs(Film film)
        {
            Film = film;
        }

        public FilmotekaEventArgs(Film[] films)
        {
            Films = films;
        }

        public Film Film { get; }
        public Film[] Films { get; }
    }
}
