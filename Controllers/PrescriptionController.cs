using kolWebApp.Dtos;
using kolWebApp.Exceptions;
using kolWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolWebApp.Controllers;


[ApiController]
[Route("/api/[Controller]")]
public class PrescriptionController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;

    public PrescriptionController(IPrescriptionService prescriptionService)
    {
        _prescriptionService = prescriptionService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrescriptions()
    {
        return Ok(await _prescriptionService.GetAllPrescriptionAsync());
    }
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPrescriptionsById([FromQuery] int id)
    {
        try
        {
            return Ok(await _prescriptionService.GetDoctorsPrescriptionsAsync(id));
        }
        catch (Exception e)
        {
            return NotFound(e);
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InsertPrescription([FromBody] PrescriptionDto dto)
    {
        try
        {
            return Ok(await _prescriptionService.InsertPrescription(dto));
        }
        catch (NotFoundException e)
        {
            return NotFound(e);
        }
        catch (ConflictException e)
        {
            return Conflict(e);
        }
    }
}