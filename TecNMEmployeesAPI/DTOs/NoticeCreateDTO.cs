using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class NoticeCreateDTO
    {


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Title { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(280)]
        public string Description { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime FinalDate { get; set; }
        public Boolean IsActive { get; set; }



        public List<int> EmployeesIds { get; set; }

        public int DepartmentId { get; set; }
        public int StaffTypeId { get; set; }
        public int WorkStationId { get; set; }


        public string Entity { get; set; }
        public int EntityId { get; set; }



    }
}
