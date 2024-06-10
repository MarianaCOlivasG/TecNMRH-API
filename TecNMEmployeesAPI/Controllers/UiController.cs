using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.Entities;
using TecNMEmployeesAPI.DTOs;

namespace TecNMEmployeesAPI.Controllers
{

    [ApiController]
    [Route("api/ui")]
    public class UiController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public UiController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<UiDTO>> Get()
        {

            var ui = await Context.UIs.FirstOrDefaultAsync(ui => ui.Id == 1);

            var uiDTO = Mapper.Map<UiDTO>(ui);

            return uiDTO;
        }



        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] UiUpdateDTO uiUpdateDTO)
        {


            var ui = await Context.UIs.FirstOrDefaultAsync(ui => ui.Id == id);

            if (ui == null)
            {
                return NotFound($"No existe un theme con el ID {id}");
            }


            ui = Mapper.Map(uiUpdateDTO, ui);

            await Context.SaveChangesAsync();

            return NoContent();
        }


    }
}
