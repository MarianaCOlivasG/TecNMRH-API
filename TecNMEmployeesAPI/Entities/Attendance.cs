using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecNMEmployeesAPI.Entities
{
    public class Attendance
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StationId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime Date { get; set; }
        [DataType(DataType.Time)]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan Time { get; set; }



        // propiedades de navegación
        public Station Station { get; set; }
        public Employee Employee { get; set; }

    }
}
