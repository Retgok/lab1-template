using Microsoft.EntityFrameworkCore;

public class PersonRepo : IPersonRepo
{
    private readonly PersonDb _db;

    public PersonRepo(PersonDb db)
    {
        _db = db;
    }

    public async Task<List<Person>> GetAllAsync() =>
        await _db.Persons.ToListAsync();

    public async Task<Person?> GetByIdAsync(int id) =>
        await _db.Persons.FindAsync(id);

    public async Task<Person> AddAsync(Person person)
    {
        _db.Persons.Add(person);
        await _db.SaveChangesAsync();
        return person;
    }

    public async Task UpdateAsync(Person person)
    {
        _db.Persons.Update(person);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Person person)
    {
        _db.Persons.Remove(person);
        await _db.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
