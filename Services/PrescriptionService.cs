using kolWebApp.Dtos;
using kolWebApp.Exceptions;
using kolWebApp.Repositories;

namespace kolWebApp.Services;
using MOdels;

public interface IPrescriptionService
{
    public Task<IEnumerable<PrescriptionReadDto>> GetAllPrescriptionAsync();
    public Task<IEnumerable<PrescriptionReadDto>> GetDoctorsPrescriptionsAsync(int id);
    public Task<Prescription> InsertPrescription(PrescriptionDto dto);
    public Task<bool> InsertDoctor(DoctorDto dto);
}

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _repository;

    public PrescriptionService(IPrescriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PrescriptionReadDto>> GetAllPrescriptionAsync()
    {
        return await _repository.GetAllPrescriptionsAsync(null);
    }

    public async Task<IEnumerable<PrescriptionReadDto>> GetDoctorsPrescriptionsAsync(int id)
    {
       if (! await _repository.CheckIfDoctorExistsAsync(id))
       {
           throw new NotFoundException("No doctor with such Id found");
       }
        return await _repository.GetAllPrescriptionsAsync(id);
    }

    public async Task<Prescription> InsertPrescription(PrescriptionDto dto)
    {
        if (dto.DueDate > dto.Date)
        {
            throw new ConflictException("Incorrect date formats entered");
        }
        if (!await _repository.CheckIfDoctorExistsAsync(dto.IdDoctor))
        {
            throw new NotFoundException("No doctor with such Id found");
        }
        if (!await _repository.CheckIfPatientExistsAsync(dto.IdPatient))
        {
            throw new NotFoundException("No patient with such Id found");
        }

        return await _repository.InsertPrescription(dto);

    }

    public async Task<bool> InsertDoctor(DoctorDto dto)
    {
        return await _repository.InsertDoctor(dto);
    }
}