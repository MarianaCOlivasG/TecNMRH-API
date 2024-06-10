

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/workstations")]
    public class WorkStationsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public WorkStationsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }




        [HttpGet("all")]
        public async Task<ActionResult<List<WorkStationDTO>>> GetAll()
        {

            var workStations = await Context.WorkStations.AsQueryable()
                                    .OrderBy(w => w.Name)
                                    .ToListAsync();

            var workStationsDTOs = Mapper.Map<List<WorkStationDTO>>(workStations);

            return workStationsDTOs;
        }



        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<WorkStationDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.WorkStations.AsQueryable();

            var workStations = await queryable.Paginar(paginationDto)
                                    .OrderBy(w => w.Name)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var workStationsDTOs = Mapper.Map<List<WorkStationDTO>>(workStations);

            var result = new PaginationResultDTO<WorkStationDTO>
            {
                Results = workStationsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }







        [HttpGet("{id:int}", Name = "GetWorkStationById")]
        public async Task<ActionResult<WorkStationDTO>> GetById(int id)
        {
            var workStation = await Context.WorkStations.FirstOrDefaultAsync(w => w.Id == id);

            if (workStation == null)
            {
                return NotFound($"No existe un puesto con el ID: {id}");
            }

            var workStationDTO = Mapper.Map<WorkStationDTO>(workStation);

            return workStationDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] WorkStationCreateDTO workStationCreateDTO)
        {

            var workStation = Mapper.Map<WorkStation>(workStationCreateDTO);

            Context.Add(workStation);
            await Context.SaveChangesAsync();


            var workStationDTO = Mapper.Map<WorkStationDTO>(workStation);

            return CreatedAtRoute("GetWorkStationById", new { id = workStationDTO.Id }, workStationDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] WorkStationCreateDTO workStationCreateDTO)
        {

            var workStation = await Context.WorkStations.FirstOrDefaultAsync(d => d.Id == id);

            if (workStation == null)
            {
                return NotFound($"No existe un puesto con el ID {id}");
            }


            workStation = Mapper.Map(workStationCreateDTO, workStation);

            await Context.SaveChangesAsync();

            return NoContent();
        }






    }
}
