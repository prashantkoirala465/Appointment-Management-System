using AppointmentSystem.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentSystem.Web.Data
{
    /// Configuration for the Appointment entity
    /// This tells Entity Framework Core exactly how to map our Appointment class to a database table
    /// We do this here instead of cluttering our model with database-specific attributes
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            // Set Id as the primary key
            // Every appointment needs a unique identifier
            builder.HasKey(x => x.Id);

            // Configure ClientName column
            // - Required: Can't book an appointment without knowing who it's for
            // - varchar(200): More efficient than nvarchar for name storage
            builder.Property(x => x.ClientName)
                .IsRequired()
                .HasColumnType("varchar(200)");

            // Email is optional (not everyone wants email notifications)
            // But if provided, we store it in a varchar(255) column
            builder.Property(x => x.ClientEmail)
                .HasColumnType("varchar(255)");

            // ClientPhone is required - we need a way to contact the client!
            // Using varchar(20) to save space since phone numbers are ASCII
            builder.Property(x => x.ClientPhone)
                .IsRequired()
                .HasColumnType("varchar(20)");

            // StartTime must be provided
            // We need to know when the appointment is happening
            builder.Property(x => x.StartTime)
                .IsRequired();

            // DurationMinutes is mandatory
            // We need to know how long to block off the staff member's time
            builder.Property(x => x.DurationMinutes)
                .IsRequired();

            // Status is required and stored as varchar
            // Examples: "Scheduled", "Completed", "Cancelled"
            // varchar(50) is plenty for any status we might need
            builder.Property(x => x.Status)
                .IsRequired()
                .HasColumnType("varchar(50)");

            // Notes are optional - not every appointment needs special instructions
            // Limited to 500 chars to prevent people from writing novels
            builder.Property(x => x.Notes)
                .HasColumnType("varchar(500)");

            // CreatedAtUtc tracks when the appointment was first made
            // This is audit trail data - always required
            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            // UpdatedAtUtc is optional - only set when the appointment is modified
            // Null means it's never been edited since creation
            builder.Property(x => x.UpdatedAtUtc);

            // Define the relationship between Appointment and Staff
            // This sets up a foreign key constraint in the database
            // - HasOne: Each appointment has one staff member
            // - WithMany: Each staff member can have many appointments
            // - HasForeignKey: The StaffId column is the foreign key
            // - OnDelete(Cascade): If we delete a staff member, delete their appointments too
            //   (though in practice, we soft-delete staff by setting IsActive=false)
            builder.HasOne(x => x.Staff)
                .WithMany(s => s.Appointments)
                .HasForeignKey(x => x.StaffId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}