namespace TecNMEmployeesAPI.DTOs
{
    public class PeriodDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
        public Boolean IsInterSemester { get; set; }
        public Boolean IsCurrent { get; set; }

    }
}
