namespace TecNMEmployeesAPI.DTOs
{
    public class EmployeeForDepartmentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public string Picture { get; set; }
        public string CardCode { get; set; }

        public WorkStationDTO WorkStation { get; set; }
    }
}
