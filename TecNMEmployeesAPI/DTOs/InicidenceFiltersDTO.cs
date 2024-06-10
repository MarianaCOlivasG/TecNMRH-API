using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class InicidenceFiltersDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StaffTypeId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
    }
}
