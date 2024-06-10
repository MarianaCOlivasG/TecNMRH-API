using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class GeneralNotice
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Title { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(280)]
        public string Description { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
        public Boolean IsActive { get; set; }



    }
}
