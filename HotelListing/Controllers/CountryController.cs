using AutoMapper;
using HotelListing.Data;
using HotelListing.Core.IRepository;
using HotelListing.Core.DTOs;
using HotelListing.Core.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelListing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CountryController> _logger;
        private readonly IMapper _mapper;

        public CountryController(IUnitOfWork unitOfWork, ILogger<CountryController> logger, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._logger = logger;
            this._mapper = mapper;
        }

        [HttpGet]
       // [ResponseCache(CacheProfileName = "120SecondsDuration")] 
      //  [ResponseCache(Duration = 60)]// Cache for 
    //  [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)] //override global settings in ServiceExtensions
    //  [HttpCacheValidation(MustRevalidate = false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries([FromQuery] RequestParams requestParams) //Ex.) api/country/?pagesize=10&pagenumber=3
        {

            var countries = await _unitOfWork.Countries.GetAll(requestParams: requestParams);
            var results = _mapper.Map<IList<CountryDTO>>(countries);
            return Ok(results); //Error handled by global error handling
        }

        [HttpGet("{id:int}", Name = "GetCountry")]
      //  [ResponseCache(CacheProfileName = "120SecondsDuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountry(int id)
        {
            var country = await _unitOfWork.Countries.Get(x => x.Id == id, new List<string> { "Hotels" }); //Include Hotel objects with individual country
            if (country == null)
            {
                return NotFound();
            }
            var result = _mapper.Map<CountryDTO>(country);
            return Ok(result);

        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCountry([FromBody] CreateCountryDTO countryDTO)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Invalid POST attempt in {nameof(CreateCountry)}: {Environment.NewLine}{JsonSerializer.Serialize(countryDTO)}");
                return BadRequest(ModelState);
            }

            var country = _mapper.Map<Country>(countryDTO);
            await _unitOfWork.Countries.Insert(country);
            await _unitOfWork.Save();

            return CreatedAtRoute("GetCountry", new { id = country.Id }, country);

        }

        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // public async Task<IActionResult> UpdateCountry(int id, [FromBody] CountryDTO countryDTO) could be used to validate that the id passed in parameter and the id passed in the object matches
        public async Task<IActionResult> UpdateCountry(int id, [FromBody] UpdateCountryDTO countryDTO)
        {
            if (!ModelState.IsValid || id < 1)
            {
                _logger.LogError($"BadRequest (invalid) for {nameof(UpdateCountry)}, id={id}, dto={System.Text.Json.JsonSerializer.Serialize(countryDTO)}");
                return BadRequest(ModelState);
            }

            try
            {
                var country = await _unitOfWork.Countries.Get(x => x.Id == id);
                if (country == null)
                {
                    _logger.LogError($"BadRequest (not found) for {nameof(UpdateCountry)}, id={id}, dto={System.Text.Json.JsonSerializer.Serialize(countryDTO)}");
                    return BadRequest("Submitted data is invalid");
                }

                _mapper.Map(countryDTO, country);
                _unitOfWork.Countries.Update(country);
                await _unitOfWork.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured in {nameof(UpdateCountry)}: {ex}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                if (id < 0)
                {
                    _logger.LogError($"BadRequest (invalid) for {nameof(DeleteCountry)}, id={id}");
                    return BadRequest("Invalid Id");
                }
                var country = await _unitOfWork.Countries.Get(x => x.Id == id);
                if (country == null)
                {
                    _logger.LogError($"BadRequest (not found) for {nameof(DeleteCountry)}, id={id}");
                    return BadRequest("Submitted data not valid");
                }

                await _unitOfWork.Countries.Delete(id);
                await _unitOfWork.Save();
                return NoContent();
            }
            catch (Exception ex)
            {

                _logger.LogError($"Error occured in {nameof(DeleteCountry)}: {ex}");
                return StatusCode(500, "Internal Server Error. Please try again later.");
            }

        }
    }
}
