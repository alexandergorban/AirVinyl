using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;

namespace AirVinyl.API.Controllers
{
    public class SingletonController : ODataController
    {
        // context
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        [HttpGet]
        [ODataRoute("Tim")]
        public IHttpActionResult GetSingletonTim()
        {
            var personTim = _ctx.People.FirstOrDefault(p => p.PersonId == 6);

            return Ok(personTim);
        }

        [HttpGet]
        [ODataRoute("Tim/Email")]
        [ODataRoute("Tim/FirstName")]
        [ODataRoute("Tim/LastName")]
        [ODataRoute("Tim/DateOfBirth")]
        [ODataRoute("Tim/Gender")]
        public IHttpActionResult GetPersonProperty()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri.Segments.Last();
            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                return StatusCode(System.Net.HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [ODataRoute("Tim/Email/$value")]
        [ODataRoute("Tim/FirstName/$value")]
        [ODataRoute("Tim/LastName/$value")]
        [ODataRoute("Tim/DateOfBirth/$value")]
        [ODataRoute("Tim/Gender/$value")]
        public object GetPersonPropertyRawValue()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
            {
                return NotFound();
            }

            var propertyToGet = Url.Request.RequestUri.Segments[Url.Request.RequestUri.Segments.Length - 2]
                .TrimEnd('/');
            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }

            var propertyValue = person.GetValue(propertyToGet);
            if (propertyValue == null)
            {
                // null = no nontent
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                // return the raw value => ToString()
                return this.CreateOKHttpActionResult(propertyValue.ToString());
            }
        }

        [HttpGet]
        [ODataRoute("Tim/Friends")]
        [EnableQuery]
        public IHttpActionResult GetPersonCollectionProperty()
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();

            var person = _ctx.People.Include(collectionPropertyToGet).FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
            {
                return NotFound();
            }

            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("Tim/VinylRecords")]
        public IHttpActionResult GetVinylRecordsForPerson()
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == 6);
            if (person == null)
            {
                return NotFound();
            }

            // return the collection
            return Ok(_ctx.VinylRecords.Where(v => v.Person.PersonId == 6));
        }

        protected override void Dispose(bool disposing)
        {
            // dispose the context
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}