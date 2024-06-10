using System.ComponentModel.DataAnnotations;
using TecNMEmployeesAPI.Validations;

namespace TecNMEmployeesAPI.DTOs
{
    public class UploadPictureDTO
    {


        //[FileTypeValidation(validTypes: new string[] { "image/jpeg", "image/jpg", "image/png", "image/gif" })]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
        [FileSizeValidation(sizeMaxMB: 4)]
        public IFormFile Picture { get; set; }


    }
}
