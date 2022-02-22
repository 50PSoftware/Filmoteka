using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filmoteka_WPF
{
    public class Film
    {
        public int Rok { get; set; }
        public string Nazev { get; set; }
        public string Filename { get; set; }
        public List<string> Zanr { get; set; }
        public string Popis { get;  set; }

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

        public Film() { }

        public void SetRok(int rok)
        {
            this.Rok = rok;
        }

        public void SetNazev(string nazev)
        {
            this.Nazev = nazev;
        }

        public void SetFilename(string filename)
        {
            this.Filename = filename;
        }

        public void SetZanr(List<string> zanr)
        {
            this.Zanr = zanr;
        }

        public void AddZanr(string zanr)
        {
            Zanr.Add(zanr);
        }

        public void SetPopis(string popis)
        {
            this.Popis = popis;
        }

        public override string ToString()
        {
            return this.Nazev;
        }
    }
    internal class Filmoteka
    {
        private List<Film> _films = null;
        private string[] _zanry;
        private int[] _roky;
        private DB _db = null;

        /// <summary>
        /// If True, operate with database
        /// </summary>
        public bool EditRemotely { get; set; }

        public string Folder { get; set; }

        public Filmoteka(string FilenameOrConnectionString, bool UseDatabase = true, bool NewFile = false)
        {
            _films = new List<Film>();
            
            if (UseDatabase)
            {
                _db = new DB(FilenameOrConnectionString);
                _films.AddRange(_db.GetFilms());
                _zanry = _db.GetZanry();
                _roky = _db.GetRoky();
            }
            else
            {
                LocalFiles localFiles = new LocalFiles(true);
                XMLFile xmlFile = new XMLFile(FilenameOrConnectionString);
                if (!NewFile)
                {
                    xmlFile.Load(this);
                }
                else
                {
                    xmlFile.Save(this);
                }
                _zanry = localFiles.GetGenres();
                _roky = localFiles.GetYears();
            }
            
            this.EditRemotely = false;
        }

        public void AutoAddFilms()
        {
            Film[] films = AutoAdd.GetFilms(Folder);
            foreach (Film film in films)
            {
                bool isAlreadyInFileOrDatabase = false;
                var query = from _film in _films where _film.Filename == film.Filename select _film;
                foreach (Film filmQuery in query)
                {
                    System.Windows.MessageBox.Show($"Tento film ({film.Nazev}) je již v databázi!");
                    isAlreadyInFileOrDatabase = true;
                }
                if (!isAlreadyInFileOrDatabase)
                    _films.Add(film);
            }
            OnFilmsAdded(new FilmotekaEventArgs(films));
        }

        public Film[] GetFilms()
        {
            return _films.ToArray();
        }

        public void Clear()
        {
            _films.Clear();
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

        public string[] GetZanry()
        {
            return _zanry;
        }

        public int[] GetRoky()
        {
            return _roky;
        }

        public string GetConnectionString()
        {
            return _db.GetConnectionString();
        }

        public void AddFilm(Film film)
        {
            _films.Add(film);
            if (EditRemotely)
            {
                if (_db != null)
                {
                    _db.Add(film);
                    OnEditedRemotely(new FilmotekaEventArgs(film));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="films"></param>
        /// <exception cref="Throws an exception about non-empty collection of films."/>
        public void AddRange(Film[] films)
        {
            if (this._films.Count == 0)
                this._films.AddRange(films);
            else
                throw new Exception($"Films collection is not empty! Count: {_films.Count}");
        }

        public void AddFilm(string nazev, string filename, string popis, string[] zanry, int rok)
        {
            Film film = new Film(nazev, filename, zanry, rok, popis);
            _films.Add(film);
            if (EditRemotely)
            {
                if (_db != null)
                {
                    _db.Add(film);
                    OnEditedRemotely(new FilmotekaEventArgs(film));
                }
            }
        }

        public void UpdateFilm(Film film, string nazev, string filename, string popis, string[] zanry, int rok)
        {
            var query = from _film in _films where _film.Filename == film.Filename select _film;
            foreach(Film f in query)
            {
                f.SetNazev(nazev);
                f.SetPopis(popis);
                f.SetFilename(filename);
                f.SetRok(rok);
                f.SetZanr(new List<string>(zanry));

                if (EditRemotely)
                {
                    if (_db != null)
                    {
                        _db.Update(f);
                        OnEditedRemotely(new FilmotekaEventArgs(f));
                    }
                }
            }
        }

        public void RemoveFilm(Film film)
        {
            _films.Remove(film);
            if (EditRemotely)
            {
                if (_db != null)
                {
                    _db.Delete(film);
                    OnEditedRemotely(new FilmotekaEventArgs(film));
                }
            }
        }

        public void RemoveFilm(int index)
        {
            Film filmToDelete = _films[index];
            _films.RemoveAt(index);
            if (EditRemotely)
            {
                if (_db != null)
                {
                    _db.Delete(filmToDelete);
                    OnEditedRemotely(new FilmotekaEventArgs(filmToDelete));
                }
            }
        }

        protected virtual void OnFilmsAdded(FilmotekaEventArgs e)
        {
            EventHandler<FilmotekaEventArgs> handler = FilmsAutoAdded;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnEditedRemotely(FilmotekaEventArgs e)
        {
            EventHandler<FilmotekaEventArgs> handler = EditedRemotely;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<FilmotekaEventArgs> EditedRemotely;
        public event EventHandler<FilmotekaEventArgs> FilmsAutoAdded;
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
