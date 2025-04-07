using Microsoft.AspNetCore.Mvc; // Base MVC attributes and types
using Microsoft.Extensions.Logging; // For logging
using Microsoft.Data.SqlClient;     // For catching SqlException
using System;                         // For Exception type
using System.Collections.Generic;   // For IEnumerable<>
using System.Threading.Tasks;       // For Task<> (async operations)
using ContactApi.Models;            // Your Contact model
using ContactApi.DataAccess;        // Your IContactRepository interface and implementation
using Microsoft.AspNetCore.Http;    // For StatusCodes

namespace ContactApi.Controllers
{
    [Route("api/[controller]")] // Base route: /api/contacts
    [ApiController] // Enables API-specific behaviors like automatic model validation
    public class ContactsController : ControllerBase
    {
        // Dependencies injected via constructor
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<ContactsController> _logger;

        // Constructor for dependency injection
        public ContactsController(IContactRepository contactRepository, ILogger<ContactsController> logger)
        {
            _contactRepository = contactRepository;
            _logger = logger;
        }

        // GET: api/Contacts
        // Retrieves all contacts
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Contact>))] // Success
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            try
            {
                var contacts = await _contactRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {ContactCount} contacts.", contacts?.Count() ?? 0);
                return Ok(contacts); // Return 200 OK with the list of contacts
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all contacts.");
                // Return 500 Internal Server Error status
                return StatusCode(500, new { message = "An internal server error occurred while retrieving contacts." });
            }
        }

        // GET: api/Contacts/5
        // Retrieves a single contact by its ID
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Contact))] // Success, contact found
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Contact not found
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<ActionResult<Contact>> GetContact(int id)
        {
            try
            {
                var contact = await _contactRepository.GetByIdAsync(id);

                if (contact == null)
                {
                    _logger.LogWarning("Attempted to retrieve non-existent contact with ID {ContactId}.", id);
                    // Return 404 Not Found status
                    return NotFound(new { message = $"Contact with ID {id} not found." });
                }

                _logger.LogInformation("Retrieved contact with ID {ContactId}.", id);
                return Ok(contact); // Return 200 OK with the found contact
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving contact with ID {ContactId}.", id);
                // Return 500 Internal Server Error status
                return StatusCode(500, new { message = $"An internal server error occurred while retrieving contact {id}." });
            }
        }

        // POST: api/Contacts
        // Creates a new contact
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Contact))] // Success, contact created
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Invalid input data
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Duplicate data (e.g., email)
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<ActionResult<Contact>> PostContact([FromBody] Contact contact) // <<<--- Ensure [FromBody] attribute is present
        {
            // Basic Server-side validation (client-side validation should also exist)
            if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
            {
                _logger.LogWarning("Create contact attempt failed due to missing required fields.");
                // Return 400 Bad Request with details
                return BadRequest(new { message = "First Name and Last Name are required." });
            }
            // Add other necessary validations here (e.g., email format if required server-side)

            try
            {
                // Call the repository to create the contact in the database
                var newId = await _contactRepository.CreateAsync(contact);
                // Assign the newly generated ID back to the contact object
                contact.ID = newId;

                _logger.LogInformation("Created new contact with ID {ContactId}.", newId);
                // Return 201 Created status.
                // Include a 'Location' header pointing to the newly created resource (GetContact action).
                // Include the created contact object in the response body.
                return CreatedAtAction(nameof(GetContact), new { id = contact.ID }, contact);
            }
            // Catch specific database exceptions, like unique constraint violations
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) // SQL Server unique constraint error codes
            {
                _logger.LogWarning(ex, "Create contact attempt failed due to duplicate email {Email}.", contact.Email);
                // Return 409 Conflict status
                return Conflict(new { message = $"A contact with the email '{contact.Email}' already exists." });
            }
            // Catch general exceptions
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new contact.");
                // Return 500 Internal Server Error status
                return StatusCode(500, new { message = "An internal server error occurred while creating the contact." });
            }
        }

        // PUT: api/Contacts/5
        // Updates an existing contact
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Success, update performed (no content returned)
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Invalid input data or ID mismatch
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Contact with the specified ID not found
        [ProducesResponseType(StatusCodes.Status409Conflict)] // Update would cause duplicate data
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<IActionResult> PutContact(int id, [FromBody] Contact contact) // <<<--- Ensure [FromBody] attribute is present
        {
            // Validate that the ID from the route matches the ID in the request body
            if (id != contact.ID)
            {
                _logger.LogWarning("Update contact attempt failed for route ID {RouteId}: Payload ID mismatch ({PayloadId}).", id, contact.ID);
                // Return 400 Bad Request status
                return BadRequest(new { message = "ID mismatch between route parameter and contact payload." });
            }

            // Basic Server-side validation
            if (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName))
            {
                _logger.LogWarning("Update contact attempt for ID {ContactId} failed due to missing required fields.", id);
                // Return 400 Bad Request status
                return BadRequest(new { message = "First Name and Last Name are required." });
            }
            // Add other necessary validations

            try
            {
                // Optional but recommended: Check if the contact actually exists before trying to update
                var existingContact = await _contactRepository.GetByIdAsync(id);
                if (existingContact == null)
                {
                    _logger.LogWarning("Update contact attempt failed: Contact with ID {ContactId} not found.", id);
                    // Return 404 Not Found status
                    return NotFound(new { message = $"Contact with ID {id} not found for update." });
                }

                // Call the repository to update the contact
                var updated = await _contactRepository.UpdateAsync(id, contact);

                // Note: UpdateAsync returns true if rows were affected. If !updated, it might mean
                // the record exists but no fields actually changed, or a concurrency issue.
                // Returning NoContent is generally acceptable even if no rows changed.
                if (!updated && existingContact != null) // Check existingContact again for clarity if needed
                {
                    _logger.LogWarning("Update operation for contact ID {ContactId} reported no rows affected (potentially no change or rare race condition).", id);
                }

                _logger.LogInformation("Updated contact with ID {ContactId}.", id);
                // Return 204 No Content on successful update (standard REST practice for PUT)
                return NoContent();
            }
            // Catch specific database exceptions, like unique constraint violations during update
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                _logger.LogWarning(ex, "Update contact attempt for ID {ContactId} failed due to duplicate email {Email}.", id, contact.Email);
                // Return 409 Conflict status
                return Conflict(new { message = $"Cannot update contact, the email '{contact.Email}' is already in use by another contact." });
            }
            // Catch general exceptions
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating contact with ID {ContactId}.", id);
                // Return 500 Internal Server Error status
                return StatusCode(500, new { message = $"An internal server error occurred while updating contact {id}." });
            }
        }

        // DELETE: api/Contacts/5
        // Deletes a contact by its ID
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Success, deletion performed (no content returned)
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Contact with the specified ID not found
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Server error
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                // Call the repository to delete the contact
                var deleted = await _contactRepository.DeleteAsync(id);

                // If DeleteAsync returns false, it means the contact wasn't found
                if (!deleted)
                {
                    _logger.LogWarning("Delete contact attempt failed: Contact with ID {ContactId} not found (or already deleted).", id);
                    // Return 404 Not Found status
                    return NotFound(new { message = $"Contact with ID {id} not found for deletion." });
                }

                _logger.LogInformation("Deleted contact with ID {ContactId}.", id);
                // Return 204 No Content on successful deletion
                return NoContent();
            }
            // Catch general exceptions during deletion
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting contact with ID {ContactId}.", id);
                // Return 500 Internal Server Error status
                return StatusCode(500, new { message = $"An internal server error occurred while deleting contact {id}." });
            }
        }
    }
}