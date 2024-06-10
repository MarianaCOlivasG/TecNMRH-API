using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StationId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
     
        public DateTime Date { get; set; }
        
        public TimeSpan Time { get; set; }


    }
}
