using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.Helpers;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/generalnotices")]
    public class GeneralNoticesController: ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public GeneralNoticesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }






        [HttpGet("all")]
        public async Task<ActionResult<List<GeneralNoticeDTO>>> Get()
        {
            var generalNotices = await Context.GeneralNotices.ToListAsync();

            var generalNoticesDTOs = Mapper.Map<List<GeneralNoticeDTO>>(generalNotices);

            return generalNoticesDTOs;
        }




        [HttpGet]
        public async Task<ActionResult<PaginationResultDTO<GeneralNoticeDTO>>> GetAll([FromQuery] PaginationDTO paginationDto)
        {

            var queryable = Context.GeneralNotices.AsQueryable();

            var notices = await queryable.Paginar(paginationDto)
                                    .ToListAsync();

            var totalResults = await queryable.CountAsync();

            var noticesDTOs = Mapper.Map<List<GeneralNoticeDTO>>(notices);

            var result = new PaginationResultDTO<GeneralNoticeDTO>
            {
                Results = noticesDTOs,
                TotalResults = totalResults,
            };

            return result;
        }





        [HttpGet("active/{id:int}")]
        public async Task<ActionResult<GeneralNoticeDTO>> ChangeActive(int id)
        {
            var notice = await Context.GeneralNotices.FirstOrDefaultAsync(n => n.Id == id);

            if (notice == null)
            {
                return NotFound($"No existe un mensaje con el ID: {id}");
            }

            notice.IsActive = notice.IsActive ? false : true;
            Context.Entry(notice).State = EntityState.Modified;
            await Context.SaveChangesAsync();

            return NoContent();
        }




        [HttpGet("{id:int}", Name = "GetGeneralNoticeById")]
        public async Task<ActionResult<GeneralNoticeDTO>> GetById(int id)
        {
            var generalNotice = await Context.GeneralNotices.FirstOrDefaultAsync(g => g.Id == id);

            if (generalNotice == null)
            {
                return NotFound($"No existe un mensaje general con el ID: {id}");
            }

            var generalNoticeeDTO = Mapper.Map<GeneralNoticeDTO>(generalNotice);

            return generalNoticeeDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] GeneralNoticeCreateDTO generalNoticeCreateDTO)
        {

            var generalNotice = Mapper.Map<GeneralNotice>(generalNoticeCreateDTO);

            Context.Add(generalNotice);
            await Context.SaveChangesAsync();


            var generalNoticeDTO = Mapper.Map<GeneralNoticeDTO>(generalNotice);

            return CreatedAtRoute("GetGeneralNoticeById", new { id = generalNoticeDTO.Id }, generalNoticeDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] GeneralNoticeCreateDTO generalNoticeCreateDTO)
        {

            var generalNotice = Mapper.Map<GeneralNotice>(generalNoticeCreateDTO);

            generalNotice.Id = id;

            Context.Entry(generalNotice).State = EntityState.Modified;

            await Context.SaveChangesAsync();


            return NoContent();
        }




    }
}
