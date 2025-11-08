public interface IPersonRepo
{
    Task<List<Person>> GetAllAsync();
    Task<Person?> GetByIdAsync(int id);
    Task<Person> AddAsync(Person person);
    Task UpdateAsync(Person person);
    Task DeleteAsync(Person person);
    Task SaveChangesAsync();
}
