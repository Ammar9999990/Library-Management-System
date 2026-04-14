using LibraryManagementSystem.Models;
using MongoDB.Driver;

namespace LibraryManagementSystem.Data;

public class LibraryDbContext
{
    private readonly IMongoCollection<Book> _books;
    private readonly IMongoCollection<Rental> _rentals;

    public LibraryDbContext(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        _books = database.GetCollection<Book>("Books");
        _rentals = database.GetCollection<Rental>("Rentals");
    }

    public async Task AddBookAsync(Book book) => await _books.InsertOneAsync(book);

    public async Task AddRentalAsync(Rental rental) => await _rentals.InsertOneAsync(rental);

    public async Task<List<Book>> GetBooksAsync() => await _books.Find(_ => true).ToListAsync();

    public async Task<List<Rental>> GetRentalsAsync() => await _rentals.Find(_ => true).ToListAsync();
}