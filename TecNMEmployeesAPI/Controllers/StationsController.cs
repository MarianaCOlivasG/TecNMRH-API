using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/stations")]
    public class StationsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public StationsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }





        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<StationDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Stations.AsQueryable();

            var stations = await queryable.Paginar(paginationDto)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var stationsDTOs = Mapper.Map<List<StationDTO>>(stations);

            var result = new PaginationResultDTO<StationDTO>
            {
                Results = stationsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }



        [HttpGet("{id:int}", Name = "GetStationById")]
        public async Task<ActionResult<StationDTO>> GetById(int id)
        {
            var station = await Context.Stations
                                    .FirstOrDefaultAsync(s => s.Id == id);

            if (station == null)
            {
                return NotFound($"No existe un checador con el ID: {id}");
            }

            var stationDTO = Mapper.Map<StationDTO>(station);

            return stationDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] StationCreateDTO stationCreateDTO)
        {

            var station = Mapper.Map<Station>(stationCreateDTO);

            Context.Add(station);
            await Context.SaveChangesAsync();

            var stationDTO = Mapper.Map<StationDTO>(station);

            return CreatedAtRoute("GetStationById", new { id = stationDTO.Id }, stationDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] StationCreateDTO stationCreateDTO)
        {

            var stationdb = await Context.Stations
                    .FirstOrDefaultAsync(s => s.Id == id);

            if (stationdb == null)
            {
                return NotFound($"No existe un checador con el ID {id}");
            }


            stationdb = Mapper.Map(stationCreateDTO, stationdb);

            await Context.SaveChangesAsync();

            return NoContent();


        }

    }
}
