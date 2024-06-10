namespace TecNMEmployeesAPI.Entities
{
    public class NoticeEmployee
    {


        public int NoticeId { get; set; }
        public int EmployeeId { get; set; }

        // Propiedades de navegación
        public Notice Notice { get; set; }
        public Employee Employee { get; set; }



    }
}
