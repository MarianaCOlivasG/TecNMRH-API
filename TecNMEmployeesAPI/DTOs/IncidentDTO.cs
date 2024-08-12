using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.DTOs
{
    public class IncidentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public TimeSpan TimeMin { get; set; }
        public TimeSpan TimeMax { get; set; }
        public bool IsEntry { get; set; }
        public bool IsBeforeCheckPoint { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StaffTypeId { get; set; }
        public string StaffTypeName { get; set; }  // Nuevo campo para el nombre del tipo de personal
    }

    public class IncidentCreateDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(7, ErrorMessage = "El color debe ser un código hexadecimal.")]
        public string Color { get; set; } // Almacenar el color en formato hexadecimal

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan TimeMin { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public TimeSpan TimeMax { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public bool IsEntry { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public bool IsBeforeCheckPoint { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StaffTypeId { get; set; }
    }

    public class IncidentFilterDTO
    {
        public string Name { get; set; }
        public bool? IsEntry { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? StaffTypeId { get; set; }
        public PaginationDTO Pagination { get; set; }
    }
}
