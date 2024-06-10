namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceProcessed
    {
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Incidence { get; set; }
    }
}
