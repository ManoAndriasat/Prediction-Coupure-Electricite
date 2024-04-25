using Npgsql;
using System.Collections.Generic;
using Utils.helper;
using System;

namespace element
{
    public class Source
    {
        public string id_source { get; set; }
        public double nb_panneau { get; set; }
        public double capacite_panneau { get; set; }
        public double capacite_batterie { get; set; }

        public Source()
        {
        }

        public Source(string idSource, double nbPanneau, double capacitePanneau, double capaciteBatterie)
        {
            id_source = idSource;
            nb_panneau = nbPanneau;
            capacite_panneau = capacitePanneau;
            capacite_batterie = capaciteBatterie;
        }

        public static List<Source> SelectAllSources(NpgsqlConnection conn)
        {
            return DatabaseHelper.Select<Source>(conn, "source");
        }

        public static Source GetSourceById(NpgsqlConnection conn, string idSource)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "id_source", idSource }
            };
            List<Source> sources = DatabaseHelper.Select<Source>(conn, "source", parameters);
            return sources.Count > 0 ? sources[0] : null;
        }


        public static double AverageConsommation(List<Coupure> coupures,NpgsqlConnection connection){
            double resultat = 0;
            double incrementation = 0;
            foreach(Coupure coupure in coupures){
                if(coupure != null){
                    double consommation = Consommation(coupure,connection);
                    Console.WriteLine(consommation);

                    if(consommation !=0){
                        resultat += consommation;
                    }else{
                        incrementation +=1;
                    }
                }
            }
            return (resultat/(coupures.Count-incrementation));
        }


        public static double Consommation(Coupure CoupureReel, NpgsqlConnection connection)
        {
            double consommation = 1;
            (Source source, List<Luminosite> list_luminosite, Presence presence) = DataConsommationMoyenne(CoupureReel.DateCoupure, CoupureReel.IdSource, connection);

            List<Prevision> list_prevision = new List<Prevision>();
            double[] heureCoupureTest = new double[2];

            double heureTest = 0;
            double minuteTest = 0;
            double incrementation = 0.1;

            while (Convert.ToDouble(CoupureReel.Heure + "," + CoupureReel.Minute) != Convert.ToDouble(heureTest + "," + minuteTest))
            {
                if(consommation>200){return 0;}
                list_prevision = Prevision.PerformPrediction(source, list_luminosite, presence, consommation);
                heureCoupureTest = Prevision.GetHeureCoupure(list_prevision, CoupureReel.IdSource, connection);
                heureTest = heureCoupureTest[0];
                minuteTest = heureCoupureTest[1];
                consommation += incrementation;
                // Console.WriteLine(heureTest + ":" + minuteTest);
                // Console.WriteLine(consommation);
            }
            return consommation-incrementation;
        }


        private static (Source, List<Luminosite>, Presence) DataConsommationMoyenne(DateTime jour, string id_source, NpgsqlConnection connection)
        {
            Source source = Source.GetSourceById(connection, id_source);
            List<Luminosite> list_luminosite = Luminosite.SelectLuminosite(jour, connection);
            Presence presence = Presence.SimplePresence(connection, jour ,id_source);

            return (source, list_luminosite, presence);
        }

    }
}
