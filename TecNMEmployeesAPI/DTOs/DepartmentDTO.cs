namespace TecNMEmployeesAPI.DTOs
{
    public class DepartmentDTO
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public string Ubication { get; set; }
     
        public EmployeeWithoutDetailsDTO Head { get; set; }

    }
}
