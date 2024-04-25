using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using Utils.helper;
using element;

public class PrevisionController : Controller
{
    public IActionResult Index()
    {
        Connection connectionManager = new Connection();
        NpgsqlConnection connection = connectionManager.GetConnection();
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("id_source"))){HttpContext.Session.SetString("id_source","SRC1");}
        string id_source = HttpContext.Session.GetString("id_source");
        string dateFromSession = HttpContext.Session.GetString("desiredDate");
        DateTime desiredDate = !string.IsNullOrEmpty(dateFromSession) ? Convert.ToDateTime(dateFromSession) : Convert.ToDateTime("2023-11-1");

        // Coupure test = Coupure.GetCoupure(connection,desiredDate);
        // double consommation = Source.Consommation(test,connection);

        List<Coupure> coupures = Coupure.AllCoupure(connection);
        if (!double.TryParse(HttpContext.Session.GetString("consommation"), out double consommation))
        {
            HttpContext.Session.SetString("consommation", Source.AverageConsommation(coupures, connection).ToString());
            consommation = Convert.ToDouble(HttpContext.Session.GetString("consommation"));
        }

        List<Prevision> previsions = Prevision.Prevoir(desiredDate, id_source,consommation,connection);

        if (previsions.Count > 0)
        {
            double[] heureCoupure = Prevision.GetHeureCoupure(previsions, id_source,connection);
            ViewBag.Previsions = previsions;
            ViewBag.Coupure = heureCoupure;
            ViewBag.consommation = consommation;
            ViewBag.date = desiredDate;
            ViewBag.classe = Source.SelectAllSources(connection);
            connection.Close();
            return View();
        }
        else
        {
            connection.Close();
            return StatusCode(500, "No data available.");
        }
    }

    public IActionResult ChangeDate(DateTime selectedDate)
    {
        HttpContext.Session.SetString("desiredDate", selectedDate.ToString("yyyy-MM-dd"));
        return RedirectToAction("Index");
    }

    public IActionResult ChangeSource(string selectedSource)
    {
        HttpContext.Session.SetString("id_source", selectedSource);
        return RedirectToAction("Index");
    }
}
