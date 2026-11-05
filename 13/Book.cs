namespace LibraryApp;

/// <summary>
/// Задача 1. Книга: название, автор, год издания и статус выдачи.
/// </summary>
public class Book
{
    public string Title { get; private set; } = "";
    public string Author { get; }
    public int Year { get; }

    /// <summary>Читатель, у которого сейчас книга (null — книга в библиотеке).</summary>
    public Reader? Borrower { get; private set; }

    /// <summary>Дата выдачи книги (null — книга не выдана).</summary>
    public DateTime? BorrowDate { get; private set; }

    public Book(string title, string author, int year)
    {
        SetTitle(title);
        Author = author.Trim();
        Year = year;
    }

    // Задача 1. Вывод информации о книге.
    public void DisplayInfo()
    {
        int age = CalculateBookAge(Year);
        string status = IsBookAvailable()
            ? "доступна"
            : $"выдана: {Borrower!.FullName}, {BorrowDate:dd.MM.yyyy}";

        Console.WriteLine($"«{Title}» — {Author}, {Year} г. ({age} {YearsWord(age)}) [{status}]");
    }

    // Задача 3. Метод с одним параметром: изменяет название книги.
    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название книги не может быть пустым.", nameof(title));

        Title = title.Trim();
    }

    // Задача 7. true — книга в библиотеке, false — книга выдана.
    public bool IsBookAvailable() => Borrower == null;

    // Задача 8. Возраст книги в годах.
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

    /// <summary>Склонение слова «год»: 1 год, 2 года, 5 лет.</summary>
    private static string YearsWord(int n)
    {
        int lastTwo = Math.Abs(n) % 100;
        int last = lastTwo % 10;

        if (lastTwo is >= 11 and <= 14) return "лет";
        if (last == 1) return "год";
        if (last is >= 2 and <= 4) return "года";
        return "лет";
    }
}
