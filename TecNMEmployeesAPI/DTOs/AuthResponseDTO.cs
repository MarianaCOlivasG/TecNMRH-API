namespace TecNMEmployeesAPI.DTOs
{
    public class AuthResponseDTO
    {

        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        public UserDetailDTO User { get; set; }
    }
}
