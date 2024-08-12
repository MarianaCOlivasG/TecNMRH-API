using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/attendances")]
    public class AttendancesController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public AttendancesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<AttendanceDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Attendances.AsQueryable();

            var attendances = await queryable.Paginar(paginationDto)
                                    .Include(a => a.Station)
                                    .Include(a => a.Employee)
                                    .OrderByDescending(a => a.Id)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var attendancesDTOs = Mapper.Map<List<AttendanceDTO>>(attendances);

            var result = new PaginationResultDTO<AttendanceDTO>
            {
                Results = attendancesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }



        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<AttendanceDTO>>> GetAllFilter([FromQuery] AttendanceFilterDTO attendancesFilterDTO)
        {

            var queryable = Context.Attendances.AsQueryable();


            if (attendancesFilterDTO.EmployeeId != 0)
            {
                queryable = queryable.Where(a => a.EmployeeId == attendancesFilterDTO.EmployeeId);
            }

            if (attendancesFilterDTO.StartDate != new DateTime() && attendancesFilterDTO.FinalDate != new DateTime())
            {
                queryable = queryable.Where(a => attendancesFilterDTO.StartDate <= a.Date && a.Date <= attendancesFilterDTO.FinalDate);
            }//01/01/0001 12:00:00 a. m.


            var attendances = await queryable.Paginar(attendancesFilterDTO.Pagination)
                                            .Include(a => a.Employee)
                                            .Include(a => a.Station)
                                            .OrderByDescending(a => a.Id)
                                            .ToListAsync();

            var totalResults = await queryable.CountAsync();



            var attendancesDTOs = Mapper.Map<List<AttendanceDTO>>(attendances);

            var result = new PaginationResultDTO<AttendanceDTO>
            {
                Results = attendancesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }




        [HttpGet("employee/{employeeId:int}")]
        public async Task<ActionResult<PaginationResultDTO<AttendanceDTO>>> GetAllByUserId([FromQuery] PaginationDTO paginationDto, int employeeId)
        {


            var queryable = Context.Attendances.AsQueryable();

            var attendances = await queryable.Paginar(paginationDto)
                                    .Where(a => a.EmployeeId == employeeId)
                                    .Include(a => a.Station)
                                    .Include(a => a.Employee)
                                    .OrderByDescending(a => a.Id)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync(a => a.EmployeeId == employeeId);

            var attendancesDTOs = Mapper.Map<List<AttendanceDTO>>(attendances);

            var result = new PaginationResultDTO<AttendanceDTO>
            {
                Results = attendancesDTOs,
                TotalResults = totalResults,
            };

            return result;

        }





        [HttpGet("{id:int}", Name = "GetAttendanceById")]
        public async Task<ActionResult<AttendanceDTO>> GetById(int id)
        {
            var attendance = await Context.Attendances
                                    .Include(a => a.Employee)
                                    .Include(a => a.Station)
                                    .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound($"No existe una asistencia con el ID: {id}");
            }

            var attendanceDTO = Mapper.Map<AttendanceDTO>(attendance);

            return attendanceDTO;
        }




        /* Usado para el reloj checador */
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] AttendanceCreateDTO attendanceCreateDTO)
        {

            var employee = await Context.Employees
                                   .FirstOrDefaultAsync(e => e.Id == attendanceCreateDTO.EmployeeId);

            if (employee == null)
            {
                AttendanceFailDTO attendanceFail = new AttendanceFailDTO()
                {
                    Employee = null,
                    Message = $"No existe un empleado con el ID: {attendanceCreateDTO.EmployeeId}"
                };
                return NotFound(attendanceFail);
            }

            if (!employee.IsActive)
            {
                AttendanceFailDTO attendanceFail = new AttendanceFailDTO()
                {
                    Employee = Mapper.Map<EmployeeWithoutDetailsDTO>(employee),
                    Message = "Empleado Inactivo. Favor de pasar a recursos humanos."
                };
                return NotFound(attendanceFail);
            }

            var schedules = await Context.WorkSchedules
                                  .FirstOrDefaultAsync(w => w.EmployeeId == attendanceCreateDTO.EmployeeId &&
                                         w.StartDate <= DateTime.Now &&
                                        w.FinalDate >= DateTime.Now);

            if (schedules == null)
            {
                AttendanceFailDTO attendanceFail = new AttendanceFailDTO()
                {
                    Employee = Mapper.Map<EmployeeWithoutDetailsDTO>(employee),
                    Message = "No cuenta con horario.Favor de pasar con su jefe académico."
                };
                return NotFound(attendanceFail);
            }



            // Verficar mensajes personales y mensajes generales
            var today = DateTime.Today;

            var notice = await Context.Notices
                                   .Include(n => n.NoticeEmployee)
                                   .ThenInclude(ne => ne.Employee)
                                   .FirstOrDefaultAsync(n => n.StartDate <= today &&
                                                       n.FinalDate >= today &&
                                                       n.NoticeEmployee.Select(e => e.EmployeeId).Contains(attendanceCreateDTO.EmployeeId) &&
                                                       n.IsActive == true);

            var noticeDTO = new NoticeDTO { };

            if (notice != null)
            {
                noticeDTO = Mapper.Map<NoticeDTO>(notice);
            }

            var generalNotice = await Context.GeneralNotices
                                  .FirstOrDefaultAsync(n => n.StartDate <= today &&
                                                      n.FinalDate >= today &&
                                                      n.IsActive == true);

            var generalNoticeDTO = new GeneralNoticeDTO { };

            if (generalNotice != null)
            {
                generalNoticeDTO = Mapper.Map<GeneralNoticeDTO>(generalNotice);
            }


            attendanceCreateDTO.Time = DateTime.Now.TimeOfDay;
            attendanceCreateDTO.Date = DateTime.Now;

            var attendance = Mapper.Map<Attendance>(attendanceCreateDTO);



            Context.Add(attendance);
            await Context.SaveChangesAsync();

            var attendanceCreated = await Context.Attendances.AsNoTracking()
                    .Include(a => a.Employee)
                    .Include(a => a.Station)
                    .FirstAsync(a => a.Id == attendance.Id);

            var attendanceDTO = Mapper.Map<AttendanceDTO>(attendanceCreated);

            //return CreatedAtRoute("GetAttendanceById", new { id = attendanceDTO.Id }, attendanceDTO);

            return Ok(
               new
               {
                   attendance = attendanceDTO,
                   notice = noticeDTO,
                   generalNotice = generalNoticeDTO
               }
           );
        }




        // Insertar con fecha y hora dinamica
        [HttpPost("simple")]
        public async Task<ActionResult> SimpleCreate([FromBody] AttendanceCreateDateRequiredDTO attendanceCreateDTO)
        {

            var employee = await Context.Employees
                                   .FirstOrDefaultAsync(e => e.Id == attendanceCreateDTO.EmployeeId);

            if (employee == null)
            {
                AttendanceFailDTO attendanceFail = new AttendanceFailDTO()
                {
                    Employee = null,
                    Message = $"No existe un empleado con el ID: {attendanceCreateDTO.EmployeeId}"
                };
                return NotFound(attendanceFail);
            }

            if (!employee.IsActive)
            {
                AttendanceFailDTO attendanceFail = new AttendanceFailDTO()
                {
                    Employee = Mapper.Map<EmployeeWithoutDetailsDTO>(employee),
                    Message = "Empleado Inactivo. Favor de pasar a recursos humanos."
                };
                return NotFound(attendanceFail);
            }

            var schedules = await Context.WorkSchedules
                                  .FirstOrDefaultAsync(w => w.EmployeeId == attendanceCreateDTO.EmployeeId &&
                                         w.StartDate <= DateTime.Now &&
                                        w.FinalDate >= DateTime.Now);

            if (schedules == null)
            {
                AttendanceFailDTO attendanceFail = new AttendanceFailDTO()
                {
                    Employee = Mapper.Map<EmployeeWithoutDetailsDTO>(employee),
                    Message = "No cuenta con horario.Favor de pasar con su jefe académico."
                };
                return NotFound(attendanceFail);
            }

            var attendance = Mapper.Map<Attendance>(attendanceCreateDTO);

            Context.Add(attendance);
            await Context.SaveChangesAsync();

            var attendanceCreated = await Context.Attendances.AsNoTracking()
                    .Include(a => a.Employee)
                    .Include(a => a.Station)
                    .FirstAsync(a => a.Id == attendance.Id);

            var attendanceDTO = Mapper.Map<AttendanceDTO>(attendanceCreated);

            return CreatedAtRoute("GetAttendanceById", new { id = attendanceDTO.Id }, attendanceDTO);

        }





        [HttpGet("exports")]
        public async Task<ActionResult<String>> Exports([FromQuery] AttendanceFilterDTO attendancesFilterDTO) 
        {

            try
            {

                var queryable = Context.Attendances.AsQueryable();


                if (attendancesFilterDTO.EmployeeId != 0)
                {
                    queryable = queryable.Where(a => a.EmployeeId == attendancesFilterDTO.EmployeeId);
                }

                if (attendancesFilterDTO.StartDate != new DateTime() && attendancesFilterDTO.FinalDate != new DateTime())
                {
                    queryable = queryable.Where(a => attendancesFilterDTO.StartDate <= a.Date && a.Date <= attendancesFilterDTO.FinalDate);
                }//01/01/0001 12:00:00 a. m.


                var attendances = await queryable.Paginar(attendancesFilterDTO.Pagination)
                                                .Include(a => a.Employee)
                                                .Include(a => a.Station)
                                                .OrderByDescending(a => a.Id)
                                                .ToListAsync();

                var totalResults = await queryable.CountAsync();



                var attendancesDTOs = Mapper.Map<List<AttendanceDTO>>(attendances);

                if (attendancesDTOs.LongCount() == 0 )
                {
                    return BadRequest(
                        new { message = "No hay asistencias por exportar." }
                    );
                }


                var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("Checadas originales");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "No. empleado";
                worksheet.Cell(currentRow, 2).Value = "Nombres";
                worksheet.Cell(currentRow, 3).Value = "Apellidos";
                worksheet.Cell(currentRow, 4).Value = "Fecha";
                worksheet.Cell(currentRow, 5).Value = "Hora";


                foreach (var attendance in attendancesDTOs)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = attendance.Employee.CardCode;
                    worksheet.Cell(currentRow, 2).Value = attendance.Employee.Name;
                    worksheet.Cell(currentRow, 3).Value = attendance.Employee.Lastname;
                    worksheet.Cell(currentRow, 4).Value = attendance.Date;
                    worksheet.Cell(currentRow, 5).Value = attendance.Time;
                }


                workbook.SaveAs($"wwwroot/exports/attendances/{ "asistencias".Normalize() }.xlsx");

            }
            


            finally
            {
                Console.WriteLine("FINALLY");
            }


            return Ok(
                new {
                    message = "Exportado con éxito.",
                    filePath = $"/exports/attendances/{"asistencias".Normalize() }.xlsx"
                }
            );



        }




    }

}
