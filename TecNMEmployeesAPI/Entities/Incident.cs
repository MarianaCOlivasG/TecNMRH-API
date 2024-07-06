using System;
using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Entities
{
    public class Incident
    {
        public int Id { get; set; }

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

        // Relación con el tipo de personal
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int StaffTypeId { get; set; }
        public StaffType StaffType { get; set; }

        // Constructor para inicializar propiedades opcionales si es necesario
        public Incident()
        {
        }
    }
}
