namespace TecNMEmployeesAPI.DTOs
{
    public class EditRoleDTO
    {
        public string UserId { get; set; }
        public List<RoleDTO> Roles { get; set; }
    }

    public class RoleDTO
    {
        public string Role { get; set; }
        public string Value { get; set; }
    }
}
