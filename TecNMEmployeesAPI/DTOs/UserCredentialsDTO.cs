using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class UserCredentialsDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }


    }
}
