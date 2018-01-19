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

            var peopleAsList = peopleResponse.ToList();

            DataServiceQueryContinuation<Person> token = peopleResponse.GetContinuation();

            peopleResponse = context.Execute(token);
            peopleAsList = peopleResponse.ToList();

            string additionalData = "Total count:" + peopleResponse.TotalCount.ToString();


            // select people by FirstName.EnsWith("n")
            //var peopleResponse = context.People
            //    .Expand(p => p.VinylRecords)
            //    .Where(p => p.FirstName.EndsWith("n"))
            //    .OrderByDescending(p => p.FirstName)
            //    .Skip(1)
            //    .Take(1);

            //var peopleAsList = peopleResponse.ToList();

            //// selest names
            //var selectFromPeople = context.People.Select(p => new { p.FirstName, p.LastName });

            //string additionalData = "";
            //foreach (var partialPerson in selectFromPeople)
            //{
            //    additionalData += partialPerson.FirstName + " " + partialPerson.LastName + "\n";
            //}
            //// 

            var personResponse = context.People.ByKey(1).GetValue();

            //// add new Person()
            //var newPerson = new Person()
            //{
            //    FirstName = "Maggie",
            //    LastName = "Smith"
            //};

            //context.AddToPeople(newPerson);
            //context.SaveChanges();

            //// Updated person
            //newPerson.FirstName = "Violet";
            //context.UpdateObject(newPerson);
            //context.SaveChanges();

            //// Delete person
            //context.DeleteObject(newPerson);
            //context.SaveChanges();

            //var peopleResponse = context.People.OrderByDescending(p => p.PersonId);
            //var peopleAsList = peopleResponse.ToList();

            return View(new AirVinylViewModel()
            {
                People = peopleAsList,
                Person = personResponse,
                AdditionalData = additionalData
            });
        }
    }
}