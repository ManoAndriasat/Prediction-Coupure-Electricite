using Npgsql;
using System;
using System.Collections.Generic;
using Utils.helper;


namespace element
{
    public class Luminosite
    {
        public DateTime date_jour { get; set; }
        public double heure { get; set; }
        public double value { get; set; }

        public Luminosite()
        {
        }

        public Luminosite(DateTime dateJour, double heure, double luminosite)
        {
            date_jour = dateJour;
            this.heure = heure;
            value = luminosite;
        }

        public static void InsertLuminosite(Luminosite luminosite, NpgsqlConnection connection)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "INSERT INTO Luminosite (date_jour, heure, value) " +
                                  "VALUES (@DateJour, @Heure, @Value)";
                cmd.Parameters.AddWithValue("@DateJour", luminosite.date_jour);
                cmd.Parameters.AddWithValue("@Heure", luminosite.heure);
                cmd.Parameters.AddWithValue("@Value", luminosite.value);

                cmd.ExecuteNonQuery();
            }
        }
        
        public static List<List<Luminosite>> AllLuminosite(NpgsqlConnection connection)
        {
            List<Luminosite> luminosites = DatabaseHelper.Select<Luminosite>(connection, "luminosite", null);
            var groupedLuminosites = luminosites.GroupBy(l => l.date_jour.Date).Select(group => group.ToList()).ToList();
            return groupedLuminosites;
        }

        public static List<Luminosite> SelectLuminosite(DateTime dateJour, NpgsqlConnection connection)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "date_jour", dateJour }
            };
            return DatabaseHelper.Select<Luminosite>(connection, "luminosite", parameters);
        }
    }
}
