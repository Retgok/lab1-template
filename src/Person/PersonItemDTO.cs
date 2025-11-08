public class PersonItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Address { get; set; }
    public string? Work { get; set; }

    public PersonItemDTO() { }

    public PersonItemDTO(Person person)
    {
        Id = person.Id;
        Name = person.Name;
        Age = person.Age;
        Address = person.Address;
        Work = person.Work;
    }
}
