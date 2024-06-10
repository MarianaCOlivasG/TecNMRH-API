using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public EmployeesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }





        [HttpGet]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EmployeesRead")]
        public async Task<ActionResult<PaginationResultDTO<EmployeeWithoutDetailsDTO>>> GetAllPaginacion([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Employees.AsQueryable();

            var employees = await queryable
                                    .OrderBy(e => e.Lastname)
                                    .Paginar(paginationDto)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var employeesDTOs = Mapper.Map<List<EmployeeWithoutDetailsDTO>>(employees);



            var result = new PaginationResultDTO<EmployeeWithoutDetailsDTO>
            {
                Results = employeesDTOs,
                TotalResults = totalResults,
                // TODO: Verificar correcto funcionamiento
                TotalPages = (totalResults / paginationDto.Limit)
            };

            return result;
        }




        [HttpGet("all")]
        public async Task<ActionResult<List<EmployeeWithoutDetailsDTO>>> GetAll()
        {

            var employees = await Context.Employees.AsQueryable()
                                    .OrderBy(e => e.Lastname)
                                    .ToListAsync();

            var employeesDTOs = Mapper.Map<List<EmployeeWithoutDetailsDTO>>(employees);

            return employeesDTOs;
        }




        [HttpGet("all/stafftype/{staffTypeId:int}")]
        public async Task<ActionResult<List<EmployeeWithoutDetailsDTO>>> GetAllByStaffTypeId(int staffTypeId)
        {

            var employees = await Context.Employees.AsQueryable()
                                    .Where(e => e.StaffTypeId == staffTypeId)
                                    .OrderBy(e => e.Lastname)
                                    .ToListAsync();

            var employeesDTOs = Mapper.Map<List<EmployeeWithoutDetailsDTO>>(employees);

            return employeesDTOs;
        }




        [HttpGet("search")]
        public async Task<ActionResult<PaginationResultDTO<EmployeeWithoutDetailsDTO>>> GetAllByQuery([FromQuery] EmployeesSearchDTO employeesSerachDTO)
        {

            var queryable = Context.Employees.AsQueryable();


            if (employeesSerachDTO.Query != null)
            {
                queryable = queryable.Where(e => e.Name.Contains(employeesSerachDTO.Query) || e.Lastname.Contains(employeesSerachDTO.Query) || e.CardCode.Contains(employeesSerachDTO.Query)).OrderBy(e => e.Lastname);
            }



            var employees = await queryable.OrderBy(e => e.Lastname)
                                            .Paginar(employeesSerachDTO.Pagination)
                                            .ToListAsync();

            var totalResults = 0;

            if (employeesSerachDTO.Query != null)
            {
                totalResults = await queryable.CountAsync(e => e.Name.Contains(employeesSerachDTO.Query) || e.Lastname.Contains(employeesSerachDTO.Query) || e.CardCode.Contains(employeesSerachDTO.Query));
            } else
            {
                totalResults = await queryable.CountAsync();
            }



            var employeesDTOs = Mapper.Map<List<EmployeeWithoutDetailsDTO>>(employees);

            var result = new PaginationResultDTO<EmployeeWithoutDetailsDTO>
            {
                Results = employeesDTOs,
                TotalResults = totalResults
            };

            return result;
        }





        [HttpGet("{id:int}", Name = "GetEmployeeById")]
        public async Task<ActionResult<EmployeeDTO>> GetById(int id)
        {
            var employee = await Context.Employees
                                    .Include(e => e.ContractType)
                                    .Include(e => e.StaffType)
                                    .Include(e => e.Degree)
                                    .Include(e => e.Department)
                                    .Include(e => e.WorkStation)
                                    .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound($"No existe un empleado con el ID: {id}");
            }

            var employeeDTO = Mapper.Map<EmployeeDTO>(employee);

            return employeeDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] EmployeeCreateDTO employeeCreateDTO)
        {

            // Validar que no sea jefe de departamento si ya existe alguno
            if (employeeCreateDTO.WorkStationId == 1)
            {
                var exist = await Context.Employees.FirstOrDefaultAsync(e => e.DepartmentId == employeeCreateDTO.DepartmentId && e.WorkStationId == employeeCreateDTO.WorkStationId);

                if (exist != null)
                {
                    return BadRequest("Ya existe un Jefe de Departamento");
                }
            }


            var employee = Mapper.Map<Employee>(employeeCreateDTO);

            Context.Add(employee);
            await Context.SaveChangesAsync();

            var employeeCreated = await Context.Employees.AsNoTracking()
                                    .Include(e => e.ContractType)
                                    .Include(e => e.StaffType)
                                    .Include(e => e.Degree)
                                    .Include(e => e.Department)
                                    .Include(e => e.WorkStation)
                                    .FirstAsync(e => e.Id == employee.Id);

            var employeeDTO = Mapper.Map<EmployeeDTO>(employeeCreated);

            return CreatedAtRoute("GetEmployeeById", new { id = employeeDTO.Id }, employeeDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] EmployeeCreateDTO employeeCreateDTO)
        {


            // Validar que no sea jefe de departamento si ya existe alguno
            if (employeeCreateDTO.WorkStationId == 1)
            {
                var exist = await Context.Employees.FirstOrDefaultAsync(e => e.DepartmentId == employeeCreateDTO.DepartmentId && e.WorkStationId == employeeCreateDTO.WorkStationId);

                if (exist != null)
                {
                    return BadRequest("Ya existe un Jefe de Departamento");
                }
            }

            var employeedb = await Context.Employees
                     .Include(e => e.ContractType)
                     .Include(e => e.StaffType)
                     .Include(e => e.Degree)
                     .Include(e => e.Department)
                     .Include(e => e.WorkStation)
                     .FirstOrDefaultAsync(e => e.Id == id);

            if (employeedb == null)
            {
                return NotFound($"No existe un empleado con el ID {id}");
            }



            employeedb = Mapper.Map(employeeCreateDTO, employeedb);

            await Context.SaveChangesAsync();

            return NoContent();


        }







        [HttpPatch("{id:int}/fingerprint")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<EmployeePatchDTO> patchDocument)
        {

            if (patchDocument == null)
            {
                return BadRequest();
            }

            var employeedb = await Context.Employees.FirstOrDefaultAsync(e => e.Id == id);

            if (employeedb == null)
            {
                return NotFound();
            }

            var employeeDTO = Mapper.Map<EmployeePatchDTO>(employeedb);

            // Llenando el pachDocument con la información del libro de la base de datos
            patchDocument.ApplyTo(employeeDTO, ModelState);

            // Verificar las reglas de validación 
            var isValid = TryValidateModel(employeeDTO);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(employeeDTO, employeedb);

            await Context.SaveChangesAsync();

            return NoContent();

        }




    }
}
