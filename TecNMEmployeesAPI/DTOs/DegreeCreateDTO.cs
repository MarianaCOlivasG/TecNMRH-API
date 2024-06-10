using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class DegreeCreateDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(10)]
        public string Acronym { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(50)]
        public string Name { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(100)]
        public string Specialty { get; set; }
    }
}
