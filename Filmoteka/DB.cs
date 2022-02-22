using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using _50P.Software.Connect.MySql;

namespace Filmoteka_WPF
{
    class DB
    {
        private string connectionString;

        private MySqlConnection connection = null;

        public DB(string connectionstring)
        {
            this.connectionString = connectionstring;
            connection = new MySqlConnection(connectionString);
        }

        public void SetConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Film[] GetFilms()
        {
            MySqlCommand selection = new MySqlCommand();
            string s_rowcount = "select count(*) from film;";
            string selectFilm = "select f.nazev as nazev, f.filename as filename, f.popis as popis, r.rok as rok from film as f inner join rok as r on r.id = f.idrok group by f.nazev;";

            selection.CommandText = s_rowcount;
            selection.Connection = connection;

            Film[] film = null;
            try
            {
                connection.Open();
                int rowcount = Convert.ToInt32(selection.ExecuteScalar());
                selection.CommandText = selectFilm;
                film = new Film[rowcount];
                MySqlDataReader reader = selection.ExecuteReader();
                int index = 0;
                while (reader.Read())
                {
                    film[index] = new Film();
                    film[index].SetNazev(reader["nazev"].ToString());
                    film[index].SetFilename(reader["filename"].ToString());
                    film[index].SetPopis(reader["popis"].ToString());
                    film[index].SetRok((int)reader["rok"]);
                    index++;
                }
                reader.Close();
                for (index = 0; index < rowcount; index++)
                {
                    string selectZanr = $"select z.nazev from zanr as z inner join film_zanr as fz on fz.idzanr = z.id inner join film as f on f.id = fz.idfilm where f.nazev = @nazev_filmu{index}";
                    selection.CommandText = selectZanr;
                    selection.Parameters.AddWithValue($"@nazev_filmu{index}", film[index].Nazev);
                    List<string> zanr = new List<string>();
                    reader = selection.ExecuteReader();
                    while (reader.Read())
                    {
                        zanr.Add(reader["nazev"].ToString());
                    }
                    reader.Close();
                    film[index].SetZanr(zanr);
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                connection.Close();
            }

            return film;
        }

        public string[] GetZanry()
        {
            List<string> zanr = new List<string>();
            MySqlCommand selection = new MySqlCommand();
            string cmd = "select nazev from zanr order by nazev;";
            selection.Connection = connection;
            selection.CommandText = cmd;

            try
            {
                connection.Open();
                MySqlDataReader reader = selection.ExecuteReader();
                while(reader.Read())
                {
                    zanr.Add(reader[0].ToString());
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                connection.Close();
            }

            return zanr.ToArray();
        }

        public int[] GetRoky()
        {
            List<int> roky = new List<int>();
            MySqlCommand selection = new MySqlCommand("select rok from rok order by rok;", connection);
            try
            {
                connection.Open();
                MySqlDataReader reader = selection.ExecuteReader();
                while (reader.Read())
                {
                    roky.Add(Convert.ToInt32(reader[0].ToString()));
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                connection.Close();
            }

            return roky.ToArray();
        }

        public string GetConnectionString()
        {
            return this.connectionString;
        }

        public void Add(Film film)
        {
            MySqlCommand insertion = new MySqlCommand();
            string filmQuery = "insert into film(nazev, filename,idrok,popis) values(@nazev, @filename, (select id from rok where rok = @rok), @popis);";
            try
            {
                insertion.Connection = connection;
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    insertion.CommandText = filmQuery;
                    insertion.Parameters.AddWithValue("@nazev", film.Nazev);
                    insertion.Parameters.AddWithValue("@filename", film.Filename);
                    insertion.Parameters.AddWithValue("@rok", (int)film.Rok);
                    insertion.Parameters.AddWithValue("@popis", film.Popis == null ? Convert.DBNull : film.Popis);
                    insertion.Transaction = transaction;
                    insertion.ExecuteNonQuery();

                    for (int index = 0; index < film.Zanr.Count;index++)
                    {
                        filmQuery = $"insert into film_zanr(idfilm, idzanr) values((select id from film where nazev = @film{index}),(select id from zanr where nazev = @zanr{index}));";
                        insertion.CommandText = filmQuery;
                        insertion.Parameters.AddWithValue($"@film{index}", film.Nazev);
                        insertion.Parameters.AddWithValue($"@zanr{index}", film.Zanr[index]);
                        insertion.Transaction = transaction;
                        insertion.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (MySqlException ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Transakce byla přerušena z následujícího důvodu:\n {ex.Message}");
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public void Update(Film film)
        {
            MySqlCommand update = new MySqlCommand();

            string filmQuery = "update film set nazev = @nazev, popis = @popis, idrok = (select id from rok where rok = @rok);";

            try
            {
                update.Connection = connection;
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    update.CommandText = filmQuery;
                    update.Parameters.AddWithValue("@nazev", film.Nazev);
                    update.Parameters.AddWithValue("@popis", film.Popis);
                    update.Parameters.AddWithValue("@rok", film.Rok);
                    update.Transaction = transaction;
                    update.ExecuteNonQuery();

                    for(int index = 0; index < film.Zanr.Count;index++)
                    {
                        filmQuery = $"update film_zanr set idZanr = (select id from zanr where nazev = @zanr{index}) where idfilm = (select id from film where nazev = @film);";
                        update.CommandText = filmQuery;
                        update.Parameters.AddWithValue("@film", film.Nazev);
                        update.Parameters.AddWithValue($"@zanr{index}", film.Zanr[index]);
                        update.Transaction = transaction;
                        update.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch(MySqlException ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Transakce byla přerušena z následujícího důvodu:\n {ex.Message}");
                }
            }
            catch(MySqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Clone();
            }
        }

        public void Delete(Film film)
        {
            MySqlCommand deletion = new MySqlCommand();

            string filmQuery = "delete from film_zanr where idfilm = (select id from film where nazev = @film);delete from film where nazev = @film;";

            try
            {
                deletion.Connection = connection;
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    deletion.CommandText = filmQuery;
                    deletion.Parameters.AddWithValue("@film", film.Nazev);
                    //deletion.Transaction = transaction;
                    deletion.ExecuteNonQuery();
                }
                catch(MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            catch(MySqlException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
