namespace MySite.Models;

/// <summary>Результат операции: успех и сообщение для пользователя.</summary>
public record OperationResult(bool Success, string Message)
{
    public static OperationResult Ok(string message) => new(true, message);
    public static OperationResult Fail(string message) => new(false, message);
}

public record LibraryStatistics(
    int TotalBooks,
    int AvailableBooks,
    int BorrowedBooks,
    int OverdueBooks,
    int Readers,
    int MaxBooksPerReader,
    int AddedThisSession);

/// <summary>
/// Библиотека: книги, читатели, выдача и возврат.
/// Один объект на всё приложение, поэтому все операции защищены блокировкой.
/// </summary>
public class Library
{
    public const string LibraryName = "Городская библиотека №1";
    public const int LoanPeriodDays = 14;
    public const int FinePerDay = 100;     // тг за день просрочки

    // Статический счётчик добавленных книг.
    private static int TotalBooks;

    private readonly List<Book> _books = new();
    private readonly List<Reader> _readers = new();
    private readonly object _sync = new();

    public static string GetLibraryName() => LibraryName;

    // ───────────────────────── Книги ─────────────────────────

    public OperationResult AddBook(string? title, string? author, int? year, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Fail("Укажите название книги.");
        if (string.IsNullOrWhiteSpace(author))
            return OperationResult.Fail("Укажите автора.");
        if (year is not int y || y < 1 || y > DateTime.Now.Year)
            return OperationResult.Fail($"Год должен быть от 1 до {DateTime.Now.Year}.");
        if (description?.Trim().Length > Book.MaxDescriptionLength)
            return OperationResult.Fail($"Описание не может быть длиннее {Book.MaxDescriptionLength} символов.");

        return AddBook(new Book(title, author, y, description));
    }

    /// <summary>Задаёт или изменяет описание существующей книги.</summary>
    public OperationResult SetBookDescription(string? title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Fail("Выберите книгу.");
        if (description?.Trim().Length > Book.MaxDescriptionLength)
            return OperationResult.Fail($"Описание не может быть длиннее {Book.MaxDescriptionLength} символов.");

        lock (_sync)
        {
            Book? book = FindBook(title);
            if (book == null)
                return OperationResult.Fail($"Книга «{title}» не найдена.");

            book.SetDescription(description);
            return OperationResult.Ok(book.Description == null
                ? $"Описание книги «{book.Title}» удалено."
                : $"Описание книги «{book.Title}» сохранено.");
        }
    }

    public OperationResult AddBook(Book book)
    {
        lock (_sync)
        {
            if (FindBook(book.Title) != null)
                return OperationResult.Fail($"Книга «{book.Title}» уже есть в библиотеке.");

            _books.Add(book);
            IncrementTotalBooks();
            return OperationResult.Ok($"Книга «{book.Title}» успешно добавлена!");
        }
    }

    private static void IncrementTotalBooks() => Interlocked.Increment(ref TotalBooks);

    public static int GetTotalBooks() => TotalBooks;

    public int GetBookCount()
    {
        lock (_sync) return _books.Count;
    }

