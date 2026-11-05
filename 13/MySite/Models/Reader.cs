namespace MySite.Models;

/// <summary>Читатель библиотеки. Фамилия и телефон необязательны.</summary>
public class Reader
{
    public string FirstName { get; }
    public string? LastName { get; }
    public string? Phone { get; }

    /// <summary>Книги, которые сейчас у читателя.</summary>
    public List<Book> BorrowedBooks { get; } = new();

    public string FullName => LastName == null ? FirstName : $"{FirstName} {LastName}";

    public Reader(string firstName, string? lastName = null, string? phone = null)
    {
        FirstName = firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }
}
