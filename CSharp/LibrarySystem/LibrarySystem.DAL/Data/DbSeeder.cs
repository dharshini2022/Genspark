using LibrarySystem.DAL.Context;

namespace LibrarySystem.DAL.Data;

public static class DbSeeder
{
    public static void Seed(LibraryDbContext context)
    {
        if (!context.BookCategories.Any())
        {
            context.BookCategories.AddRange(SeedData.BookCategories);
            context.SaveChanges();
        }

        if (!context.Books.Any())
        {
            context.Books.AddRange(SeedData.Books);
            context.SaveChanges();
        }

        if (!context.BookCopies.Any())
        {
            context.BookCopies.AddRange(SeedData.BookCopies);
            context.SaveChanges();
        }
        if (!context.Members.Any())
        {
            context.Members.AddRange(SeedData.Members);
            context.SaveChanges();
        }
    }
}