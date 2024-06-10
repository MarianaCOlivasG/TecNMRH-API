
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/nonworkingdays")]
    public class NonWorkingDaysController: ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public NonWorkingDaysController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }





        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<NonWorkingDayDTO>>> Get([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.NonWorkingDays.AsQueryable();

            var nonWorkingDays = await queryable.Paginar(paginationDto)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var nonWorkingDaysDTOs = Mapper.Map<List<NonWorkingDayDTO>>(nonWorkingDays);

            var result = new PaginationResultDTO<NonWorkingDayDTO>
            {
                Results = nonWorkingDaysDTOs,
                TotalResults = totalResults,
            };

            return result;
        }




        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<NonWorkingDayDTO>>> GetByFilter([FromQuery] NonWorkingDaysFilterDTO nonWorkingDaysFilterDTO)
        {


            var queryable = Context.NonWorkingDays.AsQueryable();


            if (nonWorkingDaysFilterDTO.PeriodId != 0)
            {
                var period = await Context.Periods.FirstOrDefaultAsync( p => p.Id == nonWorkingDaysFilterDTO.PeriodId);

                if ( period == null )
                {
                    return NotFound($"No existe un periodo con el ID { nonWorkingDaysFilterDTO.PeriodId }");
                }

                queryable = queryable.Where(n => n.StartDate >= period.StartDate && n.FinalDate <= period.FinalDate );
            }

            var nonWorkingDays = await queryable.Paginar(nonWorkingDaysFilterDTO.Pagination)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var nonWorkingDaysDTOs = Mapper.Map<List<NonWorkingDayDTO>>(nonWorkingDays);

            var result = new PaginationResultDTO<NonWorkingDayDTO>
            {
                Results = nonWorkingDaysDTOs,
                TotalResults = totalResults,
            };

            return result;
        }




        [HttpGet("all")]
        public async Task<ActionResult<List<NonWorkingDayDTO>>> GetAll()
        {
            var nonWorkingDays = await Context.NonWorkingDays.ToListAsync();

            var nonWorkingDaysDTOs = Mapper.Map<List<NonWorkingDayDTO>>(nonWorkingDays);

            return nonWorkingDaysDTOs;
        }



        [HttpGet("{id:int}", Name = "GetNonWorkingDayById")]
        public async Task<ActionResult<NonWorkingDayDTO>> GetById(int id)
        {
            var nonWorkingDay = await Context.NonWorkingDays.FirstOrDefaultAsync(s => s.Id == id);

            if (nonWorkingDay == null)
            {
                return NotFound($"No existe un dia inhábil con el ID: {id}");
            }

            var nonWorkingDayDTO = Mapper.Map<NonWorkingDayDTO>(nonWorkingDay);

            return nonWorkingDayDTO;
        }


        [HttpPost]
        public async Task<ActionResult> Create([FromBody] NonWorkingDayCreateDTO nonWorkingDayCreateDTO)
        {

            var nonWorkingDay = Mapper.Map<NonWorkingDay>(nonWorkingDayCreateDTO);

            Context.Add(nonWorkingDay);
            await Context.SaveChangesAsync();


            var nonWorkingDayDTO = Mapper.Map<NonWorkingDayDTO>(nonWorkingDay);

            return CreatedAtRoute("GetNonWorkingDayById", new { id = nonWorkingDayDTO.Id }, nonWorkingDayDTO);

        }



        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] NonWorkingDayCreateDTO nonWorkingDayCreateDTO)
        {

            var nonWorkingDay = Mapper.Map<NonWorkingDay>(nonWorkingDayCreateDTO);

            nonWorkingDay.Id = id;

            Context.Entry(nonWorkingDay).State = EntityState.Modified;

            await Context.SaveChangesAsync();


            return NoContent();
        }



    }
}
