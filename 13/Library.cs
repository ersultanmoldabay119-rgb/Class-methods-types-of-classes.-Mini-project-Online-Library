namespace LibraryApp;

/// <summary>
/// Библиотека: хранит книги и читателей, выдаёт и принимает книги.
/// </summary>
public class Library
{
    public const string LibraryName = "Городская библиотека №1";

    /// <summary>Срок, на который выдаётся книга (дней).</summary>
    public const int LoanPeriodDays = 14;

    /// <summary>Штраф за один день просрочки (тг).</summary>
    public const int FinePerDay = 100;

    // Задача 15. Статическое поле — сколько книг добавлено во все библиотеки за время работы программы.
    private static int TotalBooks;

    private readonly List<Book> _books = new();
    private readonly List<Reader> _readers = new();

    // ───────────────────────── Задачи 2, 11: приветствие и название ─────────────────────────

    // Задача 11. Статический метод — вызывается без создания объекта: Library.ShowLibraryName().
    public static void ShowLibraryName()
    {
        Console.WriteLine($"*** {LibraryName} ***");
    }

    // Задача 2. Метод без параметров.
    public void ShowWelcomeMessage()
    {
        Console.WriteLine("Добро пожаловать! Здесь вы можете вести учёт книг и читателей.");
        Console.WriteLine($"Книги выдаются на {LoanPeriodDays} дней, штраф за просрочку — {FinePerDay} тг в день.");
        Console.WriteLine();
    }

    // ───────────────────────── Книги ─────────────────────────

    // Задача 4. Метод с несколькими параметрами.
    public bool AddBook(string title, string author, int year)
    {
        return AddBook(new Book(title, author, year));
    }

    public bool AddBook(Book book)
    {
        if (FindBook(book.Title) != null)
        {
            Console.WriteLine($"Книга «{book.Title}» уже есть в библиотеке.");
            return false;
        }

        _books.Add(book);
        IncrementTotalBooks();
        return true;
    }

    // Задача 15. Увеличивает статический счётчик при добавлении книги.
    private static void IncrementTotalBooks()
    {
        TotalBooks++;
    }

    public static int GetTotalBooks() => TotalBooks;

    // Задача 6. Метод с возвращаемым значением.
    public int GetBookCount() => _books.Count;

