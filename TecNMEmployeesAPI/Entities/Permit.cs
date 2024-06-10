using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class Permit
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public Boolean RequiredAttendance { get; set; }
    }
}
