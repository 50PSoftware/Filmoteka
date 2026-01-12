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

        public void AddFilm(string nazev, string filename, string popis, string[] zanry, int rok)
        {
            Film film = new Film(nazev, filename, zanry, rok, popis);
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
                    System.Windows.MessageBox.Show($"Tento film ({film.Nazev}) je již v databázi!");
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
                var query = from film in _films where film.Zanr.Contains(zanr[i]) select film;
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
            var query = from film in _films where film.Rok == rok select film;
            List<Film> result = new List<Film>();
            foreach (Film film1 in query)
            {
                result.Add(film1);
            }
            return result.ToArray();
        }

        public int[] GetRoky()
        {
            return _years;
        }

        public string[] GetZanry()
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

        public void UpdateFilm(Film film, string nazev, string filename, string popis, string[] zanry, int rok)
        {
            var query = from _film in _films where _film.Filename == film.Filename select _film;
            foreach (Film f in query)
            {
                f.SetNazev(nazev);
                f.SetPopis(popis);
                f.SetFilename(filename);
                f.SetRok(rok);
                f.SetZanr(new List<string>(zanry));
            }
        }
    }

    public class Film
    {
        public Film(string nazev, string filename, List<string> zanr, int rok, string popis)
        {
            this.Nazev = nazev;
            this.Filename = filename;
            this.Rok = rok;
            this.Zanr = zanr;
            this.Popis = popis;
        }

        public Film(string nazev, string filename, string[] zanry, int rok, string popis)
        {
            this.Nazev = nazev;
            this.Filename = filename;
            this.Rok = rok;
            this.Popis = popis;
            this.Zanr = new List<string>();
            this.Zanr.AddRange(zanry);
        }

        public Film()
        { }

        public string Filename { get; set; }
        public string Nazev { get; set; }
        public string Popis { get; set; }
        public int Rok { get; set; }
        public List<string> Zanr { get; set; }

        public void AddZanr(string zanr)
        {
            Zanr.Add(zanr);
        }

        public void SetFilename(string filename)
        {
            this.Filename = filename;
        }

        public void SetNazev(string nazev)
        {
            this.Nazev = nazev;
        }

        public void SetPopis(string popis)
        {
            this.Popis = popis;
        }

        public void SetRok(int rok)
        {
            this.Rok = rok;
        }

        public void SetZanr(List<string> zanr)
        {
            this.Zanr = zanr;
        }

        public override string ToString()
        {
            return this.Nazev;
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
