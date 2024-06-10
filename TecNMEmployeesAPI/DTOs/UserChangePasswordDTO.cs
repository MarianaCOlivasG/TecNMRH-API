using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class UserChangePasswordDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
