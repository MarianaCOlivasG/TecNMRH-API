using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class StationCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(30)]
        public string Folio { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Ubication { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(180)]
        public string Description { get; set; }
        //[Required(ErrorMessage = "El campo {0} es requerido.")]
        //[StringLength(16)]
        public string IPAddress { get; set; }

    }
}
