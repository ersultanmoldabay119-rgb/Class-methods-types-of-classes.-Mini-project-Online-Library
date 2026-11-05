namespace LibraryApp;

/// <summary>
/// Читатель библиотеки. Фамилия и телефон необязательны (см. перегрузки AddReader).
/// </summary>
public class Reader
{
    public string FirstName { get; }
    public string? LastName { get; }
    public string? Phone { get; }

    /// <summary>Книги, которые сейчас находятся у читателя.</summary>
    public List<Book> BorrowedBooks { get; } = new();

    public string FullName => LastName == null ? FirstName : $"{FirstName} {LastName}";

    public Reader(string firstName, string? lastName = null, string? phone = null)
    {
        FirstName = firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }

    public void DisplayInfo()
    {
        string phone = Phone == null ? "" : $", тел. {Phone}";
        Console.WriteLine($"{FullName}{phone} — книг на руках: {BorrowedBooks.Count}");

        foreach (Book book in BorrowedBooks)
            Console.WriteLine($"     • «{book.Title}» (выдана {book.BorrowDate:dd.MM.yyyy})");
    }
}
