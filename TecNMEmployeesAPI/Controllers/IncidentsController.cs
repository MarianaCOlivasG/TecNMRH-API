using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/incidents")]
    public class IncidentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public IncidentsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/incidents
        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<IncidentDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {
            if (paginationDto == null)
            {
                paginationDto = new PaginationDTO();
            }

            var queryable = _context.Incidents
                .Include(i => i.StaffType)  // Incluir la relación StaffType
                .AsQueryable();

            var incidents = await queryable.Paginar(paginationDto).ToListAsync();
            var totalResults = await queryable.CountAsync();

            var incidentsDTOs = _mapper.Map<List<IncidentDTO>>(incidents);

            var result = new PaginationResultDTO<IncidentDTO>
            {
                Results = incidentsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }

        // GET: api/incidents/filter
        [HttpGet("filter")]
        public async Task<ActionResult<PaginationResultDTO<IncidentDTO>>> GetAllFilter([FromQuery] IncidentFilterDTO incidentsFilterDTO)
        {
            var queryable = _context.Incidents
                .Include(i => i.StaffType)  // Incluir la relación StaffType
                .AsQueryable();

            if (!string.IsNullOrEmpty(incidentsFilterDTO.Name))
            {
                queryable = queryable.Where(i => i.Name.Contains(incidentsFilterDTO.Name));
            }

            if (incidentsFilterDTO.IsEntry.HasValue)
            {
                queryable = queryable.Where(i => i.IsEntry == incidentsFilterDTO.IsEntry.Value);
            }

            if (incidentsFilterDTO.StartDate.HasValue)
            {
                queryable = queryable.Where(i => i.StartDate >= incidentsFilterDTO.StartDate.Value);
            }

            if (incidentsFilterDTO.EndDate.HasValue)
            {
                queryable = queryable.Where(i => i.EndDate <= incidentsFilterDTO.EndDate.Value);
            }

            if (incidentsFilterDTO.StaffTypeId.HasValue)
            {
                queryable = queryable.Where(i => i.StaffTypeId == incidentsFilterDTO.StaffTypeId.Value);
            }

            var incidents = await queryable.Paginar(incidentsFilterDTO.Pagination).ToListAsync();
            var totalResults = await queryable.CountAsync();

            var incidentsDTOs = _mapper.Map<List<IncidentDTO>>(incidents);

            var result = new PaginationResultDTO<IncidentDTO>
            {
                Results = incidentsDTOs,
                TotalResults = totalResults,
            };

            return result;
        }

        // GET: api/incidents/{id}
        [HttpGet("{id:int}", Name = "GetIncidentById")]
        public async Task<ActionResult<IncidentDTO>> GetById(int id)
        {
            var incident = await _context.Incidents
                .Include(i => i.StaffType)  // Incluir la relación StaffType
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
            {
                return NotFound($"No existe una incidencia con el ID: {id}");
            }

            var incidentDTO = _mapper.Map<IncidentDTO>(incident);
            return incidentDTO;
        }

        // POST: api/incidents
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] IncidentCreateDTO incidentCreateDTO)
        {
            var incident = _mapper.Map<Incident>(incidentCreateDTO);

            _context.Add(incident);
            await _context.SaveChangesAsync();

            var incidentDTO = _mapper.Map<IncidentDTO>(incident);

            return CreatedAtRoute("GetIncidentById", new { id = incidentDTO.Id }, incidentDTO);
        }

        // PUT: api/incidents/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] IncidentDTO incidentDTO)
        {
            if (id != incidentDTO.Id)
            {
                return BadRequest("El ID de la incidencia no coincide con el ID de la URL.");
            }

            if (string.IsNullOrEmpty(incidentDTO.Name))
            {
                return BadRequest("El campo 'Name' es requerido.");
            }

            var incident = await _context.Incidents.FindAsync(id);

            if (incident == null)
            {
                return NotFound($"No existe una incidencia con el ID: {id}");
            }

            // Mapear los valores del DTO a la entidad
            incident.Name = incidentDTO.Name;
            incident.Color = incidentDTO.Color;
            incident.TimeMin = incidentDTO.TimeMin;
            incident.TimeMax = incidentDTO.TimeMax;
            incident.IsEntry = incidentDTO.IsEntry;
            incident.IsBeforeCheckPoint = incidentDTO.IsBeforeCheckPoint;
            incident.Description = incidentDTO.Description;
            incident.StartDate = incidentDTO.StartDate;
            incident.EndDate = incidentDTO.EndDate;
            incident.StaffTypeId = incidentDTO.StaffTypeId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Manejar la excepción de la base de datos
                return StatusCode(500, "Error al actualizar la base de datos. Detalles: " + ex.Message);
            }

            var updatedIncidentDTO = _mapper.Map<IncidentDTO>(incident);
            return Ok(updatedIncidentDTO);
        }
    }
}
