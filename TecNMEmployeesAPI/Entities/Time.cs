using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class Time
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan OutputMax { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan OutputMin { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan InputMax { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan InputMin { get; set; }


        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StaffTypeId { get; set; }

        //[Required(ErrorMessage = "El campo {0} es requerido.")]
        //public int PeriodId { get; set; }


        // Propiedades de navegación
        public StaffType StaffType { get; set; }
        //public Period Period { get; set; }
    }
}
