using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using System.Threading;

// --- DOCKER COMPATIBLE CONNECTION LOGIC ---
// Use the Environment Variable if it exists (for Docker), otherwise use localhost
string connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION") ?? "mongodb://localhost:27017";
var db = new LibraryDbContext(connectionString, "LibraryML");
// ------------------------------------------

var engine = new RecommendationEngine();
var ai = new SummaryService(); // Initialize the AI Service

while (true)
{
    Console.Clear();
    Console.WriteLine("============================================");
    Console.WriteLine("    INTELLIGENT LIBRARY MANAGEMENT SYSTEM   ");
    Console.WriteLine("============================================");
    Console.WriteLine("1. [Admin] Add New Books (AI Powered)");
    Console.WriteLine("2. [User]  Rent a Book (Batch Mode)");
    Console.WriteLine("3. [System] Train Prediction Model");
    Console.WriteLine("4. [User]  Get My Recommendations");
    Console.WriteLine("5. Exit");
    Console.WriteLine("============================================");
    Console.Write("Select an option: ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await AddBookMenu(db, ai);
            break;
        case "2":
            await RentBookMenu(db);
            break;
        case "3":
            await TrainModel(db, engine);
            break;
        case "4":
            await ShowRecommendations(db, engine);
            break;
        case "5":
            return;
    }
}

async Task AddBookMenu(LibraryDbContext db, SummaryService ai)
{
    bool adding = true;
    while (adding)
    {
        Console.Clear();
        Console.WriteLine("--- ADMIN: ADD BOOK (AI POWERED) ---");
        Console.Write("Enter Book ID (Numeric): "); uint id = uint.Parse(Console.ReadLine()!);
        Console.Write("Enter Title: "); string title = Console.ReadLine()!;
        Console.Write("Enter Genre: "); string genre = Console.ReadLine()!;

        // Trigger Gemini AI for summary
        Console.WriteLine("\n[AI] Generating plot summary...");
        string summary = await ai.GenerateSummaryAsync(title, genre);
        Console.WriteLine($"[AI] Done: {summary}");

        await db.AddBookAsync(new Book
        {
            BookId = id,
            Title = title,
            Genre = genre,
            Summary = summary
        });

        Console.WriteLine("\nBook and AI summary saved to MongoDB!");
        Console.Write("Add another one? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") adding = false;
    }
}

async Task RentBookMenu(LibraryDbContext db)
{
    Console.Clear();
    Console.WriteLine("==================================================================");
    Console.WriteLine("                    USER: BATCH RENTAL                          ");
    Console.WriteLine("==================================================================");

    var books = await db.GetBooksAsync();

    if (books.Count == 0)
    {
        Console.WriteLine("[!] The library is currently empty. Add books first.");
    }
    else
    {
        Console.WriteLine($"{"ID",-6} | {"Book Title",-35} | {"Genre",-15}");
        Console.WriteLine(new string('-', 62));

        foreach (var b in books)
        {
            Console.WriteLine($"{b.BookId,-6} | {b.Title,-35} | {b.Genre,-15}");
        }
    }

    Console.WriteLine("==================================================================");

    Console.Write("\nEnter User ID to start renting: ");
    if (!uint.TryParse(Console.ReadLine(), out uint uId))
    {
        Console.WriteLine("Invalid User ID. Returning to menu...");
        Thread.Sleep(1500);
        return;
    }

    bool renting = true;
    while (renting)
    {
        Console.WriteLine($"\n>>> User [{uId}] is active.");
        Console.Write("Enter Book ID to rent (or type '0' to finish): ");

        string input = Console.ReadLine()!;
        if (input == "0")
        {
            renting = false;
        }
        else if (uint.TryParse(input, out uint bId))
        {
            if (books.Any(x => x.BookId == bId))
            {
                await db.AddRentalAsync(new Rental { UserId = uId, BookId = bId, Label = 5.0f });
                Console.WriteLine($">> SUCCESS: Book {bId} recorded!");
            }
            else
            {
                Console.WriteLine($">> [!] ERROR: Book ID {bId} not found.");
            }
        }
    }

    Console.WriteLine("\nBatch rental session closed. Press any key to return...");
    Console.ReadKey();
}

async Task TrainModel(LibraryDbContext db, RecommendationEngine engine)
{
    Console.Clear();
    Console.WriteLine("--- SYSTEM: TRAINING ---");
    var data = await db.GetRentalsAsync();
    if (data.Count < 2)
    {
        Console.WriteLine("Not enough data. Rent at least 2 books first!");
    }
    else
    {
        engine.Train(data);
    }
    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}

async Task ShowRecommendations(LibraryDbContext db, RecommendationEngine engine)
{
    Console.Clear();
    Console.WriteLine("--- HYBRID RECOMMENDATIONS (AI SUMMARY) ---");
    Console.Write("Enter User ID: "); uint uId = uint.Parse(Console.ReadLine()!);

    var allBooks = await db.GetBooksAsync();
    var allRentals = await db.GetRentalsAsync();

    var userBookIds = allRentals.Where(r => r.UserId == uId).Select(r => r.BookId).ToList();
    var userGenres = allBooks.Where(b => userBookIds.Contains(b.BookId))
                             .Select(b => b.Genre)
                             .Distinct()
                             .ToList();

    if (!userGenres.Any())
    {
        Console.WriteLine("Rent some books first so I can learn your taste!");
        Console.ReadKey();
        return;
    }

    var candidates = allBooks.Where(b => userGenres.Contains(b.Genre) && !userBookIds.Contains(b.BookId));

    var recommended = candidates.Select(b => new {
        b.Title,
        b.Genre,
        b.Summary, // Include Summary for the UI
        Score = engine.PredictScore(uId, b.BookId)
    }).OrderByDescending(x => float.IsNaN(x.Score) ? 0 : x.Score).ToList();

    Console.WriteLine($"\nBecause you like {string.Join(", ", userGenres)}:");
    foreach (var rec in recommended)
    {
        string matchText = float.IsNaN(rec.Score) ? "New Discovery" : $"Match: {rec.Score:0.##}";
        Console.WriteLine($"> {rec.Title} [{rec.Genre}] ({matchText})");

        // Wrap the summary text for a cleaner look
        Console.WriteLine($"  Summary: {rec.Summary}");
        Console.WriteLine(new string('-', 50));
    }

    Console.WriteLine("\nPress any key to return...");
    Console.ReadKey();
}