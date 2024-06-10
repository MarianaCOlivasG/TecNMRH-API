namespace TecNMEmployeesAPI.DTOs
{
    public class WorkPermitDTO
    {

        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
        public int Type { get; set; }
        public string Observation { get; set; }
        public PermitDTO Permit { get; set; }
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public WorkScheduleDTO WorkSchedule { get; set; }
        public bool IsActive { get; set; }

    }
}
