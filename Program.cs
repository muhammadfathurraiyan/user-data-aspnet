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
            policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

app.UseCors(myPolicyName);

app.MapGet("/", () => "Hello World!");

app.MapGet(
    "/api",
    async (AppDB db) =>
        await db.Personals.Include(p => p.Gender).Include(p => p.Hobby).ToListAsync()
);

app.MapPost(
    "/api",
    async (Request req, AppDB db) =>
    {
        // Step 1: Insert gender
        var existingGenders = await db.Genders.ToDictionaryAsync(g => g.Name!, g => g.Id);
        foreach (var genderName in req.Payload.Select(p => p.GenderName).Distinct())
        {
            if (genderName != null && !existingGenders.ContainsKey(genderName))
            {
                var gender = new Gender { Name = genderName };
                db.Genders.Add(gender);
                await db.SaveChangesAsync();
                existingGenders[genderName] = gender.Id;
            }
        }

        // Step 2: Insert hobbie
        var existingHobbies = await db.Hobbies.ToDictionaryAsync(h => h.Name!, h => h.Id);
        foreach (var hobbyName in req.Payload.Select(p => p.HobbyName).Distinct())
        {
            if (hobbyName != null && !existingHobbies.ContainsKey(hobbyName))
            {
                var hobby = new Hobby { Name = hobbyName };
                db.Hobbies.Add(hobby);
                await db.SaveChangesAsync();
                existingHobbies[hobbyName] = hobby.Id;
            }
        }

        // Step 3: validasi Hobby
        for (int i = 0; i < req.Payload.Count; i++)
        {
            if ((i + 1) % 100 == 0 && req.Payload[i].HobbyName == "Tidur")
            {
                return Results.Text(
                    $"Terdapat error pada baris {i + 1} tidak menyukai hobi tidur"
                );
            }
        }

        // Step 4: insert personal
        var personals = req
            .Payload.Select(p => new Personal
            {
                Name = p.Name,
                GenderId = existingGenders[p.GenderName!],
                HobbyId = existingHobbies[p.HobbyName!],
                Age = p.Age,
            })
            .ToList();

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
