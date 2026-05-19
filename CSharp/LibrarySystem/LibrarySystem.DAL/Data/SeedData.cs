using LibrarySystem.Models;

namespace LibrarySystem.DAL.Data;

public static class SeedData
{
    public static List<BookCategory> BookCategories = new()
    {
        new BookCategory
        {
            CategoryId = 1,
            CategoryName = "Programming"
        },
        new BookCategory
        {
            CategoryId = 2,
            CategoryName = "Database"
        },
        new BookCategory
        {
            CategoryId = 3,
            CategoryName = "Artificial Intelligence"
        },
        new BookCategory
        {
            CategoryId = 4,
            CategoryName = "Networking"
        },
        new BookCategory
        {
            CategoryId = 5,
            CategoryName = "Software Engineering"
        }
    };

    public static List<Book> Books = new()
    {
        new Book
        {
            BookId = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "9780132350884",
            CategoryId = 5
        },
        new Book
        {
            BookId = 2,
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt",
            ISBN = "9780201616224",
            CategoryId = 5
        },
        new Book
        {
            BookId = 3,
            Title = "Introduction to Algorithms",
            Author = "Thomas H. Cormen",
            ISBN = "9780262046305",
            CategoryId = 1
        },
        new Book
        {
            BookId = 4,
            Title = "Computer Networks",
            Author = "Andrew S. Tanenbaum",
            ISBN = "9780132126953",
            CategoryId = 4
        },
        new Book
        {
            BookId = 5,
            Title = "Database System Concepts",
            Author = "Abraham Silberschatz",
            ISBN = "9780078022159",
            CategoryId = 2
        },
        new Book
        {
            BookId = 6,
            Title = "Artificial Intelligence: A Modern Approach",
            Author = "Stuart Russell",
            ISBN = "9780134610993",
            CategoryId = 3
        }
    };

    public static List<BookCopy> BookCopies = new()
    {
        new BookCopy
        {
            CopyId = 1,
            BookId = 1,
            CopyCode = "CC-001",
            Status = BookCopy.BookCopyStatus.Available
        },
        new BookCopy
        {
            CopyId = 2,
            BookId = 1,
            CopyCode = "CC-002",
            Status = BookCopy.BookCopyStatus.Borrowed
        },

        new BookCopy
        {
            CopyId = 3,
            BookId = 2,
            CopyCode = "PP-001",
            Status = BookCopy.BookCopyStatus.Available
        },
        new BookCopy
        {
            CopyId = 4,
            BookId = 2,
            CopyCode = "PP-002",
            Status = BookCopy.BookCopyStatus.Damaged
        },

        new BookCopy
        {
            CopyId = 5,
            BookId = 3,
            CopyCode = "ALGO-001",
            Status = BookCopy.BookCopyStatus.Available
        },
        new BookCopy
        {
            CopyId = 6,
            BookId = 3,
            CopyCode = "ALGO-002",
            Status = BookCopy.BookCopyStatus.Borrowed
        },

        new BookCopy
        {
            CopyId = 7,
            BookId = 4,
            CopyCode = "NET-001",
            Status = BookCopy.BookCopyStatus.Available
        },

        new BookCopy
        {
            CopyId = 8,
            BookId = 5,
            CopyCode = "DB-001",
            Status = BookCopy.BookCopyStatus.Unavailable
        },

        new BookCopy
        {
            CopyId = 9,
            BookId = 6,
            CopyCode = "AI-001",
            Status = BookCopy.BookCopyStatus.Available
        },
        new BookCopy
        {
            CopyId = 10,
            BookId = 6,
            CopyCode = "AI-002",
            Status = BookCopy.BookCopyStatus.Borrowed
        }
    };
    public static List<Member> Members = new()
    {
        new Member
        {
            Name = "Admin User",
            Email = "admin@library.com",
            Password = "12345",
            MembershipType = Member.membershipType.Premium,
            Role = Member.memberRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.Now
        },

        new Member
        {
            Name = "John Peter",
            Email = "john@gmail.com",
            Password = "member123",
            MembershipType = Member.membershipType.Basic,
            Role = Member.memberRole.Member,
            IsActive = true,
            JoinedAt = DateTime.Now
        },

        new Member
        {
            Name = "Priya Sharma",
            Email = "priya@gmail.com",
            Password = "member123",
            MembershipType = Member.membershipType.Premium,
            Role = Member.memberRole.Member,
            IsActive = true,
            JoinedAt = DateTime.Now
        }
    };
}