using System.ComponentModel.DataAnnotations;


namespace TecNMEmployeesAPI.Entities
{
    public class ContractType
    {


        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Name { get; set; }


    }
}
