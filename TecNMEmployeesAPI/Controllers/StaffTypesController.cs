using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/stafftypes")]
    public class StaffTypesController: ControllerBase
    {




        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public StaffTypesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }




        [HttpGet("all")]
        public async Task<ActionResult<List<StaffTypeDTO>>> Get()
        {
            var staffTypes = await Context.StaffTypes.ToListAsync();

            var staffTypesDTOs = Mapper.Map<List<StaffTypeDTO>>(staffTypes);

            return staffTypesDTOs;
        }



        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<StaffTypeDTO>>> GetAllPaginacion([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.StaffTypes.AsQueryable();

            var staffTypes = await queryable.Paginar(paginationDto)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var staffTypesDTOs = Mapper.Map<List<StaffTypeDTO>>(staffTypes);

            var result = new PaginationResultDTO<StaffTypeDTO>
            {
                Results = staffTypesDTOs,
                TotalResults = totalResults
            };

            return result;
        }






        [HttpGet("{id:int}", Name = "GetStaffTypeById")]
        public async Task<ActionResult<StaffTypeDTO>> GetById(int id)
        {
            var staffType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == id);

            if (staffType == null)
            {
                return NotFound($"No existe un tipo de empleado con el ID: {id}");
            }

            var staffTypeDTO = Mapper.Map<StaffTypeDTO>(staffType);

            return staffTypeDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] StaffTypeCreateDTO staffTypeCreateDTO)
        {

            var staffType = Mapper.Map<StaffType>(staffTypeCreateDTO);

            Context.Add(staffType);
            await Context.SaveChangesAsync();


            var staffTypeDTO = Mapper.Map<StaffTypeDTO>(staffType);

            return CreatedAtRoute("GetStaffTypeById", new { id = staffTypeDTO.Id }, staffTypeDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] StaffTypeCreateDTO staffTypeCreateDTO)
        {

            var staffType = Mapper.Map<StaffType>(staffTypeCreateDTO);

            staffType.Id = id;

            Context.Entry(staffType).State = EntityState.Modified;

            await Context.SaveChangesAsync();


            return NoContent();
        }







    }
}
