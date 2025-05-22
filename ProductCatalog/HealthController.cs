namespace ProductCatalog
{
    using Microsoft.AspNetCore.Mvc;

    [Route("health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}
