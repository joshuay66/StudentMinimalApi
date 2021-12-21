using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<StudentDb>(opt => opt.UseInMemoryDatabase("StudentList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Student API");

app.MapGet("/studentrecords", async (StudentDb db) =>
    await db.Students.ToListAsync());

app.MapGet("/studentrecords/graduated", async (StudentDb db) =>
    await db.Students.Where(t => t.HasGraduated).ToListAsync());

app.MapGet("/studentrecords/{id}", async (int id, StudentDb db) =>
    await db.Students.FindAsync(id)
        is Student student
            ? Results.Ok(student)
            : Results.NotFound());

app.MapPost("/studentrecords", async (Student student, StudentDb db) =>
{
    db.Students.Add(student);
    await db.SaveChangesAsync();

    return Results.Created($"/studentrecords/{student.Id}", student);
});

app.MapPut("/studentrecords/{id}", async (int id, Student inputStudent, StudentDb db) =>
{
    var student = await db.Students.FindAsync(id);

    if (student is null) return Results.NotFound();

    student.FirstName = inputStudent.FirstName;
    student.LastName = inputStudent.LastName;
    student.HasGraduated = inputStudent.HasGraduated;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/studentrecords/{id}", async (int id, StudentDb db) =>
{
    if (await db.Students.FindAsync(id) is Student student)
    {
        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return Results.Ok(student);
    }

    return Results.NotFound();
});


app.Run();

class StudentDb : DbContext
{
    public StudentDb(DbContextOptions<StudentDb> options)
        : base(options) { }

    public DbSet<Student> Students => Set<Student>();
}

class Student
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool HasGraduated { get; set; }
}