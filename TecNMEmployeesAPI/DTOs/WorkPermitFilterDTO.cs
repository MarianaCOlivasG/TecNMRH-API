namespace TecNMEmployeesAPI.DTOs
{
    public class WorkPermitFilterDTO
    {

        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public PaginationDTO Pagination
        {
            get { return new PaginationDTO() { Page = Page, Limit = Limit }; }
        }

        public int EmployeeId { get; set; }


    }
}
