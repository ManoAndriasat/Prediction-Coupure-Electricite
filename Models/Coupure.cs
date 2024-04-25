using Npgsql;
using System;
using System.Collections.Generic;
using Utils.helper;

namespace element
{
    public class Coupure
    {
        public DateTime DateCoupure { get; set; }
        public string IdSource { get; set; }
        public int Heure { get; set; }
        public int Minute { get; set; }

        public Coupure()
        {
        }

        public Coupure(DateTime dateCoupure, string idSource, int heure, int minute)
        {
            DateCoupure = dateCoupure;
            IdSource = idSource;
            Heure = heure;
            Minute = minute;
        }

        public static Coupure GetCoupure(NpgsqlConnection connection, DateTime dateCoupure)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "dateCoupure", dateCoupure }
            };
            return DatabaseHelper.Select<Coupure>(connection, "coupure", parameters)[0];
        }

        public static List<Coupure> AllCoupure(NpgsqlConnection connection)
        {
            return DatabaseHelper.Select<Coupure>(connection, "coupure");
        }
    }
}
