// Make sure you have the necessary using statements at the top
using ContactApi.Models; // You need access to the Contact class definition
using System.Collections.Generic; // Required for IEnumerable<T>
using System.Threading.Tasks; // Required for Task<> (for async methods)

// Define the namespace matching your folder structure and project name
namespace ContactApi.DataAccess
{
    /// <summary>
    /// Interface defining the contract for contact data operations.
    /// Specifies WHAT operations can be performed, not HOW they are performed.
    /// </summary>
    public interface IContactRepository
    {
        /// <summary>
        /// Gets all contacts asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains a collection of all contacts.</returns>
        Task<IEnumerable<Contact>> GetAllAsync();

        /// <summary>
        /// Gets a single contact by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the contact to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains the contact if found; otherwise, null.</returns>
        Task<Contact?> GetByIdAsync(int id); // Use Contact? for nullable return type

        /// <summary>
        /// Creates a new contact asynchronously.
        /// </summary>
        /// <param name="contact">The contact object to create (ID will be ignored/generated).</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains the ID of the newly created contact.</returns>
        Task<int> CreateAsync(Contact contact);

        /// <summary>
        /// Updates an existing contact asynchronously.
        /// </summary>
        /// <param name="id">The ID of the contact to update.</param>
        /// <param name="contact">The contact object with updated information.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result is true if the update was successful; otherwise, false (e.g., not found).</returns>
        Task<bool> UpdateAsync(int id, Contact contact);

        /// <summary>
        /// Deletes a contact by its ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the contact to delete.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result is true if the deletion was successful; otherwise, false (e.g., not found).</returns>
        Task<bool> DeleteAsync(int id);
    }
}