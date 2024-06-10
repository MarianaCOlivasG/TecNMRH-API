namespace TecNMEmployeesAPI.DTOs
{
    public class PaginationDTO
    {


        public int Page { get; set; } = 1;
        private int limit = 10;
        private readonly int LimitMax = 20;

        public int Limit
        {
            get { return limit; }
            set { limit = (value) > LimitMax ? Limit : value; }
        }


    }
}
