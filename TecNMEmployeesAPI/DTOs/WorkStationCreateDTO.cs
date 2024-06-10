using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class WorkStationCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Name { get; set; }
    }
}
