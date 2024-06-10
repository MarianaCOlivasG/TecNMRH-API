namespace TecNMEmployeesAPI.DTOs
{
    public class TimeDTO
    {
        public int Id { get; set; }
        public TimeSpan OutputMax { get; set; }
        public TimeSpan OutputMin { get; set; }
        public TimeSpan InputMax { get; set; }
        public TimeSpan InputMin { get; set; }
        public StaffTypeDTO StaffType { get; set; }
        //public PeriodDTO Period { get; set; }
    }
}
