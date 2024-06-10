using System.ComponentModel.DataAnnotations;

namespace TecNMEmployeesAPI.Validations
{
    public class FileTypeValidation: ValidationAttribute
    {

        private readonly string[] ValidTypes;

        public FileTypeValidation(string[] validTypes)
        {
            ValidTypes = validTypes;
        }


        public FileTypeValidation(FileTypeGroup fileTypeGroup)
        {
            if ( fileTypeGroup == FileTypeGroup.Image)
            {
                ValidTypes = new string[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            }
        }



        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            if (value == null)
            {
                // No hay algo que validar
                return ValidationResult.Success;
            }

            // Transformar el valor a IFromFile
            IFormFile formFile = value as IFormFile;

            // Si no se puede trasformar
            if (formFile == null)
            {
                return ValidationResult.Success;
            }


            if( !ValidTypes.Contains(formFile.ContentType)) {
                return new ValidationResult($"Tipo de archivo inválido. Solo se aceptan: { string.Join(",", ValidTypes) }");
            }


            return ValidationResult.Success;

        }



    }
}
