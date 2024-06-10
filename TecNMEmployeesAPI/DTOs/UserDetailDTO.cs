namespace TecNMEmployeesAPI.DTOs
{
    public class UserDetailDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<RoleDetailDTO> Roles { get; set; }
    }
}
