using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

const string connectionString =
    "Server=Jivo;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;";

var passwordHasher = new PasswordHasher<string>();

Console.WriteLine("RaceDay Organiser Password Reset");
Console.WriteLine("--------------------------------");

Console.Write("Enter the organiser's email address: ");
string email = Console.ReadLine()?.Trim() ?? "";

Console.Write("Enter a new password: ");
string newPassword = Console.ReadLine() ?? "";

if (string.IsNullOrWhiteSpace(email) ||
    string.IsNullOrWhiteSpace(newPassword) ||
    newPassword.Length < 8)
{
    Console.WriteLine(
        "Email and password are required. Password must have at least 8 characters.");
    return;
}

try
{
    using var connection = new SqlConnection(connectionString);

    await connection.OpenAsync();

    const string selectSql = @"
        SELECT UserID
        FROM dbo.[USER]
        WHERE Email = @Email
          AND Role = @Role;";

    using var selectCommand = new SqlCommand(
        selectSql, connection);

    selectCommand.Parameters.AddWithValue("@Email", email);
    selectCommand.Parameters.AddWithValue("@Role", "Organiser");

    object? result = await selectCommand.ExecuteScalarAsync();

    if (result == null || result == DBNull.Value)
    {
        Console.WriteLine(
            "No organiser account was found with that email address.");
        return;
    }

    int userId = Convert.ToInt32(result);

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
        "@UserID", userId);

    updateCommand.Parameters.AddWithValue(
        "@Role", "Organiser");

    int rowsUpdated = await updateCommand.ExecuteNonQueryAsync();

    if (rowsUpdated == 1)
    {
        Console.WriteLine(
            "Password reset successfully.");
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