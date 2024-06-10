using Microsoft.EntityFrameworkCore;

namespace TecNMEmployeesAPI.Helpers
{
    public static class HttpContextExtensions
    {

        public async static Task InsertPaginationParamsInHeaders<T>(this HttpContext httpContext,
            IQueryable<T> queryable, int limit)
        {

            double totalResults = await queryable.CountAsync();
            double totalPages = Math.Ceiling(totalResults / limit);

            httpContext.Response.Headers.Add("totalResults", totalResults.ToString());
            httpContext.Response.Headers.Add("totalPages", totalPages.ToString());


        }

    }
}
