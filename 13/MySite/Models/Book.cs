namespace MySite.Models;

/// <summary>Книга: название, автор, год издания и статус выдачи.</summary>
public class Book
{
    public const int MaxDescriptionLength = 1000;

    public string Title { get; private set; } = "";
    public string Author { get; }
    public int Year { get; }

    /// <summary>Краткое описание книги (null — описания нет).</summary>
    public string? Description { get; private set; }

    /// <summary>Читатель, у которого сейчас книга (null — книга в библиотеке).</summary>
    public Reader? Borrower { get; private set; }
    public DateTime? BorrowDate { get; private set; }

    /// <summary>Дата, до которой книгу нужно вернуть.</summary>
    public DateTime? DueDate => BorrowDate?.AddDays(Library.LoanPeriodDays);

    public bool IsOverdue => DueDate is DateTime due && DateTime.Now.Date > due.Date;

    public Book(string title, string author, int year, string? description = null)
    {
        SetTitle(title);
        Author = author.Trim();
        Year = year;
        SetDescription(description);
    }

    /// <summary>Задаёт или изменяет описание. Пустая строка удаляет описание.</summary>
    public void SetDescription(string? description)
    {
        string? text = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (text != null && text.Length > MaxDescriptionLength)
            throw new ArgumentException($"Описание не может быть длиннее {MaxDescriptionLength} символов.", nameof(description));

        Description = text;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название книги не может быть пустым.", nameof(title));

        Title = title.Trim();
    }

    public bool IsBookAvailable() => Borrower == null;

    public static int CalculateBookAge(int year) => DateTime.Now.Year - year;

    internal void MarkBorrowed(Reader reader, DateTime date)
    {
        Borrower = reader;
        BorrowDate = date;
    }

    internal void MarkReturned()
    {
        Borrower = null;
        BorrowDate = null;
    }
}
