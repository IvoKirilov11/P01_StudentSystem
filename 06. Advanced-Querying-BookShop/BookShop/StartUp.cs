namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);

            Console.WriteLine(CountCopiesByAuthor(db));
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {

            var ageRestriction = Enum.Parse<AgeRestriction>(command, true);
            var allBook = context.Books
                .Where(x => x.AgeRestriction == ageRestriction)
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToArray();

            var result = string.Join(Environment.NewLine, allBook);

            return result;
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var goldenBook = context.Books
                .Where(x => x.EditionType == EditionType.Gold && x.Copies < 5000)
                .Select(x => new
                {
                    x.Title,
                    x.BookId
                })
                .OrderBy(x => x.BookId)
                .ToArray();

            var result = string.Join(Environment.NewLine, goldenBook.Select(x => x.Title));

            return result;
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var bookPrice = context.Books
                .Where(x => x.Price > 40)
                .Select(x => new
                {
                    x.Title,
                    x.Price
                })
                .OrderByDescending(x => x.Price)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var item in bookPrice)
            {
                sb.AppendLine($"{item.Title} - ${item.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var bookNotReleased = context.Books
                .Where(x => x.ReleaseDate.Value.Year != year)
                .Select(x => new
                {
                    x.Title,
                    x.BookId
                })
                .OrderBy(x => x.BookId)
                .ToArray();

            var result = string.Join(Environment.NewLine, bookNotReleased.Select(x => x.Title));
            return result;
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categoties = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .ToArray();

            var bookCategotiy = context.Books
                .Include(x=>x.BookCategories)
                .ThenInclude(x=>x.Category)
                .Where(x => x.BookCategories.Any(categoty => categoties.Contains(categoty.Category.Name.ToLower())))
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToArray();

            var result = string.Join(Environment.NewLine, bookCategotiy);

            return result;
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var targetTime = DateTime.ParseExact(date, "dd-MM-yyyy",CultureInfo.InvariantCulture);

            var bookBefore = context.Books
                .Where(x => x.ReleaseDate.Value < targetTime)
                .Select(x => new
                {
                    x.Title,
                    x.EditionType,
                    x.Price,
                    x.ReleaseDate.Value
                })
                .OrderByDescending(x => x.Value)
                .ToArray();

            var result = string.Join(Environment.NewLine, bookBefore.Select(x => $"{x.Title} - {x.EditionType} - ${x.Price:F2}"));

            return result;
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Where(x => x.FirstName.EndsWith(input))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToArray();

            var result = string.Join(Environment.NewLine, authors.Select(x => $"{x.FirstName} {x.LastName}"));
            return result;


        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(x => EF.Functions.Like(x.Title, $"%{input}%"))
                .Select(x => new 
                { 
                    x.Title
                })
                .OrderBy(x => x.Title)
                .ToArray();

            var result = string.Join(Environment.NewLine, books.Select(x=> $"{x.Title}"));
            return result;
        }
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(x => EF.Functions.Like(x.Author.LastName, $"{input}%"))
                .Select(x => new
                {
                    x.Title,
                    AuthorName = x.Author.FirstName + " " + x.Author.LastName,
                    x.BookId

                })
                .OrderBy(x => x.BookId)
                .ToArray();

            var result = string.Join(Environment.NewLine, books.Select(x => $"{x.Title} ({x.AuthorName})"));
            return result;
        }
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(x => x.Title.Length > lengthCheck)
                .Select(x => new
                {
                    x.BookId
                })
                .ToArray();

            var result = books.Count();
            return result;
            
        }
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    TotalCopies = x.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(x => x.TotalCopies)
                .ToArray();

            var result = string.Join(Environment.NewLine, authors.Select(x => $"{x.FirstName} {x.LastName} - {x.TotalCopies}"));
            return result;
        }
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select(x => new
                {
                    x.Name,
                    Profit = x.CategoryBooks.Sum(b => b.Book.Price * b.Book.Copies)
                })
                .OrderByDescending(x => x.Profit)
                .ThenBy(x => x.Name)
                .ToArray();

            var result = string.Join(Environment.NewLine, categories.Select(x => $"{x.Name} ${x.Profit:F2}"));
            return result;

        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categotyBooks = context.Categories
                .Select(x => new
                {
                    CatName = x.Name,
                    Books = x.CategoryBooks.Select(b => new
                    {
                        b.Book.Title,
                        b.Book.ReleaseDate.Value
                    })
                    .OrderByDescending(b => b.Value)
                    .Take(3)
                    .ToArray()

                })
                .OrderBy(x => x.CatName)
                .ToArray();

            var sb = new StringBuilder();

            foreach (var category in categotyBooks)
            {
                sb.AppendLine($"--{category.CatName}");

                foreach (var item in category.Books)
                {
                    sb.AppendLine($"{item.Title} ({item.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var item in books)
            {
                item.Price += 5;
            }
            context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.Copies < 4200)
                .ToList();
            context.Books.RemoveRange(books);

             context.SaveChanges();
            return books.Count;
        }
    }
}
