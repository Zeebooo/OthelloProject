var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Lägg till session-tjänster
builder.Services.AddDistributedMemoryCache(); // behövs för session
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Tvinga inloggning på alla sidor utom login/registrering och statiska filer
app.Use(async (context, next) =>
{
	var path = context.Request.Path;
	var isAuthenticated = context.Session.GetInt32("UserID") != null;

	var isPublicPath =
		path.HasValue &&
		(
			path.StartsWithSegments("/user/login", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/user/registeruser", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
			path.StartsWithSegments("/images", StringComparison.OrdinalIgnoreCase)
		);

	if (!isAuthenticated && !isPublicPath)
	{
		context.Response.Redirect("/User/Login");
		return;
	}

	await next();
});

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
