namespace TecNMEmployeesAPI.Services
{
    public interface IFileStorage
    {


        Task<string> SaveFile(byte[] content, string extension, string container, string fileCurrentlName, string contentType);

        Task DeleteFile(string path, string container);


    }
}
