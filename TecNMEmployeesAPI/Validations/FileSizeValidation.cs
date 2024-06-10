using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Validations
{
    public class FileSizeValidation: ValidationAttribute
    {

        private readonly int SizeMaxMB;
        public FileSizeValidation(int sizeMaxMB)
        {
            SizeMaxMB = sizeMaxMB;
        }



        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if ( value == null)
            {
                // No hay algo que validar
                return ValidationResult.Success;
            }

            // Transformar el valor a IFromFile
            IFormFile formFile = value as IFormFile;

            // Si no se puede trasformar
            if ( formFile == null)
            {
                return ValidationResult.Success;
            }

            // El peso esta en bytes
            // Multiplicarlo para convertirlo a bytes
            if ( formFile.Length > SizeMaxMB * 1024 * 1024)
            {
                return new ValidationResult($"El peso del archivo no debe ser mayor a {SizeMaxMB} MB");
            }


            return ValidationResult.Success;

        }

    }
}
