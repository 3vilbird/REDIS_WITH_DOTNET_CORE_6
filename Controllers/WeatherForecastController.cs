using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace RedisDemoapplication.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[]
            Summaries =
                new []
                {
                    "Freezing",
                    "Bracing",
                    "Chilly",
                    "Cool",
                    "Mild",
                    "Warm",
                    "Balmy",
                    "Hot",
                    "Sweltering",
                    "Scorching"
                };

        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IDistributedCache _distributedcache;

        private readonly string cacheKey = "weatherlist";

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IDistributedCache distributedCache
        )
        {
            _logger = logger;
            _distributedcache = distributedCache;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            string serilizedcustomerlist;
            List<string> weatherlist = new List<string>();
            var redisDataList = await _distributedcache.GetAsync(cacheKey);
            if (redisDataList != null)
            {
                serilizedcustomerlist = Encoding.UTF8.GetString(redisDataList);
                weatherlist =
                    JsonConvert
                        .DeserializeObject<List<string>>(serilizedcustomerlist);
            }
            else
            {
                // get data from db and add it into the cache.
                weatherlist = GetDataromDB();
                serilizedcustomerlist =
                    JsonConvert.SerializeObject(weatherlist);
                redisDataList = Encoding.UTF8.GetBytes(serilizedcustomerlist);
                var options =
                    new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                await _distributedcache
                    .SetAsync(cacheKey, redisDataList, options);
            }
            return Ok(weatherlist);
        }

        [HttpDelete(Name = "DeleteCache")]
        public async Task<IActionResult> DeleteCache()
        {
            await _distributedcache.RemoveAsync(cacheKey);
            return Ok();
        }

        [HttpGet(Name = "Ping")]
        public async Task<IActionResult> Ping()
        {
            return Ok("Pong");
        }

        [HttpPost(Name = "AddToCache")]
        public async Task<IActionResult> AddToCache(string Key, string Value)
        {
            var data = Encoding.UTF8.GetBytes(Value);
            var options =
                new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            await _distributedcache.SetAsync(Key, data, options);
            return Ok("Added to cache");
        }

        [HttpGet(Name = "GetByKey")]
        public async Task<IActionResult> GetByKey(string Key)
        {
            var redisDataList = await _distributedcache.GetAsync(Key);
            if (redisDataList != null)
            {
                return Ok(Encoding.UTF8.GetString(redisDataList));
            }
            return Ok("Data not found");
        }

        [HttpDelete(Name = "DeleteCacheByKey")]
        public async Task<IActionResult> DeleteCacheByKey(string key)
        {
            await _distributedcache.RemoveAsync(key);
            return Ok();
        }

        private List<string> GetDataromDB()
        {
            return new List<string> {
                "Freezing",
                "Bracing",
                "Chilly",
                "Cool",
                "Mild",
                "Warm",
                "Balmy",
                "Hot",
                "Sweltering",
                "Scorching"
            };
        }
    }
}
