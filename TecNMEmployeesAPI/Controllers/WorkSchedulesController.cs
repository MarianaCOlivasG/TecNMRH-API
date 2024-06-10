
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/workschedules")]
    public class WorkSchedulesController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public WorkSchedulesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<WorkScheduleDTO>>> GetAllFilter([FromQuery] WorkSchedulesFilterDTO workSchedulesFilterDTO)
        {

            var queryable = Context.WorkSchedules.AsQueryable();

            if (workSchedulesFilterDTO.PeriodId != 0)
            {
                queryable = queryable.Where(w => w.PeriodId == workSchedulesFilterDTO.PeriodId);
            }


            if (workSchedulesFilterDTO.EmployeeId != 0)
            {
                queryable = queryable.Where(w => w.EmployeeId == workSchedulesFilterDTO.EmployeeId);
            }


            var workSchedules = await queryable.Include(w => w.Employee)
                                            .Include(w => w.Period)
                                            .OrderByDescending( w => w.Id )
                                            .Paginar(workSchedulesFilterDTO.Pagination)
                                            .ToListAsync();

            var totalResults = await queryable.CountAsync();
            


            var workSchedulesDTOs = Mapper.Map<List<WorkScheduleDTO>>(workSchedules);

            var result = new PaginationResultDTO<WorkScheduleDTO>
            {
                Results = workSchedulesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }






        [HttpGet("{id:int}", Name = "GetWorkScheduleById")]
        public async Task<ActionResult<WorkScheduleDTO>> GetById(int id)
        {
            var workSchedule = await Context.WorkSchedules
                                .Include(w => w.Employee)
                                .Include(w => w.Period)
                                .FirstOrDefaultAsync(w => w.Id == id);

            if (workSchedule == null)
            {
                return NotFound($"No existe un horario con el ID: {id}");
            }

            var workScheduleDTO = Mapper.Map<WorkScheduleDTO>(workSchedule);

            return workScheduleDTO;
        }





        [HttpGet("employee/{employeeId:int}")]
        public async Task<ActionResult<PaginationResultDTO<WorkScheduleDTO>>> GetAllByEmployeeId([FromQuery] PaginationDTO paginationDto, int employeeId)
        {

            var queryable = Context.WorkSchedules.AsQueryable();

            var workSchedules = await queryable
                                     .Where(w => w.EmployeeId == employeeId)
                                     .OrderByDescending(w => w.Id)
                                     .Include(w => w.Employee)
                                     .Include(w => w.Period)
                                     .Paginar(paginationDto)
                                     .ToListAsync();

            var totalResults = await queryable.CountAsync(w => w.EmployeeId == employeeId);

            var workSchedulesDTOs = Mapper.Map<List<WorkScheduleDTO>>(workSchedules);

            var result = new PaginationResultDTO<WorkScheduleDTO>
            {
                Results = workSchedulesDTOs,
                TotalResults = totalResults,
                // TODO: Verificar correcto funcionamiento
                TotalPages = (totalResults / paginationDto.Limit)
            };

            return result;
        }




        [HttpGet("employee/all/{employeeId:int}")]
        public async Task<ActionResult<List<WorkScheduleDTO>>> GetAllByEmployeeIdWithoutPaginator( int employeeId)
        {


            var workSchedules = await Context.WorkSchedules.Where(w => w.EmployeeId == employeeId)
                                     .Include(w => w.Employee)
                                     .Include(w => w.Period)
                                     .ToListAsync();

            var workSchedulesDTOs = Mapper.Map<List<WorkScheduleDTO>>(workSchedules);
 

            return workSchedulesDTOs;
        }





        [HttpPost]
        public async Task<ActionResult> Create([FromBody] WorkScheduleCreateDTO workScheduleCreateDTO)
        {


            var employee = await Context.Employees.FirstOrDefaultAsync(e => e.Id == workScheduleCreateDTO.EmployeeId);

            if( employee == null )
            {
                return BadRequest($"No existe un empleado con el ID {workScheduleCreateDTO.EmployeeId}");
            }


            var period = await Context.Periods.FirstOrDefaultAsync(e => e.Id == workScheduleCreateDTO.PeriodId);

            if (period == null)
            {
                return BadRequest($"No existe un periodo con el ID {workScheduleCreateDTO.PeriodId}");
            }


            // Verificar que si ya tiene horario(s)
      

            // Verificar total de horas del horario(s) anteriores
     



            // Verificar que la fecha de inicio del horario que desea crear sea superior 


            // Validar que no exeda el total de horas
            var totalHours = 0.0;

            if (workScheduleCreateDTO.MondayCheckIn != new TimeSpan(0, 0, 0) )
            {
                totalHours = totalHours + (workScheduleCreateDTO.MondayCheckOut.TotalHours - workScheduleCreateDTO.MondayCheckIn.TotalHours);
            }

            if (workScheduleCreateDTO.TuesdayCheckIn != new TimeSpan(0, 0, 0))
            {
                totalHours = totalHours + (workScheduleCreateDTO.TuesdayCheckOut.TotalHours - workScheduleCreateDTO.TuesdayCheckIn.TotalHours);
            }

            if (workScheduleCreateDTO.WednesdayCheckIn != new TimeSpan(0, 0, 0))
            {
                totalHours = totalHours + (workScheduleCreateDTO.WednesdayCheckOut.TotalHours - workScheduleCreateDTO.WednesdayCheckIn.TotalHours);
            }


            if (workScheduleCreateDTO.ThursdayCheckIn != new TimeSpan(0, 0, 0))
            {
                totalHours = totalHours + (workScheduleCreateDTO.ThursdayCheckOut.TotalHours - workScheduleCreateDTO.ThursdayCheckIn.TotalHours);
            }


            if (workScheduleCreateDTO.FridayCheckIn != new TimeSpan(0, 0, 0))
            {
                totalHours = totalHours + (workScheduleCreateDTO.FridayCheckOut.TotalHours - workScheduleCreateDTO.FridayCheckIn.TotalHours);
            }


            if (workScheduleCreateDTO.SaturdayCheckIn != new TimeSpan(0, 0, 0))
            {
                totalHours = totalHours + (workScheduleCreateDTO.SaturdayCheckOut.TotalHours - workScheduleCreateDTO.SaturdayCheckIn.TotalHours);
            }


            if (workScheduleCreateDTO.SundayCheckIn != new TimeSpan(0, 0, 0))
            {
                totalHours = totalHours + (workScheduleCreateDTO.SundayCheckOut.TotalHours - workScheduleCreateDTO.SundayCheckIn.TotalHours);
            }


            if ( totalHours > employee.TotalHours )
            {
                return BadRequest($"El horario tiene {totalHours} y excede con el total de horas autorizadas para el empleado. Total de horas autorizadas: {employee.TotalHours}");
            }

            //if ( totalHours < employee.TotalHours )
            //{
            //return BadRequest($"El horario tiene {totalHours} y no cumple con el total de horas autorizadas para el empleado. Total// de horas autorizadas: {employee.TotalHours}");
            //}

            workScheduleCreateDTO.TotalHours = (int)totalHours;
            var workSchedule = Mapper.Map<WorkSchedule>(workScheduleCreateDTO);

            Context.Add(workSchedule);
            await Context.SaveChangesAsync();

            var workScheduleCreated = await Context.WorkSchedules.AsNoTracking()
                    .Include(w => w.Period)
                    .Include(w => w.Employee)
                    .FirstAsync(w => w.Id == workSchedule.Id);

            var workScheduleDTO = Mapper.Map<WorkScheduleDTO>(workScheduleCreated);

            return CreatedAtRoute("GetWorkScheduleById", new { id = workScheduleDTO.Id }, workScheduleDTO);

        }





        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] WorkScheduleCreateDTO workScheduleCreateDTO)
        {

            var workSchedule = Mapper.Map<WorkSchedule>(workScheduleCreateDTO);

            workSchedule.Id = id;

            Context.Entry(workSchedule).State = EntityState.Modified;

            await Context.SaveChangesAsync();


            return NoContent();
        }







    }
}
