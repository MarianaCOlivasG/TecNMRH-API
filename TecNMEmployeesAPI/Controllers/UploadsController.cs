using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TecNMEmployeesAPI.DTOs;
using TecNMEmployeesAPI.Services;

namespace TecNMEmployeesAPI.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    public class UploadsController : ControllerBase
    {


        private readonly ApplicationDbContext Context;
        private readonly IFileStorage FileStorage;
        private readonly IMapper Mapper;

        private readonly string Container = "employees";

        public UploadsController(ApplicationDbContext context, IFileStorage fileStorage, IMapper mapper)
        {
            Context = context;
            FileStorage = fileStorage;
            Mapper = mapper;
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmployeeWithoutDetailsDTO>> Update(int id, [FromForm] UploadPictureDTO uploadPictureDTO)
        {
            var employee = await Context.Employees.FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound($"No existe un empleado con el ID: {id}");
            }

            if (uploadPictureDTO.Picture != null)
            {

                using (var memoryStream = new MemoryStream())
                {
                    await uploadPictureDTO.Picture.CopyToAsync(memoryStream);

                    var content = memoryStream.ToArray();

                    var extension = Path.GetExtension(uploadPictureDTO.Picture.FileName);

                    employee.Picture = await FileStorage.SaveFile(content, extension, Container, employee.Picture, uploadPictureDTO.Picture.ContentType);
                }


            }


            Context.Entry(employee).State = EntityState.Modified;
            await Context.SaveChangesAsync();


            var employeeDTO = Mapper.Map<EmployeeWithoutDetailsDTO>(employee);

            return employeeDTO;
        }



        [HttpPut("ui/logo")]
        public async Task<ActionResult<String>> UpdateLogo([FromForm] UploadPictureDTO uploadPictureDTO)
        {
            var ui = await Context.UIs.FirstOrDefaultAsync(e => e.Id == 1);
            

            if (uploadPictureDTO.Picture != null)
            {

                using (var memoryStream = new MemoryStream())
                {
                    await uploadPictureDTO.Picture.CopyToAsync(memoryStream);

                    var content = memoryStream.ToArray();

                    var extension = Path.GetExtension(uploadPictureDTO.Picture.FileName);

                    ui.Logo = await FileStorage.SaveFile(content, extension, "assets", ui.Logo, uploadPictureDTO.Picture.ContentType);
                }


            }

            Context.Entry(ui).State = EntityState.Modified;
            await Context.SaveChangesAsync();


            return Ok(
                new {
                    message = "Logo actualizado con éxito",
                    logo = ui.Logo
                }
            );
        }


    }

}
