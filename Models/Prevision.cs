using Npgsql;
using System;
using System.Collections.Generic;
using Utils.helper;

namespace element
{
    public class Prevision
    {
        public double heure { get; set; }
        public double luminosite { get; set; }
        public double puissance_panneau { get; set; }
        public double puissance_requis { get; set; }
        public double reste_batterie { get; set; }

        public Prevision()
        {
        }

        public Prevision(double heure, double luminosite, double puissance_panneau, double puissance_requis, double reste_batterie)
        {
            this.heure = heure;
            this.luminosite = luminosite;
            this.puissance_panneau = puissance_panneau;
            this.puissance_requis = puissance_requis;
            this.reste_batterie = reste_batterie;
        }

        //////////// PREDICTION COUPURE
        public static List<Prevision> Prevoir(DateTime jour, string id_source,double puissance_par_eleve, NpgsqlConnection connection)
        {
            Source source;
            List<Luminosite> list_luminosite;
            Presence presence;
            
            (source, list_luminosite, presence) = GetDataAndCalculatePower(jour, id_source, connection);
            List<Prevision> list_prevision = PerformPrediction(source, list_luminosite, presence,puissance_par_eleve);
            return list_prevision;
        }

        private static (Source, List<Luminosite>, Presence) GetDataAndCalculatePower(DateTime jour, string id_source, NpgsqlConnection connection)
        {
            int day = DayOfWeek(jour);
            Source source = Source.GetSourceById(connection, id_source);
            List<Luminosite> list_luminosite = Luminosite.SelectLuminosite(jour, connection);
            Presence presence = Presence.AvgEtudiant(connection, day);
            return (source, list_luminosite, presence);
        }

        public static List<Prevision> PerformPrediction(Source source, List<Luminosite> list_luminosite, Presence presence, double puissance_par_eleve)
        {
            double puissance_Batterie = source.capacite_batterie;
            double moitie = puissance_Batterie / 2;
            double puissance_requis_matin = presence.avg_nb_matin * puissance_par_eleve;
            double puissance_requis_midi = presence.avg_nb_midi * puissance_par_eleve;
            double PR = puissance_requis_matin;

            List<Prevision> list_prevision = new List<Prevision>();
            list_prevision.Add(new Prevision(000, 000, 000, 000,puissance_Batterie));

            foreach (Luminosite lum in list_luminosite)
            {
                double pourcentage = lum.value * 10;
                double puissance_total = ((source.nb_panneau * source.capacite_panneau) * pourcentage) / 100;

                if (lum.heure >= 12) { PR = puissance_requis_midi; }
                if (PR > puissance_total)
                {
                    double puissance_manquante = PR - puissance_total;
                    puissance_Batterie -= puissance_manquante;
                }else{
                    puissance_Batterie += (puissance_total - PR);
                    if(puissance_Batterie > source.capacite_batterie){
                        puissance_Batterie = source.capacite_batterie;
                    }
                }
                Prevision previsionMatin = new Prevision(lum.heure, lum.value, puissance_total, PR, puissance_Batterie);
                list_prevision.Add(previsionMatin);
                if(puissance_Batterie < moitie){break;}
            }

            return list_prevision;
        }

        public static double[] GetHeureCoupure(List<Prevision> Previsions, string id_source, NpgsqlConnection connection)
        {
            double minute = 0;
            Prevision fin = Previsions[ Previsions.Count-1];
            Prevision Avantfin = Previsions[ Previsions.Count-2];
            Source source = Source.GetSourceById(connection, id_source);
            double MoitierBatterie = source.capacite_batterie / 2;
            double reste_puissance = (Avantfin.reste_batterie - MoitierBatterie) + fin.puissance_panneau;
            if(Avantfin.puissance_requis == 0){
                return new double[] { 0, 0 };
            }
            if(reste_puissance < fin.puissance_requis){
                minute = (reste_puissance * 60) / fin.puissance_requis;
            }else{
                minute = ((fin.reste_batterie - MoitierBatterie) * 60) / fin.puissance_requis;
                int hs = (int)minute / 60;
                int min = (int)minute % 60;
                return new double[] { fin.heure + hs, min };
            }
            return new double[] { Avantfin.heure, (int)minute };
        }

        public static int DayOfWeek(DateTime date)
        {
            int dayOfWeekNumber = (int)date.DayOfWeek;
            if (dayOfWeekNumber == 8)
            {
                dayOfWeekNumber = 1;
            }
            return dayOfWeekNumber;
        }


    }
}
