using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/permits")]
    public class PermitsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public PermitsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet("all")]
        public async Task<ActionResult<List<PermitDTO>>> GetAll()
        {
            var permits = await Context.Permits.ToListAsync();

            var permitsDTOs = Mapper.Map<List<PermitDTO>>(permits);

            return permitsDTOs;
        }


        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<PermitDTO>>> Get([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Permits.AsQueryable();

            var permits = await queryable.Paginar(paginationDto).ToListAsync();

            var totalResults = await queryable.CountAsync();

            var permitsDTOs = Mapper.Map<List<PermitDTO>>(permits);

            var result = new PaginationResultDTO<PermitDTO>
            {
                Results = permitsDTOs,
                TotalResults = totalResults
            };

            return result;
        }





        [HttpGet("{id:int}", Name = "GetPermitById")]
        public async Task<ActionResult<PermitDTO>> GetById(int id)
        {
            var permit = await Context.Permits.FirstOrDefaultAsync(s => s.Id == id);

            if (permit == null)
            {
                return NotFound($"No existe un tipo de titulo con el ID: {id}");
            }

            var permitDTO = Mapper.Map<PermitDTO>(permit);

            return permitDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PermitCreateDTO permitCreateDTO)
        {

            var permit = Mapper.Map<Permit>(permitCreateDTO);

            Context.Add(permit);
            await Context.SaveChangesAsync();


            var permitDTO = Mapper.Map<PermitDTO>(permit);

            return CreatedAtRoute("GetPermitById", new { id = permitDTO.Id }, permitDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] PermitCreateDTO permitCreateDTO)
        {

            var permit = await Context.Permits.FirstOrDefaultAsync(p => p.Id == id);

            if (permit == null)
            {
                return NotFound($"No existe un permiso con el ID {id}");
            }


            permit = Mapper.Map(permitCreateDTO, permit);

            await Context.SaveChangesAsync();


            return NoContent();
        }







    }
}
