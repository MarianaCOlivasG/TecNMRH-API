namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceLogDTO
    {

        public int Id { get; set; }
        public int StationId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }


    }
}
