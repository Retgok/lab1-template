using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[ApiController]
[Route("api/v1/persons")]
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly IPersonRepo _repo;

    public PersonsController(IPersonRepo repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PersonResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(list.Select(p => new PersonResponse(p)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        if (person == null)
            return NotFound(new ErrorResponse($"Person with id={id} not found"));

        return Ok(new PersonResponse(person));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PersonRequest dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new ValidationErrorResponse(
                "Invalid data",
                new Dictionary<string, string> { { "name", "Name is required" } }));
        }

        var person = new Person
        {
            Name = dto.Name,
            Age = dto.Age,
            Address = dto.Address,
            Work = dto.Work
        };

        await _repo.AddAsync(person);
        return CreatedAtAction(nameof(GetById), new { id = person.Id }, null);
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] PersonRequest dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem();

        var person = await _repo.GetByIdAsync(id);
        if (person == null)
            return NotFound(new ErrorResponse($"Person with id={id} not found"));

        if (dto.Name != null && string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new ValidationErrorResponse(
                "Invalid data",
                new Dictionary<string, string> { { "name", "Name cannot be empty" } }));
        }

        if (dto.Name != null) person.Name = dto.Name;
        if (dto.Age.HasValue) person.Age = dto.Age;
        if (dto.Address != null) person.Address = dto.Address;
        if (dto.Work != null) person.Work = dto.Work;

        await _repo.UpdateAsync(person);
        return Ok(new PersonResponse(person));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        if (person == null)
            return NotFound(new ErrorResponse($"Person with id={id} not found"));

        await _repo.DeleteAsync(person);
        return NoContent();
    }

    public override ActionResult ValidationProblem()
    {
        var errors = ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => string.Join("; ", e.Value!.Errors.Select(er => er.ErrorMessage))
            );

        return BadRequest(new ValidationErrorResponse("Invalid data", errors));
    }
}
