using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/notices")]
    public class NoticesController: ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public NoticesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }



        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<NoticeDTO>>> GetAll([FromQuery] NoticeFiltersDTO noticeFiltersDTO)
        {

            var queryable = Context.Notices.AsQueryable();

            if (noticeFiltersDTO.EmployeeId != 0)
            {
                queryable = queryable.Include(n => n.NoticeEmployee)
                                    .ThenInclude(ne => ne.Employee)
                                    .Where(n => n.NoticeEmployee.Select(e => e.EmployeeId).Contains(noticeFiltersDTO.EmployeeId));
            }

            var notices = await queryable.Paginar(noticeFiltersDTO.Pagination)
                                    .Include(n => n.NoticeEmployee)
                                    .ThenInclude(ne => ne.Employee)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var noticesDTOs = Mapper.Map<List<NoticeDTO>>(notices);

            var result = new PaginationResultDTO<NoticeDTO>
            {
                Results = noticesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }




        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<NoticeDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.Notices.AsQueryable();

            var notices = await queryable.Paginar(paginationDto)
                                    .Include(n => n.NoticeEmployee)
                                    .ThenInclude(ne => ne.Employee)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var noticesDTOs = Mapper.Map<List<NoticeDTO>>(notices);

            var result = new PaginationResultDTO<NoticeDTO>
            {
                Results = noticesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }



        // Usado para el reloj checador
        [HttpGet("employee/{employeeId:int}")]
        public async Task<ActionResult<NoticeDTO>> GetByUserId(int employeeId)
        {

            var today = DateTime.Today;

            var notice = await Context.Notices
                                    .Include(n => n.NoticeEmployee)
                                    .ThenInclude(ne => ne.Employee)
                                    .FirstOrDefaultAsync(n => n.StartDate <= today &&
                                    n.FinalDate >= today && 
                                    n.NoticeEmployee.Select(e => e.EmployeeId).Contains(employeeId) &&
                                    n.IsActive == true );

            if ( notice == null )
            {
                return NotFound();
            }


            var noticeDTO = Mapper.Map<NoticeDTO>(notice);

            return noticeDTO;
        }





        [HttpGet("{id:int}", Name = "GetNoticeById")]
        public async Task<ActionResult<NoticeDTO>> GetById(int id)
        {
            var notice = await Context.Notices
                            .Include(n => n.NoticeEmployee)
                            .ThenInclude(ne => ne.Employee)
                            .FirstOrDefaultAsync(n => n.Id == id);

            if (notice == null)
            {
                return NotFound($"No existe un mensaje con el ID: {id}");
            }

            var noticeDTO = Mapper.Map<NoticeDTO>(notice);

            return noticeDTO;
        }



        [HttpGet("active/{id:int}")]
        public async Task<ActionResult<NoticeDTO>> ChangeActive(int id)
        {
            var notice = await Context.Notices.FirstOrDefaultAsync(n => n.Id == id);

            if (notice == null)
            {
                return NotFound($"No existe un mensaje con el ID: {id}");
            }

            notice.IsActive = notice.IsActive ? false : true;
            Context.Entry(notice).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return NoContent();
        }






        [HttpPost]
        public async Task<ActionResult> Create([FromBody] NoticeCreateDTO noticeCreateDTO)
        {

            //if ( noticeCreateDTO.EmployeesIds == null )
            //{
            //return BadRequest("Se requiere almenos un empleado.");
            //}

            var ids = new List<int>();
            var entity = "Employee";
            var entityId = 0;

            if (noticeCreateDTO.DepartmentId != 0)
            {

                var department = await Context.Departments.FirstOrDefaultAsync(d => d.Id == noticeCreateDTO.DepartmentId);

                if ( department == null )
                {
                    return BadRequest($"No existe un departamento con el ID: {noticeCreateDTO.DepartmentId}");
                }

                entity = "Department";
                entityId = noticeCreateDTO.DepartmentId;
                ids = await Context.Employees
                    .Where(e => e.DepartmentId == noticeCreateDTO.DepartmentId)
                    .Select(e => e.Id)
                    .ToListAsync();

            } else if (noticeCreateDTO.StaffTypeId != 0)
            {

                var staffType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == noticeCreateDTO.StaffTypeId);

                if (staffType == null)
                {
                    return BadRequest($"No existe un tipo de empleado con el ID: {noticeCreateDTO.StaffTypeId}");
                }

                entity = "StaffType";
                entityId = noticeCreateDTO.StaffTypeId;
                ids = await Context.Employees
                     .Where(e => e.StaffTypeId == noticeCreateDTO.StaffTypeId)
                     .Select(e => e.Id)
                     .ToListAsync();
            }
            else if (noticeCreateDTO.WorkStationId != 0)
            {
                var workStation = await Context.WorkStations.FirstOrDefaultAsync(w => w.Id == noticeCreateDTO.WorkStationId);

                if (workStation == null)
                {
                    return BadRequest($"No existe un puesto de tabajo con el ID: {noticeCreateDTO.WorkStationId}");
                }

                entity = "WorkStation";
                entityId = noticeCreateDTO.WorkStationId;
                ids = await Context.Employees
                     .Where(e => e.WorkStationId == noticeCreateDTO.WorkStationId)
                     .Select(e => e.Id)
                     .ToListAsync();
            }
            else if ( noticeCreateDTO.EmployeesIds != null)
            {
                ids = noticeCreateDTO.EmployeesIds;
            }

            var employeesIds = await Context.Employees
                    .Where(e => ids.Contains(e.Id))
                    .Select(e => e.Id)
                    .ToListAsync();

                // Si alguno de los id de las empleados no esta,
                // significa que mando uno que no es válid

                if (ids.Count != employeesIds.Count)
                {
                    return BadRequest("No existe alguno de los empleados.");
                }

            noticeCreateDTO.EmployeesIds = ids;
            noticeCreateDTO.Entity = entity;
            noticeCreateDTO.EntityId = entityId;

            var notice = Mapper.Map<Notice>(noticeCreateDTO);

            Context.Add(notice);

            await Context.SaveChangesAsync();

            var newNotice = await Context.Notices.AsNoTracking()
                    .Include(n => n.NoticeEmployee)
                    .ThenInclude(ne => ne.Employee)
                    .FirstAsync(e => e.Id == notice.Id);

            var noticeDTO = Mapper.Map<NoticeDTO>(newNotice);

            return CreatedAtRoute("GetNoticeById", new { id = noticeDTO.Id }, noticeDTO);

        }





        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, NoticeCreateDTO noticeCreateDTO)
        {
            var noticedb = await Context.Notices
                    .FirstOrDefaultAsync(n => n.Id == id);


            if (noticedb == null)
            {
                return BadRequest($"El mensaje con el ID {id} no existe.");
            }

            var ids = new List<int>();
            var entity = "Employee";
            var entityId = 0;

            if (noticeCreateDTO.DepartmentId != 0)
            {

                var department = await Context.Departments.FirstOrDefaultAsync(d => d.Id == noticeCreateDTO.DepartmentId);

                if (department == null)
                {
                    return BadRequest($"No existe un departamento con el ID: {noticeCreateDTO.DepartmentId}");
                }

                entity = "Department";
                entityId = noticeCreateDTO.DepartmentId;
                ids = await Context.Employees
                     .Where(e => e.DepartmentId == noticeCreateDTO.DepartmentId)
                     .Select(e => e.Id)
                     .ToListAsync();
            }
            else if (noticeCreateDTO.StaffTypeId != 0)
            {

                var staffType = await Context.StaffTypes.FirstOrDefaultAsync(s => s.Id == noticeCreateDTO.StaffTypeId);

                if (staffType == null)
                {
                    return BadRequest($"No existe un tipo de empleado con el ID: {noticeCreateDTO.StaffTypeId}");
                }

                entity = "StaffType";
                entityId = noticeCreateDTO.StaffTypeId;
                ids = await Context.Employees
                     .Where(e => e.StaffTypeId == noticeCreateDTO.StaffTypeId)
                     .Select(e => e.Id)
                     .ToListAsync();
            }
            else if (noticeCreateDTO.WorkStationId != 0)
            {
                var workStation = await Context.WorkStations.FirstOrDefaultAsync(w => w.Id == noticeCreateDTO.WorkStationId);

                if (workStation == null)
                {
                    return BadRequest($"No existe un puesto de tabajo con el ID: {noticeCreateDTO.WorkStationId}");
                }

                entity = "WorkStation";
                entityId = noticeCreateDTO.WorkStationId;
                ids = await Context.Employees
                     .Where(e => e.WorkStationId == noticeCreateDTO.WorkStationId)
                     .Select(e => e.Id)
                     .ToListAsync();
            }
            else if (noticeCreateDTO.EmployeesIds != null)
            {
                ids = noticeCreateDTO.EmployeesIds;
            }

            var employeesIds = await Context.Employees
                    .Where(e => ids.Contains(e.Id))
                    .Select(e => e.Id)
                    .ToListAsync();

            // Si alguno de los id de las empleados no esta,
            // significa que mando uno que no es válid

            if (ids.Count != employeesIds.Count)
            {
                return BadRequest("No existe alguno de los empleados.");
            }

            noticeCreateDTO.EmployeesIds = ids;
            noticeCreateDTO.Entity = entity;
            noticeCreateDTO.EntityId = entityId;

            Context.NoticesEmployees.RemoveRange(Context.NoticesEmployees.Where(ne => ne.NoticeId == id));

            noticedb = Mapper.Map(noticeCreateDTO, noticedb);

            await Context.SaveChangesAsync();

            return NoContent();
        }



    }
}
