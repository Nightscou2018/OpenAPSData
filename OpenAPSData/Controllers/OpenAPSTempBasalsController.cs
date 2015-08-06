using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using OpenAPSData;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace OpenAPSData.Controllers
{
    public class OpenAPSTempBasalsController : ApiController
    {
        private OpenAPSTempBasalEntities db = new OpenAPSTempBasalEntities();
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;


        // GET: api/OpenAPSTempBasals
        public IQueryable<OpenAPSTempBasal> GetOpenAPSTempBasals()
        {
            return db.OpenAPSTempBasals;
        }

        // GET: api/OpenAPSTempBasals/5
        [ResponseType(typeof(OpenAPSTempBasal))]
        public IHttpActionResult GetOpenAPSTempBasal(int id)
        {
            OpenAPSTempBasal openAPSTempBasal = db.OpenAPSTempBasals.Find(id);
            if (openAPSTempBasal == null)
            {
                return NotFound();
            }

            return Ok(openAPSTempBasal);
        }

        // PUT: api/OpenAPSTempBasals/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOpenAPSTempBasal(int id, OpenAPSTempBasal openAPSTempBasal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != openAPSTempBasal.Id)
            {
                return BadRequest();
            }

            db.Entry(openAPSTempBasal).State = System.Data.Entity.EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OpenAPSTempBasalExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/OpenAPSTempBasals
        [ResponseType(typeof(OpenAPSTempBasal))]
        public async Task<IHttpActionResult> PostOpenAPSTempBasal(OpenAPSTempBasal openAPSTempBasal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.OpenAPSTempBasals.Add(openAPSTempBasal);
            db.SaveChanges();
           return await updateMongo(openAPSTempBasal);
            
            
        }

        // DELETE: api/OpenAPSTempBasals/5
        [ResponseType(typeof(OpenAPSTempBasal))]
        public IHttpActionResult DeleteOpenAPSTempBasal(int id)
        {
            OpenAPSTempBasal openAPSTempBasal = db.OpenAPSTempBasals.Find(id);
            if (openAPSTempBasal == null)
            {
                return NotFound();
            }

            db.OpenAPSTempBasals.Remove(openAPSTempBasal);
            db.SaveChanges();

            return Ok(openAPSTempBasal);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OpenAPSTempBasalExists(int id)
        {
            return db.OpenAPSTempBasals.Count(e => e.Id == id) > 0;
        }

        private async Task<IHttpActionResult> updateMongo(OpenAPSTempBasal openAPSTempBasal)
        {
            _client = new MongoClient(new MongoUrl("[Your Mongo URL]"));
            _database = _client.GetDatabase("[Your Collection]");
            var document = new BsonDocument();
            document.Add("enteredBy", "OpenAPS");
            document.Add("eventType","Temp Basal");
            document.Add("glucose", openAPSTempBasal.bg.ToString());
            document.Add("glucoseType", "Sensor");
            document.Add("carbs", "0");
            document.Add("insulin", openAPSTempBasal.rate.ToString());
            document.Add("duration", openAPSTempBasal.duration.ToString());
            document.Add("units","mg/dl");
            document.Add("created_at", openAPSTempBasal.timestamp.Value.GetDateTimeFormats()[101].ToString());
            var collection = _database.GetCollection<BsonDocument>("treatments");
            await collection.InsertOneAsync(document);
            return CreatedAtRoute("DefaultApi", new { id = openAPSTempBasal.Id }, openAPSTempBasal);
        }
    }
}