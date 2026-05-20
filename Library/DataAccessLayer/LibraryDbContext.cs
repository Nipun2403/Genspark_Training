using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Entities;

namespace DataAccessLayer;

/// Library system.

public class LibraryDbContext : DbContext
{

    private const string ConnectionString =
        "Host=localhost;Database=LibDB;Username=peewee;Password=";

    // DbSets — each is a table in the database.
    public DbSet<BookCategory> BookCategories { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<MembershipConfig> MembershipConfigs { get; set; }
    public DbSet<Borrowing> Borrowings { get; set; }
    public DbSet<Fine> Fines { get; set; }
    public DbSet<FinePayment> FinePayments { get; set; }
    public DbSet<FineConfig> FineConfigs { get; set; }

    /// To establish the connection
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionString);
    }

    /// Fallback incase migration does not happen
    public void InitializeDatabase()
    {
        // Creates the database if not present
        Database.EnsureCreated();

        // Add published_year column if it does not exist
        Database.ExecuteSqlRaw("ALTER TABLE books ADD COLUMN IF NOT EXISTS published_year INTEGER DEFAULT 0 NOT NULL;");

        // calculate_member_fine Function
        Database.ExecuteSqlRaw(@"
            CREATE OR REPLACE FUNCTION calculate_member_fine(p_member_id INT)
            RETURNS DECIMAL AS $$
            BEGIN
                RETURN COALESCE(
                    (SELECT SUM(amount - paid_amount)
                     FROM fines
                     WHERE member_id = p_member_id AND is_paid = FALSE),
                    0
                );
            END;
            $$ LANGUAGE plpgsql;
        ");

        // get_available_books_by_category function
        Database.ExecuteSqlRaw(@"
            CREATE OR REPLACE FUNCTION get_available_books_by_category(p_category_id INT)
            RETURNS TABLE(isbn VARCHAR, title VARCHAR, author VARCHAR, copy_id INT, status VARCHAR) AS $$
            BEGIN
                RETURN QUERY
                SELECT b.isbn, b.title, b.author, bc.copy_id, bc.status
                FROM books b
                INNER JOIN book_copies bc ON b.isbn = bc.isbn
                WHERE b.category_id = p_category_id
                  AND bc.status IN ('Available', 'MinorDamage');
            END;
            $$ LANGUAGE plpgsql;
        ");

        // get_member_borrowing_summary function
        Database.ExecuteSqlRaw(@"
            CREATE OR REPLACE FUNCTION get_member_borrowing_summary(p_member_id INT)
            RETURNS TABLE(
                active_borrowings BIGINT,
                returned_borrowings BIGINT,
                total_unpaid_fine DECIMAL
            ) AS $$
            BEGIN
                RETURN QUERY
                SELECT
                    (SELECT COUNT(*) FROM borrowings WHERE member_id = p_member_id AND status = 'Active'),
                    (SELECT COUNT(*) FROM borrowings WHERE member_id = p_member_id AND status = 'Returned'),
                    COALESCE(
                        (SELECT SUM(amount - paid_amount) FROM fines WHERE member_id = p_member_id AND is_paid = FALSE),
                        0
                    );
            END;
            $$ LANGUAGE plpgsql;
        ");
    }

    /// Entity mapping
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BOOK CATEGORY
        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.ToTable("book_categories");
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id");

            entity.Property(e => e.CategoryName)
                .HasColumnName("category_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.CategoryName)
                .IsUnique();
        });

        // BOOK (ISBN is PK)
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books");
            entity.HasKey(e => e.ISBN);

            entity.Property(e => e.ISBN)
                .HasColumnName("isbn")
                .HasMaxLength(20);

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(e => e.Author)
                .HasColumnName("author")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            entity.Property(e => e.PublishedYear)
                .HasColumnName("published_year")
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Book belongs to one Category
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexing **
            entity.HasIndex(e => e.Title).HasDatabaseName("idx_books_title");
            entity.HasIndex(e => e.Author).HasDatabaseName("idx_books_author");
        });

        // BOOK COPY

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.ToTable("book_copies");
            entity.HasKey(e => e.CopyId);

            entity.Property(e => e.CopyId)
                .HasColumnName("copy_id");

            entity.Property(e => e.ISBN)
                .HasColumnName("isbn")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(30)
                .HasDefaultValue("Available")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // BookCopy belongs to one Book (ISBN FK)
            entity.HasOne(e => e.Book)
                .WithMany(b => b.Copies)
                .HasForeignKey(e => e.ISBN)
                .OnDelete(DeleteBehavior.Restrict);

            // constraint for valid status values
            entity.ToTable(t => t.HasCheckConstraint(
                "chk_book_copy_status",
                "status IN ('Available', 'Borrowed', 'MinorDamage', 'DamagedBeyondUsable', 'Lost')"
            ));
        });

        // MEMBER

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members");
            entity.HasKey(e => e.MemberId);

            entity.Property(e => e.MemberId)
                .HasColumnName("member_id");

            entity.Property(e => e.FullName)
                .HasColumnName("full_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.PhoneNumber)
                .HasColumnName("phone_number")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.MembershipType)
                .HasColumnName("membership_type")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.JoinDate)
                .HasColumnName("join_date")
                .HasDefaultValueSql("NOW()");

            // Member links to MembershipConfig via MembershipType
            entity.HasOne(e => e.MembershipConfig)
                .WithMany(mc => mc.Members)
                .HasForeignKey(e => e.MembershipType)
                .HasPrincipalKey(mc => mc.MembershipType)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexing **
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("idx_members_email");
            entity.HasIndex(e => e.PhoneNumber).IsUnique().HasDatabaseName("idx_members_phone");
        });

        // MEMBERSHIP CONFIG 
        // Truth Table
        modelBuilder.Entity<MembershipConfig>(entity =>
        {
            entity.ToTable("membership_config");
            entity.HasKey(e => e.ConfigId);

            entity.Property(e => e.ConfigId)
                .HasColumnName("config_id");

            entity.Property(e => e.MembershipType)
                .HasColumnName("membership_type")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.MaxActiveBorrowings)
                .HasColumnName("max_active_borrowings")
                .IsRequired();

            entity.Property(e => e.MaxBorrowDays)
                .HasColumnName("max_borrow_days")
                .IsRequired();

            entity.HasIndex(e => e.MembershipType).IsUnique();

            // default membership types and their limits
            entity.HasData(
                new MembershipConfig { ConfigId = 1, MembershipType = "Basic", MaxActiveBorrowings = 2, MaxBorrowDays = 7 },
                new MembershipConfig { ConfigId = 2, MembershipType = "Student", MaxActiveBorrowings = 3, MaxBorrowDays = 10 },
                new MembershipConfig { ConfigId = 3, MembershipType = "Premium", MaxActiveBorrowings = 5, MaxBorrowDays = 15 }
            );
        });


        // BORROWING

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.ToTable("borrowings");
            entity.HasKey(e => e.BorrowingId);

            entity.Property(e => e.BorrowingId)
                .HasColumnName("borrowing_id");

            entity.Property(e => e.MemberId)
                .HasColumnName("member_id")
                .IsRequired();

            entity.Property(e => e.CopyId)
                .HasColumnName("copy_id")
                .IsRequired();

            entity.Property(e => e.BorrowDate)
                .HasColumnName("borrow_date")
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.DueDate)
                .HasColumnName("due_date")
                .IsRequired();

            entity.Property(e => e.ReturnDate)
                .HasColumnName("return_date");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("Active")
                .IsRequired();

            entity.Property(e => e.ConditionAtBorrow)
                .HasColumnName("condition_at_borrow")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.ConditionAtReturn)
                .HasColumnName("condition_at_return")
                .HasMaxLength(30);

            // Each transaction is of one member, which can have multiple transactions.
            entity.HasOne(e => e.Member)
                .WithMany(m => m.Borrowings)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // One transcation will include onnly one book copy at a time, but each copy can have multiple transactions over time
            entity.HasOne(e => e.BookCopy)
                .WithMany(bc => bc.Borrowings)
                .HasForeignKey(e => e.CopyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FINE

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.ToTable("fines");
            entity.HasKey(e => e.FineId);

            entity.Property(e => e.FineId)
                .HasColumnName("fine_id");

            entity.Property(e => e.MemberId)
                .HasColumnName("member_id")
                .IsRequired();

            entity.Property(e => e.BorrowingId)
                .HasColumnName("borrowing_id")
                .IsRequired();

            entity.Property(e => e.FineType)
                .HasColumnName("fine_type")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.PaidAmount)
                .HasColumnName("paid_amount")
                .HasPrecision(10, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.IsPaid)
                .HasColumnName("is_paid")
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // Each fine is for one member only. But a single member can have a lot of fines.
            entity.HasOne(e => e.Member)
                .WithMany(m => m.Fines)
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Each Fine is related to a single transaction. But for each transaction, there can be multiple fines. Late, damage, etc
            entity.HasOne(e => e.Borrowing)
                .WithMany(b => b.Fines)
                .HasForeignKey(e => e.BorrowingId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // FINE PAYMENT

        modelBuilder.Entity<FinePayment>(entity =>
        {
            entity.ToTable("fine_payments");
            entity.HasKey(e => e.PaymentId);

            entity.Property(e => e.PaymentId)
                .HasColumnName("payment_id");

            entity.Property(e => e.FineId)
                .HasColumnName("fine_id")
                .IsRequired();

            entity.Property(e => e.AmountPaid)
                .HasColumnName("amount_paid")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.PaymentDate)
                .HasColumnName("payment_date")
                .HasDefaultValueSql("NOW()");

            // Each payment is only issues for one fine. 
            // But Each fine can have multiple small paymetns. Partial Payments
            entity.HasOne(e => e.Fine)
                .WithMany(f => f.Payments)
                .HasForeignKey(e => e.FineId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // FINE CONFIG
        // Truth Table
        modelBuilder.Entity<FineConfig>(entity =>
        {
            entity.ToTable("fine_config");
            entity.HasKey(e => e.FineConfigId);

            entity.Property(e => e.FineConfigId)
                .HasColumnName("fine_config_id");

            entity.Property(e => e.FineType)
                .HasColumnName("fine_type")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.MaxUnpaidFineThreshold)
                .HasColumnName("max_unpaid_fine_threshold")
                .HasPrecision(10, 2);

            entity.HasIndex(e => e.FineType).IsUnique();

            // default fine amounts
            entity.HasData(
                new FineConfig { FineConfigId = 1, FineType = "LateReturn", Amount = 10.00m, MaxUnpaidFineThreshold = 500.00m },
                new FineConfig { FineConfigId = 2, FineType = "MinorDamage", Amount = 200.00m, MaxUnpaidFineThreshold = 500.00m },
                new FineConfig { FineConfigId = 3, FineType = "DamagedBeyondUsable", Amount = 500.00m, MaxUnpaidFineThreshold = 500.00m },
                new FineConfig { FineConfigId = 4, FineType = "Lost", Amount = 1000.00m, MaxUnpaidFineThreshold = 500.00m }
            );
        });
    }
}
