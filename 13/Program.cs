using System.Text;

namespace LibraryApp;

/// <summary>
/// Задача 25. Консольная программа «Управление библиотекой».
/// </summary>
internal static class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Library.ShowLibraryName();          // Задача 11: вызов без создания объекта
        var library = new Library();
        library.ShowWelcomeMessage();       // Задача 2

        AddStartingBooks(library);

        bool running = true;
        while (running)
        {
            ShowMenu();
            string choice = ReadText("Выберите действие: ");
            Console.WriteLine();

            switch (choice)
            {
                case "1": AddBookAction(library); break;
                case "2": ShowBooksAction(library); break;
                case "3": FindBookAction(library); break;
                case "4": AddReaderAction(library); break;
                case "5": library.ShowReaders(); break;
                case "6": BorrowBookAction(library); break;
                case "7": ReturnBookAction(library); break;
                case "8": library.ShowStatistics(); break;
                case "0": running = false; break;
                default: Console.WriteLine("Неизвестная команда. Введите число от 0 до 8."); break;
            }

            Console.WriteLine();
        }

        Console.WriteLine("Работа программы завершена. До свидания!");
    }

    // Задача 5. Три объекта Book и вызов DisplayInfo() для каждого.
    private static void AddStartingBooks(Library library)
    {
        var book1 = new Book("Путь Абая", "Мухтар Ауэзов", 1942);
        var book2 = new Book("Мастер и Маргарита", "Михаил Булгаков", 1967);
        var book3 = new Book("Преступление и наказание", "Фёдор Достоевский", 1866);

        Console.WriteLine("Книги в фонде библиотеки:");
        foreach (Book book in new[] { book1, book2, book3 })
        {
            Console.Write("  ");
            book.DisplayInfo();
            library.AddBook(book);
        }

        Console.WriteLine();
    }

    // Задача 24. Главное меню.
    private static void ShowMenu()
    {
        Console.WriteLine("==============================");
        Console.WriteLine(" УПРАВЛЕНИЕ БИБЛИОТЕКОЙ");
        Console.WriteLine("==============================");
        Console.WriteLine();
        Console.WriteLine("1. Добавить книгу");
        Console.WriteLine("2. Показать все книги");
        Console.WriteLine("3. Найти книгу");
        Console.WriteLine("4. Добавить читателя");
        Console.WriteLine("5. Показать читателей");
        Console.WriteLine("6. Выдать книгу");
        Console.WriteLine("7. Вернуть книгу");
        Console.WriteLine("8. Статистика библиотеки");
        Console.WriteLine("0. Выход");
        Console.WriteLine();
    }

    // ───────────────────────── Пункты меню ─────────────────────────

    private static void AddBookAction(Library library)
    {
        string title = ReadText("Название: ");
        string author = ReadText("Автор: ");
        int year = ReadInt("Год: ", 1, DateTime.Now.Year);
        Console.WriteLine();

        if (library.AddBook(title, author, year))
            Console.WriteLine($"Книга успешно добавлена! Книг в библиотеке: {library.GetBookCount()}.");
    }

    private static void ShowBooksAction(Library library)
    {
        if (library.GetBookCount() > 1 && ReadYesNo("Отсортировать по году издания? (д/н): "))
            library.SortBooksByYear();

        library.ShowBooks();
    }

    private static void FindBookAction(Library library)
    {
        Console.WriteLine("Искать по: 1 — названию, 2 — автору, 3 — году издания");
        int mode = ReadInt("Ваш выбор: ", 1, 3);

        List<Book> found = mode switch
        {
            1 => library.SearchBook(ReadText("Название (или его часть): ")),
            2 => library.SearchBook(ReadText("Автор (или часть имени): "), byAuthor: true),
            _ => library.SearchBook(ReadInt("Год издания: ", 1, DateTime.Now.Year)),
        };

        Console.WriteLine();
        if (found.Count == 0)
        {
            Console.WriteLine("Ничего не найдено.");
            return;
        }

        Console.WriteLine($"Найдено книг: {found.Count}");
        Library.PrintBooks(found);
    }

    private static void AddReaderAction(Library library)
    {
        string firstName = ReadText("Имя: ");
        string lastName = ReadText("Фамилия (Enter — пропустить): ", allowEmpty: true);
        long? phone = ReadPhone("Телефон (Enter — пропустить): ");
        Console.WriteLine();

        bool hasLastName = lastName.Length > 0;
        bool added = (hasLastName, phone) switch
        {
            (false, null) => library.AddReader(firstName),
            (true, null) => library.AddReader(firstName, lastName),
            (false, long p) => library.AddReader(firstName, p),
            (true, long p) => library.AddReader(firstName, lastName, p),
        };

        if (added)
            Console.WriteLine("Читатель успешно зарегистрирован!");
    }

    private static void BorrowBookAction(Library library)
    {
        List<Book> available = library.GetAvailableBooks();
        if (available.Count == 0)
        {
            Console.WriteLine("Свободных книг нет.");
            return;
        }

        Console.WriteLine("Доступные книги:");
        Library.PrintBooks(available);
        Console.WriteLine();

        string title = ReadText("Название книги: ");
        string reader = ReadText("Имя читателя (как при регистрации): ");
        Console.WriteLine();

        library.BorrowBook(title, reader);
    }

    private static void ReturnBookAction(Library library)
    {
        string title = ReadText("Название книги: ");
        int? days = ReadOptionalInt("Сколько дней книга была у читателя (Enter — по дате выдачи): ", 0, 3650);
        Console.WriteLine();

        if (days == null)
            library.ReturnBook(title);
        else
            library.ReturnBook(title, days.Value);
    }

    // ───────────────────────── Ввод с проверкой ─────────────────────────

    private static string ReadText(string prompt, bool allowEmpty = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string? line = Console.ReadLine();
            if (line == null)                 // конец ввода (Ctrl+Z) — выходим
                Environment.Exit(0);

            line = line.Trim();
            if (line.Length > 0 || allowEmpty)
                return line;

            Console.WriteLine("Значение не может быть пустым.");
        }
    }

    private static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            int? value = ReadOptionalInt(prompt, min, max);
            if (value != null)
                return value.Value;

            Console.WriteLine($"Введите целое число от {min} до {max}.");
        }
    }

    private static int? ReadOptionalInt(string prompt, int min, int max)
    {
        while (true)
        {
            string text = ReadText(prompt, allowEmpty: true);
            if (text.Length == 0)
                return null;

            if (int.TryParse(text, out int value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Введите целое число от {min} до {max}.");
        }
    }

    private static long? ReadPhone(string prompt)
    {
        while (true)
        {
            string text = ReadText(prompt, allowEmpty: true);
            if (text.Length == 0)
                return null;

            string digits = new string(text.Where(char.IsDigit).ToArray());
            if (digits.Length is >= 5 and <= 15 && long.TryParse(digits, out long phone))
                return phone;

            Console.WriteLine("Номер должен содержать от 5 до 15 цифр.");
        }
    }

    private static bool ReadYesNo(string prompt)
    {
        string answer = ReadText(prompt, allowEmpty: true).ToLowerInvariant();
        return answer is "д" or "да" or "y" or "yes";
    }
}
