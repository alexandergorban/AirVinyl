﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using AirVinyl.DataAccessLayer;
using AirVinyl.API.Helpers;
using AirVinyl.Model;

namespace AirVinyl.API.Controllers
{
    [EnableCors(origins: "http://localhost:4329", headers: "*", methods: "*")]
    public class PeopleController : ODataController
    {
        private AirVinylDbContext _ctx = new AirVinylDbContext();

        [EnableQuery(MaxExpansionDepth = 3, MaxSkip = 10, MaxTop = 5, PageSize = 4)]
        public IHttpActionResult Get()
        {
            return Ok(_ctx.People);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            var people = _ctx.People.Where(p => p.PersonId == key);
            if (!people.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(people));
        }

        [HttpGet]
        [ODataRoute("People({key})/Email")]
        [ODataRoute("People({key})/FirstName")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/DateOfBirth")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonProperty([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
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
        [ODataRoute("People({key})/Friends")]
        [EnableQuery]
        public IHttpActionResult GetPersonCollectionProperty([FromODataUri] int key)
        {
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();

            var person = _ctx.People.Include(collectionPropertyToGet).FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);

            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult GetVinylRecordsForPerson([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            // return the collection
            return Ok(_ctx.VinylRecords.Include("DynamicVinylRecordProperties").Where(v => v.Person.PersonId == key));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult GetVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            // queryable, no FirstOrDefauld
            var vinylRecords =
                _ctx.VinylRecords.Include("DynamicVinylRecordProperties")
                    .Where(v => v.Person.PersonId == key && v.VinylRecordId == vinylRecordKey);
            if (!vinylRecords.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(vinylRecords));
        }

        [HttpPost]
        [ODataRoute("People({key})/VinylRecords")]
        public IHttpActionResult CreateVinylRecordForPerson([FromODataUri] int key, VinylRecord vinylRecord)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // does the person exist?
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            // link the person to the VinylRecord (also avoids an invalid person
            // key on the passed-in record - key from the URL wins)
            vinylRecord.Person = person;

            // add the VinylRecord
            _ctx.VinylRecords.Add(vinylRecord);
            _ctx.SaveChanges();

            return Created(vinylRecord);
        }

        [HttpPatch]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult PartiallyUpdateVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey, Delta<VinylRecord> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // does the person exist?
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }

            // find a matching vinyl record
            var currentVinylRecord =_ctx.VinylRecords
                .Include("DynamicVinylRecordProperties")
                .FirstOrDefault(p => p.VinylRecordId == vinylRecordKey && p.Person.PersonId == key);
            if (currentVinylRecord == null)
            {
                return NotFound();
            }

            // apply patch
            patch.Patch(currentVinylRecord);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [ODataRoute("People({key})/VinylRecords({vinylRecordKey})")]
        public IHttpActionResult DeleteVinylRecordForPerson([FromODataUri] int key, [FromODataUri] int vinylRecordKey)
        {
            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            // find a matching vinyl record
            var currentVinylRecord =
                _ctx.VinylRecords.FirstOrDefault(p => p.VinylRecordId == vinylRecordKey && p.Person.PersonId == key);
            if (currentVinylRecord == null)
            {
                return NotFound();
            }

            _ctx.VinylRecords.Remove(currentVinylRecord);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ODataRoute("People({key})/Email/$value")]
        [ODataRoute("People({key})/FirstName/$value")]
        [ODataRoute("People({key})/LastName/$value")]
        [ODataRoute("People({key})/DateOfBirth/$value")]
        [ODataRoute("People({key})/Gender/$value")]
        public object GetPersonPropertyRawValue([FromODataUri] int key)
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
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

        // [HttpPost]
        // [ODataRoute("People")]
        // POST odata/People
        public IHttpActionResult Post(Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _ctx.People.Add(person);
            _ctx.SaveChanges();

            return Created(person);
        }

        // PUT odata/People('key')
        // [HttpPut]
        // [ODataRoute("People({key})")]
        // PUT is for full updates
        public IHttpActionResult Put([FromODataUri] int key, Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            person.PersonId = currentPerson.PersonId;
            _ctx.Entry(currentPerson).CurrentValues.SetValues(person);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PATCH odata/People('key')
        // [HttpPatch]
        // [ODataRoute("People({key})")]
        // PATCH is for partial updates
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            patch.Patch(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE odata/People('key')
        // [HttpDelete]
        // [ODataRoute("People({key})")]
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            var peopleWithCurrentPersonAsFriend = _ctx.People.Include("Friends")
                .Where(p => p.Friends.Select(f => f.PersonId).AsQueryable().Contains(key));

            foreach (var person in peopleWithCurrentPersonAsFriend.ToList())
            {
                person.Friends.Remove(currentPerson);
            }

            _ctx.People.Remove(currentPerson);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("People({key})/Friends/$ref")]
        public IHttpActionResult CreateLinkToFriend([FromODataUri] int key, [FromBody] Uri link)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            int keyOfFriendToAdd = Request.GetKeyValue<int>(link);
            if (currentPerson.Friends.Any(i => i.PersonId == keyOfFriendToAdd))
            {
                return BadRequest(string.Format("The person with id {0} is already linked to the person with id {1}",
                    key, keyOfFriendToAdd));
            }

            var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);
            if (friendToLinkTo == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult UpdateLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey,
            [FromBody] Uri link)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            var currentFriend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
            if (currentFriend == null)
            {
                return NotFound();
            }

            int keyOfFriendToAdd = Request.GetKeyValue<int>(link);
            if (currentPerson.Friends.Any(f => f.PersonId == keyOfFriendToAdd))
            {
                return BadRequest(string.Format("The person with id {0} is already linked to the person with id {1}",
                    key, keyOfFriendToAdd));
            }

            var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAdd);
            if (friendToLinkTo == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Remove(currentFriend);
            currentPerson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE odata/People('key')/Friends/$ref?$id={'relatedUriWithRalatedKey'}
        [HttpDelete]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]
        public IHttpActionResult DeleteLinkToFriend([FromODataUri] int key, [FromODataUri] int relatedKey)
        {
            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }

            var friend = currentPerson.Friends.FirstOrDefault(f => f.PersonId == relatedKey);
            if (friend == null)
            {
                return NotFound();
            }

            currentPerson.Friends.Remove(friend);
            _ctx.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            _ctx.Dispose();
            base.Dispose(disposing);
        }
    }
}