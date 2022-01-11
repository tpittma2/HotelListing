using AutoMapper;
using HotelListing.Data;
using HotelListing.Core.IRepository;
using HotelListing.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [ApiVersion("2.0", Deprecated = true)]
   // [ApiVersion("2.0")]
    [Route("api/{v:apiversion}/country")] //api/2.0/country/
    [ApiController]
    public class CountryV2Controller : ControllerBase
    {
        private DatabaseContext _context; //TESTING PURPOSE ONLY!!!!! NEVER DIRECTLY USE DBContext
        private readonly ILogger<CountryController> _logger;
        private readonly IMapper _mapper;

        public CountryV2Controller(DatabaseContext context, ILogger<CountryController> logger, IMapper mapper)
        {
            this._context = context;
            this._logger = logger;
            this._mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries() //Ex.) /api/country/?api-version=2.0 (if not setting apiversion in class attribute)
        {
            return Ok(_context.Countries); 
        }
    }
}
