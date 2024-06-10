namespace TecNMEmployeesAPI.DTOs
{
    public class IncidenceTestDTO
    {
        public EmployeeWithoutDetailsDTO Employee { get; set; }
        public DateTime Date { get; set; }
        public List<AttendanceDTO> Attendances { get; set; }
        public List<TimeSpan> Checks { get; set; }
        public List<int> Types { get; set; }
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
        public List<AttendanceDTO> AttendancesAll { get; set; }
        public List<string> Descriptions { get; set; }
    }
}
