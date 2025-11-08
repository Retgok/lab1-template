using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class PersonsControllerTests
{
    private readonly Mock<IPersonRepo> _repoMock;
    private readonly PersonsController _controller;

    public PersonsControllerTests()
    {
        _repoMock = new Mock<IPersonRepo>();
        _controller = new PersonsController(_repoMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        var persons = new List<Person> {
            new Person { Id = 1, Name = "John" },
            new Person { Id = 2, Name = "Alice" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(persons);

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsAssignableFrom<IEnumerable<PersonResponse>>(ok.Value);
        Assert.Equal(2, data.Count());
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenPersonExists()
    {
        var person = new Person { Id = 1, Name = "John" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(person);

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PersonResponse>(ok.Value);
        Assert.Equal("John", response.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenPersonMissing()
    {
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((Person?)null);

        var result = await _controller.GetById(5);

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorResponse>(nf.Value);
        Assert.Contains("not found", err.Message);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValid()
    {
        var dto = new PersonRequest { Name = "John", Age = 30 };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Person>())).ReturnsAsync(new Person(1, "John", 30, null, null));

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(PersonsController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var dto = new PersonRequest { Name = "" };

        var result = await _controller.Create(dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var err = Assert.IsType<ValidationErrorResponse>(bad.Value);
        Assert.True(err.Errors.ContainsKey("name"));
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenValid()
    {
        var person = new Person { Id = 1, Name = "Old" };
        var dto = new PersonRequest { Name = "New" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(person);
        _repoMock.Setup(r => r.UpdateAsync(person)).Returns(Task.CompletedTask);

        var result = await _controller.Update(1, dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var resp = Assert.IsType<PersonResponse>(ok.Value);
        Assert.Equal("New", resp.Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenPersonMissing()
    {
        _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Person?)null);
        var dto = new PersonRequest { Name = "X" };

        var result = await _controller.Update(10, dto);

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorResponse>(nf.Value);
        Assert.Contains("not found", err.Message);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenNameEmpty()
    {
        var person = new Person { Id = 1, Name = "Old" };
        var dto = new PersonRequest { Name = " " };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(person);

        var result = await _controller.Update(1, dto);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        var err = Assert.IsType<ValidationErrorResponse>(bad.Value);
        Assert.Contains("Name cannot be empty", err.Errors["name"]);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        var person = new Person { Id = 1, Name = "John" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(person);
        _repoMock.Setup(r => r.DeleteAsync(person)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenPersonMissing()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Person?)null);

        var result = await _controller.Delete(1);

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorResponse>(nf.Value);
        Assert.Contains("not found", err.Message);
    }
}
