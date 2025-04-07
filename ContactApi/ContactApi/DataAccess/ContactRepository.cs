using ContactApi.Models;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient for modern .NET
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data; // Contains DBNull
using System.Threading.Tasks;

namespace ContactApi.DataAccess
{
    /// <summary>
    /// Implements IContactRepository using ADO.NET for direct SQL Server interaction.
    /// </summary>
    public class ContactRepository : IContactRepository // Ensure it implements the interface
    {
        // Readonly field to store the connection string retrieved from configuration.
        private readonly string _connectionString;

        /// <summary>
        /// Constructor that receives IConfiguration via dependency injection.
        /// Used to access application settings like connection strings.
        /// </summary>
        public ContactRepository(IConfiguration configuration)
        {
            // Retrieve the connection string named "DefaultConnection" from appsettings.json.
            // Throw an exception if it's not found to prevent runtime errors later.
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
        }

        // --- Helper Methods ---

        /// <summary>
        /// Creates, opens, and returns a new SqlConnection based on the stored connection string.
        /// </summary>
        private SqlConnection GetOpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Maps a single row from a SqlDataReader to a Contact object.
        /// </summary>
        /// <param name="reader">The active SqlDataReader positioned at the row to map.</param>
        /// <returns>A populated Contact object.</returns>
        private Contact MapToContact(SqlDataReader reader)
        {
            return new Contact
            {
                // Get column values by name using GetOrdinal for resilience to column order changes.
                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                // Handle potential database NULL values gracefully.
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"))
            };
        }

        // --- Interface Implementations (CRUD Operations using ADO.NET) ---

        /// <summary>
        /// Retrieves all contacts from the database asynchronously.
        /// </summary>
        public async Task<IEnumerable<Contact>> GetAllAsync()
        {
            var contacts = new List<Contact>();
            const string query = "SELECT ID, FirstName, LastName, Email, Phone FROM dbo.Contacts ORDER BY LastName, FirstName;";

            // 'using' statements ensure disposal of resources.
            using (var connection = GetOpenConnection())
            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync()) // Await the async database call
            {
                while (await reader.ReadAsync()) // Await reading each row
                {
                    contacts.Add(MapToContact(reader));
                }
            }
            return contacts; // Return the populated list
        }

        /// <summary>
        /// Retrieves a single contact by its ID asynchronously.
        /// </summary>
        public async Task<Contact?> GetByIdAsync(int id) // Returns nullable Contact
        {
            Contact? contact = null; // Initialize as null
            const string query = "SELECT ID, FirstName, LastName, Email, Phone FROM dbo.Contacts WHERE ID = @ID;";

            using (var connection = GetOpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                // Use parameters to prevent SQL injection.
                command.Parameters.AddWithValue("@ID", id);

                using (var reader = await command.ExecuteReaderAsync()) // Await the database call
                {
                    // Check if any row was returned.
                    if (await reader.ReadAsync()) // Await reading the row
                    {
                        // Map data if found.
                        contact = MapToContact(reader);
                    }
                }
            }
            // Return the found contact or null if no row was read.
            return contact;
        }

        /// <summary>
        /// Creates a new contact in the database asynchronously.
        /// </summary>
        public async Task<int> CreateAsync(Contact contact)
        {
            // SQL query to insert data and return the new identity ID.
            const string query = @"
                INSERT INTO dbo.Contacts (FirstName, LastName, Email, Phone)
                OUTPUT INSERTED.ID
                VALUES (@FirstName, @LastName, @Email, @Phone);";

            using (var connection = GetOpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                // Add parameters safely.
                command.Parameters.AddWithValue("@FirstName", contact.FirstName);
                command.Parameters.AddWithValue("@LastName", contact.LastName);
                // Handle potential nulls from C# -> DB.
                command.Parameters.AddWithValue("@Email", (object?)contact.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object?)contact.Phone ?? DBNull.Value);

                // ExecuteScalarAsync is used for queries returning a single value (the new ID).
                var result = await command.ExecuteScalarAsync(); // Await the database call

                // Check if the result is valid before converting.
                if (result == null || result == DBNull.Value)
                {
                    // Throw exception if ID wasn't returned, indicating a problem.
                    throw new Exception("Failed to create contact, database did not return a new ID.");
                }
                // Convert the returned ID (object) to int and return it.
                return Convert.ToInt32(result);
            }
        }

        /// <summary>
        /// Updates an existing contact in the database asynchronously.
        /// </summary>
        /// <returns>True if a row was updated; otherwise false.</returns>
        public async Task<bool> UpdateAsync(int id, Contact contact)
        {
            // Define the SQL UPDATE statement.
            const string query = @"
                UPDATE dbo.Contacts SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Phone = @Phone
                WHERE ID = @ID;"; // Use ID in WHERE clause

            using (var connection = GetOpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                // Add parameters safely, including the ID for the WHERE clause.
                command.Parameters.AddWithValue("@ID", id); // Use the 'id' passed to the method
                command.Parameters.AddWithValue("@FirstName", contact.FirstName);
                command.Parameters.AddWithValue("@LastName", contact.LastName);
                command.Parameters.AddWithValue("@Email", (object?)contact.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object?)contact.Phone ?? DBNull.Value);

                // ExecuteNonQueryAsync is used for commands that don't return data (UPDATE, DELETE, INSERT without OUTPUT).
                // It returns the number of rows affected.
                int affectedRows = await command.ExecuteNonQueryAsync(); // Await the database call

                // Return true if one or more rows were affected, indicating success.
                return affectedRows > 0;
            }
        }

        /// <summary>
        /// Deletes a contact from the database asynchronously.
        /// </summary>
        /// <returns>True if a row was deleted; otherwise false.</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            // Define the SQL DELETE statement.
            const string query = "DELETE FROM dbo.Contacts WHERE ID = @ID;";

            using (var connection = GetOpenConnection())
            using (var command = new SqlCommand(query, connection))
            {
                // Add the ID parameter safely.
                command.Parameters.AddWithValue("@ID", id);

                // ExecuteNonQueryAsync returns the number of rows affected.
                int affectedRows = await command.ExecuteNonQueryAsync(); // Await the database call

                // Return true if one or more rows were affected (deleted).
                return affectedRows > 0;
            }
        }
    }
}