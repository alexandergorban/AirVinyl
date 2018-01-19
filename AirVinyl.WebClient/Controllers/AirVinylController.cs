using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AirVinyl.Model;
using AirVinyl.WebClient.Models;
using Microsoft.OData.Client;

namespace AirVinyl.WebClient.Controllers
{
    public class AirVinylController : Controller
    {
        // GET: AirVinyl
        public ActionResult Index()
        {
            var context = new AirVinylContainer(new Uri("http://localhost:15707/odata"));

            var peopleResponse = context.People
                .IncludeTotalCount()
                .Expand(p => p.VinylRecords)
                .Execute() as QueryOperationResponse<Person>;

            string additionalData = "Total count:" + peopleResponse.TotalCount.ToString();

            var personResponse = context.People.ByKey(1).GetValue();

            return View(new AirVinylViewModel()
            {
                People = peopleResponse,
                Person = personResponse,
                AdditionalData = additionalData
            });
        }
    }
}