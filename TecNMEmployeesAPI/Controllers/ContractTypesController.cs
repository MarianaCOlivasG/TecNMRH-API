
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Entities;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/contracttypes")]
    public class ContractTypesController : ControllerBase
    {

        private readonly ApplicationDbContext Context;
        private readonly IMapper Mapper;

        public ContractTypesController(ApplicationDbContext context, IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }




        [HttpGet]
        public async Task<ActionResult<List<ContractTypeDTO>>> Get()
        {
            var contractTypes = await Context.ContractTypes.ToListAsync();

            var contractTypesDTOs = Mapper.Map<List<ContractTypeDTO>>(contractTypes);

            return contractTypesDTOs;
        }








        [HttpGet("{id:int}", Name = "GetContractTypeById")]
        public async Task<ActionResult<ContractTypeDTO>> GetById(int id)
        {
            var contractType = await Context.ContractTypes.FirstOrDefaultAsync(s => s.Id == id);

            if (contractType == null)
            {
                return NotFound($"No existe un tipo de contrato con el ID: {id}");
            }

            var contractTypeDTO = Mapper.Map<ContractTypeDTO>(contractType);

            return contractTypeDTO;
        }




        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ContractTypeCreateDTO contractTypeCreateDTO)
        {

            var contractType = Mapper.Map<ContractType>(contractTypeCreateDTO);

            Context.Add(contractType);
            await Context.SaveChangesAsync();


            var contractTypeDTO = Mapper.Map<ContractTypeDTO>(contractType);

            return CreatedAtRoute("GetContractTypeById", new { id = contractTypeDTO.Id }, contractTypeDTO);

        }






        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] ContractTypeCreateDTO contractTypeCreateDTO)
        {

            var contractType = Mapper.Map<ContractType>(contractTypeCreateDTO);

            contractType.Id = id;

            Context.Entry(contractType).State = EntityState.Modified;

            await Context.SaveChangesAsync();


            return NoContent();
        }







    }
}
