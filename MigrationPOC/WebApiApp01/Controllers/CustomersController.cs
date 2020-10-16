using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
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
using WebApiApp01;

using SharedCacheContract;
using System.Threading.Tasks;
using log4net;

namespace WebApiApp01.Controllers
{
    public class CustomersController : ApiController
    {
        private ServerlessPOCEntities db = new ServerlessPOCEntities();

        private TelemetryClient _telemetryClient;
        private ISharedCache _sharedCache;
        private ILog _logger;

        public CustomersController()
        {
            _sharedCache = new RedisSharedCache.SharedCache();

            _telemetryClient = new TelemetryClient();
            _telemetryClient.TrackTrace("CustomerController started");

            _logger = LogManager.GetLogger("webapi");
        }
   

        // GET: api/Customers
        public IQueryable<Customer> GetCustomers()
        {
            return db.Customers;
        }

        // GET: api/Customers/5
        [ResponseType(typeof(Customer))]
        public async Task<IHttpActionResult> GetCustomer(Guid id)
        {
            _telemetryClient.TrackTrace($"Customer Id: {id}");

            _logger.Info($"log4net logger Customer id: {id}");
            _logger.Error($"log4net test error message, customer id: {id}");
            Customer customer = null;
            try
            {
                customer = await _sharedCache.GetItemAsync<Customer>(id.ToString());
            }
            catch( Exception e)
            {
                _telemetryClient.TrackTrace($"Failed to get customer from Cache: {id}, error:{e.Message}");
                _telemetryClient.TrackException(e);
            }
            if (customer == null)
            {
                _telemetryClient.TrackTrace($"Customer Id: {id} is not in cache, get it from db.");

                customer = db.Customers.Where(x => x.CustomerId == id).FirstOrDefault();
                //customer = db.Customers.FirstOrDefault<Customer>();
                if (customer == null)
                {
                    return NotFound();
                }
                try
                {

                    await _sharedCache.SetItemAsync<Customer>(id.ToString(), customer);
                }
                catch( Exception e)
                {
                    _telemetryClient.TrackTrace($"Failed to add customer to cache: {id}, error:{e.Message}");
                    _telemetryClient.TrackException(e);
                }
            }
            else
            {
                _telemetryClient.TrackTrace($"Customer Id: {id} is in cache, return from cache");
            }

            LogManager.Flush(1000);
            return Ok(customer);
        }

        // PUT: api/Customers/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCustomer(Guid id, Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            db.Entry(customer).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        [ResponseType(typeof(Customer))]
        public IHttpActionResult PostCustomer(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Customers.Add(customer);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (CustomerExists(customer.CustomerId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = customer.CustomerId }, customer);
        }

        // DELETE: api/Customers/5
        [ResponseType(typeof(Customer))]
        public IHttpActionResult DeleteCustomer(Guid id)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            db.Customers.Remove(customer);
            db.SaveChanges();

            return Ok(customer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CustomerExists(Guid id)
        {
            return db.Customers.Count(e => e.CustomerId == id) > 0;
        }
    }
}