    /// <summary>Поиск по точному названию (без учёта регистра).</summary>
    public Book? FindBook(string title)
    {
        string key = title.Trim();
        lock (_sync)
            return _books.Find(b => b.Title.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public List<Book> GetBooks()
    {
        lock (_sync) return new List<Book>(_books);
    }

    public List<Book> GetAvailableBooks()
    {
        lock (_sync) return _books.FindAll(b => b.IsBookAvailable());
    }

    public List<Book> GetBorrowedBooks()
    {
        lock (_sync) return _books.FindAll(b => !b.IsBookAvailable());
    }

    // Перегрузка SearchBook(): по названию, по автору, по году.
    public List<Book> SearchBook(string title) => SearchBook(title, byAuthor: false);

    public List<Book> SearchBook(string text, bool byAuthor)
    {
        string key = text.Trim();
        lock (_sync)
            return _books.FindAll(b =>
                (byAuthor ? b.Author : b.Title).Contains(key, StringComparison.OrdinalIgnoreCase));
    }

    public List<Book> SearchBook(int year)
    {
        lock (_sync) return _books.FindAll(b => b.Year == year);
    }

    // Рекурсивный подсчёт книг.
    public int CountBooksRecursive()
    {
        lock (_sync) return CountBooksRecursive(0);
    }

    private int CountBooksRecursive(int index) =>
        index >= _books.Count ? 0 : 1 + CountBooksRecursive(index + 1);

    // Сортировка книг по году издания (вставками).
    public void SortBooksByYear()
    {
        lock (_sync)
        {
            for (int i = 1; i < _books.Count; i++)
            {
                Book current = _books[i];
                int j = i - 1;
                while (j >= 0 && _books[j].Year > current.Year)
                {
                    _books[j + 1] = _books[j];
                    j--;
                }
                _books[j + 1] = current;
            }
        }
    }

    // ───────────────────────── Читатели ─────────────────────────

    public OperationResult AddReader(string? firstName, string? lastName = null, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return OperationResult.Fail("Укажите имя читателя.");

        if (!string.IsNullOrWhiteSpace(phone))
        {
            int digits = phone.Count(char.IsDigit);
            bool onlyPhoneChars = phone.All(c => char.IsDigit(c) || " +-()".Contains(c));
            if (!onlyPhoneChars || digits is < 5 or > 15)
                return OperationResult.Fail("Телефон должен содержать от 5 до 15 цифр.");
        }

        var reader = new Reader(firstName, lastName, phone);
        lock (_sync)
        {
            if (IsReaderRegistered(reader.FullName))
                return OperationResult.Fail($"Читатель «{reader.FullName}» уже зарегистрирован.");

            _readers.Add(reader);
        }
        return OperationResult.Ok($"Читатель «{reader.FullName}» зарегистрирован!");
    }

    public bool IsReaderRegistered(string name) => FindReader(name) != null;

    public Reader? FindReader(string name)
    {
        string key = name.Trim();
        lock (_sync)
            return _readers.Find(r => r.FullName.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public List<Reader> GetReaders()
    {
        lock (_sync) return new List<Reader>(_readers);
    }

    // ───────────────────────── Выдача и возврат ─────────────────────────

    public OperationResult BorrowBook(string? title, string? readerName)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(readerName))
            return OperationResult.Fail("Выберите книгу и читателя.");

        lock (_sync)
        {
            Book? book = FindBook(title);
            if (book == null)
                return OperationResult.Fail($"Книга «{title}» не найдена.");

            if (!book.IsBookAvailable())
                return OperationResult.Fail($"Книга «{book.Title}» уже выдана читателю {book.Borrower!.FullName}.");

            Reader? reader = FindReader(readerName);
            if (reader == null)
                return OperationResult.Fail($"Читатель «{readerName}» не зарегистрирован.");

            book.MarkBorrowed(reader, DateTime.Now);
            reader.BorrowedBooks.Add(book);
            return OperationResult.Ok(
                $"Книга «{book.Title}» выдана читателю {reader.FullName}. Вернуть до {book.DueDate:dd.MM.yyyy}.");
        }
    }

    /// <summary>Возврат: срок хранения считается по дате выдачи.</summary>
    public OperationResult ReturnBook(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Fail("Выберите книгу.");

        lock (_sync)
        {
            Book? book = FindBook(title);
            int daysKept = book?.BorrowDate is DateTime date ? (DateTime.Now - date).Days : 0;
            return ReturnBook(title, daysKept);
        }
    }

    /// <summary>Возврат с проверкой статуса и расчётом штрафа. daysKept — сколько дней книга была у читателя.</summary>
    public OperationResult ReturnBook(string? title, int daysKept)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Fail("Выберите книгу.");
        if (daysKept < 0)
            return OperationResult.Fail("Количество дней не может быть отрицательным.");

        lock (_sync)
        {
            Book? book = FindBook(title);
            if (book == null)
                return OperationResult.Fail($"Книга «{title}» не найдена.");

            if (book.IsBookAvailable())
                return OperationResult.Fail($"Книга «{book.Title}» не выдавалась — она уже в библиотеке.");

            Reader reader = book.Borrower!;
            reader.BorrowedBooks.Remove(book);
            book.MarkReturned();

            int overdueDays = daysKept - LoanPeriodDays;
            int fine = CalculateFine(overdueDays);

            string message = $"Книга «{book.Title}» возвращена читателем {reader.FullName}. ";
            message += fine > 0
                ? $"Просрочка {overdueDays} дн., штраф {fine} тг."
                : "Возвращена в срок, штрафа нет.";

            return OperationResult.Ok(message);
        }
    }

    public static int CalculateFine(int overdueDays) => overdueDays > 0 ? overdueDays * FinePerDay : 0;

    public static int GetMaxBooks(int first, int second) => first > second ? first : second;

    // ───────────────────────── Статистика ─────────────────────────

    public LibraryStatistics GetStatistics()
    {
        lock (_sync)
        {
            int total = CountBooksRecursive();
            int available = GetAvailableBooks().Count;

            int maxPerReader = 0;
            foreach (Reader reader in _readers)
                maxPerReader = GetMaxBooks(maxPerReader, reader.BorrowedBooks.Count);

            return new LibraryStatistics(
                TotalBooks: total,
                AvailableBooks: available,
                BorrowedBooks: total - available,
                OverdueBooks: _books.Count(b => b.IsOverdue),
                Readers: _readers.Count,
                MaxBooksPerReader: maxPerReader,
                AddedThisSession: GetTotalBooks());
        }
    }
}
