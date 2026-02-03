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
            LocalFiles localFiles = new LocalFiles(true);
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
            Film film = new Film(name, filename, genres, year, description);
            _films.Add(film);
        }

        public void AddRange(Film[] films)
        {
            if (this._films.Count == 0)
                this._films.AddRange(films);
            else
                throw new Exception($"Films collection is not empty! Count: {this._films.Count}");
        }

        public void AutoAddFilms()
        {
            Film[] films = AutoAdd.GetFilms(Folder);
            foreach (Film film in films)
            {
                bool isAlreadyInFileOrDatabase = false;
                var query = from _film in this._films where _film.Filename == film.Filename select _film;
                foreach (Film filmQuery in query)
                {
                    System.Windows.MessageBox.Show($"Tento film ({film.Name}) je již v databázi!");
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
            List<Film> result = new List<Film>();
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
            List<Film> result = new List<Film>();
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
            Film filmToDelete = _films[index];
            _films.RemoveAt(index);
        }

        public void UpdateFilm(Film film, string name, string filename, string description, string[] genres, int year)
        {
            var query = from _film in _films where _film.Filename == film.Filename select _film;
            foreach (Film f in query)
            {
                f.SetName(name);
                f.SetDescription(description);
                f.SetFilename(filename);
                f.SetYear(year);
                f.SetGenres(new List<string>(genres));
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

        public string Filename { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Year { get; private set; }
        public List<string> Genres { get; private set; }

        public void AddGenre(string zanr)
        {
            Genres.Add(zanr);
        }

        public void SetFilename(string filename)
        {
            this.Filename = filename;
        }

        public void SetName(string nazev)
        {
            this.Name = nazev;
        }

        public void SetDescription(string popis)
        {
            this.Description = popis;
        }

        public void SetYear(int rok)
        {
            this.Year = rok;
        }

        public void SetGenres(List<string> zanr)
        {
            this.Genres = zanr;
        }

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
