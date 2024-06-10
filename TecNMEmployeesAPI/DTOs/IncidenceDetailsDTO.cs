namespace TecNMEmployeesAPI.DTOs
{
    public class IncidenceDetailsDTO
    {
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public DateTime Date { get; set; }
        public AttendanceDTO Attendance { get; set; }
        public TimeSpan Check { get; set; }
        public int Type { get; set; }
        /*
            1 - Sin incidencia Entrada
            2 - Entrada Previa
            3 - Retardo A
            4 - Retardo B
            5 - Entrada Tardia
            6 - Omisión de Entrada

            7 - Falta 

            8 - Sin incidencia Salida
            9 - Salida Previa
            10 - Omisión de Salida
            11 - Salida Tardia
         */
        public WorkPermitDTO Permit { get; set; }

        public string Descriptions { get; set; }
        public List<TimeSpan> Checks { get; set; }
    }
}
