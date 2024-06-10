using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/times")]
    public class TimesController: ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public TimesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<TimeDTO>>> GetAllPaginacion([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Times.AsQueryable();

            var times = await queryable.Paginar(paginationDto)
                                    .Include(t => t.StaffType)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var timesDTOs = Mapper.Map<List<TimeDTO>>(times);

            var result = new PaginationResultDTO<TimeDTO>
            {
                Results = timesDTOs,
                TotalResults = totalResults
            };

            return result;
        }





        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<TimeDTO>>> GetAllFilter([FromQuery] TimesFilterDTO timesFilterDTO)
        {

            var queryable = Context.Times.AsQueryable();


            //if (timesFilterDTO.PeriodId != 0)
            //{
            //    queryable = queryable.Where(t => t.PeriodId == timesFilterDTO.PeriodId);
            //}

            if (timesFilterDTO.StaffTypeId != 0)
            {
                queryable = queryable.Where(t => t.StaffTypeId == timesFilterDTO.StaffTypeId);
            }


            var times = await queryable.Paginar(timesFilterDTO.Pagination)
                                            .Include(t => t.StaffType)
                                            .ToListAsync();

            var totalResults = await queryable.CountAsync();



            var timesDTOs = Mapper.Map<List<TimeDTO>>(times);

            var result = new PaginationResultDTO<TimeDTO>
            {
                Results = timesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }






        [HttpGet("{id:int}", Name = "GetTimeById")]
        public async Task<ActionResult<TimeDTO>> GetById(int id)
        {
            var time = await Context.Times
                                .Include(t => t.StaffType)
                                .FirstOrDefaultAsync(t => t.Id == id);

            if (time == null)
            {
                return NotFound($"No existe un registro con el ID: {id}");
            }

            var timeDTO = Mapper.Map<TimeDTO>(time);

            return timeDTO;
        }





        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TimeCreateDTO timeCreateDTO)
        {

            var exits = await Context.Times
                                 .FirstOrDefaultAsync(p => p.StaffTypeId == timeCreateDTO.StaffTypeId);

            if (exits != null)
            {
                return BadRequest($"Ya existe un registro para el tipo de empleado seleccionados.");
            }


            var time = Mapper.Map<Time>(timeCreateDTO);

            Context.Add(time);
            await Context.SaveChangesAsync();

            var timeCreated = await Context.Times.AsNoTracking()
                                    .Include(t => t.StaffType)
                                    .FirstAsync(t => t.Id == time.Id);

            var timeDTO = Mapper.Map<TimeDTO>(timeCreated);

            return CreatedAtRoute("GetTimeById", new { id = timeDTO.Id }, timeDTO);

        }





        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] TimeCreateDTO timeCreateDTO)
        {

            var timesdb = await Context.Times.FirstOrDefaultAsync(t => t.Id == id);

            if (timesdb == null )
            {
                return NotFound($"No existe un registro con el ID { id }");
            } 

            var exists = await Context.Times
                                 .FirstOrDefaultAsync(p => p.StaffTypeId == timeCreateDTO.StaffTypeId);

            if (exists != null && exists.Id != id)
            {
                return BadRequest($"Ya existe un registro con el periodo y el tipo de empleado seleccionados.");
            }


            timesdb = Mapper.Map(timeCreateDTO, timesdb);

            await Context.SaveChangesAsync();

            return NoContent();


        }




    }
}
