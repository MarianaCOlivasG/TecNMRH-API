namespace TecNMEmployeesAPI.DTOs
{
    public class GeneralNoticeDTO
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
        public Boolean isActive { get; set; }



    }
}
