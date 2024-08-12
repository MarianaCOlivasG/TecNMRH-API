namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceDTO
    {

        public int Id { get; set; }
        public StationDTO Station { get; set; }
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }


    }
}
