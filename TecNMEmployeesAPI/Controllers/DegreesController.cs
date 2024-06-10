using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/degrees")]
    public class DegreesController : ControllerBase
    {




        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public DegreesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }




        [HttpGet]
        public async Task<ActionResult<List<DegreeDTO>>> Get()
        {
            var degrees = await Context.Degrees.ToListAsync();

            var degreesDTOs = Mapper.Map<List<DegreeDTO>>(degrees);

            return degreesDTOs;
        }








        [HttpGet("{id:int}", Name = "GetDegreeById")]
        public async Task<ActionResult<DegreeDTO>> GetById(int id)
        {
            var degree = await Context.Degrees.FirstOrDefaultAsync(s => s.Id == id);

            if (degree == null)
            {
                return NotFound($"No existe un tipo de titulo con el ID: {id}");
            }

            var degreeDTO = Mapper.Map<DegreeDTO>(degree);

            return degreeDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] DegreeCreateDTO degreeCreateDTO)
        {

            var degree = Mapper.Map<Degree>(degreeCreateDTO);

            Context.Add(degree);
            await Context.SaveChangesAsync();


            var degreeeDTO = Mapper.Map<DegreeDTO>(degree);

            return CreatedAtRoute("GetDegreeById", new { id = degreeeDTO.Id }, degreeeDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] DegreeCreateDTO degreeCreateDTO)
        {

            var degree = await Context.Degrees.FirstOrDefaultAsync(d => d.Id == id);

            if (degree == null)
            {
                return NotFound($"No existe un grado de estudios con el ID {id}");
            }


            degree = Mapper.Map(degreeCreateDTO, degree);

            await Context.SaveChangesAsync();


            return NoContent();
        }







    }
}
