using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using System.IO;
using ClosedXML.Excel;
using System.ComponentModel;
using System.Diagnostics;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Linq;

namespace TecNMEmployeesAPI.Controllers
{


    [ApiController]
    [Route("api/incidences")]
    public class IncidencesController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public IncidencesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }






        [HttpGet("procesadas2")]
        public async Task<ActionResult<List<IncidenceTestDTO>>> GetAllProcesadas([FromQuery] InicidenceFiltersDTO inicidenceFiltersDTO)
        {


            var sfattType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == inicidenceFiltersDTO.StaffTypeId);

            if (sfattType == null)
            {
                return NotFound($"No existe un tipo de empleado con el ID ${inicidenceFiltersDTO.StaffTypeId}.");
            }

            // Obtener el dia de hoy
            // DIA DE HOY
            System.DateTime dateToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            // VALIDAR QUE LOS FILTROS NO SEAN MENOR AL DIA DE HOY
            if (inicidenceFiltersDTO.StartDate >= dateToday || inicidenceFiltersDTO.FinalDate >= dateToday)
            {
                return BadRequest("No se permite hacer incidencia en el día o posterior.");
            }

            // Obtener los tiempos de tolerancia
            var timesTolerancia = await Context.Times
                            .FirstOrDefaultAsync(t => t.StaffTypeId == inicidenceFiltersDTO.StaffTypeId);
            if (timesTolerancia == null)
            {
                return NotFound("No existe tiempos de tolerancia");
            }

            var incidencesDTOs = new List<IncidenceTestDTO>();

            for (var day = inicidenceFiltersDTO.StartDate; day <= inicidenceFiltersDTO.FinalDate; day = day.AddDays(1))
            {


                // Verificar si es dia inhábil
                var isNonWorkingDay = await Context.NonWorkingDays.FirstOrDefaultAsync(n => n.StartDate <= day && n.FinalDate >= day);

                if (isNonWorkingDay == null)
                {


                    var queryable = Context.WorkSchedules.AsQueryable();

                    if (inicidenceFiltersDTO.EmployeeId != 0)
                    {

                        queryable = queryable.Where(w => w.EmployeeId == inicidenceFiltersDTO.EmployeeId);
                    }

                    queryable = queryable
                                .Where(w =>
                               w.Employee.StaffTypeId == inicidenceFiltersDTO.StaffTypeId &&
                               w.StartDate <= day &&
                               w.FinalDate >= day)
                            .Include(w => w.Employee);


                    var schedules = new List<WorkSchedule>();
                    // Quitar los que no trabajan hoy
                    switch ((int)day.DayOfWeek)
                    {
                        case 1:
                            schedules = await queryable.Where(w => w.MondayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.MondayCheckIn).ToListAsync();
                            break;
                        case 2:
                            schedules = await queryable.Where(w => w.TuesdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.TuesdayCheckIn).ToListAsync();
                            break;
                        case 3:
                            schedules = await queryable.Where(w => w.WednesdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.WednesdayCheckIn).ToListAsync();
                            break;
                        case 4:
                            schedules = await queryable.Where(w => w.ThursdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.ThursdayCheckIn).ToListAsync();
                            break;
                        case 5:
                            schedules = await queryable.Where(w => w.FridayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.FridayCheckIn).ToListAsync();
                            break;
                        case 6:
                            schedules = await queryable.Where(w => w.SaturdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.SaturdayCheckIn).ToListAsync();
                            break;
                        case 0:
                            schedules = await queryable.Where(w => w.SundayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.SundayCheckIn).ToListAsync();
                            break;
                        default:
                            return BadRequest();
                    }




                    // Limite para separar checadas de entrada y checadas de salida
                    TimeSpan limitTime;
                    // Horario de entrada dependiendo el día
                    TimeSpan timeIn;
                    // Horario de salida dependiendo el día
                    TimeSpan timeOut;

                    // Final e Inicio del dia
                    TimeSpan endOfTheDay;
                    TimeSpan startOfTheDay;



                    for (int i = 0; i < schedules.LongCount(); i++)
                    {

                        var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(schedules[i].Employee);

                        // Obtener los permisos de la base de datos que esten en fecha
                        var permitEmployee = await Context.WorkPermits
                                                    .Include(w => w.Permit)
                                                    .FirstOrDefaultAsync(p =>
                                                         p.EmployeeId == employeeDTO.Id &&
                                                         p.IsActive == true &&
                                                         p.StartDate <= day &&
                                                         p.FinalDate >= day);




                        // Calcular la hora que corresponde a la mitad de su horario.
                        switch ((int)day.DayOfWeek)
                        {
                            case 1:
                                timeIn = schedules[i].MondayCheckIn;
                                timeOut = schedules[i].MondayCheckOut;
                                limitTime = (schedules[i].MondayCheckIn.Add(schedules[i].MondayCheckOut.Subtract(schedules[i].MondayCheckIn) / 2));
                                break;
                            case 2:
                                timeIn = schedules[i].TuesdayCheckIn;
                                timeOut = schedules[i].TuesdayCheckOut;
                                limitTime = (schedules[i].TuesdayCheckIn.Add(schedules[i].TuesdayCheckOut.Subtract(schedules[i].TuesdayCheckIn) / 2));
                                break;
                            case 3:
                                timeIn = schedules[i].WednesdayCheckIn;
                                timeOut = schedules[i].WednesdayCheckOut;
                                limitTime = (schedules[i].WednesdayCheckIn.Add(schedules[i].WednesdayCheckOut.Subtract(schedules[i].WednesdayCheckIn) / 2));
                                break;
                            case 4:
                                timeIn = schedules[i].ThursdayCheckIn;
                                timeOut = schedules[i].ThursdayCheckOut;
                                limitTime = (schedules[i].ThursdayCheckIn.Add(schedules[i].ThursdayCheckOut.Subtract(schedules[i].ThursdayCheckIn) / 2));
                                break;
                            case 5:
                                timeIn = schedules[i].FridayCheckIn;
                                timeOut = schedules[i].FridayCheckOut;
                                limitTime = (schedules[i].FridayCheckIn.Add(schedules[i].FridayCheckOut.Subtract(schedules[i].FridayCheckIn) / 2));
                                break;
                            case 6:
                                timeIn = schedules[i].SaturdayCheckIn;
                                timeOut = schedules[i].SaturdayCheckOut;
                                limitTime = (schedules[i].SaturdayCheckIn.Add(schedules[i].SaturdayCheckOut.Subtract(schedules[i].SaturdayCheckIn) / 2));
                                break;
                            case 0:
                                timeIn = schedules[i].SundayCheckIn;
                                timeOut = schedules[i].SundayCheckOut;
                                limitTime = (schedules[i].SundayCheckIn.Add(schedules[i].SundayCheckOut.Subtract(schedules[i].SundayCheckIn) / 2));
                                break;
                            default:
                                return BadRequest();
                        }

                        Console.WriteLine(limitTime);

                        /// // ////////////////////////////////////
                        /// TEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEES
                        /// // ////////////////////////////////////

                        // Verificar si el empleado tiene otro horario

                        var queryableHasSchedules = Context.WorkSchedules.AsQueryable();
                        queryableHasSchedules = Context.WorkSchedules
                                .Where(w => w.EmployeeId == employeeDTO.Id &&
                                    w.StartDate <= day &&
                                    w.FinalDate >= day);

                        var hasAnotherSchedules = new List<WorkSchedule>();

                        switch ((int)day.DayOfWeek)
                        {
                            case 1:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.MondayCheckIn > schedules[i].MondayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.MondayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.MondayCheckIn)
                                        .FirstOrDefaultAsync(w => w.MondayCheckIn < hasAnotherSchedules[0].MondayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.MondayCheckIn.Add(timeOut.Subtract(lastSchedule.MondayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].MondayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].MondayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.MondayCheckIn)
                                        .FirstOrDefaultAsync(w => w.MondayCheckIn < schedules[i].MondayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.MondayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.MondayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;


                            case 2:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.TuesdayCheckIn > schedules[i].TuesdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.TuesdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.TuesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.TuesdayCheckIn < hasAnotherSchedules[0].TuesdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.TuesdayCheckIn.Add(timeOut.Subtract(lastSchedule.TuesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].TuesdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].TuesdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.TuesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.TuesdayCheckIn < schedules[i].TuesdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.TuesdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.TuesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            case 3:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.WednesdayCheckIn > schedules[i].WednesdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.WednesdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.WednesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.WednesdayCheckIn < hasAnotherSchedules[0].WednesdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.WednesdayCheckIn.Add(timeOut.Subtract(lastSchedule.WednesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].WednesdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].WednesdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.WednesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.WednesdayCheckIn < schedules[i].WednesdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.WednesdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.WednesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;


                            case 4:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.ThursdayCheckIn > schedules[i].ThursdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.ThursdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.ThursdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.ThursdayCheckIn < hasAnotherSchedules[0].ThursdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.ThursdayCheckIn.Add(timeOut.Subtract(lastSchedule.ThursdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].ThursdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].ThursdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.ThursdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.ThursdayCheckIn < schedules[i].ThursdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.ThursdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.ThursdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            case 5:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.FridayCheckIn > schedules[i].FridayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.FridayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.FridayCheckIn)
                                        .FirstOrDefaultAsync(w => w.FridayCheckIn < hasAnotherSchedules[0].FridayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.FridayCheckIn.Add(timeOut.Subtract(lastSchedule.FridayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].FridayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].FridayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.FridayCheckIn)
                                        .FirstOrDefaultAsync(w => w.FridayCheckIn < schedules[i].FridayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.FridayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.FridayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            case 6:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.SaturdayCheckIn > schedules[i].SaturdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.SaturdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SaturdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SaturdayCheckIn < hasAnotherSchedules[0].SaturdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.SaturdayCheckIn.Add(timeOut.Subtract(lastSchedule.SaturdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].SaturdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].SaturdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SaturdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SaturdayCheckIn < schedules[i].SaturdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.SaturdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.SaturdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;


                            case 0:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.SundayCheckIn > schedules[i].SundayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.SundayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SundayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SundayCheckIn < hasAnotherSchedules[0].SundayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.SundayCheckIn.Add(timeOut.Subtract(lastSchedule.SundayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].SundayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].SundayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SundayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SundayCheckIn < schedules[i].SundayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.SundayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.SundayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            default:
                                return BadRequest();

                        }


                        // ======= CALCULAR CHECADA GANADORA DE ENTRADA ====== //


                        Attendance attendanceIn = new Attendance();

                        var attendanceInBeforeIn = await Context.Attendances
                                           .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                           && a.Date.Date == day.Date // Que sea del dia 
                                           && a.Time <= timeIn // que sea menor o igual a la hora que tiene de entrada
                                           && a.Time <= limitTime // que sea menor o igual al limite
                                           && a.Time >= startOfTheDay
                                           && a.Time <= endOfTheDay)
                                           .OrderByDescending(a => a.Time)
                                           .Include(a => a.Station)
                                           .FirstOrDefaultAsync();


                        var attendanceInAffterIn = await Context.Attendances
                                          .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                          && a.Date.Date == day.Date
                                          && a.Time > timeIn // Que sea mayor a la hora que tiene de entrada
                                          && a.Time <= limitTime // que sea menor o igual al limite
                                           && a.Time >= startOfTheDay
                                           && a.Time <= endOfTheDay)
                                          .OrderBy(a => a.Time)
                                          .Include(a => a.Station)
                                          .FirstOrDefaultAsync();


                        // Verficar fecha más cercana a la de la entrada
                        if (attendanceInBeforeIn == null && attendanceInAffterIn == null)
                        {
                            //Console.WriteLine("No hay nada que procesar");
                            //Console.WriteLine(employeeDTO.Name);

                        }
                        else if (attendanceInBeforeIn != null && attendanceInAffterIn != null)
                        {
                            TimeSpan difInMin = timeIn.Subtract(attendanceInBeforeIn.Time);
                            TimeSpan difInMax = attendanceInAffterIn.Time.Subtract(timeIn);

                            int result = TimeSpan.Compare(difInMin, difInMax);

                            if (result < 0)
                                attendanceIn = attendanceInBeforeIn;
                            else
                                attendanceIn = attendanceInAffterIn;

                        }
                        else if (attendanceInBeforeIn == null && attendanceInAffterIn != null)
                        {
                            attendanceIn = attendanceInAffterIn;
                        }
                        else if (attendanceInBeforeIn != null && attendanceInAffterIn == null)
                        {
                            attendanceIn = attendanceInBeforeIn;
                        }


                        // ======= CALCULAR CHECADA GANADORA DE SALIDA ====== //


                        Attendance attendanceOut = new Attendance();

                        var attendanceInBeforeOut = await Context.Attendances
                                           .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                           && a.Date.Date == day.Date // Que sea de hoy 
                                           && a.Time <= timeOut // que sea menor o igual a la hora que tiene de salida
                                           && a.Time > limitTime // que sea mayor al limite
                                           && a.Time >= startOfTheDay
                                           && a.Time <= endOfTheDay)
                                           .OrderByDescending(a => a.Time)
                                           .Include(a => a.Station)
                                           .FirstOrDefaultAsync();


                        var attendanceInAffterOut = await Context.Attendances
                                          .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                          && a.Date.Date == day.Date
                                          && a.Time > timeOut // Que sea mayor a la hora que tiene de entrada
                                          && a.Time >= startOfTheDay
                                          && a.Time <= endOfTheDay)
                                          .OrderBy(a => a.Time)
                                          .Include(a => a.Station)
                                          .FirstOrDefaultAsync();


                        // Verficar fecha más cercana a la de la salida
                        if (attendanceInBeforeOut == null && attendanceInAffterOut == null)
                        {
                            //Console.WriteLine("No hay nada que procesar");
                            //Console.WriteLine(employeeDTO.Name);

                        }
                        else if (attendanceInBeforeOut != null && attendanceInAffterOut != null)
                        {
                            TimeSpan difInMin = timeOut.Subtract(attendanceInBeforeOut.Time);
                            TimeSpan difInMax = attendanceInAffterOut.Time.Subtract(timeOut);

                            int result = TimeSpan.Compare(difInMin, difInMax);

                            if (result < 0)
                                attendanceOut = attendanceInBeforeOut;
                            else
                                attendanceOut = attendanceInAffterOut;

                        }
                        else if (attendanceInBeforeOut == null && attendanceInAffterOut != null)
                        {
                            attendanceOut = attendanceInAffterOut;
                        }
                        else if (attendanceInBeforeOut != null && attendanceInAffterOut == null)
                        {
                            attendanceOut = attendanceInBeforeOut;
                        }

                        var attendanceInDTO = Mapper.Map<AttendanceDTO>(attendanceIn);
                        var attendanceOutDTO = Mapper.Map<AttendanceDTO>(attendanceOut);




                        // Crear la incidencia
                        IncidenceTestDTO incidenceDTO = new()
                        {
                            Employee = employeeDTO,
                            Date = day,
                            Attendances = new List<AttendanceDTO> { attendanceInDTO, attendanceOutDTO },
                            Checks = new List<TimeSpan> { timeIn, timeOut },
                            Descriptions = new List<string>(),
                            Types = new List<int>()
                        };




                        // ============ INCIDENCIAAA ENTRADA ============ 

                        // attendanceIn.Time tengo la hora de la checada ganadora

                        if (attendanceIn.Time == new TimeSpan(0, 0, 0, 0, 0))
                        {
                            // Verificar si hay registro superior
                            var attendanceToday = await Context.Attendances
                                                            .FirstOrDefaultAsync(a =>
                                                             a.EmployeeId == schedules[i].EmployeeId &&
                                                            a.Date.Date == day.Date &&
                                                            a.Time > timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax) &&
                                                             a.Time >= startOfTheDay &&
                                                             a.Time <= endOfTheDay);

                            if (attendanceToday == null)
                            {
                                //Console.WriteLine("FALTA");
                                //Console.WriteLine("NO HAY CHECADA");

                                incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                incidenceDTO.Types.Add(7);

                            }
                            else
                            {
                                //Console.WriteLine("OMISIÓN DE ENTRADA");
                                //Console.WriteLine("CHECO SALIDA A LAS");
                                //Console.WriteLine(attendanceToday.Time);

                                incidenceDTO.Descriptions.Add("Omisión de entrada. No hay asistencia correspondiente a su entrada.");
                                incidenceDTO.Types.Add(6);

                            }

                        }
                        else if (attendanceIn.Time >= timeIn.Subtract(timesTolerancia.InputMin) && attendanceIn.Time <= timeIn.Add(timesTolerancia.InputMax))
                        {
                            //Console.WriteLine("SIN INCIDENCIA, ENTRADA CORRECTA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("ENTRE");
                            //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));

                            incidenceDTO.Descriptions.Add($"Sin incidencia, entrada correcta. Su asistencia fue registrada dentro del limite de tolerancia. {timeIn.Subtract(timesTolerancia.InputMin)} y {timeIn.Add(timesTolerancia.InputMax)}");
                            incidenceDTO.Types.Add(1);

                        }
                        else if (attendanceIn.Time < timeIn.Subtract(timesTolerancia.InputMin))
                        {
                            //Console.WriteLine("ENTRADA PREVIA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("ANTES");
                            //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));

                            incidenceDTO.Descriptions.Add($"Entrada previa. Su asistencia fue registrada antes de {timeIn.Subtract(timesTolerancia.InputMin)}");
                            incidenceDTO.Types.Add(2);

                        }
                        else if (attendanceIn.Time > timeIn.Add(timesTolerancia.InputMax) && attendanceIn.Time <= timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax))
                        {
                            //Console.WriteLine("RETARDO A");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));
                            //Console.WriteLine("ANTES O IGUAL A");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));

                            incidenceDTO.Descriptions.Add($"Retardo A. Su asistencia fue registrada entre {timeIn.Add(timesTolerancia.InputMax).Add(new TimeSpan(0, 0, 1))} y {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax)}");
                            incidenceDTO.Types.Add(3);

                        }
                        else if (attendanceIn.Time > timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax) && attendanceIn.Time <= timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax))
                        {
                            //Console.WriteLine("RETARDO B");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));
                            //Console.WriteLine("ANTES O IGUAL A");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));

                            incidenceDTO.Descriptions.Add($"Retardo B. Su asistencia fue registrada entre {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(new TimeSpan(0, 0, 1))} y {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax)}");
                            incidenceDTO.Types.Add(4);

                        }
                        else if (attendanceIn.Time > timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax) && attendanceIn.Time <= limitTime)
                        {
                            //Console.WriteLine("ENTRADA TARDIA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));
                            //Console.WriteLine("ANTES O IGUAL A");
                            //Console.WriteLine(limitTime);

                            incidenceDTO.Descriptions.Add($"Entrada tardia. Su asistencia fue registrada después {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(new TimeSpan(0, 0, 1))}.");
                            incidenceDTO.Types.Add(5);

                        }

                        // RETARDO A -
                        // RETARDO B -
                        // SIN INCIDENCIA - 
                        // ENTRADA PREVIA - 
                        // ENTRADA TARDIA -
                        // FALTA -
                        // OMISIÓN DE ENTRADA -




                        //Console.WriteLine("====================");

                        //Console.WriteLine("Hora de entrada");
                        //Console.WriteLine(timeIn);

                        //Console.WriteLine("Hora de salida");
                        //Console.WriteLine(timeOut);

                        //Console.WriteLine("Hora de checada entrada");
                        //Console.WriteLine(attendanceIn.Time);

                        //Console.WriteLine("Hora de checada salida");
                        //Console.WriteLine(attendanceOut.Time);

                        //Console.WriteLine("Hora tolerancia entrada antes");
                        //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));
                        //Console.WriteLine("Hora tolerancia entrada después");
                        //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));

                        //Console.WriteLine("Hora tolerancia salida antes");
                        //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));
                        //Console.WriteLine("Hora tolerancia salida después");
                        //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));



                        // ============ INCIDENCIAAA SALIDA ============ 



                        if (attendanceOut.Time == new TimeSpan(0, 0, 0, 0, 0))
                        {
                            // Verificar si hay registro inferior (de entrada)
                            var attendanceToday = await Context.Attendances
                                                            .FirstOrDefaultAsync(a =>
                                                             a.EmployeeId == schedules[i].EmployeeId &&
                                                            a.Date.Date == day.Date &&
                                                            a.Time < limitTime
                                                             && a.Time >= startOfTheDay
                                                            && a.Time <= endOfTheDay);

                            if (attendanceToday == null)
                            {
                                //Console.WriteLine("FALTA");
                                //Console.WriteLine("NO HAY CHECADA");

                                incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                incidenceDTO.Types.Add(7);

                            }
                            else
                            {
                                //Console.WriteLine("OMISIÓN DE SALIDA");
                                //Console.WriteLine("CHECO SALIDA A LAS");
                                //Console.WriteLine(attendanceToday.Time);

                                incidenceDTO.Descriptions.Add("Omisión de salida. No hay asistencia correspondiente a su salida.");
                                incidenceDTO.Types.Add(10);

                            }

                        }
                        else if (attendanceOut.Time >= timeOut.Subtract(timesTolerancia.OutputMin) && attendanceOut.Time <= timeOut.Add(timesTolerancia.OutputMax))
                        {
                            //Console.WriteLine("SIN INCIDENCIA, SALIDA CORRECTA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceOut.Time);
                            //Console.WriteLine("ENTRE");
                            //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));
                            //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));

                            incidenceDTO.Descriptions.Add($"Sin incidencia, salida correcta. Su salida fue registrada dentro del limite de tolerancia. {timeOut.Subtract(timesTolerancia.OutputMin)} y {timeOut.Add(timesTolerancia.OutputMax)}");
                            incidenceDTO.Types.Add(8);

                        }
                        else if (attendanceOut.Time < timeOut.Subtract(timesTolerancia.OutputMin))
                        {
                            //Console.WriteLine("SALIDA PREVIA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceOut.Time);
                            //Console.WriteLine("ANTES");
                            //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));

                            incidenceDTO.Descriptions.Add($"Salida previa. Su salida fue registrada antes de {timeOut.Subtract(timesTolerancia.OutputMin)}");
                            incidenceDTO.Types.Add(9);

                        }
                        else if (attendanceOut.Time > timeOut.Add(timesTolerancia.OutputMax))
                        {
                            //Console.WriteLine("SALIDA TARDÍA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceOut.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));

                            incidenceDTO.Descriptions.Add($"Salida tardía. Su salida fue registrada despues de {timeOut.Add(timesTolerancia.OutputMax).Add(new TimeSpan(0, 0, 1))}");
                            incidenceDTO.Types.Add(11);

                        }



                        //Console.WriteLine("====================");

                        //Console.WriteLine("Hora de entrada");
                        //Console.WriteLine(timeIn);

                        //Console.WriteLine("Hora de salida");
                        //Console.WriteLine(timeOut);

                        //Console.WriteLine("Hora de checada salida");
                        //Console.WriteLine(attendanceOut.Time);

                        //Console.WriteLine("Hora tolerancia entrada antes");
                        //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));
                        //Console.WriteLine("Hora tolerancia entrada después");
                        //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));

                        //Console.WriteLine("Hora tolerancia salida antes");
                        //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));
                        //Console.WriteLine("Hora tolerancia salida después");
                        //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));


                        if (permitEmployee != null && permitEmployee.Type == 0)
                        {
                            //Console.WriteLine("=================================");
                            //Console.WriteLine("TIENE PERMISO TODO EL DIA");
                            //Console.WriteLine("=================================");
                            //Console.WriteLine(day);

                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            incidenceDTO.Descriptions[0] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                            incidenceDTO.Descriptions[1] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                        }

                        if (permitEmployee != null && permitEmployee.WorkScheduleId == schedules[i].Id)
                        {
                            //Console.WriteLine("=================================");
                            //Console.WriteLine("TIENE PERMISO EN UN HORARIO ESPECIFICO");
                            //Console.WriteLine("=================================");
                            //Console.WriteLine(day);

                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            incidenceDTO.Descriptions[0] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                            incidenceDTO.Descriptions[1] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                        }

                        var allAttendances = await Context.Attendances
                                                .Where(a => a.EmployeeId == schedules[i].EmployeeId &&
                                                            a.Date.Date == day.Date)
                                                .ToListAsync();
                        var attendanceDTO = Mapper.Map<List<AttendanceDTO>>(allAttendances);
                        incidenceDTO.AttendancesAll = attendanceDTO;

                        incidencesDTOs.Add(incidenceDTO);


                    }
                }
                else
                {
                    Console.WriteLine("DIA INHABIL");
                }
            }





            return incidencesDTOs;

        }


        [HttpGet("procesadas3")]
        public async Task<ActionResult<List<IncidenceTestDTO>>> GetAllProcesadas2([FromQuery] InicidenceFiltersDTO inicidenceFiltersDTO)
        {


            var sfattType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == inicidenceFiltersDTO.StaffTypeId);

            if (sfattType == null)
            {
                return NotFound($"No existe un tipo de empleado con el ID ${inicidenceFiltersDTO.StaffTypeId}.");
            }

            // Obtener el dia de hoy
            // DIA DE HOY
            System.DateTime dateToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            // VALIDAR QUE LOS FILTROS NO SEAN MENOR AL DIA DE HOY
            if (inicidenceFiltersDTO.StartDate >= dateToday || inicidenceFiltersDTO.FinalDate >= dateToday)
            {
                return BadRequest("No se permite hacer incidencia en el día o posterior.");
            }

            // Obtener los tiempos de tolerancia
            var timesTolerancia = await Context.Times
                            .FirstOrDefaultAsync(t => t.StaffTypeId == inicidenceFiltersDTO.StaffTypeId);
            if (timesTolerancia == null)
            {
                return NotFound("No existe tiempos de tolerancia");
            }
            //Creamos un filtro base para evitar volver a crearla en cada iteracion
            var baseQueryable = Context.WorkSchedules.AsQueryable();

            if (inicidenceFiltersDTO.EmployeeId != 0)
            {
                baseQueryable = baseQueryable.Where(w => w.EmployeeId == inicidenceFiltersDTO.EmployeeId);
            }

            baseQueryable = baseQueryable.Where(w => w.Employee.StaffTypeId == inicidenceFiltersDTO.StaffTypeId)
                             .Include(w => w.Employee);

            var incidencesDTOs = new List<IncidenceTestDTO>();
            //Llamamos solo una vez a la base todos los dias inhabiles
            var nonWorkingDaysList = await Context.NonWorkingDays
            .Where(n => n.StartDate <= inicidenceFiltersDTO.FinalDate && n.FinalDate >= inicidenceFiltersDTO.StartDate)
            .ToListAsync();

            // Crear un diccionario para acceder rápidamente a los días inhábiles
            var nonWorkingDaysDict = new Dictionary<DateTime, bool>();
            foreach (var nonWorkingDay in nonWorkingDaysList)
            {
                for (var date = nonWorkingDay.StartDate; date <= nonWorkingDay.FinalDate; date = date.AddDays(1))
                {
                    nonWorkingDaysDict[date] = true;
                }
            }


            // Cargar todos los registros de horarios relevantes en una sola llamada
            var allSchedules = await baseQueryable.Where(w =>
            w.StartDate <= inicidenceFiltersDTO.FinalDate &&
            w.FinalDate >= inicidenceFiltersDTO.StartDate)
            .ToListAsync();
            // Crear un diccionario para indexar los horarios por día de la semana
            var schedulesByDayOfWeek = new Dictionary<DayOfWeek, List<WorkSchedule>>
    {
        { DayOfWeek.Monday, new List<WorkSchedule>() },
        { DayOfWeek.Tuesday, new List<WorkSchedule>() },
        { DayOfWeek.Wednesday, new List<WorkSchedule>() },
        { DayOfWeek.Thursday, new List<WorkSchedule>() },
        { DayOfWeek.Friday, new List<WorkSchedule>() },
        { DayOfWeek.Saturday, new List<WorkSchedule>() },
        { DayOfWeek.Sunday, new List<WorkSchedule>() }
    };
            // Indexar los horarios por empleado y día de la semana
            var schedulesByEmployeeAndDay = allSchedules
                .GroupBy(w => new { w.EmployeeId, Day = (int)w.StartDate.DayOfWeek })
                .ToDictionary(g => g.Key, g => g.ToList());
            // Indexar los horarios por día de la semana, excluyendo los que tienen CheckIn en TimeSpan.Zero
            foreach (var schedule in allSchedules)
            {
                if (schedule.MondayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Monday].Add(schedule);
                if (schedule.TuesdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Tuesday].Add(schedule);
                if (schedule.WednesdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Wednesday].Add(schedule);
                if (schedule.ThursdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Thursday].Add(schedule);
                if (schedule.FridayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Friday].Add(schedule);
                if (schedule.SaturdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Saturday].Add(schedule);
                if (schedule.SundayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Sunday].Add(schedule);
            }
            // Cargar todos los permisos de trabajo relevantes en una sola llamada
            var allPermits = await Context.WorkPermits
                .Include(w => w.Permit)
                .Where(p => p.IsActive == true && p.StartDate <= inicidenceFiltersDTO.FinalDate && p.FinalDate >= inicidenceFiltersDTO.StartDate)
                .ToListAsync();

            // Crear un diccionario para acceder rápidamente a los permisos por empleado y fecha
            var permitsByEmployeeId = allPermits.GroupBy(p => p.EmployeeId)
                                                .ToDictionary(g => g.Key, g => g.ToList());

            // Delegados para acceder a los horarios de check-in y check-out
            Func<WorkSchedule, TimeSpan>[] checkIns = new Func<WorkSchedule, TimeSpan>[]
            {
            s => s.SundayCheckIn,   // DayOfWeek.Sunday = 0
            s => s.MondayCheckIn,   // DayOfWeek.Monday = 1
            s => s.TuesdayCheckIn,  // DayOfWeek.Tuesday = 2
            s => s.WednesdayCheckIn,// DayOfWeek.Wednesday = 3
            s => s.ThursdayCheckIn, // DayOfWeek.Thursday = 4
            s => s.FridayCheckIn,   // DayOfWeek.Friday = 5
            s => s.SaturdayCheckIn  // DayOfWeek.Saturday = 6
            };

            Func<WorkSchedule, TimeSpan>[] checkOuts = new Func<WorkSchedule, TimeSpan>[]
            {
            s => s.SundayCheckOut,
            s => s.MondayCheckOut,
            s => s.TuesdayCheckOut,
            s => s.WednesdayCheckOut,
            s => s.ThursdayCheckOut,
            s => s.FridayCheckOut,
            s => s.SaturdayCheckOut
            };
            var allAttendances = await Context.Attendances
            .Where(a => a.Date >= inicidenceFiltersDTO.StartDate && a.Date <= inicidenceFiltersDTO.FinalDate)
            .Include(a => a.Station)
            .ToListAsync();


            for (var day = inicidenceFiltersDTO.StartDate; day <= inicidenceFiltersDTO.FinalDate; day = day.AddDays(1))
            {


                // Verificar si es día inhábil usando el diccionario
                var isNonWorkingDay = nonWorkingDaysDict.ContainsKey(day);
                if (!isNonWorkingDay)
                {
                    // Obtener el día de la semana actual
                    var currentDayOfWeek = day.DayOfWeek;

                    // Filtrar y ordenar los horarios del día actual en memoria
                    var schedules = schedulesByDayOfWeek[currentDayOfWeek]
                        .Where(w => w.StartDate <= day && w.FinalDate >= day)
                        .OrderBy(w =>
                            currentDayOfWeek switch
                            {
                                DayOfWeek.Monday => w.MondayCheckIn,
                                DayOfWeek.Tuesday => w.TuesdayCheckIn,
                                DayOfWeek.Wednesday => w.WednesdayCheckIn,
                                DayOfWeek.Thursday => w.ThursdayCheckIn,
                                DayOfWeek.Friday => w.FridayCheckIn,
                                DayOfWeek.Saturday => w.SaturdayCheckIn,
                                DayOfWeek.Sunday => w.SundayCheckIn,
                                _ => TimeSpan.MaxValue
                            })
                        .ToList();


                    // Limite para separar checadas de entrada y checadas de salida
                    TimeSpan limitTime;
                    // Horario de entrada dependiendo el día
                    TimeSpan timeIn;
                    // Horario de salida dependiendo el día
                    TimeSpan timeOut;

                    // Final e Inicio del dia
                    TimeSpan endOfTheDay;
                    TimeSpan startOfTheDay;



                    for (int i = 0; i < schedules.LongCount(); i++)
                    {

                        var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(schedules[i].Employee);

                        // Obtener los permisos de la base de datos que esten en fecha
                        var employeePermits = permitsByEmployeeId.TryGetValue(employeeDTO.Id, out var permits) ? permits : new List<WorkPermit>();
                        var permitEmployee = employeePermits.FirstOrDefault(p =>
                            p.StartDate <= day && p.FinalDate >= day);

                        // Calcular la hora que corresponde a la mitad de su horario.
                        var dayOfWeek = (int)day.DayOfWeek;
                        var schedule = schedules[i];
                        timeIn = checkIns[dayOfWeek](schedule);
                        timeOut = checkOuts[dayOfWeek](schedule);
                        limitTime = timeIn.Add(timeOut.Subtract(timeIn) / 2);

                        // Verificar si el empleado tiene otro horario

                        var hasAnotherSchedules = schedulesByEmployeeAndDay
                                        .Where(s => s.Key.EmployeeId == employeeDTO.Id && s.Key.Day == dayOfWeek)
                                        .SelectMany(s => s.Value)
                                        .Where(w => w.StartDate <= day && w.FinalDate >= day && w.Id != schedules[i].Id)
                                        .ToList();

                        var nextSchedule = hasAnotherSchedules
                            .Where(w => checkIns[dayOfWeek](w) > checkIns[dayOfWeek](schedule))
                            .OrderBy(w => checkIns[dayOfWeek](w))
                            .FirstOrDefault();

                        var previousSchedule = hasAnotherSchedules
                            .Where(w => checkIns[dayOfWeek](w) < checkIns[dayOfWeek](schedule))
                            .OrderByDescending(w => checkIns[dayOfWeek](w))
                            .FirstOrDefault();

                        if (nextSchedule != null)
                        {
                            var endOfTheLastDay = previousSchedule != null
                                ? checkIns[dayOfWeek](previousSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](previousSchedule)) / 2)
                                : new TimeSpan(0, 0, 0);

                            startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                            endOfTheDay = checkIns[dayOfWeek](nextSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](nextSchedule)) / 2);
                        }
                        else
                        {
                            var endOfTheLastDay = previousSchedule != null
                                ? checkIns[dayOfWeek](previousSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](previousSchedule)) / 2)
                                : new TimeSpan(0, 0, 0);

                            startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                            endOfTheDay = new TimeSpan(23, 0, 0);
                        }


                        // ======= CALCULAR CHECADA GANADORA DE ENTRADA ====== //
                        var attendanceIn = allAttendances
                            .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                        && a.Date.Date == day.Date
                                        && a.Time >= startOfTheDay
                                        && a.Time <= endOfTheDay
                                        && a.Time <= limitTime)
                            .OrderBy(a => Math.Abs((a.Time - timeIn).Ticks))
                            .FirstOrDefault();

                        // ======= CALCULAR CHECADA GANADORA DE SALIDA ====== //
                        var attendanceOut = allAttendances
                            .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                        && a.Date.Date == day.Date
                                        && a.Time >= startOfTheDay
                                        && a.Time <= endOfTheDay
                                        && a.Time > limitTime)
                            .OrderBy(a => Math.Abs((a.Time - timeOut).Ticks))
                            .FirstOrDefault();
                        if (attendanceIn == null)
                        {
                            attendanceIn = new Attendance();
                        }
                        if (attendanceOut == null)
                        {
                            attendanceOut = new Attendance();

                        }
                        var attendanceInDTO = Mapper.Map<AttendanceDTO>(attendanceIn);
                        var attendanceOutDTO = Mapper.Map<AttendanceDTO>(attendanceOut);

                        // Crear la incidencia
                        var incidenceDTO = new IncidenceTestDTO
                        {
                            Employee = employeeDTO,
                            Date = day,
                            Attendances = new List<AttendanceDTO> { attendanceInDTO, attendanceOutDTO },
                            Checks = new List<TimeSpan> { timeIn, timeOut },
                            Descriptions = new List<string>(),
                            Types = new List<int>()
                        };


                        // ============ INCIDENCIAAA ENTRADA ============ 

                        // attendanceIn.Time tengo la hora de la checada ganadora

                        TimeSpan timeInSubMin = timeIn.Subtract(timesTolerancia.InputMin);
                        TimeSpan timeInAddMax = timeIn.Add(timesTolerancia.InputMax);
                        TimeSpan timeInAddMaxTwice = timeInAddMax.Add(timesTolerancia.InputMax);
                        TimeSpan timeInAddMaxThrice = timeInAddMaxTwice.Add(timesTolerancia.InputMax);

                        if (attendanceIn.Time == TimeSpan.Zero)
                        {
                            var attendanceToday = allAttendances
                                .FirstOrDefault(a =>
                                    a.EmployeeId == schedules[i].EmployeeId &&
                                    a.Date.Date == day.Date &&
                                    a.Time > timeInAddMaxThrice &&
                                    a.Time >= startOfTheDay &&
                                    a.Time <= endOfTheDay);

                            if (attendanceToday == null)
                            {
                                incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                incidenceDTO.Types.Add(7);
                            }
                            else
                            {
                                incidenceDTO.Descriptions.Add("Omisión de entrada. No hay asistencia correspondiente a su entrada.");
                                incidenceDTO.Types.Add(6);
                            }
                        }
                        else if (attendanceIn.Time >= timeInSubMin && attendanceIn.Time <= timeInAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Sin incidencia, entrada correcta. Su asistencia fue registrada dentro del límite de tolerancia. {timeInSubMin} y {timeInAddMax}");
                            incidenceDTO.Types.Add(1);
                        }
                        else if (attendanceIn.Time < timeInSubMin)
                        {
                            incidenceDTO.Descriptions.Add($"Entrada previa. Su asistencia fue registrada antes de {timeInSubMin}");
                            incidenceDTO.Types.Add(2);
                        }
                        else if (attendanceIn.Time > timeInAddMax && attendanceIn.Time <= timeInAddMaxTwice)
                        {
                            incidenceDTO.Descriptions.Add($"Retardo A. Su asistencia fue registrada entre {timeInAddMax.Add(new TimeSpan(0, 0, 1))} y {timeInAddMaxTwice}");
                            incidenceDTO.Types.Add(3);
                        }
                        else if (attendanceIn.Time > timeInAddMaxTwice && attendanceIn.Time <= timeInAddMaxThrice)
                        {
                            incidenceDTO.Descriptions.Add($"Retardo B. Su asistencia fue registrada entre {timeInAddMaxTwice.Add(new TimeSpan(0, 0, 1))} y {timeInAddMaxThrice}");
                            incidenceDTO.Types.Add(4);
                        }
                        else if (attendanceIn.Time > timeInAddMaxThrice && attendanceIn.Time <= limitTime)
                        {
                            incidenceDTO.Descriptions.Add($"Entrada tardía. Su asistencia fue registrada después de {timeInAddMaxThrice.Add(new TimeSpan(0, 0, 1))}.");
                            incidenceDTO.Types.Add(5);
                        }

                        // RETARDO A -
                        // RETARDO B -
                        // SIN INCIDENCIA - 
                        // ENTRADA PREVIA - 
                        // ENTRADA TARDIA -
                        // FALTA -
                        // OMISIÓN DE ENTRADA -

                        // ============ INCIDENCIAAA SALIDA ============ 


                        TimeSpan timeOutSubMin = timeOut.Subtract(timesTolerancia.OutputMin);
                        TimeSpan timeOutAddMax = timeOut.Add(timesTolerancia.OutputMax);

                        if (attendanceOut.Time == TimeSpan.Zero)
                        {
                            var attendanceToday = allAttendances
                                .FirstOrDefault(a =>
                                    a.EmployeeId == schedules[i].EmployeeId &&
                                    a.Date.Date == day.Date &&
                                    a.Time < limitTime &&
                                    a.Time >= startOfTheDay &&
                                    a.Time <= endOfTheDay);

                            if (attendanceToday == null)
                            {
                                incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                incidenceDTO.Types.Add(7);
                            }
                            else
                            {
                                incidenceDTO.Descriptions.Add("Omisión de salida. No hay asistencia correspondiente a su salida.");
                                incidenceDTO.Types.Add(10);
                            }
                        }
                        else if (attendanceOut.Time >= timeOutSubMin && attendanceOut.Time <= timeOutAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Sin incidencia, salida correcta. Su salida fue registrada dentro del límite de tolerancia. {timeOutSubMin} y {timeOutAddMax}");
                            incidenceDTO.Types.Add(8);
                        }
                        else if (attendanceOut.Time < timeOutSubMin)
                        {
                            incidenceDTO.Descriptions.Add($"Salida previa. Su salida fue registrada antes de {timeOutSubMin}");
                            incidenceDTO.Types.Add(9);
                        }
                        else if (attendanceOut.Time > timeOutAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Salida tardía. Su salida fue registrada después de {timeOutAddMax.Add(new TimeSpan(0, 0, 1))}");
                            incidenceDTO.Types.Add(11);
                        }

                        if (permitEmployee != null)
                        {
                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            var description = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";

                            if (permitEmployee.Type == 0 || permitEmployee.WorkScheduleId == schedules[i].Id)
                            {
                                incidenceDTO.Descriptions[0] = description;
                                incidenceDTO.Descriptions[1] = description;
                            }
                        }


                        var Attendances = allAttendances
                        .Where(a => a.EmployeeId == schedules[i].EmployeeId && a.Date.Date == day.Date)
                        .ToList();
                        var attendanceDTO = Mapper.Map<List<AttendanceDTO>>(Attendances);
                        incidenceDTO.AttendancesAll = attendanceDTO;

                        incidencesDTOs.Add(incidenceDTO);


                    }
                }
                else
                {
                    Console.WriteLine("DIA INHABIL");
                }
            }





            return incidencesDTOs;

        }



        [HttpGet("procesadas")]
        public async Task<ActionResult<List<IncidenceTestDTO>>> GetAllProcesadas3([FromQuery] InicidenceFiltersDTO inicidenceFiltersDTO)
        {


            var sfattType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == inicidenceFiltersDTO.StaffTypeId);

            if (sfattType == null)
            {
                return NotFound($"No existe un tipo de empleado con el ID ${inicidenceFiltersDTO.StaffTypeId}.");
            }

            var incidents = await Context.Incidents
             .Where(n => n.StaffTypeId == inicidenceFiltersDTO.StaffTypeId &&
                         ((n.StartDate <= inicidenceFiltersDTO.FinalDate && n.EndDate >= inicidenceFiltersDTO.StartDate)))
             .ToListAsync();

            
            // Obtener el dia de hoy
            // DIA DE HOY
            System.DateTime dateToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            // VALIDAR QUE LOS FILTROS NO SEAN MENOR AL DIA DE HOY
            if (inicidenceFiltersDTO.StartDate >= dateToday || inicidenceFiltersDTO.FinalDate >= dateToday)
            {
                return BadRequest("No se permite hacer incidencia en el día o posterior.");
            }

            // Obtener los tiempos de tolerancia
            var timesTolerancia = await Context.Times
                            .FirstOrDefaultAsync(t => t.StaffTypeId == inicidenceFiltersDTO.StaffTypeId);
            if (timesTolerancia == null)
            {
                return NotFound("No existe tiempos de tolerancia");
            }
            //Creamos un filtro base para evitar volver a crearla en cada iteracion
            var baseQueryable = Context.WorkSchedules.AsQueryable();

            if (inicidenceFiltersDTO.EmployeeId != 0)
            {
                baseQueryable = baseQueryable.Where(w => w.EmployeeId == inicidenceFiltersDTO.EmployeeId);
            }

            baseQueryable = baseQueryable.Where(w => w.Employee.StaffTypeId == inicidenceFiltersDTO.StaffTypeId)
                             .Include(w => w.Employee);

            var incidencesDTOs = new List<IncidenceTestDTO>();
            //Llamamos solo una vez a la base todos los dias inhabiles
            var nonWorkingDaysList = await Context.NonWorkingDays
            .Where(n => n.StartDate <= inicidenceFiltersDTO.FinalDate && n.FinalDate >= inicidenceFiltersDTO.StartDate)
            .ToListAsync();
           

            // Crear un diccionario para acceder rápidamente a los días inhábiles
            var nonWorkingDaysDict = new Dictionary<DateTime, bool>();
            foreach (var nonWorkingDay in nonWorkingDaysList)
            {
                for (var date = nonWorkingDay.StartDate; date <= nonWorkingDay.FinalDate; date = date.AddDays(1))
                {
                    nonWorkingDaysDict[date] = true;
                }
            }


            // Cargar todos los registros de horarios relevantes en una sola llamada
            var allSchedules = await baseQueryable.Where(w =>
            w.StartDate <= inicidenceFiltersDTO.FinalDate &&
            w.FinalDate >= inicidenceFiltersDTO.StartDate)
            .ToListAsync();
            // Crear un diccionario para indexar los horarios por día de la semana
            var schedulesByDayOfWeek = new Dictionary<DayOfWeek, List<WorkSchedule>>
    {
        { DayOfWeek.Monday, new List<WorkSchedule>() },
        { DayOfWeek.Tuesday, new List<WorkSchedule>() },
        { DayOfWeek.Wednesday, new List<WorkSchedule>() },
        { DayOfWeek.Thursday, new List<WorkSchedule>() },
        { DayOfWeek.Friday, new List<WorkSchedule>() },
        { DayOfWeek.Saturday, new List<WorkSchedule>() },
        { DayOfWeek.Sunday, new List<WorkSchedule>() }
    };
            // Indexar los horarios por empleado y día de la semana
            var schedulesByEmployeeAndDay = allSchedules
                .GroupBy(w => new { w.EmployeeId, Day = (int)w.StartDate.DayOfWeek })
                .ToDictionary(g => g.Key, g => g.ToList());
            // Indexar los horarios por día de la semana, excluyendo los que tienen CheckIn en TimeSpan.Zero
            foreach (var schedule in allSchedules)
            {
                if (schedule.MondayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Monday].Add(schedule);
                if (schedule.TuesdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Tuesday].Add(schedule);
                if (schedule.WednesdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Wednesday].Add(schedule);
                if (schedule.ThursdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Thursday].Add(schedule);
                if (schedule.FridayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Friday].Add(schedule);
                if (schedule.SaturdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Saturday].Add(schedule);
                if (schedule.SundayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Sunday].Add(schedule);
            }
            // Cargar todos los permisos de trabajo relevantes en una sola llamada
            var allPermits = await Context.WorkPermits
                .Include(w => w.Permit)
                .Where(p => p.IsActive == true && p.StartDate <= inicidenceFiltersDTO.FinalDate && p.FinalDate >= inicidenceFiltersDTO.StartDate)
                .ToListAsync();

            // Crear un diccionario para acceder rápidamente a los permisos por empleado y fecha
            var permitsByEmployeeId = allPermits.GroupBy(p => p.EmployeeId)
                                                .ToDictionary(g => g.Key, g => g.ToList());

            // Delegados para acceder a los horarios de check-in y check-out
            Func<WorkSchedule, TimeSpan>[] checkIns = new Func<WorkSchedule, TimeSpan>[]
            {
            s => s.SundayCheckIn,   // DayOfWeek.Sunday = 0
            s => s.MondayCheckIn,   // DayOfWeek.Monday = 1
            s => s.TuesdayCheckIn,  // DayOfWeek.Tuesday = 2
            s => s.WednesdayCheckIn,// DayOfWeek.Wednesday = 3
            s => s.ThursdayCheckIn, // DayOfWeek.Thursday = 4
            s => s.FridayCheckIn,   // DayOfWeek.Friday = 5
            s => s.SaturdayCheckIn  // DayOfWeek.Saturday = 6
            };

            Func<WorkSchedule, TimeSpan>[] checkOuts = new Func<WorkSchedule, TimeSpan>[]
            {
            s => s.SundayCheckOut,
            s => s.MondayCheckOut,
            s => s.TuesdayCheckOut,
            s => s.WednesdayCheckOut,
            s => s.ThursdayCheckOut,
            s => s.FridayCheckOut,
            s => s.SaturdayCheckOut
            };
            var allAttendances = await Context.Attendances
            .Where(a => a.Date >= inicidenceFiltersDTO.StartDate && a.Date <= inicidenceFiltersDTO.FinalDate)
            .Include(a => a.Station)
            .ToListAsync();


            for (var day = inicidenceFiltersDTO.StartDate; day <= inicidenceFiltersDTO.FinalDate; day = day.AddDays(1))
            {

                var incidentsForDate = incidents
                .Where(incident => day >= incident.StartDate && day <= incident.EndDate)
                .ToList();

                // Verificar si es día inhábil usando el diccionario
                var isNonWorkingDay = nonWorkingDaysDict.ContainsKey(day);
                if (!isNonWorkingDay)
                {
                    // Obtener el día de la semana actual
                    var currentDayOfWeek = day.DayOfWeek;

                    // Filtrar y ordenar los horarios del día actual en memoria
                    var schedules = schedulesByDayOfWeek[currentDayOfWeek]
                        .Where(w => w.StartDate <= day && w.FinalDate >= day)
                        .OrderBy(w =>
                            currentDayOfWeek switch
                            {
                                DayOfWeek.Monday => w.MondayCheckIn,
                                DayOfWeek.Tuesday => w.TuesdayCheckIn,
                                DayOfWeek.Wednesday => w.WednesdayCheckIn,
                                DayOfWeek.Thursday => w.ThursdayCheckIn,
                                DayOfWeek.Friday => w.FridayCheckIn,
                                DayOfWeek.Saturday => w.SaturdayCheckIn,
                                DayOfWeek.Sunday => w.SundayCheckIn,
                                _ => TimeSpan.MaxValue
                            })
                        .ToList();


                    // Limite para separar checadas de entrada y checadas de salida
                    TimeSpan limitTime;
                    // Horario de entrada dependiendo el día
                    TimeSpan timeIn;
                    // Horario de salida dependiendo el día
                    TimeSpan timeOut;

                    // Final e Inicio del dia
                    TimeSpan endOfTheDay;
                    TimeSpan startOfTheDay;



                    for (int i = 0; i < schedules.LongCount(); i++)
                    {

                        var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(schedules[i].Employee);

                        // Obtener los permisos de la base de datos que esten en fecha
                        var employeePermits = permitsByEmployeeId.TryGetValue(employeeDTO.Id, out var permits) ? permits : new List<WorkPermit>();
                        var permitEmployee = employeePermits.FirstOrDefault(p =>
                            p.StartDate <= day && p.FinalDate >= day);

                        // Calcular la hora que corresponde a la mitad de su horario.
                        var dayOfWeek = (int)day.DayOfWeek;
                        var schedule = schedules[i];
                        timeIn = checkIns[dayOfWeek](schedule);
                        timeOut = checkOuts[dayOfWeek](schedule);
                        limitTime = timeIn.Add(timeOut.Subtract(timeIn) / 2);

                        // Verificar si el empleado tiene otro horario

                        var hasAnotherSchedules = schedulesByEmployeeAndDay
                                        .Where(s => s.Key.EmployeeId == employeeDTO.Id && s.Key.Day == dayOfWeek)
                                        .SelectMany(s => s.Value)
                                        .Where(w => w.StartDate <= day && w.FinalDate >= day && w.Id != schedules[i].Id)
                                        .ToList();

                        var nextSchedule = hasAnotherSchedules
                            .Where(w => checkIns[dayOfWeek](w) > checkIns[dayOfWeek](schedule))
                            .OrderBy(w => checkIns[dayOfWeek](w))
                            .FirstOrDefault();

                        var previousSchedule = hasAnotherSchedules
                            .Where(w => checkIns[dayOfWeek](w) < checkIns[dayOfWeek](schedule))
                            .OrderByDescending(w => checkIns[dayOfWeek](w))
                            .FirstOrDefault();

                        if (nextSchedule != null)
                        {
                            var endOfTheLastDay = previousSchedule != null
                                ? checkIns[dayOfWeek](previousSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](previousSchedule)) / 2)
                                : new TimeSpan(0, 0, 0);

                            startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                            endOfTheDay = checkIns[dayOfWeek](nextSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](nextSchedule)) / 2);
                        }
                        else
                        {
                            var endOfTheLastDay = previousSchedule != null
                                ? checkIns[dayOfWeek](previousSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](previousSchedule)) / 2)
                                : new TimeSpan(0, 0, 0);

                            startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                            endOfTheDay = new TimeSpan(23, 0, 0);
                        }


                        // ======= CALCULAR CHECADA GANADORA DE ENTRADA ====== //
                        var attendanceIn = allAttendances
                            .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                        && a.Date.Date == day.Date
                                        && a.Time >= startOfTheDay
                                        && a.Time <= endOfTheDay
                                        && a.Time <= limitTime)
                            .OrderBy(a => Math.Abs((a.Time - timeIn).Ticks))
                            .FirstOrDefault();

                        // ======= CALCULAR CHECADA GANADORA DE SALIDA ====== //
                        var attendanceOut = allAttendances
                            .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                        && a.Date.Date == day.Date
                                        && a.Time >= startOfTheDay
                                        && a.Time <= endOfTheDay
                                        && a.Time > limitTime)
                            .OrderBy(a => Math.Abs((a.Time - timeOut).Ticks))
                            .FirstOrDefault();
                        if (attendanceIn == null)
                        {
                            attendanceIn = new Attendance();
                        }
                        if (attendanceOut == null)
                        {
                            attendanceOut = new Attendance();

                        }
                        var attendanceInDTO = Mapper.Map<AttendanceDTO>(attendanceIn);
                        var attendanceOutDTO = Mapper.Map<AttendanceDTO>(attendanceOut);

                        // Crear la incidencia
                        var incidenceDTO = new IncidenceTestDTO
                        {
                            Employee = employeeDTO,
                            Date = day,
                            Attendances = new List<AttendanceDTO> { attendanceInDTO, attendanceOutDTO },
                            Checks = new List<TimeSpan> { timeIn, timeOut },
                            Descriptions = new List<string>(),
                            Types = new List<int>(),
                            Color = new List<string>(),
                            Name = new List<string>(),
                        };


                        // ============ INCIDENCIAAA ENTRADA ============ 

                        // attendanceIn.Time tengo la hora de la checada ganadora

                        TimeSpan timeInSubMin = timeIn.Subtract(timesTolerancia.InputMin);
                        TimeSpan timeInAddMax = timeIn.Add(timesTolerancia.InputMax);
                        TimeSpan timeInAddMaxTwice = timeInAddMax.Add(timesTolerancia.InputMax);
                        TimeSpan timeInAddMaxThrice = timeInAddMaxTwice.Add(timesTolerancia.InputMax);
                        if (attendanceIn.Time >= timeInSubMin && attendanceIn.Time <= timeInAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Sin incidencia, entrada correcta. Su asistencia fue registrada dentro del límite de tolerancia. {timeInSubMin} y {timeInAddMax}");
                            incidenceDTO.Name.Add("Entrada correcta");
                            incidenceDTO.Color.Add("#007BFF");

                        }
                        else
                        {

                            foreach (var incident in incidentsForDate)
                            {
                                if (attendanceIn.Time == TimeSpan.Zero && (incident.TimeMin != TimeSpan.Zero || incident.TimeMax != TimeSpan.Zero))
                                {
                                    continue;
                                }
                                    if (incident.IsEntry == false)
                                {
                                    continue;
                                }
                                TimeSpan timeMin;
                                TimeSpan timeMax;


                                if (incident.IsBeforeCheckPoint)
                                {
                                    timeMin = timeIn.Subtract(incident.TimeMin);
                                    timeMax = timeIn.Subtract(incident.TimeMax);
                                }
                                else
                                {
                                    timeMin = timeIn.Add(incident.TimeMin);
                                    timeMax = timeIn.Add(incident.TimeMax);
                                }

                                if (incident.TimeMin == TimeSpan.Zero && incident.TimeMax == TimeSpan.Zero)
                                {
                                    if (attendanceIn.Time == TimeSpan.Zero)
                                    {
                                        var attendanceToday = allAttendances
                               .FirstOrDefault(a =>
                                   a.EmployeeId == schedules[i].EmployeeId &&
                                   a.Date.Date == day.Date &&
                                   a.Time > timeInAddMaxThrice &&
                                   a.Time >= startOfTheDay &&
                                   a.Time <= endOfTheDay);

                                        if (attendanceToday == null)
                                        {
                                            incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                            incidenceDTO.Name.Add("Falta");
                                            incidenceDTO.Color.Add("#FF0000");

                                        }
                                        else
                                        {
                                            incidenceDTO.Descriptions.Add(incident.Description);
                                            incidenceDTO.Color.Add(incident.Color);
                                            incidenceDTO.Name.Add(incident.Name);

                                        }
                                        break;
                                    }
                                    continue;


                                }
                                else if (incident.TimeMin == TimeSpan.Zero)
                                {
                                    timeMin = attendanceIn.Time;
                                }
                                else if (incident.TimeMax == TimeSpan.Zero)
                                {
                                    timeMax = attendanceIn.Time;
                                }
                                else if (timeMin > timeMax)
                                {
                                    var aux = timeMin;
                                    timeMin = timeMax;
                                    timeMax = aux;

                                }

                                if (attendanceIn.Time >= timeMin && attendanceIn.Time <= timeMax)
                                {
                                    incidenceDTO.Descriptions.Add(incident.Description);
                                   
                                    incidenceDTO.Name.Add(incident.Name); 
                                    incidenceDTO.Color.Add(incident.Color);
                                    break;
                                }
                            }
                        }



                     

                        // RETARDO A -
                        // RETARDO B -
                        // SIN INCIDENCIA - 
                        // ENTRADA PREVIA - 
                        // ENTRADA TARDIA -
                        // FALTA -
                        // OMISIÓN DE ENTRADA -

                        // ============ INCIDENCIAAA SALIDA ============ 


                        TimeSpan timeOutSubMin = timeOut.Subtract(timesTolerancia.OutputMin);
                        TimeSpan timeOutAddMax = timeOut.Add(timesTolerancia.OutputMax);


                        if (attendanceOut.Time >= timeOutSubMin && attendanceOut.Time <= timeOutAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Sin incidencia, salida correcta. Su salida fue registrada dentro del límite de tolerancia. {timeOutSubMin} y {timeOutAddMax}");
                            incidenceDTO.Name.Add("Salida correcta");
                            incidenceDTO.Color.Add("#007BFF"); 
                           
                        }
                        else
                        {
                            foreach (var incident in incidentsForDate)
                            {
                                if (attendanceOut.Time == TimeSpan.Zero && (incident.TimeMin != TimeSpan.Zero || incident.TimeMax != TimeSpan.Zero))
                                {
                                    continue;
                                }
                                if (incident.IsEntry )
                                {
                                    continue;
                                }
                                TimeSpan timeMin;
                                TimeSpan timeMax;


                                if (incident.IsBeforeCheckPoint)
                                {
                                    timeMin = timeOut.Subtract(incident.TimeMin);
                                    timeMax = timeOut.Subtract(incident.TimeMax);
                                }
                                else
                                {
                                    timeMin = timeOut.Add(incident.TimeMin);
                                    timeMax = timeOut.Add(incident.TimeMax);
                                }

                                if (incident.TimeMin == TimeSpan.Zero && incident.TimeMax == TimeSpan.Zero)
                                {
                                    if (attendanceOut.Time == TimeSpan.Zero)
                                    {
                                        var attendanceToday = allAttendances
                               .FirstOrDefault(a =>
                                   a.EmployeeId == schedules[i].EmployeeId &&
                                   a.Date.Date == day.Date &&
                                   a.Time < limitTime &&
                                   a.Time >= startOfTheDay &&
                                   a.Time <= endOfTheDay);

                                        if (attendanceToday == null)
                                        {
                                            incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                            incidenceDTO.Name.Add("Falta");
                                            incidenceDTO.Color.Add("#FF0000");
                                        }
                                        else
                                        {
                                            incidenceDTO.Descriptions.Add(incident.Description);
                                            incidenceDTO.Color.Add(incident.Color);
                                            incidenceDTO.Name.Add(incident.Name);
                                        }
                                        break;
                                    }
                                    continue;


                                }
                                else if (incident.TimeMin == TimeSpan.Zero)
                                {
                                    timeMin = attendanceOut.Time;
                                }
                                else if (incident.TimeMax == TimeSpan.Zero)
                                {
                                    timeMax = attendanceOut.Time;
                                }
                                else if (timeMin > timeMax)
                                {
                                    var aux = timeMin;
                                    timeMin = timeMax;
                                    timeMax = aux;

                                }

                                if (attendanceOut.Time >= timeMin && attendanceOut.Time <= timeMax)
                                {
                                    incidenceDTO.Descriptions.Add(incident.Description);
                                    incidenceDTO.Color.Add(incident.Color);
                                    incidenceDTO.Name.Add(incident.Name);
                                    break;
                                }
                            }



                        }

                     
                        if (permitEmployee != null)
                        {
                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            var description = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";

                            if (permitEmployee.Type == 0 || permitEmployee.WorkScheduleId == schedules[i].Id)
                            {
                                incidenceDTO.Descriptions[0] = description;
                                incidenceDTO.Descriptions[1] = description;
                            }
                        }


                        var Attendances = allAttendances
                        .Where(a => a.EmployeeId == schedules[i].EmployeeId && a.Date.Date == day.Date)
                        .ToList();
                        var attendanceDTO = Mapper.Map<List<AttendanceDTO>>(Attendances);
                        incidenceDTO.AttendancesAll = attendanceDTO;

                        incidencesDTOs.Add(incidenceDTO);


                    }
                }
                else
                {
                    Console.WriteLine("DIA INHABIL");
                }
            }





          return incidencesDTOs;

        }




        [HttpPost("export")]
        public async Task<ActionResult<String>> Export([FromBody] List<IncidenceTestDTO> incidencesDTOs, [FromQuery] String fileName)
        {

            try
            {

                var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("Nombre hoja");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Nombre";
                worksheet.Cell(currentRow, 2).Value = "Apellidos";
                worksheet.Cell(currentRow, 3).Value = "Hora entrada";
                worksheet.Cell(currentRow, 4).Value = "Hora salida";
                worksheet.Cell(currentRow, 5).Value = "Fecha incidencia";
        

                worksheet.Cell(currentRow, 6).Value = "Entrada / check";
                worksheet.Cell(currentRow, 7).Value = "Entrada / incidencia";
                worksheet.Cell(currentRow, 8).Value = "Entrrada / decripción";

                worksheet.Cell(currentRow, 9).Value = "Salida / check";
                worksheet.Cell(currentRow, 10).Value = "Salida / incidencia";
                worksheet.Cell(currentRow, 11).Value = "Salida / decripción";

                worksheet.Cell(currentRow, 12).Value = "Checadas";


                foreach (var user in incidencesDTOs)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = user.Employee.Name;
                    worksheet.Cell(currentRow, 2).Value = user.Employee.Lastname;
                    for (int i = 0; i < user.Checks.Count; i++)
                    {
                        worksheet.Cell(currentRow, 3 + i).Value = user.Checks[i];
                    }
                    worksheet.Cell(currentRow, 5).Value = user.Date.ToString("d");


                    worksheet.Cell(currentRow, 6).Value = user.Name[0] != "Entrada correcta" && user.Attendances[0].Time.ToString(@"hh\:mm\:ss") != "00:00:00" ? (user.Attendances[0].Time) : "--";
                    worksheet.Cell(currentRow, 7).Value = user.Name[0];
                    worksheet.Cell(currentRow, 8).Value = user.Name[0] != "Entrada correcta" ? user.Descriptions[0] : "--";




                    worksheet.Cell(currentRow, 9).Value = user.Name[1] != "Salida correcta" && user.Attendances[1].Time.ToString(@"hh\:mm\:ss") != "00:00:00" ? (user.Attendances[1].Time) : "--";
                    worksheet.Cell(currentRow, 10).Value = user.Name[1];
                    worksheet.Cell(currentRow, 11).Value = user.Name[1] != "Salida correcta" ? user.Descriptions[1] : "--";

                    for (int i = 0; i < user.AttendancesAll.Count; i++)
                    {
                        worksheet.Cell(currentRow, 12 + i).Value = user.AttendancesAll[i].Time;
                    }

                }

                DateTime now = DateTime.UtcNow;
                TimeSpan timeSpan = now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long milliseconds = (long)timeSpan.TotalMilliseconds;

                workbook.SaveAs($"wwwroot/exports/{milliseconds}.xlsx");

            }
            finally
            {
                Console.WriteLine("FINALLY");
            }

            return Ok(
                new
                {
                    message = "Exportado con éxito.",
                    filePath = $"/exports/attendances/{"asistencias".Normalize()}.xlsx"
                }
            );

        }





        [HttpGet("bad")]
        public async Task<ActionResult<List<IncidenceTestDTO>>> GetAllIncidencias2([FromQuery] InicidenceFiltersDTO inicidenceFiltersDTO)

        {


            var sfattType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == inicidenceFiltersDTO.StaffTypeId);

            if (sfattType == null)
            {
                return NotFound($"No existe un tipo de empleado con el ID ${inicidenceFiltersDTO.StaffTypeId}.");
            }

            var incidents = await Context.Incidents
             .Where(n => n.StaffTypeId == inicidenceFiltersDTO.StaffTypeId &&
                         ((n.StartDate <= inicidenceFiltersDTO.FinalDate && n.EndDate >= inicidenceFiltersDTO.StartDate)))
             .ToListAsync();


            // Obtener el dia de hoy
            // DIA DE HOY
            System.DateTime dateToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            // VALIDAR QUE LOS FILTROS NO SEAN MENOR AL DIA DE HOY
            if (inicidenceFiltersDTO.StartDate >= dateToday || inicidenceFiltersDTO.FinalDate >= dateToday)
            {
                return BadRequest("No se permite hacer incidencia en el día o posterior.");
            }

            // Obtener los tiempos de tolerancia
            var timesTolerancia = await Context.Times
                            .FirstOrDefaultAsync(t => t.StaffTypeId == inicidenceFiltersDTO.StaffTypeId);
            if (timesTolerancia == null)
            {
                return NotFound("No existe tiempos de tolerancia");
            }
            //Creamos un filtro base para evitar volver a crearla en cada iteracion
            var baseQueryable = Context.WorkSchedules.AsQueryable();

            if (inicidenceFiltersDTO.EmployeeId != 0)
            {
                baseQueryable = baseQueryable.Where(w => w.EmployeeId == inicidenceFiltersDTO.EmployeeId);
            }

            baseQueryable = baseQueryable.Where(w => w.Employee.StaffTypeId == inicidenceFiltersDTO.StaffTypeId)
                             .Include(w => w.Employee);

            var incidencesDTOs = new List<IncidenceTestDTO>();
            //Llamamos solo una vez a la base todos los dias inhabiles
            var nonWorkingDaysList = await Context.NonWorkingDays
            .Where(n => n.StartDate <= inicidenceFiltersDTO.FinalDate && n.FinalDate >= inicidenceFiltersDTO.StartDate)
            .ToListAsync();


            // Crear un diccionario para acceder rápidamente a los días inhábiles
            var nonWorkingDaysDict = new Dictionary<DateTime, bool>();
            foreach (var nonWorkingDay in nonWorkingDaysList)
            {
                for (var date = nonWorkingDay.StartDate; date <= nonWorkingDay.FinalDate; date = date.AddDays(1))
                {
                    nonWorkingDaysDict[date] = true;
                }
            }


            // Cargar todos los registros de horarios relevantes en una sola llamada
            var allSchedules = await baseQueryable.Where(w =>
            w.StartDate <= inicidenceFiltersDTO.FinalDate &&
            w.FinalDate >= inicidenceFiltersDTO.StartDate)
            .ToListAsync();
            // Crear un diccionario para indexar los horarios por día de la semana
            var schedulesByDayOfWeek = new Dictionary<DayOfWeek, List<WorkSchedule>>
    {
        { DayOfWeek.Monday, new List<WorkSchedule>() },
        { DayOfWeek.Tuesday, new List<WorkSchedule>() },
        { DayOfWeek.Wednesday, new List<WorkSchedule>() },
        { DayOfWeek.Thursday, new List<WorkSchedule>() },
        { DayOfWeek.Friday, new List<WorkSchedule>() },
        { DayOfWeek.Saturday, new List<WorkSchedule>() },
        { DayOfWeek.Sunday, new List<WorkSchedule>() }
    };
            // Indexar los horarios por empleado y día de la semana
            var schedulesByEmployeeAndDay = allSchedules
                .GroupBy(w => new { w.EmployeeId, Day = (int)w.StartDate.DayOfWeek })
                .ToDictionary(g => g.Key, g => g.ToList());
            // Indexar los horarios por día de la semana, excluyendo los que tienen CheckIn en TimeSpan.Zero
            foreach (var schedule in allSchedules)
            {
                if (schedule.MondayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Monday].Add(schedule);
                if (schedule.TuesdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Tuesday].Add(schedule);
                if (schedule.WednesdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Wednesday].Add(schedule);
                if (schedule.ThursdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Thursday].Add(schedule);
                if (schedule.FridayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Friday].Add(schedule);
                if (schedule.SaturdayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Saturday].Add(schedule);
                if (schedule.SundayCheckIn != TimeSpan.Zero)
                    schedulesByDayOfWeek[DayOfWeek.Sunday].Add(schedule);
            }
            // Cargar todos los permisos de trabajo relevantes en una sola llamada
            var allPermits = await Context.WorkPermits
                .Include(w => w.Permit)
                .Where(p => p.IsActive == true && p.StartDate <= inicidenceFiltersDTO.FinalDate && p.FinalDate >= inicidenceFiltersDTO.StartDate)
                .ToListAsync();

            // Crear un diccionario para acceder rápidamente a los permisos por empleado y fecha
            var permitsByEmployeeId = allPermits.GroupBy(p => p.EmployeeId)
                                                .ToDictionary(g => g.Key, g => g.ToList());

            // Delegados para acceder a los horarios de check-in y check-out
            Func<WorkSchedule, TimeSpan>[] checkIns = new Func<WorkSchedule, TimeSpan>[]
            {
            s => s.SundayCheckIn,   // DayOfWeek.Sunday = 0
            s => s.MondayCheckIn,   // DayOfWeek.Monday = 1
            s => s.TuesdayCheckIn,  // DayOfWeek.Tuesday = 2
            s => s.WednesdayCheckIn,// DayOfWeek.Wednesday = 3
            s => s.ThursdayCheckIn, // DayOfWeek.Thursday = 4
            s => s.FridayCheckIn,   // DayOfWeek.Friday = 5
            s => s.SaturdayCheckIn  // DayOfWeek.Saturday = 6
            };

            Func<WorkSchedule, TimeSpan>[] checkOuts = new Func<WorkSchedule, TimeSpan>[]
            {
            s => s.SundayCheckOut,
            s => s.MondayCheckOut,
            s => s.TuesdayCheckOut,
            s => s.WednesdayCheckOut,
            s => s.ThursdayCheckOut,
            s => s.FridayCheckOut,
            s => s.SaturdayCheckOut
            };
            var allAttendances = await Context.Attendances
            .Where(a => a.Date >= inicidenceFiltersDTO.StartDate && a.Date <= inicidenceFiltersDTO.FinalDate)
            .Include(a => a.Station)
            .ToListAsync();


            for (var day = inicidenceFiltersDTO.StartDate; day <= inicidenceFiltersDTO.FinalDate; day = day.AddDays(1))
            {

                var incidentsForDate = incidents
                .Where(incident => day >= incident.StartDate && day <= incident.EndDate)
                .ToList();

                // Verificar si es día inhábil usando el diccionario
                var isNonWorkingDay = nonWorkingDaysDict.ContainsKey(day);
                if (!isNonWorkingDay)
                {
                    // Obtener el día de la semana actual
                    var currentDayOfWeek = day.DayOfWeek;

                    // Filtrar y ordenar los horarios del día actual en memoria
                    var schedules = schedulesByDayOfWeek[currentDayOfWeek]
                        .Where(w => w.StartDate <= day && w.FinalDate >= day)
                        .OrderBy(w =>
                            currentDayOfWeek switch
                            {
                                DayOfWeek.Monday => w.MondayCheckIn,
                                DayOfWeek.Tuesday => w.TuesdayCheckIn,
                                DayOfWeek.Wednesday => w.WednesdayCheckIn,
                                DayOfWeek.Thursday => w.ThursdayCheckIn,
                                DayOfWeek.Friday => w.FridayCheckIn,
                                DayOfWeek.Saturday => w.SaturdayCheckIn,
                                DayOfWeek.Sunday => w.SundayCheckIn,
                                _ => TimeSpan.MaxValue
                            })
                        .ToList();


                    // Limite para separar checadas de entrada y checadas de salida
                    TimeSpan limitTime;
                    // Horario de entrada dependiendo el día
                    TimeSpan timeIn;
                    // Horario de salida dependiendo el día
                    TimeSpan timeOut;

                    // Final e Inicio del dia
                    TimeSpan endOfTheDay;
                    TimeSpan startOfTheDay;



                    for (int i = 0; i < schedules.LongCount(); i++)
                    {

                        var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(schedules[i].Employee);

                        // Obtener los permisos de la base de datos que esten en fecha
                        var employeePermits = permitsByEmployeeId.TryGetValue(employeeDTO.Id, out var permits) ? permits : new List<WorkPermit>();
                        var permitEmployee = employeePermits.FirstOrDefault(p =>
                            p.StartDate <= day && p.FinalDate >= day);

                        // Calcular la hora que corresponde a la mitad de su horario.
                        var dayOfWeek = (int)day.DayOfWeek;
                        var schedule = schedules[i];
                        timeIn = checkIns[dayOfWeek](schedule);
                        timeOut = checkOuts[dayOfWeek](schedule);
                        limitTime = timeIn.Add(timeOut.Subtract(timeIn) / 2);

                        // Verificar si el empleado tiene otro horario

                        var hasAnotherSchedules = schedulesByEmployeeAndDay
                                        .Where(s => s.Key.EmployeeId == employeeDTO.Id && s.Key.Day == dayOfWeek)
                                        .SelectMany(s => s.Value)
                                        .Where(w => w.StartDate <= day && w.FinalDate >= day && w.Id != schedules[i].Id)
                                        .ToList();

                        var nextSchedule = hasAnotherSchedules
                            .Where(w => checkIns[dayOfWeek](w) > checkIns[dayOfWeek](schedule))
                            .OrderBy(w => checkIns[dayOfWeek](w))
                            .FirstOrDefault();

                        var previousSchedule = hasAnotherSchedules
                            .Where(w => checkIns[dayOfWeek](w) < checkIns[dayOfWeek](schedule))
                            .OrderByDescending(w => checkIns[dayOfWeek](w))
                            .FirstOrDefault();

                        if (nextSchedule != null)
                        {
                            var endOfTheLastDay = previousSchedule != null
                                ? checkIns[dayOfWeek](previousSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](previousSchedule)) / 2)
                                : new TimeSpan(0, 0, 0);

                            startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                            endOfTheDay = checkIns[dayOfWeek](nextSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](nextSchedule)) / 2);
                        }
                        else
                        {
                            var endOfTheLastDay = previousSchedule != null
                                ? checkIns[dayOfWeek](previousSchedule).Add(timeOut.Subtract(checkIns[dayOfWeek](previousSchedule)) / 2)
                                : new TimeSpan(0, 0, 0);

                            startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                            endOfTheDay = new TimeSpan(23, 0, 0);
                        }


                        // ======= CALCULAR CHECADA GANADORA DE ENTRADA ====== //
                        var attendanceIn = allAttendances
                            .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                        && a.Date.Date == day.Date
                                        && a.Time >= startOfTheDay
                                        && a.Time <= endOfTheDay
                                        && a.Time <= limitTime)
                            .OrderBy(a => Math.Abs((a.Time - timeIn).Ticks))
                            .FirstOrDefault();

                        // ======= CALCULAR CHECADA GANADORA DE SALIDA ====== //
                        var attendanceOut = allAttendances
                            .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                        && a.Date.Date == day.Date
                                        && a.Time >= startOfTheDay
                                        && a.Time <= endOfTheDay
                                        && a.Time > limitTime)
                            .OrderBy(a => Math.Abs((a.Time - timeOut).Ticks))
                            .FirstOrDefault();
                        if (attendanceIn == null)
                        {
                            attendanceIn = new Attendance();
                        }
                        if (attendanceOut == null)
                        {
                            attendanceOut = new Attendance();

                        }
                        var attendanceInDTO = Mapper.Map<AttendanceDTO>(attendanceIn);
                        var attendanceOutDTO = Mapper.Map<AttendanceDTO>(attendanceOut);

                        // Crear la incidencia
                        var incidenceDTO = new IncidenceTestDTO
                        {
                            Employee = employeeDTO,
                            Date = day,
                            Attendances = new List<AttendanceDTO> { attendanceInDTO, attendanceOutDTO },
                            Checks = new List<TimeSpan> { timeIn, timeOut },
                            Descriptions = new List<string>(),
                            Types = new List<int>(),
                            Color = new List<string>(),
                            Name = new List<string>(),
                        };


                        // ============ INCIDENCIAAA ENTRADA ============ 

                        // attendanceIn.Time tengo la hora de la checada ganadora

                        TimeSpan timeInSubMin = timeIn.Subtract(timesTolerancia.InputMin);
                        TimeSpan timeInAddMax = timeIn.Add(timesTolerancia.InputMax);
                        TimeSpan timeInAddMaxTwice = timeInAddMax.Add(timesTolerancia.InputMax);
                        TimeSpan timeInAddMaxThrice = timeInAddMaxTwice.Add(timesTolerancia.InputMax);
                        if (attendanceIn.Time >= timeInSubMin && attendanceIn.Time <= timeInAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Sin incidencia, entrada correcta. Su asistencia fue registrada dentro del límite de tolerancia. {timeInSubMin} y {timeInAddMax}");
                            incidenceDTO.Name.Add("Entrada correcta");
                            incidenceDTO.Color.Add("#007BFF");

                        }
                        else
                        {

                            foreach (var incident in incidentsForDate)
                            {
                                if (attendanceIn.Time == TimeSpan.Zero && (incident.TimeMin != TimeSpan.Zero || incident.TimeMax != TimeSpan.Zero))
                                {
                                    continue;
                                }
                                if (incident.IsEntry == false)
                                {
                                    continue;
                                }
                                TimeSpan timeMin;
                                TimeSpan timeMax;


                                if (incident.IsBeforeCheckPoint)
                                {
                                    timeMin = timeIn.Subtract(incident.TimeMin);
                                    timeMax = timeIn.Subtract(incident.TimeMax);
                                }
                                else
                                {
                                    timeMin = timeIn.Add(incident.TimeMin);
                                    timeMax = timeIn.Add(incident.TimeMax);
                                }

                                if (incident.TimeMin == TimeSpan.Zero && incident.TimeMax == TimeSpan.Zero)
                                {
                                    if (attendanceIn.Time == TimeSpan.Zero)
                                    {
                                        var attendanceToday = allAttendances
                               .FirstOrDefault(a =>
                                   a.EmployeeId == schedules[i].EmployeeId &&
                                   a.Date.Date == day.Date &&
                                   a.Time > timeInAddMaxThrice &&
                                   a.Time >= startOfTheDay &&
                                   a.Time <= endOfTheDay);

                                        if (attendanceToday == null)
                                        {
                                            incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                            incidenceDTO.Name.Add("Falta");
                                            incidenceDTO.Color.Add("#FF0000");

                                        }
                                        else
                                        {
                                            incidenceDTO.Descriptions.Add(incident.Description);
                                            incidenceDTO.Color.Add(incident.Color);
                                            incidenceDTO.Name.Add(incident.Name);

                                        }
                                        break;
                                    }
                                    continue;


                                }
                                else if (incident.TimeMin == TimeSpan.Zero)
                                {
                                    timeMin = attendanceIn.Time;
                                }
                                else if (incident.TimeMax == TimeSpan.Zero)
                                {
                                    timeMax = attendanceIn.Time;
                                }
                                else if (timeMin > timeMax)
                                {
                                    var aux = timeMin;
                                    timeMin = timeMax;
                                    timeMax = aux;

                                }

                                if (attendanceIn.Time >= timeMin && attendanceIn.Time <= timeMax)
                                {
                                    incidenceDTO.Descriptions.Add(incident.Description);

                                    incidenceDTO.Name.Add(incident.Name);
                                    incidenceDTO.Color.Add(incident.Color);
                                    break;
                                }
                            }
                        }





                        // RETARDO A -
                        // RETARDO B -
                        // SIN INCIDENCIA - 
                        // ENTRADA PREVIA - 
                        // ENTRADA TARDIA -
                        // FALTA -
                        // OMISIÓN DE ENTRADA -

                        // ============ INCIDENCIAAA SALIDA ============ 


                        TimeSpan timeOutSubMin = timeOut.Subtract(timesTolerancia.OutputMin);
                        TimeSpan timeOutAddMax = timeOut.Add(timesTolerancia.OutputMax);


                        if (attendanceOut.Time >= timeOutSubMin && attendanceOut.Time <= timeOutAddMax)
                        {
                            incidenceDTO.Descriptions.Add($"Sin incidencia, salida correcta. Su salida fue registrada dentro del límite de tolerancia. {timeOutSubMin} y {timeOutAddMax}");
                            incidenceDTO.Name.Add("Salida correcta");
                            incidenceDTO.Color.Add("#007BFF");

                        }
                        else
                        {
                            foreach (var incident in incidentsForDate)
                            {
                                if (attendanceOut.Time == TimeSpan.Zero && (incident.TimeMin != TimeSpan.Zero || incident.TimeMax != TimeSpan.Zero))
                                {
                                    continue;
                                }
                                if (incident.IsEntry)
                                {
                                    continue;
                                }
                                TimeSpan timeMin;
                                TimeSpan timeMax;


                                if (incident.IsBeforeCheckPoint)
                                {
                                    timeMin = timeOut.Subtract(incident.TimeMin);
                                    timeMax = timeOut.Subtract(incident.TimeMax);
                                }
                                else
                                {
                                    timeMin = timeOut.Add(incident.TimeMin);
                                    timeMax = timeOut.Add(incident.TimeMax);
                                }

                                if (incident.TimeMin == TimeSpan.Zero && incident.TimeMax == TimeSpan.Zero)
                                {
                                    if (attendanceOut.Time == TimeSpan.Zero)
                                    {
                                        var attendanceToday = allAttendances
                               .FirstOrDefault(a =>
                                   a.EmployeeId == schedules[i].EmployeeId &&
                                   a.Date.Date == day.Date &&
                                   a.Time < limitTime &&
                                   a.Time >= startOfTheDay &&
                                   a.Time <= endOfTheDay);

                                        if (attendanceToday == null)
                                        {
                                            incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                            incidenceDTO.Name.Add("Falta");
                                            incidenceDTO.Color.Add("#FF0000");
                                        }
                                        else
                                        {
                                            incidenceDTO.Descriptions.Add(incident.Description);
                                            incidenceDTO.Color.Add(incident.Color);
                                            incidenceDTO.Name.Add(incident.Name);
                                        }
                                        break;
                                    }
                                    continue;


                                }
                                else if (incident.TimeMin == TimeSpan.Zero)
                                {
                                    timeMin = attendanceOut.Time;
                                }
                                else if (incident.TimeMax == TimeSpan.Zero)
                                {
                                    timeMax = attendanceOut.Time;
                                }
                                else if (timeMin > timeMax)
                                {
                                    var aux = timeMin;
                                    timeMin = timeMax;
                                    timeMax = aux;

                                }

                                if (attendanceOut.Time >= timeMin && attendanceOut.Time <= timeMax)
                                {
                                    incidenceDTO.Descriptions.Add(incident.Description);
                                    incidenceDTO.Color.Add(incident.Color);
                                    incidenceDTO.Name.Add(incident.Name);
                                    break;
                                }
                            }



                        }


                        if (permitEmployee != null)
                        {
                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            var description = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";

                            if (permitEmployee.Type == 0 || permitEmployee.WorkScheduleId == schedules[i].Id)
                            {
                                incidenceDTO.Descriptions[0] = description;
                                incidenceDTO.Descriptions[1] = description;
                            }
                        }


                        var Attendances = allAttendances
                        .Where(a => a.EmployeeId == schedules[i].EmployeeId && a.Date.Date == day.Date)
                        .ToList();
                        var attendanceDTO = Mapper.Map<List<AttendanceDTO>>(Attendances);
                        incidenceDTO.AttendancesAll = attendanceDTO;

                        incidencesDTOs.Add(incidenceDTO);


                    }
                }
                else
                {
                    Console.WriteLine("DIA INHABIL");
                }
            }

            var filters = incidencesDTOs.FindAll(item => item.Name[0] != "Entrada correcta" || item.Name[1] != "Salida correcta");
          

            return filters;

        }




        [HttpGet("bad2")]
        public async Task<ActionResult<List<IncidenceTestDTO>>> GetAllIncidencias([FromQuery] InicidenceFiltersDTO inicidenceFiltersDTO)
        {


            var sfattType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == inicidenceFiltersDTO.StaffTypeId);

            if (sfattType == null)
            {
                return NotFound($"No existe un tipo de empleado con el ID ${inicidenceFiltersDTO.StaffTypeId}.");
            }

            // Obtener el dia de hoy
            // DIA DE HOY
            System.DateTime dateToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            // VALIDAR QUE LOS FILTROS NO SEAN MENOR AL DIA DE HOY
            if (inicidenceFiltersDTO.StartDate >= dateToday || inicidenceFiltersDTO.FinalDate >= dateToday)
            {
                return BadRequest("No se permite hacer incidencia en el día o posterior.");
            }

            // Obtener los tiempos de tolerancia
            var timesTolerancia = await Context.Times
                            .FirstOrDefaultAsync(t => t.StaffTypeId == inicidenceFiltersDTO.StaffTypeId);
            if (timesTolerancia == null)
            {
                return NotFound("No existe tiempos de tolerancia");
            }

            var incidencesDTOs = new List<IncidenceTestDTO>();


            // COMENTARIO TEMPORAL 

            for (var day = inicidenceFiltersDTO.StartDate; day <= inicidenceFiltersDTO.FinalDate; day = day.AddDays(1))
            {


                // Verificar si es dia inhábil
                var isNonWorkingDay = await Context.NonWorkingDays.FirstOrDefaultAsync(n => n.StartDate <= day && n.FinalDate >= day);

                if (isNonWorkingDay == null)
                {


                    var queryable = Context.WorkSchedules.AsQueryable();


                    if (inicidenceFiltersDTO.EmployeeId != 0)
                    {

                        queryable = queryable.Where(w => w.EmployeeId == inicidenceFiltersDTO.EmployeeId);
                    }

                    queryable = queryable.Where(w =>
                               //w.Period.IsCurrent == true &&
                               w.Employee.StaffTypeId == inicidenceFiltersDTO.StaffTypeId &&
                               w.StartDate <= day &&
                               w.FinalDate >= day)
                            .Include(w => w.Employee);


                    var schedules = new List<WorkSchedule>();
                    // Quitar los que no trabajan hoy
                    switch ((int)day.DayOfWeek)
                    {
                        case 1:
                            schedules = await queryable.Where(w => w.MondayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.MondayCheckIn).ToListAsync();
                            break;
                        case 2:
                            schedules = await queryable.Where(w => w.TuesdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.TuesdayCheckIn).ToListAsync();
                            break;
                        case 3:
                            schedules = await queryable.Where(w => w.WednesdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.WednesdayCheckIn).ToListAsync();
                            break;
                        case 4:
                            schedules = await queryable.Where(w => w.ThursdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.ThursdayCheckIn).ToListAsync();
                            break;
                        case 5:
                            schedules = await queryable.Where(w => w.FridayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.FridayCheckIn).ToListAsync();
                            break;
                        case 6:
                            schedules = await queryable.Where(w => w.SaturdayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.SaturdayCheckIn).ToListAsync();
                            break;
                        case 0:
                            schedules = await queryable.Where(w => w.SundayCheckIn != new TimeSpan(0, 0, 0, 0, 0)).OrderBy(w => w.SundayCheckIn).ToListAsync();
                            break;
                        default:
                            return BadRequest();
                    }




                    // Limite para separar checadas de entrada y checadas de salida
                    TimeSpan limitTime;
                    // Horario de entrada dependiendo el día
                    TimeSpan timeIn;
                    // Horario de salida dependiendo el día
                    TimeSpan timeOut;

                    // Final e Inicio del dia
                    TimeSpan endOfTheDay;
                    TimeSpan startOfTheDay;



                    for (int i = 0; i < schedules.LongCount(); i++)
                    {

                        var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(schedules[i].Employee);

                        // Obtener los permisos de la base de datos que esten en fecha
                        var permitEmployee = await Context.WorkPermits
                                                    .Include(w => w.Permit)
                                                    .FirstOrDefaultAsync(p =>
                                                         p.EmployeeId == employeeDTO.Id &&
                                                         p.IsActive == true &&
                                                         p.StartDate <= day &&
                                                         p.FinalDate >= day);




                        // Calcular la hora que corresponde a la mitad de su horario.
                        switch ((int)day.DayOfWeek)
                        {
                            case 1:
                                timeIn = schedules[i].MondayCheckIn;
                                timeOut = schedules[i].MondayCheckOut;
                                limitTime = (schedules[i].MondayCheckIn.Add(schedules[i].MondayCheckOut.Subtract(schedules[i].MondayCheckIn) / 2));
                                break;
                            case 2:
                                timeIn = schedules[i].TuesdayCheckIn;
                                timeOut = schedules[i].TuesdayCheckOut;
                                limitTime = (schedules[i].TuesdayCheckIn.Add(schedules[i].TuesdayCheckOut.Subtract(schedules[i].TuesdayCheckIn) / 2));
                                break;
                            case 3:
                                timeIn = schedules[i].WednesdayCheckIn;
                                timeOut = schedules[i].WednesdayCheckOut;
                                limitTime = (schedules[i].WednesdayCheckIn.Add(schedules[i].WednesdayCheckOut.Subtract(schedules[i].WednesdayCheckIn) / 2));
                                break;
                            case 4:
                                timeIn = schedules[i].ThursdayCheckIn;
                                timeOut = schedules[i].ThursdayCheckOut;
                                limitTime = (schedules[i].ThursdayCheckIn.Add(schedules[i].ThursdayCheckOut.Subtract(schedules[i].ThursdayCheckIn) / 2));
                                break;
                            case 5:
                                timeIn = schedules[i].FridayCheckIn;
                                timeOut = schedules[i].FridayCheckOut;
                                limitTime = (schedules[i].FridayCheckIn.Add(schedules[i].FridayCheckOut.Subtract(schedules[i].FridayCheckIn) / 2));
                                break;
                            case 6:
                                timeIn = schedules[i].SaturdayCheckIn;
                                timeOut = schedules[i].SaturdayCheckOut;
                                limitTime = (schedules[i].SaturdayCheckIn.Add(schedules[i].SaturdayCheckOut.Subtract(schedules[i].SaturdayCheckIn) / 2));
                                break;
                            case 0:
                                timeIn = schedules[i].SundayCheckIn;
                                timeOut = schedules[i].SundayCheckOut;
                                limitTime = (schedules[i].SundayCheckIn.Add(schedules[i].SundayCheckOut.Subtract(schedules[i].SundayCheckIn) / 2));
                                break;
                            default:
                                return BadRequest();
                        }

                        Console.WriteLine(limitTime);

                        /// // ////////////////////////////////////
                        /// TEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEES
                        /// // ////////////////////////////////////

                        // Verificar si el empleado tiene otro horario

                        var queryableHasSchedules = Context.WorkSchedules.AsQueryable();
                        queryableHasSchedules = Context.WorkSchedules
                                .Where(w => w.EmployeeId == employeeDTO.Id &&
                                    w.StartDate <= day &&
                                    w.FinalDate >= day);

                        var hasAnotherSchedules = new List<WorkSchedule>();

                        switch ((int)day.DayOfWeek)
                        {
                            case 1:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.MondayCheckIn > schedules[i].MondayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.MondayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.MondayCheckIn)
                                        .FirstOrDefaultAsync(w => w.MondayCheckIn < hasAnotherSchedules[0].MondayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.MondayCheckIn.Add(timeOut.Subtract(lastSchedule.MondayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].MondayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].MondayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.MondayCheckIn)
                                        .FirstOrDefaultAsync(w => w.MondayCheckIn < schedules[i].MondayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.MondayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.MondayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;


                            case 2:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.TuesdayCheckIn > schedules[i].TuesdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.TuesdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.TuesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.TuesdayCheckIn < hasAnotherSchedules[0].TuesdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.TuesdayCheckIn.Add(timeOut.Subtract(lastSchedule.TuesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].TuesdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].TuesdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.TuesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.TuesdayCheckIn < schedules[i].TuesdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.TuesdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.TuesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            case 3:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.WednesdayCheckIn > schedules[i].WednesdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.WednesdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.WednesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.WednesdayCheckIn < hasAnotherSchedules[0].WednesdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.WednesdayCheckIn.Add(timeOut.Subtract(lastSchedule.WednesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].WednesdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].WednesdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.WednesdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.WednesdayCheckIn < schedules[i].WednesdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.WednesdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.WednesdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;


                            case 4:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.ThursdayCheckIn > schedules[i].ThursdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.ThursdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.ThursdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.ThursdayCheckIn < hasAnotherSchedules[0].ThursdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.ThursdayCheckIn.Add(timeOut.Subtract(lastSchedule.ThursdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].ThursdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].ThursdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.ThursdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.ThursdayCheckIn < schedules[i].ThursdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.ThursdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.ThursdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            case 5:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.FridayCheckIn > schedules[i].FridayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.FridayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.FridayCheckIn)
                                        .FirstOrDefaultAsync(w => w.FridayCheckIn < hasAnotherSchedules[0].FridayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.FridayCheckIn.Add(timeOut.Subtract(lastSchedule.FridayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].FridayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].FridayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.FridayCheckIn)
                                        .FirstOrDefaultAsync(w => w.FridayCheckIn < schedules[i].FridayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.FridayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.FridayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            case 6:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.SaturdayCheckIn > schedules[i].SaturdayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.SaturdayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SaturdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SaturdayCheckIn < hasAnotherSchedules[0].SaturdayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.SaturdayCheckIn.Add(timeOut.Subtract(lastSchedule.SaturdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].SaturdayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].SaturdayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SaturdayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SaturdayCheckIn < schedules[i].SaturdayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.SaturdayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.SaturdayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;


                            case 0:
                                hasAnotherSchedules = await queryableHasSchedules
                                        .Where(w => w.SundayCheckIn > schedules[i].SundayCheckIn && w.EmployeeId == schedules[i].EmployeeId)
                                        .OrderBy(w => w.SundayCheckIn).ToListAsync();



                                if (hasAnotherSchedules.LongCount() > 0)
                                {
                                    var lastSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SundayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SundayCheckIn < hasAnotherSchedules[0].SundayCheckIn
                                        && w.Id != schedules[i].Id
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (lastSchedule == null)
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);
                                    }
                                    else
                                    {
                                        var endOfTheLastDay = (lastSchedule.SundayCheckIn.Add(timeOut.Subtract(lastSchedule.SundayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }

                                    endOfTheDay = (hasAnotherSchedules[0].SundayCheckIn.Add(timeOut.Subtract(hasAnotherSchedules[0].SundayCheckIn) / 2));

                                }
                                else
                                {

                                    var hasPreviusSchedule = await Context.WorkSchedules
                                        .OrderByDescending(w => w.SundayCheckIn)
                                        .FirstOrDefaultAsync(w => w.SundayCheckIn < schedules[i].SundayCheckIn
                                        && w.EmployeeId == schedules[i].EmployeeId &&
                                         w.StartDate <= day &&
                                        w.FinalDate >= day);

                                    if (hasPreviusSchedule != null)
                                    {
                                        var endOfTheLastDay = (hasPreviusSchedule.SundayCheckIn.Add(timeOut.Subtract(hasPreviusSchedule.SundayCheckIn) / 2));
                                        startOfTheDay = endOfTheLastDay.Add(new TimeSpan(0, 0, 1));
                                    }
                                    else
                                    {
                                        startOfTheDay = new TimeSpan(0, 0, 0);

                                    }
                                    endOfTheDay = new TimeSpan(23, 0, 0);
                                }


                                //Console.WriteLine("//////////////////////////");
                                //Console.WriteLine(startOfTheDay);
                                //Console.WriteLine(endOfTheDay);
                                //Console.WriteLine("//////////////////////////");
                                break;

                            default:
                                return BadRequest();

                        }


                        // ======= CALCULAR CHECADA GANADORA DE ENTRADA ====== //


                        Attendance attendanceIn = new Attendance();

                        var attendanceInBeforeIn = await Context.Attendances
                                           .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                           && a.Date.Date == day.Date // Que sea del dia 
                                           && a.Time <= timeIn // que sea menor o igual a la hora que tiene de entrada
                                           && a.Time <= limitTime // que sea menor o igual al limite
                                           && a.Time >= startOfTheDay
                                           && a.Time <= endOfTheDay)
                                           .OrderByDescending(a => a.Time)
                                           .Include(a => a.Station)
                                           .FirstOrDefaultAsync();


                        var attendanceInAffterIn = await Context.Attendances
                                          .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                          && a.Date.Date == day.Date
                                          && a.Time > timeIn // Que sea mayor a la hora que tiene de entrada
                                          && a.Time <= limitTime // que sea menor o igual al limite
                                           && a.Time >= startOfTheDay
                                           && a.Time <= endOfTheDay)
                                          .OrderBy(a => a.Time)
                                          .Include(a => a.Station)
                                          .FirstOrDefaultAsync();


                        // Verficar fecha más cercana a la de la entrada
                        if (attendanceInBeforeIn == null && attendanceInAffterIn == null)
                        {
                            //Console.WriteLine("No hay nada que procesar");
                            //Console.WriteLine(employeeDTO.Name);

                        }
                        else if (attendanceInBeforeIn != null && attendanceInAffterIn != null)
                        {
                            TimeSpan difInMin = timeIn.Subtract(attendanceInBeforeIn.Time);
                            TimeSpan difInMax = attendanceInAffterIn.Time.Subtract(timeIn);

                            int result = TimeSpan.Compare(difInMin, difInMax);

                            if (result < 0)
                                attendanceIn = attendanceInBeforeIn;
                            else
                                attendanceIn = attendanceInAffterIn;

                        }
                        else if (attendanceInBeforeIn == null && attendanceInAffterIn != null)
                        {
                            attendanceIn = attendanceInAffterIn;
                        }
                        else if (attendanceInBeforeIn != null && attendanceInAffterIn == null)
                        {
                            attendanceIn = attendanceInBeforeIn;
                        }


                        // ======= CALCULAR CHECADA GANADORA DE SALIDA ====== //


                        Attendance attendanceOut = new Attendance();

                        var attendanceInBeforeOut = await Context.Attendances
                                           .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                           && a.Date.Date == day.Date // Que sea de hoy 
                                           && a.Time <= timeOut // que sea menor o igual a la hora que tiene de salida
                                           && a.Time > limitTime // que sea mayor al limite
                                           && a.Time >= startOfTheDay
                                           && a.Time <= endOfTheDay)
                                           .OrderByDescending(a => a.Time)
                                           .Include(a => a.Station)
                                           .FirstOrDefaultAsync();


                        var attendanceInAffterOut = await Context.Attendances
                                          .Where(a => a.EmployeeId == schedules[i].EmployeeId
                                          && a.Date.Date == day.Date
                                          && a.Time > timeOut // Que sea mayor a la hora que tiene de entrada
                                          && a.Time >= startOfTheDay
                                          && a.Time <= endOfTheDay)
                                          .OrderBy(a => a.Time)
                                          .Include(a => a.Station)
                                          .FirstOrDefaultAsync();


                        // Verficar fecha más cercana a la de la salida
                        if (attendanceInBeforeOut == null && attendanceInAffterOut == null)
                        {
                            //Console.WriteLine("No hay nada que procesar");
                            //Console.WriteLine(employeeDTO.Name);

                        }
                        else if (attendanceInBeforeOut != null && attendanceInAffterOut != null)
                        {
                            TimeSpan difInMin = timeOut.Subtract(attendanceInBeforeOut.Time);
                            TimeSpan difInMax = attendanceInAffterOut.Time.Subtract(timeOut);

                            int result = TimeSpan.Compare(difInMin, difInMax);

                            if (result < 0)
                                attendanceOut = attendanceInBeforeOut;
                            else
                                attendanceOut = attendanceInAffterOut;

                        }
                        else if (attendanceInBeforeOut == null && attendanceInAffterOut != null)
                        {
                            attendanceOut = attendanceInAffterOut;
                        }
                        else if (attendanceInBeforeOut != null && attendanceInAffterOut == null)
                        {
                            attendanceOut = attendanceInBeforeOut;
                        }

                        var attendanceInDTO = Mapper.Map<AttendanceDTO>(attendanceIn);
                        var attendanceOutDTO = Mapper.Map<AttendanceDTO>(attendanceOut);




                        // Crear la incidencia
                        IncidenceTestDTO incidenceDTO = new()
                        {
                            Employee = employeeDTO,
                            Date = day,
                            Attendances = new List<AttendanceDTO> { attendanceInDTO, attendanceOutDTO },
                            Checks = new List<TimeSpan> { timeIn, timeOut },
                            Descriptions = new List<string>(),
                            Types = new List<int>()
                        };


                        // ============ INCIDENCIAAA ENTRADA ============ 

                        // attendanceIn.Time tengo la hora de la checada ganadora

                        if (attendanceIn.Time == new TimeSpan(0, 0, 0, 0, 0))
                        {
                            // Verificar si hay registro superior
                            var attendanceToday = await Context.Attendances
                                                            .FirstOrDefaultAsync(a =>
                                                             a.EmployeeId == schedules[i].EmployeeId &&
                                                            a.Date.Date == day.Date &&
                                                            a.Time > timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax) &&
                                                             a.Time >= startOfTheDay &&
                                                             a.Time <= endOfTheDay);

                            if (attendanceToday == null)
                            {
                                //Console.WriteLine("FALTA");
                                //Console.WriteLine("NO HAY CHECADA");

                                incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                incidenceDTO.Types.Add(7);

                            }
                            else
                            {
                                //Console.WriteLine("OMISIÓN DE ENTRADA");
                                //Console.WriteLine("CHECO SALIDA A LAS");
                                //Console.WriteLine(attendanceToday.Time);

                                incidenceDTO.Descriptions.Add("Omisión de entrada. No hay asistencia correspondiente a su entrada.");
                                incidenceDTO.Types.Add(6);

                            }

                        }
                        else if (attendanceIn.Time >= timeIn.Subtract(timesTolerancia.InputMin) && attendanceIn.Time <= timeIn.Add(timesTolerancia.InputMax))
                        {
                            //Console.WriteLine("SIN INCIDENCIA, ENTRADA CORRECTA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("ENTRE");
                            //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));

                            incidenceDTO.Descriptions.Add($"Sin incidencia, entrada correcta. Su asistencia fue registrada dentro del limite de tolerancia. {timeIn.Subtract(timesTolerancia.InputMin)} y {timeIn.Add(timesTolerancia.InputMax)}");
                            incidenceDTO.Types.Add(1);

                        }
                        else if (attendanceIn.Time < timeIn.Subtract(timesTolerancia.InputMin))
                        {
                            //Console.WriteLine("ENTRADA PREVIA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("ANTES");
                            //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));

                            incidenceDTO.Descriptions.Add($"Entrada previa. Su asistencia fue registrada antes de {timeIn.Subtract(timesTolerancia.InputMin)}");
                            incidenceDTO.Types.Add(2);

                        }
                        else if (attendanceIn.Time > timeIn.Add(timesTolerancia.InputMax) && attendanceIn.Time <= timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax))
                        {
                            //Console.WriteLine("RETARDO A");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));
                            //Console.WriteLine("ANTES O IGUAL A");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));

                            incidenceDTO.Descriptions.Add($"Retardo A. Su asistencia fue registrada entre {timeIn.Add(timesTolerancia.InputMax).Add(new TimeSpan(0, 0, 1))} y {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax)}");
                            incidenceDTO.Types.Add(3);

                        }
                        else if (attendanceIn.Time > timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax) && attendanceIn.Time <= timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax))
                        {
                            //Console.WriteLine("RETARDO B");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));
                            //Console.WriteLine("ANTES O IGUAL A");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));

                            incidenceDTO.Descriptions.Add($"Retardo B. Su asistencia fue registrada entre {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(new TimeSpan(0, 0, 1))} y {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax)}");
                            incidenceDTO.Types.Add(4);

                        }
                        else if (attendanceIn.Time > timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax) && attendanceIn.Time <= limitTime)
                        {
                            //Console.WriteLine("ENTRADA TARDIA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceIn.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax));
                            //Console.WriteLine("ANTES O IGUAL A");
                            //Console.WriteLine(limitTime);

                            incidenceDTO.Descriptions.Add($"Entrada tardia. Su asistencia fue registrada después {timeIn.Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(timesTolerancia.InputMax).Add(new TimeSpan(0, 0, 1))}.");
                            incidenceDTO.Types.Add(5);

                        }

                        // RETARDO A -
                        // RETARDO B -
                        // SIN INCIDENCIA - 
                        // ENTRADA PREVIA - 
                        // ENTRADA TARDIA -
                        // FALTA -
                        // OMISIÓN DE ENTRADA -




                        //Console.WriteLine("====================");

                        //Console.WriteLine("Hora de entrada");
                        //Console.WriteLine(timeIn);

                        //Console.WriteLine("Hora de salida");
                        //Console.WriteLine(timeOut);

                        //Console.WriteLine("Hora de checada entrada");
                        //Console.WriteLine(attendanceIn.Time);

                        //Console.WriteLine("Hora de checada salida");
                        //Console.WriteLine(attendanceOut.Time);

                        //Console.WriteLine("Hora tolerancia entrada antes");
                        //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));
                        //Console.WriteLine("Hora tolerancia entrada después");
                        //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));

                        //Console.WriteLine("Hora tolerancia salida antes");
                        //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));
                        //Console.WriteLine("Hora tolerancia salida después");
                        //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));



                        // ============ INCIDENCIAAA SALIDA ============ 



                        if (attendanceOut.Time == new TimeSpan(0, 0, 0, 0, 0))
                        {
                            // Verificar si hay registro inferior (de entrada)
                            var attendanceToday = await Context.Attendances
                                                            .FirstOrDefaultAsync(a =>
                                                             a.EmployeeId == schedules[i].EmployeeId &&
                                                            a.Date.Date == day.Date &&
                                                            a.Time < limitTime
                                                             && a.Time >= startOfTheDay
                                                            && a.Time <= endOfTheDay);

                            if (attendanceToday == null)
                            {
                                //Console.WriteLine("FALTA");
                                //Console.WriteLine("NO HAY CHECADA");

                                incidenceDTO.Descriptions.Add("No hay un registro de asistencia.");
                                incidenceDTO.Types.Add(7);

                            }
                            else
                            {
                                //Console.WriteLine("OMISIÓN DE SALIDA");
                                //Console.WriteLine("CHECO SALIDA A LAS");
                                //Console.WriteLine(attendanceToday.Time);

                                incidenceDTO.Descriptions.Add("Omisión de salida. No hay asistencia correspondiente a su salida.");
                                incidenceDTO.Types.Add(10);

                            }

                        }
                        else if (attendanceOut.Time >= timeOut.Subtract(timesTolerancia.OutputMin) && attendanceOut.Time <= timeOut.Add(timesTolerancia.OutputMax))
                        {
                            //Console.WriteLine("SIN INCIDENCIA, SALIDA CORRECTA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceOut.Time);
                            //Console.WriteLine("ENTRE");
                            //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));
                            //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));

                            incidenceDTO.Descriptions.Add($"Sin incidencia, salida correcta. Su salida fue registrada dentro del limite de tolerancia. {timeOut.Subtract(timesTolerancia.OutputMin)} y {timeOut.Add(timesTolerancia.OutputMax)}");
                            incidenceDTO.Types.Add(8);

                        }
                        else if (attendanceOut.Time < timeOut.Subtract(timesTolerancia.OutputMin))
                        {
                            //Console.WriteLine("SALIDA PREVIA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceOut.Time);
                            //Console.WriteLine("ANTES");
                            //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));

                            incidenceDTO.Descriptions.Add($"Salida previa. Su salida fue registrada antes de {timeOut.Subtract(timesTolerancia.OutputMin)}");
                            incidenceDTO.Types.Add(9);

                        }
                        else if (attendanceOut.Time > timeOut.Add(timesTolerancia.OutputMax))
                        {
                            //Console.WriteLine("SALIDA TARDÍA");
                            //Console.WriteLine("Hora de la checada");
                            //Console.WriteLine(attendanceOut.Time);
                            //Console.WriteLine("DESPUES DE ");
                            //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));

                            incidenceDTO.Descriptions.Add($"Salida tardía. Su salida fue registrada despues de {timeOut.Add(timesTolerancia.OutputMax).Add(new TimeSpan(0, 0, 1))}");
                            incidenceDTO.Types.Add(11);

                        }



                        //Console.WriteLine("====================");

                        //Console.WriteLine("Hora de entrada");
                        //Console.WriteLine(timeIn);

                        //Console.WriteLine("Hora de salida");
                        //Console.WriteLine(timeOut);

                        //Console.WriteLine("Hora de checada salida");
                        //Console.WriteLine(attendanceOut.Time);

                        //Console.WriteLine("Hora tolerancia entrada antes");
                        //Console.WriteLine(timeIn.Subtract(timesTolerancia.InputMin));
                        //Console.WriteLine("Hora tolerancia entrada después");
                        //Console.WriteLine(timeIn.Add(timesTolerancia.InputMax));

                        //Console.WriteLine("Hora tolerancia salida antes");
                        //Console.WriteLine(timeOut.Subtract(timesTolerancia.OutputMin));
                        //Console.WriteLine("Hora tolerancia salida después");
                        //Console.WriteLine(timeOut.Add(timesTolerancia.OutputMax));


                        if (permitEmployee != null && permitEmployee.Type == 0)
                        {
                            //Console.WriteLine("=================================");
                            //Console.WriteLine("TIENE PERMISO TODO EL DIA");
                            //Console.WriteLine("=================================");
                            //Console.WriteLine(day);

                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            incidenceDTO.Descriptions[0] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                            incidenceDTO.Descriptions[1] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                        }

                        if (permitEmployee != null && permitEmployee.WorkScheduleId == schedules[i].Id)
                        {
                            //Console.WriteLine("=================================");
                            //Console.WriteLine("TIENE PERMISO EN UN HORARIO ESPECIFICO");
                            //Console.WriteLine("=================================");
                            //Console.WriteLine(day);

                            var permitEmployeeDTO = Mapper.Map<WorkPermitDTO>(permitEmployee);
                            incidenceDTO.Permit = permitEmployeeDTO;
                            incidenceDTO.Descriptions[0] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                            incidenceDTO.Descriptions[1] = $"Permiso {permitEmployeeDTO.Permit.Title} " + (permitEmployeeDTO.Permit.RequiredAttendance ? "con " : "sin ") + "registro de reloj.";
                        }


                        var allAttendances = await Context.Attendances
                                                .Where(a => a.EmployeeId == schedules[i].EmployeeId &&
                                                            a.Date.Date == day.Date)
                                                .ToListAsync();
                        var attendanceDTO = Mapper.Map<List<AttendanceDTO>>(allAttendances);
                        incidenceDTO.AttendancesAll = attendanceDTO;

                        incidencesDTOs.Add(incidenceDTO);

                    }
                }
                else
                {
                    Console.WriteLine("DIA INHABIL");
                }
            }

            var filters = incidencesDTOs.FindAll(item => item.Types[0] != 1 || item.Types[1] != 8);
 
            return filters;

        }






    }


}