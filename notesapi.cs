// Project named "notesapi" for making notes, editing, deleting, by Bartosz "Barteeq" Skowroński
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace NotesAPI
{
    public class Program
    {
        private static string connectionString = "Server=localhost;Database=notes;Uid=username;Pwd=password;";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000");
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class NotesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            List<Note> notes = new List<Note>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM notes";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Note note = new Note();
                            note.Id = reader.GetInt32("id");
                            note.Title = reader.GetString("title");
                            note.Content = reader.GetString("content");

                            notes.Add(note);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(notes);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Note note = new Note();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM notes WHERE id = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            note.Id = reader.GetInt32("id");
                            note.Title = reader.GetString("title");
                            note.Content = reader.GetString("content");
                        }
                        else
                        {
                            return NotFound(new { message = "Note not found" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(note);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Note note)
        {
            if (string.IsNullOrEmpty(note.Title) || string.IsNullOrEmpty(note.Content))
            {
                return BadRequest(new { message = "Title and content are required" });
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO notes (title, content) VALUES (@title, @content)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@title", note.Title);
                    command.Parameters.AddWithValue("@content", note.Content);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 1)
                    {
                        note.Id = (int)command.LastInsertedId;
                        return CreatedAtAction(nameof(Get), new { id = note.Id }, note);
                    }
                    else
                    {
                        return BadRequest(new { message = "Note not created" });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Note note)
        {
            if (string.IsNullOrEmpty(note.Title) || string.IsNullOrEmpty(note.Content))
            {
                return BadRequest(new { message = "Title and content are required" });
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
                {
                    connection.Open();

                    string query = "UPDATE notes SET title = @title, content = @content WHERE id = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@title", note.Title);
                    command.Parameters.AddWithValue("@content", note.Content);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 1)
                    {
                        return Ok(note);
                    }
                    else
                    {
                        return NotFound(new { message = "Note not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM notes WHERE id = @id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 1)
                    {
                        return Ok(new { message = "Note deleted" });
                    }
                    else
                    {
                        return NotFound(new { message = "Note not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
