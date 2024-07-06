

using TecNMEmployeesAPI.DTOs;

namespace TecNMEmployeesAPI.Helpers
{
    public static class QueryableExtensions
    {


        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginationDTO paginationDto)
        {
            if (paginationDto == null)
            {
                paginationDto = new PaginationDTO();
            }
            return queryable.Skip((paginationDto.Page - 1) * paginationDto.Limit)
                    .Take(paginationDto.Limit);
        }


    }
}
