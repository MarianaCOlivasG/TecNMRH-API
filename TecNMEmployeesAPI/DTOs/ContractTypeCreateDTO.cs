using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class ContractTypeCreateDTO
    {

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Name { get; set; }


    }
}
