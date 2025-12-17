var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Lägg till session-tjänster
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.Use(async (context, next) =>
{
	var path = context.Request.Path;
	var isAuthenticated = context.Session.GetInt32("UserID") != null;
	var endpoint = context.GetEndpoint();

	var allowAnonymous = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>();
	var isPublicPath =
		path.HasValue &&
		(
			path.StartsWithSegments("/user/login", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/user/registeruser", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/othelloproject.styles.css", StringComparison.OrdinalIgnoreCase)
		);

// Om endpoint inte är resolved ännu → låt requesten passera
if (endpoint == null)
{
    await next();
    return;
}

if (!isAuthenticated && allowAnonymous is null && !isPublicPath)
{
    context.Response.Redirect("/User/Login");
    return;
}

	await next();
});

// Lås fast användaren på brädet när ett spel pågår (GameName i session)
app.Use(async (context, next) =>
{
	var inGame = !string.IsNullOrEmpty(context.Session.GetString("GameName"));

	if (!inGame)
	{
		await next();
		return;
	}

	var path = context.Request.Path;
	var allowedPath =
		path.StartsWithSegments("/games/othelloboard", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/games/othellogameboard", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/games/makemove", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/games/leavegame", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase) ||
		path.StartsWithSegments("/othelloproject.styles.css", StringComparison.OrdinalIgnoreCase)||
		path.StartsWithSegments("/games/finishedgame", StringComparison.OrdinalIgnoreCase);

	if (!allowedPath)
	{
		context.Response.Redirect("/Games/OthelloBoard");
		return;
	}

	await next();
});

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
