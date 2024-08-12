using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/attendanceslogs")]
    public class AttendancesLogsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public AttendancesLogsController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet("{id:int}", Name = "GetAttendanceLogById")]
        public async Task<ActionResult<AttendanceLogDTO>> GetById(int id)
        {
            var attendanceLog = await Context.AttendanceLogs
                                    .FirstOrDefaultAsync(a => a.Id == id);

            if (attendanceLog == null)
            {
                return NotFound($"No existe una asistencia con el ID: {id}");
            }

            var attendanceLogDTO = Mapper.Map<AttendanceLogDTO>(attendanceLog);

            return attendanceLogDTO;
        }


        // Insertar con fecha y hora dinamica
        [HttpPost("create")]
        public async Task<ActionResult> SimpleCreate([FromBody] AttendanceLogCreateDTO attendanceLogCreateDTO)
        {

            var attendanceLog = Mapper.Map<AttendanceLog>(attendanceLogCreateDTO);

            Context.Add(attendanceLog);
            await Context.SaveChangesAsync();

            var attendanceLogCreated = await Context.AttendanceLogs.AsNoTracking()
                    .FirstAsync(a => a.Id == attendanceLog.Id);

            var attendanceLogDTO = Mapper.Map<AttendanceLogDTO>(attendanceLogCreated);

            return CreatedAtRoute("GetAttendanceLogById", new { id = attendanceLogDTO.Id }, attendanceLogDTO);

        }







    }

}
