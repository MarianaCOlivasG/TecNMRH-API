using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class DepartmentCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Name { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(180)]
        public string Ubication { get; set; }


    }
}
