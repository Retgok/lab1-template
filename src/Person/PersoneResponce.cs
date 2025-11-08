public class PersonResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int? Age { get; set; }
    public string? Address { get; set; }
    public string? Work { get; set; }

    public PersonResponse(Person person)
    {
        Id = person.Id;
        Name = person.Name;
        Age = person.Age;
        Address = person.Address;
        Work = person.Work;
    }
}
