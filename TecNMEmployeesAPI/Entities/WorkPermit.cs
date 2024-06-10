using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class WorkPermit
    {

        public int Id { get; set; }
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
        public bool IsActive { get; set; }


        // Propiedades de Navegación
        public Permit Permit { get; set; }
        public Employee Employee { get; set; }
        public WorkSchedule WorkSchedule { get; set; }

    }
}
