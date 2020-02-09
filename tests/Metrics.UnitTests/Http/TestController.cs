using System;
using Microsoft.AspNetCore.Mvc;

namespace Metrics.UnitTests.Http
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("okresult")]
        public ActionResult OkResult()
        {
            return Ok();
        }

        [HttpGet("exception")]
        public ActionResult Exception()
        {
            throw new NotImplementedException();
        }
    }
}
