using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Threading.Tasks;

namespace NotesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly string _connectionString;

        public NotesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// Gets a paginated list of notes.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of notes.</returns>
        [HttpGet]
             public async Task<ActionResult> GetNotes(int page = 1, int pageSize = 10)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var offset = (page - 1) * pageSize;
                var notes = (await connection.QueryAsync<Note>("SELECT * FROM Notes ORDER BY Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY", new { Offset = offset, PageSize = pageSize })).ToList();
                var totalNotes = await connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM Notes");
        
                var result = new
                {
                    TotalCount = totalNotes,
                    Page = page,
                    PageSize = pageSize,
                    Notes = notes
                };
        
                return Ok(result);
            }
        }

        /// <summary>
        /// Gets a note by its ID.
        /// </summary>
        /// <param name="id">The ID of the note.</param>
        /// <returns>The note with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNoteById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var note = await connection.QuerySingleOrDefaultAsync<Note>("SELECT * FROM Notes WHERE Id = @Id", new { Id = id });
                if (note == null)
                {
                    return NotFound();
                }
                return Ok(note);
            }
        }

        /// <summary>
        /// Creates a new note.
        /// </summary>
        /// <param name="note">The note to create.</param>
        /// <returns>The created note.</returns>
        [HttpPost]
        public async Task<ActionResult<Note>> CreateNote([FromBody] Note note)
        {
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            using (var connection = new SqlConnection(_connectionString))
            {
                var id = await connection.QuerySingleAsync<int>("INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt) VALUES (@Title, @Content, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() as int);", note);
                note.Id = id;
                return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
            }
        }

        /// <summary>
        /// Updates an existing note.
        /// </summary>
        /// <param name="id">The ID of the note to update.</param>
        /// <param name="note">The updated note.</param>
        /// <returns>No content if the update was successful.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
        {
            note.UpdatedAt = DateTime.UtcNow;

            using (var connection = new SqlConnection(_connectionString))
            {
                var affectedRows = await connection.ExecuteAsync("UPDATE Notes SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt WHERE Id = @Id", new { note.Title, note.Content, note.UpdatedAt, Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }

        /// <summary>
        /// Deletes a note by its ID.
        /// </summary>
        /// <param name="id">The ID of the note to delete.</param>
        /// <returns>No content if the deletion was successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var affectedRows = await connection.ExecuteAsync("DELETE FROM Notes WHERE Id = @Id", new { Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
    }
}