using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class WorkScheduleCreateDTO
    {


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int PeriodId { get; set; }
        //[Required(ErrorMessage = "El campo {0} es requerido.")]
        //public int TotalHours { get; set; }
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
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
        public int TotalHours { get; set; }

    }
}
