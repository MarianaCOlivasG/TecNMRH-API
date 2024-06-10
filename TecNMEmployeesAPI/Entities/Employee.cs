using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TecNMEmployeesAPI.Entities
{
    public class Employee
    {

        public int Id { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Name { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        public string Lastname { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(10)]
        public string Gender { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(20)]
        public string RFC { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(20)]
        public string CURP { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(120)]
        [EmailAddress(ErrorMessage = "El campo {0} no es válido.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int ContractTypeId { get; set; }



        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StaffTypeId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int DegreeId { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime RecruitmentDate { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime BirthdayDate { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public string CardCode { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int DepartmentId { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int WorkStationId { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public Boolean IsActive { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int TotalHours { get; set; }


        public string Picture { get; set; }
        [Column(TypeName = "LONGTEXT")]
        public string Finger { get; set; }

        [Column(TypeName = "LONGTEXT")]
        public string AnotherFinger { get; set; }





        // Propiedades de navegación
        public StaffType StaffType { get; set; }
        public ContractType ContractType { get; set; }
        public Degree Degree { get; set; }
        public Department Department { get; set; }
        public WorkStation WorkStation { get; set; }

    }
}
