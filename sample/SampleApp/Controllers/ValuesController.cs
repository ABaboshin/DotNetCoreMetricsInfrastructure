using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Metrics.Extensions.Tracking;
using Microsoft.AspNetCore.Mvc;
using SampleApp.Database;
using SampleApp.Database.Entities;
using SampleApp.MessageBus;

namespace SampleApp.Controllers
{
    /// <summary>
    /// demo controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IBusControl _busControl;
        private readonly MyDbContext _myDbContext;
        private readonly ICustomTracker _customTracker;

        public ValuesController(IBusControl busControl, MyDbContext myDbContext, ICustomTracker customTracker)
        {
            _busControl = busControl;
            _myDbContext = myDbContext;
            _customTracker = customTracker;
        }


        /// <summary>
        /// executes an 'ok' sql query
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<MyEntity>> Get()
        {
            var x = 0;
            return _myDbContext.MyEntities.Where(e => e.Id > x).ToList();
        }

        /// <summary>
        /// executes a bad sql query
        /// </summary>
        /// <returns></returns>
        [HttpGet("bad")]
        public ActionResult<IEnumerable<BadEntity>> Bad()   
        {
            try
            {
                return _myDbContext.BadEntities;
            }
            catch (Exception)
            {
                return Ok();
            }
        }

        /// <summary>
        /// publish a message with masstransit
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("publish")]
        public async Task<ActionResult<string>> Publish(int id)
        {
            await _busControl.Publish(new MyMessage { Id = id });
            return Ok();
        }

        /// <summary>
        /// produces an http exception
        /// </summary>
        /// <returns></returns>
        [HttpGet("exception")]
        public ActionResult<string> Exception()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// healthcheck
        /// </summary>
        /// <returns></returns>
        [HttpGet("hc")]
        public ActionResult<string> HealthCheck()
        {
            var dl = new DiagnosticListener("HealthChecks");
            dl.Write("healthcheck", new Dictionary<string, bool>
            {
                {"dep1", true },
                {"dep2", false }
            });
            return Ok();
        }

        /// <summary>
        /// track successfull execution
        /// </summary>
        /// <returns></returns>
        [HttpGet("trackok")]
        public async Task<ActionResult<string>> TrackOk()
        {
            _customTracker.ActivityName = "SampleActivity";
            _customTracker.TraceIdentifier = HttpContext.TraceIdentifier;
            _customTracker.Start();
            await Task.Delay(1000);

            _customTracker.Finish();

            return Ok();
        }

        /// <summary>
        /// track successfull execution
        /// </summary>
        /// <returns></returns>
        [HttpGet("trackexception")]
        public async Task<ActionResult<string>> TrackException()
        {
            _customTracker.ActivityName = "SampleActivity";
            _customTracker.TraceIdentifier = HttpContext.TraceIdentifier;
            _customTracker.Start();
            await Task.Delay(1000);

            try
            {
                throw new NotImplementedByDesignException();
            }
            catch (Exception exception)
            {
                _customTracker.Finish(exception);
            }

            return Ok();
        }
    }
}
