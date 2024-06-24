using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceLogCreateDTO
    {

        public int StationId { get; set; }
        
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

    }
}
