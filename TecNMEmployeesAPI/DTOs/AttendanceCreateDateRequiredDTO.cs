using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceCreateDateRequiredDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StationId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan Time { get; set; }


    }
}