    // Задача 9. Поиск книги по точному названию (без учёта регистра). null — не найдена.
    public Book? FindBook(string title)
    {
        string key = title.Trim();
        return _books.Find(b => b.Title.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    // Задача 12. Нестатический метод — выводит книги конкретного объекта библиотеки.
    public void ShowBooks()
    {
        if (_books.Count == 0)
        {
            Console.WriteLine("В библиотеке пока нет книг.");
            return;
        }

        PrintBooks(_books);
    }

    // Задача 14. Нестатический метод — список книг, которые можно выдать.
    public List<Book> GetAvailableBooks() => _books.FindAll(b => b.IsBookAvailable());

    // ───── Задача 16. Перегрузка SearchBook(): по названию, по автору, по году ─────

    /// <summary>Поиск по части названия.</summary>
    public List<Book> SearchBook(string title) => SearchBook(title, byAuthor: false);

    /// <summary>Поиск по части имени автора (byAuthor = true) или названия (byAuthor = false).</summary>
    public List<Book> SearchBook(string text, bool byAuthor)
    {
        string key = text.Trim();
        return _books.FindAll(b =>
            (byAuthor ? b.Author : b.Title).Contains(key, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Поиск по году издания.</summary>
    public List<Book> SearchBook(int year) => _books.FindAll(b => b.Year == year);

    // Задача 19. Рекурсивный подсчёт книг в списке.
    public int CountBooksRecursive() => CountBooksRecursive(0);

    private int CountBooksRecursive(int index)
    {
        if (index >= _books.Count)
            return 0;                              // база рекурсии: список закончился

        return 1 + CountBooksRecursive(index + 1); // текущая книга + остальные
    }

    // Задача 20. Сортировка книг по году издания (сортировка вставками).
    public void SortBooksByYear()
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

    public static void PrintBooks(List<Book> books)
    {
        for (int i = 0; i < books.Count; i++)
        {
            Console.Write($"{i + 1,3}. ");
            books[i].DisplayInfo();
        }
    }

    // ───────────────────────── Читатели ─────────────────────────

    // ───── Задача 17. Перегрузка AddReader() ─────

    /// <summary>Только по имени.</summary>
    public bool AddReader(string firstName) => AddReader(new Reader(firstName));

    /// <summary>По имени и фамилии.</summary>
    public bool AddReader(string firstName, string lastName) => AddReader(new Reader(firstName, lastName));

    /// <summary>По имени и номеру телефона.</summary>
    public bool AddReader(string firstName, long phone) => AddReader(new Reader(firstName, phone: phone.ToString()));

    /// <summary>По имени, фамилии и номеру телефона.</summary>
    public bool AddReader(string firstName, string lastName, long phone) =>
        AddReader(new Reader(firstName, lastName, phone.ToString()));

    private bool AddReader(Reader reader)
    {
        if (IsReaderRegistered(reader.FullName))
        {
            Console.WriteLine($"Читатель «{reader.FullName}» уже зарегистрирован.");
            return false;
        }

        _readers.Add(reader);
        return true;
    }

    // Задача 10. Проверка наличия читателя (по полному имени, без учёта регистра).
    public bool IsReaderRegistered(string name) => FindReader(name) != null;

    public Reader? FindReader(string name)
    {
        string key = name.Trim();
        return _readers.Find(r => r.FullName.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public void ShowReaders()
    {
        if (_readers.Count == 0)
        {
            Console.WriteLine("Читателей пока нет.");
            return;
        }

        for (int i = 0; i < _readers.Count; i++)
        {
            Console.Write($"{i + 1,3}. ");
            _readers[i].DisplayInfo();
        }
    }

    // ───────────────────────── Выдача и возврат ─────────────────────────

    // Задача 21. Проверяет книгу и читателя, затем оформляет выдачу.
    public bool BorrowBook(string title, string readerName)
    {
        Book? book = FindBook(title);
        if (book == null)
        {
            Console.WriteLine($"Книга «{title}» не найдена.");
            return false;
        }

        if (!book.IsBookAvailable())
        {
            Console.WriteLine($"Книга «{book.Title}» уже выдана читателю {book.Borrower!.FullName}.");
            return false;
        }

        Reader? reader = FindReader(readerName);
        if (reader == null)
        {
            Console.WriteLine($"Читатель «{readerName}» не зарегистрирован.");
            return false;
        }

        book.MarkBorrowed(reader, DateTime.Now);
        reader.BorrowedBooks.Add(book);

        DateTime dueDate = DateTime.Now.AddDays(LoanPeriodDays);
        Console.WriteLine($"Книга «{book.Title}» выдана читателю {reader.FullName}. Вернуть до {dueDate:dd.MM.yyyy}.");
        return true;
    }

    // Задача 18. Возвращает книгу, если она была выдана. Срок хранения считается по дате выдачи.
    public bool ReturnBook(string title)
    {
        Book? book = FindBook(title);
        int daysKept = book?.BorrowDate is DateTime date ? (DateTime.Now - date).Days : 0;
        return ReturnBook(title, daysKept);
    }

    // Задача 22. Возврат с проверкой статуса и расчётом штрафа.
    // daysKept — сколько дней книга была у читателя.
    public bool ReturnBook(string title, int daysKept)
    {
        Book? book = FindBook(title);
        if (book == null)
        {
            Console.WriteLine($"Книга «{title}» не найдена.");
            return false;
        }

        if (book.IsBookAvailable())
        {
            Console.WriteLine($"Книга «{book.Title}» не выдавалась — она уже в библиотеке.");
            return false;
        }

        Reader reader = book.Borrower!;
        reader.BorrowedBooks.Remove(book);
        book.MarkReturned();

        int overdueDays = daysKept - LoanPeriodDays;
        int fine = CalculateFine(overdueDays);

        Console.WriteLine($"Книга «{book.Title}» возвращена читателем {reader.FullName}.");
        if (fine > 0)
            Console.WriteLine($"Просрочка: {overdueDays} дн. Штраф: {fine} тг.");
        else
            Console.WriteLine("Книга возвращена в срок, штрафа нет.");

        return true;
    }

    // Задача 13. Статический метод расчёта штрафа.
    public static int CalculateFine(int overdueDays) => overdueDays > 0 ? overdueDays * FinePerDay : 0;

    // Задача 14. Статический метод — большее из двух количеств книг.
    public static int GetMaxBooks(int first, int second) => first > second ? first : second;

    // ───────────────────────── Задача 23. Статистика ─────────────────────────

    public void ShowStatistics()
    {
        int total = CountBooksRecursive();
        int available = GetAvailableBooks().Count;
        int borrowed = total - available;

        int maxPerReader = 0;
        foreach (Reader reader in _readers)
            maxPerReader = GetMaxBooks(maxPerReader, reader.BorrowedBooks.Count);

        Console.WriteLine("------ СТАТИСТИКА БИБЛИОТЕКИ ------");
        Console.WriteLine($"Всего книг:                      {total}");
        Console.WriteLine($"Доступно книг:                   {available}");
        Console.WriteLine($"Выдано книг:                     {borrowed}");
        Console.WriteLine($"Зарегистрировано читателей:      {_readers.Count}");
        Console.WriteLine($"Больше всего книг у читателя:    {maxPerReader}");
        Console.WriteLine($"Добавлено книг за сеанс (всего): {GetTotalBooks()}");
    }
}
