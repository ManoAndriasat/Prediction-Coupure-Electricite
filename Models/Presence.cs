using Npgsql;
using System;
using System.Collections.Generic;
using Utils.helper;

namespace element
{
    public class Presence
    {
        public int day_of_week { get; set; }
        public string id_source { get; set; }
        public double avg_nb_matin { get; set; }
        public double avg_nb_midi { get; set; }

        public Presence()
        {
        }

        public Presence( int dayOfWeek, string idSource, double avgNbMatin, double avgNbMidi)
        {
            day_of_week = dayOfWeek;
            id_source = idSource;
            avg_nb_matin = avgNbMatin;
            avg_nb_midi = avgNbMidi;
        }
        
        public static Presence AvgEtudiant(NpgsqlConnection conn, int date)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "day_of_week", date }
            };
            List<Presence> presenceList = DatabaseHelper.Select<Presence>(conn, "avg_etudiant", parameters);
            return presenceList[0];
        }

        public static Presence SimplePresence(NpgsqlConnection connection, DateTime targetDate, string idSource)
        {
            List<Presence> results = new List<Presence>();

            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT day_of_week, id_source, nb_matin, nb_midi FROM etudiant_present WHERE date_presence = @targetDate AND id_source = @idSource", connection))
            {
                cmd.Parameters.AddWithValue("targetDate", targetDate);
                cmd.Parameters.AddWithValue("idSource", idSource);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Presence presence = new Presence
                        {
                            day_of_week = reader.GetInt32(reader.GetOrdinal("day_of_week")),
                            id_source = reader.GetString(reader.GetOrdinal("id_source")),
                            avg_nb_matin = reader.GetDouble(reader.GetOrdinal("nb_matin")),
                            avg_nb_midi = reader.GetDouble(reader.GetOrdinal("nb_midi"))
                        };

                        results.Add(presence);
                    }
                }
            }

            return results.Count > 0 ? results[0] : null;
        }


        public static void Insert(NpgsqlConnection conn, DateTime datePresence, string classe, double nbMatin, double nbMidi)
        {
            string query = "INSERT INTO presence VALUES ('" + datePresence.ToString("yyyy-MM-dd") + "', '" +
                classe + "', " +
                nbMatin + ", " +
                nbMidi + "')";

            using (var command = new NpgsqlCommand(query, conn))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
