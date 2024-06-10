using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class WorkStation
    {

        public int Id { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string Name { get; set; }



    }
}
