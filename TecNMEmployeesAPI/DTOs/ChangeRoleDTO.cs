using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class ChangeRoleDTO
    {
            [Required]
            public string UserName { get; set; }

            [Required]
            // TODO: Validar que sea ADMIN o USER en mayuscula
            public string Role { get; set; }
    }
}
