using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class WorkPermitCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int Type { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Observation { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int PermitId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
        public int? WorkScheduleId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public bool IsActive { get; set; }
    }
}
