using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class NonWorkingDay
    {
        public int Id { get; set; }
        [StringLength(280)]
        public string Description { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
    }
}
