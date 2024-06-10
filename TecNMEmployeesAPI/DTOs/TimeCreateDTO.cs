using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class TimeCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan OutputMax { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan OutputMin { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan InputMax { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan InputMin { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StaffTypeId { get; set; }

        // [Required(ErrorMessage = "El campo {0} es requerido.")]
        // public int PeriodId { get; set; }
    

    }
}
