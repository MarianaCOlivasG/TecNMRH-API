namespace TecNMEmployeesAPI.Services
{
    public class LocalFileStorage : IFileStorage
    {

        private readonly IWebHostEnvironment Env;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public LocalFileStorage( IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            Env = env;
            HttpContextAccessor = httpContextAccessor;
        }


        public Task DeleteFile(string fileCurrentlName, string container)
        {
            if (fileCurrentlName != null )
            {

                var currentPath = $"{HttpContextAccessor.HttpContext.Request.Scheme}://{HttpContextAccessor.HttpContext.Request.Host}";

                var fullPath = Path.Combine(currentPath, container, fileCurrentlName);
                    //.Replace("\\", "/");

                var fileName = Path.GetFileName(fullPath);
                string filePath = Path.Combine(Env.WebRootPath, container, fileName);

                // Si existe el archivo en el directorio lo borramos 
                if ( File.Exists(filePath) )
                {
                    File.Delete(filePath);
                }

            }

            return Task.FromResult(0);
        }




        public async Task<string> Save(byte[] content, string extension, string container, string contentType)
        {
            // Crear el nombre del archivo
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Creamos el nombre de la carpeta donde se va a guardar
            string folder = Path.Combine(Env.WebRootPath, container);

            // Si no existe la carpeta la creamos
            if ( !Directory.Exists(folder) )
            {
                Directory.CreateDirectory(folder);
            }

            // Todo el path de la imagen
            string path = Path.Combine(folder, fileName);

            // Crear la foto, guardar la foto
            await File.WriteAllBytesAsync(path, content);


            //var currentPath = $"{HttpContextAccessor.HttpContext.Request.Scheme}://{HttpContextAccessor.HttpContext.Request.Host}";

            // Si quiero todo el path, Ejemplo: https://localhost:7039/actors/d9c22d95-f852-4e93-a264-f31d7f7a00f7.jpg
            //var pathdb = Path.Combine(currentPath, container, fileName).Replace("\\", "/");

            // Si quiero solo el nombre del archivo
            var pathdb = fileName;

            return pathdb;

        }




        public async Task<string> SaveFile(byte[] content, string extension, string container, string fileCurrentlName, string contentType)
        {


            await DeleteFile(fileCurrentlName, container);

            return await Save(content, extension, container, contentType);


        }



    }
}
