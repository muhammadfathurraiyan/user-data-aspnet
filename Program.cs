using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDB>(opt => opt.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var myPolicyName = "MyPolicyName";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: myPolicyName,
        configurePolicy: policy =>
        {
            policy.WithOrigins("http://localhost:5173");
        }
    );
});

var app = builder.Build();

app.UseCors(configurePolicy: policy => policy.WithOrigins("http://localhost:5173"));

app.MapGet("/", () => "Hello World!");

app.MapGet(
    "/api",
    async (AppDB db) =>
        await db.Personals.Include(p => p.Gender).Include(p => p.Hobby).ToListAsync()
);

app.MapPost(
    "/api",
    async (Data data, AppDB db) =>
    {
        var personals = data.Personals;
        var hobbies = data.Hobbies;
        var genders = data.Genders;

        for (int i = 0; i < personals.Count; i++)
        {
            if ((i + 1) % 100 == 0 && personals[i].HobbyId == 1)
            {
                return Results.BadRequest($"Error at row {i + 1}: hobby 'Tidur' is not allowed.");
            }
        }

        db.Hobbies.AddRange(hobbies);
        db.Genders.AddRange(genders);
        db.Personals.AddRange(personals);
        await db.SaveChangesAsync();

        return Results.Ok("Data successfully inserted.");
    }
);

app.MapDelete(
    "/api",
    async (AppDB db) =>
    {
        db.Personals.RemoveRange(db.Personals);
        db.Genders.RemoveRange(db.Genders);
        db.Hobbies.RemoveRange(db.Hobbies);

        await db.SaveChangesAsync();

        return Results.Ok("All records have been deleted.");
    }
);

app.Run();
