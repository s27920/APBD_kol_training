using kolWebApp.Dtos;
using kolWebApp.Exceptions;
using kolWebApp.Services;
using Microsoft.Data.SqlClient;

namespace kolWebApp.Repositories;
using kolWebApp.MOdels;

public interface IPrescriptionRepository
{
    public Task<IEnumerable<PrescriptionReadDto>> GetAllPrescriptionsAsync(int? doctorId);
    public Task<bool> CheckIfDoctorExistsAsync(int doctorId);
    public Task<bool> CheckIfPatientExistsAsync(int patientId);
    public Task<Prescription> InsertPrescription(PrescriptionDto dto);

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
    public async Task<IEnumerable<PrescriptionReadDto>> GetAllPrescriptionsAsync(int? doctorId)
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

        var prescriptionsRead = new List<PrescriptionReadDto>();

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
            prescriptionsRead.Add(new PrescriptionReadDto{IdPrescription = prescriptionId,Date = date,DueDate = dueDate,PatientName = patientName,DoctorName = doctorName});
        }

        return prescriptionsRead;
    }

    public async Task<bool> CheckIfDoctorExistsAsync(int doctorId)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        var query = "SELECT * FROM Doctor WHERE @IdDoctor = IdDoctor";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdDoctor", doctorId);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> CheckIfPatientExistsAsync(int patientId)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        var query = "SELECT * FROM Patient WHERE @IdPatient = IdDoctor";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdPatient", patientId);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<Prescription> InsertPrescription(PrescriptionDto dto)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionString:DefaultConnection"]);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var query = "INSERT INTO Prescription(Date, DueDate, IdPatient, IdDoctor) OUTPUT Inserted.IdPrescription VALUES @Date, @DueDate, @IdPatient, @IdDoctor";
            await using var command = new SqlCommand(query, connection);
            command.Transaction = (SqlTransaction)transaction;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Date", dto.Date);
            command.Parameters.AddWithValue("@DueDate", dto.DueDate);
            command.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
            command.Parameters.AddWithValue("@IdDoctor", dto.IdDoctor);
            var PrescId = (int) await command.ExecuteScalarAsync();
            await transaction.CommitAsync();
            return new Prescription{IdPrescription = PrescId, Date = dto.Date, DueDate = dto.DueDate, PatientId = dto.IdPatient, DoctorId = dto.IdDoctor};
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new ConflictException("Transaction failed. Rollback initiated");
        }
    }
}