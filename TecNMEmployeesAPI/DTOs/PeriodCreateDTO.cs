using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class PeriodCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Name { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public Boolean IsInterSemester { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public Boolean IsCurrent { get; set; }


    }
}
