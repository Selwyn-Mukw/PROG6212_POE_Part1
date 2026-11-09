using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

const string connectionString =
    "Server=Jivo;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;";

var passwordHasher = new PasswordHasher<string>();

Console.WriteLine("RaceDay Participant Password Reset");
Console.WriteLine("-----------------------------------");

Console.Write("Enter Participant ID: ");

if (!int.TryParse(Console.ReadLine(), out int participantId))
{
    Console.WriteLine("Invalid Participant ID.");
    return;
}

Console.Write("Enter a new password: ");
string newPassword = Console.ReadLine() ?? "";

if (string.IsNullOrWhiteSpace(newPassword) ||
    newPassword.Length < 8)
{
    Console.WriteLine(
        "Password must contain at least 8 characters.");
    return;
}

try
{
    using var connection = new SqlConnection(connectionString);

    await connection.OpenAsync();

    const string selectSql = @"
        SELECT Email
        FROM dbo.[USER]
        WHERE UserID = @UserID
          AND Role = @Role;";

    using var selectCommand = new SqlCommand(
        selectSql, connection);

    selectCommand.Parameters.AddWithValue(
        "@UserID", participantId);

    selectCommand.Parameters.AddWithValue(
        "@Role", "Participant");

    object? result = await selectCommand.ExecuteScalarAsync();

    if (result == null || result == DBNull.Value)
    {
        Console.WriteLine(
            "No participant account was found with that ID.");
        return;
    }

    string email = Convert.ToString(result)!;

    string passwordHash =
        passwordHasher.HashPassword(email, newPassword);

    const string updateSql = @"
        UPDATE dbo.[USER]
        SET PasswordHash = @PasswordHash
        WHERE UserID = @UserID
          AND Role = @Role;";

    using var updateCommand = new SqlCommand(
        updateSql, connection);

    updateCommand.Parameters.AddWithValue(
        "@PasswordHash", passwordHash);

    updateCommand.Parameters.AddWithValue(
        "@UserID", participantId);

    updateCommand.Parameters.AddWithValue(
        "@Role", "Participant");

    int rowsUpdated = await updateCommand.ExecuteNonQueryAsync();

    if (rowsUpdated == 1)
    {
        Console.WriteLine(
            $"Password reset successfully for Participant ID {participantId}.");
    }
    else
    {
        Console.WriteLine(
            "Password reset failed. No account was updated.");
    }
}
catch (SqlException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}