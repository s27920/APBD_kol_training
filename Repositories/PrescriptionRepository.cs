using kolWebApp.Exceptions;
using kolWebApp.Services;
using Microsoft.Data.SqlClient;

namespace kolWebApp.Repositories;
using kolWebApp.MOdels;

public interface IPrescriptionRepository
{
    public Task<IEnumerable<Prescription>> GetAllPrescriptions(int? doctorId);

}

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly IConfiguration _configuration;

    public PrescriptionRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    
    /*tutaj pojawia się pewien dylemat ponieważ w aktualnej wersji poniższa funkcja łamie poniekąd zasdę 1 zadania
    z drugiej strony aktualna implementacja pozwala na zaoszczędzeniu przestrzeni i unika łamania DRY 
    czego skutkiem byłaby druga funckja robiąca to samo dla konkretnego lekarza*/
    public async Task<IEnumerable<Prescription>> GetAllPrescriptions(int? doctorId)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        var query =
            "SELECT IdPrescription, Date, DueDate, Patient.LastName, Doctor.LastName FROM Prescription " +
            "INNER JOIN Doctor ON Doctor.IdDoctor = Prescription.IdDoctor " +
            "INNER JOIN Patient ON Patient.IdPatient = Doctor.IdPatient";
        if (doctorId != null)
        {
            query += " WHERE @IdDoctor = IdDoctor";
        }
        query += ";";
        await using var command = new SqlCommand(query, connection);
        if (doctorId != null)
        {
            command.Parameters.AddWithValue("@IdDoctor", doctorId);
        }
        var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            throw new NotFoundException("No Prescriptions Found");
        }

        var prescriptionsRead = new List<Prescription>();

        var prescriptionOrdinal = reader.GetOrdinal("IdPrescription");
        var dateOrdinal = reader.GetOrdinal("Date");
        var dueDateOrdinal = reader.GetOrdinal("DueDate");
        var lastNameOrdinal = reader.GetOrdinal("Patient.LastName");
        var doctorNameOrdinal = reader.GetOrdinal("Doctor.LastName");

        while (await reader.ReadAsync())
        {
            int prescriptionId = reader.GetInt32(prescriptionOrdinal);
            DateTime date = reader.GetDateTime(dateOrdinal);
            DateTime dueDate = reader.GetDateTime(dueDateOrdinal);
            String? patientName = reader.GetString(lastNameOrdinal);
            String? doctorName = reader.GetString(doctorNameOrdinal);
            prescriptionsRead.Add(new Prescription{IdPrescription = prescriptionId,Date = date,DueDate = dueDate,PatientName = patientName,DoctorName = doctorName});
        }

        return prescriptionsRead;
    }
    
}