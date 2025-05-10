using ChatServer;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

app.MapControllers();

app.Run();
