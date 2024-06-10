namespace TecNMEmployeesAPI.DTOs
{
    public class NoticeDTO
    {

        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }
        public List<EmployeeWithoutDetailsDTO> Employees { get; set; }
        public Boolean IsActive { get; set; }

        public string Entity { get; set; }
        public int EntityId { get; set; }


    }
}
