using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/periods")]
    public class PeriodsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public PeriodsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }





        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<PeriodDTO>>> GetAllPaginado([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Periods.AsQueryable();

            var periods = await queryable.OrderByDescending(p => p.Id).Paginar(paginationDto)
                                            .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var periodsDTOs = Mapper.Map<List<PeriodDTO>>(periods);

            var result = new PaginationResultDTO<PeriodDTO>
            {
                Results = periodsDTOs,
                TotalResults = totalResults,
                // TODO: Verificar correcto funcionamiento
                TotalPages = (totalResults / paginationDto.Limit)
            };

            return result;
        }



        [HttpGet("all")]
        public async Task<ActionResult<List<PeriodDTO>>> GetAll()
        {

            var periods = await Context.Periods.OrderByDescending(p => p.Id)
                                               .ToListAsync();

            var periodsDTOs = Mapper.Map<List<PeriodDTO>>(periods);

            return periodsDTOs;
        }








        [HttpGet("{id:int}", Name = "GetPeriodById")]
        public async Task<ActionResult<PeriodDTO>> GetById(int id)
        {
            var period = await Context.Periods.FirstOrDefaultAsync(p => p.Id == id);

            if (period == null)
            {
                return NotFound($"No existe un periodo con el ID: {id}");
            }

            var periodDTO = Mapper.Map<PeriodDTO>(period);

            return periodDTO;
        }



        [HttpGet("current")]
        public async Task<ActionResult<PeriodDTO>> GetCurrent()
        {
            var period = await Context.Periods.FirstOrDefaultAsync(p => p.IsCurrent == true);

            if (period == null)
            {
                return NotFound($"No existe un periodo asignado como actual");
            }

            var periodDTO = Mapper.Map<PeriodDTO>(period);

            return periodDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PeriodCreateDTO periodCreateDTO)
        {

            var exist = await Context.Periods.FirstOrDefaultAsync(p => p.IsCurrent == true);



            if ( exist != null )
            {
                exist.IsCurrent = false;
                Context.Entry(exist).State = EntityState.Modified;
                await Context.SaveChangesAsync();
            }

            var period = Mapper.Map<Period>(periodCreateDTO);

            Context.Add(period);
            await Context.SaveChangesAsync();


            var periodDTO = Mapper.Map<PeriodDTO>(period);

            

            return CreatedAtRoute("GetPeriodById", new { id = periodDTO.Id }, periodDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] PeriodCreateDTO periodCreateDTO)
        {


            var period = await Context.Periods.FirstOrDefaultAsync(p => p.Id == id);

            if (period == null)
            {
                return NotFound($"No existe un periodo con el ID {id}");
            }



            if (periodCreateDTO.IsCurrent)
            {

                var periodLast = await Context.Periods.FirstOrDefaultAsync(p => p.IsCurrent == true);

                if (periodLast != null)
                {
                    periodLast.IsCurrent = false;
                    Context.Entry(periodLast).State = EntityState.Modified;
                    await Context.SaveChangesAsync();
                }

            }


            period = Mapper.Map(periodCreateDTO, period);

            await Context.SaveChangesAsync();

            return NoContent();

        }







        [HttpGet("current/{id:int}")]
        public async Task<ActionResult<PeriodDTO>> ChangeIsCurrent(int id)
        {
            var period = await Context.Periods.FirstOrDefaultAsync(p => p.Id == id);

            if (period == null)
            {
                return NotFound($"No existe un periodo con el ID: {id}");
            }


            var periodLast = await Context.Periods.FirstOrDefaultAsync(p => p.IsCurrent == true);

            if (periodLast != null)
            {
                periodLast.IsCurrent = false;
                Context.Entry(periodLast).State = EntityState.Modified;
                await Context.SaveChangesAsync();
            }


            period.IsCurrent = period.IsCurrent ? false : true;
            Context.Entry(period).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            var periodDTO = Mapper.Map<PeriodDTO>(period);

            return CreatedAtRoute("GetPeriodById", new { id = periodDTO.Id }, periodDTO);


        }




    }
}
