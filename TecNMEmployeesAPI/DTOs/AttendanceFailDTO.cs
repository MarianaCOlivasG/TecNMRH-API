namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceFailDTO
    {
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public string Message { get; set; }
    }
}
