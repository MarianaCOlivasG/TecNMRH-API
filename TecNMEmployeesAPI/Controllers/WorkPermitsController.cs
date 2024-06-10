
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/workpermits")]
    public class WorkPermitsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public WorkPermitsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<WorkPermitDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.WorkPermits.AsQueryable();

            var workPermits = await queryable.Paginar(paginationDto)
                                    .Include(w => w.Employee)
                                    .Include(w => w.Permit)
                                    .Include(w => w.WorkSchedule)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var workPermitsDTOs = Mapper.Map<List<WorkPermitDTO>>(workPermits);

            var result = new PaginationResultDTO<WorkPermitDTO>
            {
                Results = workPermitsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }





        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<WorkPermitDTO>>> GetByFilter([FromQuery] WorkPermitFilterDTO workPermitFilterDTO)
        {


            var queryable = Context.WorkPermits.AsQueryable();


            if (workPermitFilterDTO.EmployeeId != 0)
            {
   
                queryable = queryable.Where(w => w.EmployeeId == workPermitFilterDTO.EmployeeId);
            }

            var workpermits = await queryable.Paginar(workPermitFilterDTO.Pagination)
                                    .Include(w => w.Employee)
                                    .Include(w => w.Permit)
                                    .Include(w => w.WorkSchedule)
                                    .OrderByDescending( w => w.Id)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var workpermitsDTOs = Mapper.Map<List<WorkPermitDTO>>(workpermits);

            var result = new PaginationResultDTO<WorkPermitDTO>
            {
                Results = workpermitsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }



        [HttpGet("employee/{employeeId:int}")]
        public async Task<ActionResult<PaginationResultDTO<WorkPermitDTO>>> GetAllByUserId([FromQuery] PaginationDTO paginationDto, int employeeId)
        {

            var queryable = Context.WorkPermits.AsQueryable();

            var workPermitions = await queryable.Paginar(paginationDto)
                                    .Include(w => w.Employee)
                                    .Include(w => w.Permit)
                                    .Include(w => w.WorkSchedule)
                                    .Where(w => w.EmployeeId == employeeId)
                                    .ToListAsync();



            var totalResults = await queryable.CountAsync();

            var workPermitsDTOs = Mapper.Map<List<WorkPermitDTO>>(workPermitions);

            var result = new PaginationResultDTO<WorkPermitDTO>
            {
                Results = workPermitsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }





        [HttpGet("{id:int}", Name = "GetWorkPermitById")]
        public async Task<ActionResult<WorkPermitDTO>> GetById(int id)
        {
            var workPermit = await Context.WorkPermits
                            .Include(w => w.Employee)
                            .Include(w => w.Permit)
                            .Include(w => w.WorkSchedule)
                            .FirstOrDefaultAsync(w => w.Id == id);

            if (workPermit == null)
            {
                return NotFound($"No existe un permiso con el ID: {id}");
            }

            var workPermitDTO = Mapper.Map<WorkPermitDTO>(workPermit);

            return workPermitDTO;
        }



        [HttpPost]
        public async Task<ActionResult> Create([FromBody] WorkPermitCreateDTO workPermitCreateDTO)
        {

            var employee = await Context.Employees.FirstOrDefaultAsync(e => e.Id == workPermitCreateDTO.EmployeeId);

            if ( employee == null )
            {
                return BadRequest($"No existe un empleado con el ID {workPermitCreateDTO.EmployeeId}");
            }


            /*
                TYPES:
                0 - Todo el dia (todo los horarios del empleado, no requiere especificar horario)
                1 - Horario (aplica a un horario entrada y salida)
                2 - Entrada (requiere un horario)
                3 - Salida (requiere un horario)
                
             */

            var permitdb = await Context.Permits.FirstOrDefaultAsync(p => p.Id == workPermitCreateDTO.PermitId);

            if (permitdb == null)
            {
                return BadRequest($"No existe un permiso con el ID {workPermitCreateDTO.PermitId}");
            }

            
            if (workPermitCreateDTO.Type != 0 && workPermitCreateDTO.WorkScheduleId == null)
            {
                return BadRequest($"El horario del empleado es requerido.");
            }


            if ( workPermitCreateDTO.WorkScheduleId != null )
            {
                var schedule = await Context.WorkSchedules.FirstOrDefaultAsync(s => s.Id == workPermitCreateDTO.WorkScheduleId && s.EmployeeId == workPermitCreateDTO.EmployeeId );

                if (schedule == null)
                {
                    return BadRequest($"No existe un horario con el ID {workPermitCreateDTO.WorkScheduleId} correspondiente al empleado");
                }
            }


            var workPermit = Mapper.Map<WorkPermit>(workPermitCreateDTO);

            Context.Add(workPermit);

            await Context.SaveChangesAsync();

            var newWorkPermit = await Context.WorkPermits.AsNoTracking()
                    .Include(w => w.Employee)
                    .Include(w => w.Permit)
                    .Include(w => w.WorkSchedule)
                    .FirstAsync(w => w.Id == workPermit.Id);

            var workPermitDTO = Mapper.Map<WorkPermitDTO>(newWorkPermit);

            return CreatedAtRoute("GetWorkPermitById", new { id = workPermitDTO.Id }, workPermitDTO);

        }




        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, WorkPermitCreateDTO workPermitCreateDTO)
        {
            var workpermitdb = await Context.WorkPermits
                    .FirstOrDefaultAsync(n => n.Id == id);

            if (workpermitdb == null)
            {
                return NotFound($"No existe un permiso con el ID {id}");
            }

            var employee = await Context.Employees.FirstOrDefaultAsync(e => e.Id == workPermitCreateDTO.EmployeeId);

            if (employee == null)
            {
                return BadRequest($"No existe un empleado con el ID {workPermitCreateDTO.EmployeeId}");
            }


            /*
                TYPES:
                0 - Todo el dia (todo los horarios del empleado, no requiere especificar horario)
                1 - Horario (aplica a un horario entrada y salida)
                2 - Entrada (requiere un horario)
                3 - Salida (requiere un horario)
                
             */

            var permitdb = await Context.Permits.FirstOrDefaultAsync(p => p.Id == workPermitCreateDTO.PermitId);

            if (permitdb == null)
            {
                return BadRequest($"No existe un permiso con el ID {workPermitCreateDTO.PermitId}");
            }


            if (workPermitCreateDTO.Type != 0 && workPermitCreateDTO.WorkScheduleId == null)
            {
                return BadRequest($"El horario del empleado es requerido.");
            }


            if (workPermitCreateDTO.WorkScheduleId != null)
            {
                var schedule = await Context.WorkSchedules.FirstOrDefaultAsync(s => s.Id == workPermitCreateDTO.WorkScheduleId && s.EmployeeId == workPermitCreateDTO.EmployeeId);

                if (schedule == null)
                {
                    return BadRequest($"No existe un horario con el ID {workPermitCreateDTO.WorkScheduleId} correspondiente al empleado");
                }
            }

            workpermitdb = Mapper.Map(workPermitCreateDTO, workpermitdb);

            await Context.SaveChangesAsync();

            return NoContent();
        }





        [HttpGet("active/{id:int}")]
        public async Task<ActionResult<NoticeDTO>> ChangeActive(int id)
        {
            var permit = await Context.WorkPermits.FirstOrDefaultAsync(n => n.Id == id);

            if (permit == null)
            {
                return NotFound($"No existe un permiso con el ID: {id}");
            }

            permit.IsActive = permit.IsActive ? false : true;
            Context.Entry(permit).State = EntityState.Modified;
            await Context.SaveChangesAsync();


            return NoContent();
        }


    }
}
