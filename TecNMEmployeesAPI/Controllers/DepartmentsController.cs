

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public DepartmentsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }




        [HttpGet("all")]
        public async Task<ActionResult<List<DepartmentDTO>>> GetAll()
        {

            var departments = await Context.Departments.AsQueryable()
                                    .OrderBy(d => d.Name)
                                    .ToListAsync();

            var departmentsDTOs = Mapper.Map<List<DepartmentDTO>>(departments);

            return departmentsDTOs;
        }



        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<DepartmentDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Departments.AsQueryable();

            //await HttpContext.InsertPaginationParamsInHeaders(queryable, paginationDto.Limit);

            var departments = await queryable.Paginar(paginationDto)
                                    .OrderBy(d => d.Name)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var departmentsDTOs = Mapper.Map<List<DepartmentDTO>>(departments);

            var result = new PaginationResultDTO<DepartmentDTO>
            {
                Results = departmentsDTOs,
                TotalResults = totalResults,
                // TODO: Verificar correcto funcionamiento
                TotalPages = (totalResults / paginationDto.Limit)
            };

            return result;
        }




        [HttpGet("search")]
        public async Task<ActionResult<PaginationResultDTO<DepartmentDTO>>> GetAllByQuery([FromQuery] DepartmentSearchDTO departmentSearchDTO)
        {

            var queryable = Context.Departments.AsQueryable();


            if (departmentSearchDTO.Query != null)
            {
                queryable = queryable.Where(d => d.Name.Contains(departmentSearchDTO.Query)).OrderBy(d => d.Name);
            }



            var departments = await queryable.Paginar(departmentSearchDTO.Pagination)
                                            .OrderBy(d => d.Name)
                                            .ToListAsync();

            var totalResults = 0;

            if (departmentSearchDTO.Query != null)
            {
                totalResults = await queryable.CountAsync(d => d.Name.Contains(departmentSearchDTO.Query));
            }
            else
            {
                totalResults = await queryable.CountAsync();
            }



            var departmentsDTOs = Mapper.Map<List<DepartmentDTO>>(departments);

            var result = new PaginationResultDTO<DepartmentDTO>
            {
                Results = departmentsDTOs,
                TotalResults = totalResults
            };

            return result;
        }




        [HttpGet("{id:int}", Name = "GetDepartmentById")]
        public async Task<ActionResult<DepartmentDTO>> GetById(int id)
        {
            var department = await Context.Departments.FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound($"No existe un departamento con el ID: {id}");
            }

            var departmentDTO = Mapper.Map<DepartmentDTO>(department);

            var employee = await Context.Employees.FirstOrDefaultAsync(e => e.WorkStationId == 1 && e.DepartmentId == id);
            var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(employee);

            departmentDTO.Head = employeeDTO;

            return departmentDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] DepartmentCreateDTO departmentCreateDTO)
        {

            var department = Mapper.Map<Department>(departmentCreateDTO);

            Context.Add(department);
            await Context.SaveChangesAsync();


            var departmentDTO = Mapper.Map<DepartmentDTO>(department);

            return CreatedAtRoute("GetDepartmentById", new { id = departmentDTO.Id }, departmentDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] DepartmentCreateDTO departmentCreateDTO)
        {

            var departmentdb = await Context.Departments.FirstOrDefaultAsync(d => d.Id == id);

            if (departmentdb == null)
            {
                return NotFound($"No existe un departamento con el ID {id}");
            }


            departmentdb = Mapper.Map(departmentCreateDTO, departmentdb);

            await Context.SaveChangesAsync();

            return NoContent();
        }






        // Obtener empleados por departamento
        [HttpGet("{id:int}/employees")]
        public async Task<ActionResult<PaginationResultDTO<EmployeeForDepartmentDTO>>> GetEmployeesByDepartment(int id, [FromQuery] PaginationDTO paginationDto)
        {


            var department = await Context.Departments.FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound($"No existe un departamento con el ID: {id}");
            }

            var queryable = Context.Employees.AsQueryable();


            var employees = await queryable
                                    .Where(e => e.DepartmentId == id && e.WorkStationId != 1)
                                    .Paginar(paginationDto)
                                    .Include(e => e.WorkStation )
                                    .OrderBy(e => e.Lastname)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync(e => e.DepartmentId == id);

            var employeesDTOs = Mapper.Map<List<EmployeeForDepartmentDTO>>(employees);

            var result = new PaginationResultDTO<EmployeeForDepartmentDTO>
            {
                Results = employeesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }




    }
}
