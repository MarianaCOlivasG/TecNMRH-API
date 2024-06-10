namespace TecNMEmployeesAPI.DTOs
{
    public class WorkScheduleDTO
    {


        public int Id { get; set; }
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public PeriodDTO Period { get; set; }
       // public int TotalHours { get; set; }
        public TimeSpan MondayCheckIn { get; set; }
        public TimeSpan MondayCheckOut { get; set; }
        public TimeSpan TuesdayCheckIn { get; set; }
        public TimeSpan TuesdayCheckOut { get; set; }
        public TimeSpan WednesdayCheckIn { get; set; }
        public TimeSpan WednesdayCheckOut { get; set; }
        public TimeSpan ThursdayCheckIn { get; set; }
        public TimeSpan ThursdayCheckOut { get; set; }
        public TimeSpan FridayCheckIn { get; set; }
        public TimeSpan FridayCheckOut { get; set; }
        public TimeSpan SaturdayCheckIn { get; set; }
        public TimeSpan SaturdayCheckOut { get; set; }
        public TimeSpan SundayCheckIn { get; set; }
        public TimeSpan SundayCheckOut { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
        public int TotalHours { get; set; }


    }
}
