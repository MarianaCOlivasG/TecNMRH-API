namespace TecNMEmployeesAPI.DTOs
{
    public class NonWorkingDayDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
    }
}
