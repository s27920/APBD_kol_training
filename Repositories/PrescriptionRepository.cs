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
    public Task<bool> InsertDoctor(DoctorDto dto);

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
            "SELECT IdPrescription, Date, DueDate, Patient.LastName AS PatientLastName, Doctor.LastName AS DoctorLastName FROM Prescription " +
            "INNER JOIN Doctor ON Doctor.IdDoctor = Prescription.IdDoctor " +
            "INNER JOIN Patient ON Patient.IdPatient = Prescription.IdPatient " +
            "ORDER BY Date DESC";
        if (doctorId != null)
        {
            query += " WHERE Doctor.IdDoctor = @IdDoctor";
        }
        query += ";";
        await using var command = new SqlCommand(query, connection);
        if (doctorId != null)
        {
            command.Parameters.AddWithValue("@IdDoctor", doctorId);
        }
        await using var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            throw new NotFoundException("No Prescriptions Found");
        }

        var prescriptionsRead = new List<PrescriptionReadDto>();
        var prescriptionOrdinal = reader.GetOrdinal("IdPrescription");
        var dateOrdinal = reader.GetOrdinal("Date");
        var dueDateOrdinal = reader.GetOrdinal("DueDate");
        var patientNameOrdinal = reader.GetOrdinal("PatientLastName");
        var doctorNameOrdinal = reader.GetOrdinal("DoctorLastName");

        while (await reader.ReadAsync())
        {
            int prescriptionId = reader.GetInt32(prescriptionOrdinal);
            DateTime date = reader.GetDateTime(dateOrdinal);
            DateTime dueDate = reader.GetDateTime(dueDateOrdinal);
            string patientName = reader.GetString(patientNameOrdinal);
            string doctorName = reader.GetString(doctorNameOrdinal);
            prescriptionsRead.Add(new PrescriptionReadDto { IdPrescription = prescriptionId, Date = date, DueDate = dueDate, PatientName = patientName, DoctorName = doctorName });
        }

        return prescriptionsRead;
    }


    public async Task<bool> CheckIfDoctorExistsAsync(int doctorId)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        var query = "SELECT COUNT(1) FROM Doctor WHERE IdDoctor = @IdDoctor";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdDoctor", doctorId);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }


    public async Task<bool> CheckIfPatientExistsAsync(int patientId)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        var query = "SELECT COUNT(1) FROM Patient WHERE @IdPatient = IdDoctor";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdPatient", patientId);
        var rowsAffected = await command.ExecuteReaderAsync();
        return Convert.ToInt32(rowsAffected) > 0;
    }

    public async Task<Prescription> InsertPrescription(PrescriptionDto dto)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionString:DefaultConnection"]);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var query = "INSERT INTO Prescription (Date, DueDate, IdPatient, IdDoctor) OUTPUT INSERTED.IdPrescription VALUES (@Date, @DueDate, @IdPatient, @IdDoctor)";
            await using var command = new SqlCommand(query, connection);
            command.Transaction = (SqlTransaction)transaction;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Date", dto.Date);
            command.Parameters.AddWithValue("@DueDate", dto.DueDate);
            command.Parameters.AddWithValue("@IdPatient", dto.IdPatient);
            command.Parameters.AddWithValue("@IdDoctor", dto.IdDoctor);
            var prescId = (int) await command.ExecuteScalarAsync();
            await transaction.CommitAsync();
            return new Prescription{IdPrescription = prescId, Date = dto.Date, DueDate = dto.DueDate, PatientId = dto.IdPatient, DoctorId = dto.IdDoctor};
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new ConflictException("Transaction failed. Rollback initiated");
        }
    }

    public async Task<bool> InsertDoctor(DoctorDto dto)
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var query = "INSERT INTO Doctor(FirstName, LastName, Email) OUTPUT INSERTED.IdDoctor VALUES (@FirstName, @LastName, @Email);";
            await using var command = new SqlCommand(query, connection);
            command.Transaction = (SqlTransaction)transaction;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@FirstName", dto.FirstName);
            command.Parameters.AddWithValue("@LastName", dto.LastName);
            command.Parameters.AddWithValue("@Email", dto.email);
            int insertedId = Convert.ToInt32(await command.ExecuteScalarAsync());
            await transaction.CommitAsync();
            Console.WriteLine(insertedId);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new ConflictException("transaction interrupted, rollback initiated");
        }
         
    }
}