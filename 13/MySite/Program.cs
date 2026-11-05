using MySite.Models;

var builder = WebApplication.CreateBuilder(args);

// Одна библиотека на всё приложение (данные хранятся в памяти, пока сайт запущен).
builder.Services.AddSingleton(_ =>
{
    var library = new Library();
    library.AddBook(new Book("Путь Абая", "Мухтар Ауэзов", 1942,
        "Роман-эпопея о жизни казахского поэта и просветителя Абая Кунанбаева " +
        "и о казахской степи XIX века: её обычаях, людях и переменах."));
    library.AddBook(new Book("Мастер и Маргарита", "Михаил Булгаков", 1967,
        "Роман о визите Воланда и его свиты в Москву 1930-х годов, о Мастере и его " +
        "возлюбленной Маргарите и о Понтии Пилате — история любви, милосердия и выбора."));
    library.AddBook(new Book("Преступление и наказание", "Фёдор Достоевский", 1866,
        "Бедный петербургский студент Родион Раскольников совершает убийство, чтобы " +
        "проверить свою теорию, и мучительно переживает последствия своего поступка."));
    library.AddBook(new Book("Чистый код", "Роберт Мартин", 2008,
        "Практическое руководство о том, как писать понятный и удобный в сопровождении код: " +
        "именование, функции, комментарии, обработка ошибок, тесты и рефакторинг."));
    return library;
});

var app = builder.Build();

app.UseDefaultFiles();   // открывает wwwroot/index.html по адресу "/"
app.UseStaticFiles();

var api = app.MapGroup("/api");

api.MapGet("/info", () => new
{
    name = Library.GetLibraryName(),
    loanPeriodDays = Library.LoanPeriodDays,
    finePerDay = Library.FinePerDay,
});

// Книги: ?q=текст&by=title|author|year
api.MapGet("/books", (Library library, string? q, string? by) =>
{
    List<Book> books;
    if (string.IsNullOrWhiteSpace(q))
        books = library.GetBooks();
    else if (by == "year")
        books = int.TryParse(q, out int year) ? library.SearchBook(year) : new List<Book>();
    else if (by == "author")
        books = library.SearchBook(q, byAuthor: true);
    else
        books = library.SearchBook(q);

    return books.Select(ToDto);
});

api.MapPost("/books", (Library library, AddBookRequest req) =>
    ToResult(library.AddBook(req.Title, req.Author, req.Year, req.Description)));

api.MapPost("/books/description", (Library library, DescriptionRequest req) =>
    ToResult(library.SetBookDescription(req.Title, req.Description)));

api.MapPost("/books/sort", (Library library) =>
{
    library.SortBooksByYear();
    return OperationResult.Ok("Книги отсортированы по году издания.");
});

api.MapGet("/readers", (Library library) => library.GetReaders().Select(r => new
{
    fullName = r.FullName,
    phone = r.Phone,
    books = r.BorrowedBooks.Select(ToDto),
}));

api.MapPost("/readers", (Library library, AddReaderRequest req) =>
    ToResult(library.AddReader(req.FirstName, req.LastName, req.Phone)));

api.MapPost("/borrow", (Library library, BorrowRequest req) =>
    ToResult(library.BorrowBook(req.Title, req.ReaderName)));

api.MapPost("/return", (Library library, ReturnRequest req) =>
    ToResult(req.DaysKept is int days
        ? library.ReturnBook(req.Title, days)
        : library.ReturnBook(req.Title)));

api.MapGet("/stats", (Library library) => library.GetStatistics());

app.Run();

static object ToDto(Book b) => new
{
    title = b.Title,
    author = b.Author,
    year = b.Year,
    description = b.Description,
    age =Book.CalculateBookAge(b.Year),
    available = b.IsBookAvailable(),
    borrower = b.Borrower?.FullName,
    borrowDate = b.BorrowDate?.ToString("dd.MM.yyyy"),
    dueDate = b.DueDate?.ToString("dd.MM.yyyy"),
    overdue = b.IsOverdue,
};

static IResult ToResult(OperationResult result) =>
    result.Success ? Results.Ok(result) : Results.BadRequest(result);

record AddBookRequest(string? Title, string? Author, int? Year, string? Description);
record DescriptionRequest(string? Title, string? Description);
record AddReaderRequest(string? FirstName, string? LastName, string? Phone);
record BorrowRequest(string? Title, string? ReaderName);
record ReturnRequest(string? Title, int? DaysKept);
