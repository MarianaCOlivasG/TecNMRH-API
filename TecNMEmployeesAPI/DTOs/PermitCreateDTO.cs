using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class PermitCreateDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public Boolean RequiredAttendance { get; set; }

    }
}
