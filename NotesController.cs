using Microsoft.AspNetCore.Mvc;
using NotesApi.Models;
using Microsoft.Data.SqlClient;
using Dapper;

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

        [HttpGet]
        public ActionResult<IEnumerable<Note>> GetNotes()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var notes = connection.Query<Note>("SELECT * FROM Notes").ToList();
                return Ok(notes);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<Note> GetNoteById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var note = connection.QuerySingleOrDefault<Note>("SELECT * FROM Notes WHERE Id = @Id", new { Id = id });
                if (note == null)
                {
                    return NotFound();
                }
                return Ok(note);
            }
        }

        [HttpPost]
        public ActionResult<Note> CreateNote([FromBody] Note note)
        {
            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            using (var connection = new SqlConnection(_connectionString))
            {
                var id = connection.QuerySingle<int>("INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt) VALUES (@Title, @Content, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() as int);", note);
                note.Id = id;
                return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateNote(int id, [FromBody] Note note)
        {
            note.UpdatedAt = DateTime.UtcNow;

            using (var connection = new SqlConnection(_connectionString))
            {
                var affectedRows = connection.Execute("UPDATE Notes SET Title = @Title, Content = @Content, UpdatedAt = @UpdatedAt WHERE Id = @Id", new { note.Title, note.Content, note.UpdatedAt, Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNote(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var affectedRows = connection.Execute("DELETE FROM Notes WHERE Id = @Id", new { Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
    }
}