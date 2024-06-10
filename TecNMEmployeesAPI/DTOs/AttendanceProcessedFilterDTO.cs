﻿using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class AttendanceProcessedFilterDTO
    {

        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public PaginationDTO Pagination
        {
            get { return new PaginationDTO() { Page = Page, Limit = Limit }; }
        }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FinalDate { get; set; }

    }
}
