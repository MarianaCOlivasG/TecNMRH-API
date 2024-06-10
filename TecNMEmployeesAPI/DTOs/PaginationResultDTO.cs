namespace TecNMEmployeesAPI.DTOs
{
    public class PaginationResultDTO<T>
    {

        public List<T> Results { get; set; }

        public int TotalResults { get; set; }
        public int TotalPages { get; set; }

    }

}
