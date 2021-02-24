# Section 2 - Building a Walking Skeleton Part One - API

## Creating the .Net API Project Using the DotNet CLI
- install dotnet 5 SDK
- `dotnet new sln`
    - creates the sln file in the containing folder with the folder's name
- `dotnet new webapi -o API`
    - creates the new web api project
    - -o gives the output directory
- `dotnet sln add API/`
    - adds the project to the solution

## Setting up VS Code to Work With C#
Install the following extensions:

1. C# for Visual Studio Code (powered by OmniSharp)
    - generate assets for build and debug
2. C# Extensions (make sure to get the new one, not the outdated one)
3. Material Icon Theme

Turned on the following VS Code settings

1. Auto Save
2. Can exclude folders from sidebar if you wish (I did not do this)

`Ctrl + Shift + P` shows all commands (has a search bar at the top) on windows.

## Getting to Know the API Project Files
`dotnet run`:

- builds + runs the project locally
- has http and https addresses
- need to make sure https trusts the cert provided with the sdk
- `dotnet dev-certs https --trust`
    - needs elevated permissions (run as admin)
    - tells the browser to trust the certificate from dotnet

Inside the controller
- controller in square brackets `[controller]` means that this is a route that will be replaced by our controller name
    - i.e. `WeatherForecastController` is decorated with `[Route("[controller]")]` becomes the route `/weatherforecast`

Every .Net app has a Program.cs class with a Main method.  This is where everything starts.  The `CreateHostBuilder` function starts the configuration of the app.  Sets up logging.  Returns inititialized host builder.  Points to the startup class (`startup.cs`).

Configuration is injected into `Startup.cs`. 

Inside `startup.cs`, there is a `ConfigureServices` Method.  This is referred to as the **dependancy injection container**.  If we want to make a class or services to other areas of our application, we can add them inside this container.  Dotnet Core will take care of creation and destruction of these services.

There is also a `Configure` Method.  This is used to configure the HTTP request pipeline.  When we make a request, it goes through a series of middleware on the way in and the way out.

1. Has a check for development
    - if it is development, calls the app.UseDeveloperExceptionPage
    - self explanatory
2. UseHttpsRedirection
    - if we came in on HTTP, redirects to HTTPS endpoint
3. UseRouting
    - enables us to, from the browser, route to the `/WeatherForecast` endpoint
4. UseAuthorization
    - doesn't do much just yet since we haven't configured authorization
5. UseEndpoints
    - Middleware to actually use endpoints 
    - MapControllers maps the controllers
        - sees what endpoints are available
            - Looks for `[Route]`, then the `[HttpGet]` (for a get request)

Inside appsettings.development.json, we changed the logging level for Microsoft from "Warning" to "Information" so that we get more information in the console when something is happening.

Inside Properties folder, there is a launchSettings.json.  When we run the app, it looks at the **API** section (since that is what we've named the project) to see what to use when we start the API

1. Set `"launchBrowser"` to false since we're creating an api and this isn't needed
2. `"applicationURL"` are the URLs we will run the app on
3. `"environmentVariables"` is where we would set any envvironment variables
    - currently has one called `"ASPNETCORE_ENVIRONMENT"` that tells the app to run in `"Development"` mode

We can also add a file watcher to watch for any changes on our files.  This command is `dotnet watch run`.  Now, every time we make a change and save, the application rebuilds itself and restarts.

# Creating Our First Entity

Our first entity is users, since a large part of our app will revolve around Users.

We created a new folder called **"entities"** to organize these new constructs.

An **Entity** is an abstraction of a physical thing.  A physical thing, in our app, is a user.  So we'll create a User Entity that will contain user related properties.  

To do this, right click the folder and add a new class called `"AppUser"`, which is in the file `"AppUser.cs"`.

We called this AppUser instead of just User since User is overused and to avoid confusion

We started off with adding properties by typing `"prop"` then tab to get an **automatically implemented property**.  Changed the name/data type to be what we need it to be.  Our first prop was `private int Id`.  By naming this property `"Id"`, Entity Framework will automatically increment the Id field each time a record is added to the database.

Second property was `"UserName"`.  we did not use a lowercase n for username to avoid a conflict with AspNet Core Identity that uses username with uppercase N.  We would need to do some refactoring later if we used the lowercase n here.

We talked about access settings:

1. `Public`
    - property can be get or set from anywhere in the app
2. `Protected`
    - property can be accessed in this class or any class that inherits from it
3. `Private`
    - property is only accessible from inside this class itself

We are using `public` so that entity framework can modify the properties and the entity itself.

Next, we'll introduce Entity Framework

# Introduction to Entity Framework

- An Object Relational Mapper (ORM)
- Translates our code into SQL commands that updat eour tables in the database
- Automates database related activities
- We have an AppUser entity, with an id int and username string prop
- When we add Entity Framework (EF), we create a class that derives from DbContext that we get from EF.
    - this class acts as a bridge from the entity classes and the database
    - primary class we use for interacting with the database
- EF lets us write **"LINQ"** queries (language integrated queries)

```c#
DbContext.Users.Add(new User {Id = 4, Name = John})
``` 
is transformed into (using EF & Sqlite for our project; we'll change the database later)

```sql
INSERT INTO Users(Id, Name) VALUES (4, John)
```

Entity Framework Features
- Querying (linq queries)
- Change Tracking
- Saving
- Concurrency
    - Uses optimistic Concurrency by default to protect overwriting changes by another user since data was fetched from the database
- Transactions
- Caching
    - repeated querying will return data from the cache
- Build-in conventions
    - i.e. the Id property follows a convention for creating the schema
- Configurations
    - if we wish to override conventions
- Migrations
    - gives us the ability to create a DB schema so that we can automatically generate our database in our database server
    - looks at the code we write, and creates the database schema we need to manage the database
    - This is referred to as Code First Database creation

# Adding Entity Framework to Our Project

First, we installed a nuget extension in VS Code called **"NuGet Gallery."**  This gives us a way to find and install packages within VS Code.  We wouldn't need this in Visual Studio.

Inside VS code, we used the show all commands command (ctrl + shift + p) to search for nuget gallery.

We found **Microsoft.EntityFrameworkCore.Sqlite** and installed the version corresponding to our SDK version.  If we were using a different database, we would install that package instead of Sqlite.  This package gets added to the API.csproj file.

Next, we'll create the DbContext class we mentioned previously.

# Adding a DbContext Class

DbContext acts as the bridge between our code and the database.

We added a new folder called **"Data"** to house anything related to our database.

We created a new class called `DataContext.cs.`  In this class, we will derive from the DbContext class included with EF.  We had an error since DbContext was not found.  

```
Note: If you see an error, you can hover over it to see what the error actually is.  The lightbulb on the left side will show fixes.  Clicking into the line with the error then pressing "ctrl + ." will give you a list of quick fixes that you can select from
```

The namespace this class is in (`API.Data`) is based off of the folder structure (`API/Data/DataContext.cs`)

The **DbContext** Instance represents a session with the database that we can use to query and save isntances of our entities.

We can use the quick fix to create a constructor for this class.  We added the options constructor and removed the `[Nullable]` line.  We'll be passing some options into this constructor when we add it to our dependency injection class.

We added a property of type `DbSet<AppUser>` called Users.  We had to include `API.Entities` here to use `AppUser`.

Open the `Startup` class (can search with `CTRL + P`).  We can use the `ConfigureServices` to inject our new DbContext class, and the `Configure` method to add a connection string for our database.

Inside the Dependancy Injection Container (`ConfigureServices` Method), ordering is not impportant. Here, we called the following method, making sure to use `DataContext` (the class we created) instead of `DbContext`
```c#
// "=>" is an arrow function, or lambda expression.  Options is the parameter //we'll pass to a statement block that we have inside the curly brackets.  These //are commonly used if we want to pass an expression as a parameter.
services.addDbContext<DataContext>(options => {
    options.UseSqlite("Connection String"); //this is a dummy connection string
});
```

# Creating the Connection String

This will allow us to connect to the database.

Typically, we add this to a configuration file.  While we are developing, we can use the `appsettings.Development.json` file since we don't mind other people seeing it.

At the top of this file, we added the following
```json
"ConnectionStrings": {
    "DefaultConnection": "Data source=datingapp.db"
}
```

This Sqlite connection string is super simple and only contains the name of the file we want the data to be stored in.

We are injecting the configuration into the `Startup` class, but we made some changes to the boilerplate code to change how we specify dependency injection.

Our configuration comes from a class called `IConfiguration`.

The code we started with:

```c#
public startup(IConfiguration configuration) 
{
    Configuration = configuration;
}

public IConfiguration Configuration { get; }
```

1. We first removed the variable declaration of `Iconfiguration Configuration`.  
2. Then, in the `startup` method, we shortened `configuration` to be `config`, and used the quick fix to `initialize the field from parameter`.  
3. We removed `Configuration = configuration`.
4. We removed the `this` keyword, and added `_` to our `startup` class property `config` so that it becomes `_config`
   - this is a common way of referring to properties from within the class instead of using `this`
5. When we construct this class, the `IConfiguration` is being injected into the class.  This gives us access to the configuration everywhere we need it inside the class.
6. We modified the **"Private Member Prefix"** to use `_`
7. We then searched for **"this"** and unchecked the setting to use `this` in ctors 
   - this comes from the C# extension

The final code is as follows
```c#
private readonly IConfiguration _config;

public Startup(IConfiguration config)
{
    _config = config;
}
```

Now, we can use this configuration!

Inside the Dependancy Injection Container (`ConfigureServices`), we can now set up our real connection string.

```c#
services.addDbContext<DataContext>(options => {
    options.UseSqlite(_config.GetConnectionString("DefaultConnection"); 
});
```

In order to actually create our Database, we need a tool.  This tool is called `dotnet-if`.  We can find this on `nuget.org`.  We need to find the version of EF we installed earlier, then copy the command (to prevent any problems) and run it in the terminal.  It is a global tool, so you don't need to worry about the folder you are in.

Once this is installed (with the app stopped), we can create the migration that will create the database based on the code we've written so far.

```
dotnet ef migrations add InitialCreate -o Data/Migrations

--note: If you don't call this folder "Migrations", you may get errors.
```

We got an error telling us that we need `Micorosft.EntityFrameworkCore.Design`.  We installed this package from NuGet Gallery and ran the command again to complete the Migration.

This command has created a "Migrations" folder inside our "Data" folder.  The `Initial Create` file has what to do going up (creating) and coming down (destroying).

EF knows, conventionally, to use the "Id" property from our entity as the primary key.

# Creating the Database Using Entity Framework Code First Migrations

To create our database, we will use the dotnet ef tool.

```
dotnet ef database update
```

If you get an error, such as build failed, make sure you stopped your API from running.  It does the following things:
1. Builds the application
2. Reads the migration history
3. Reads from our migrations and creates the table with the corresponding columns
4. Inserts record into migrations history

We can use an extension called `SQLite` to view our database.  We can `ctrl + shift + p` and search for `SQLite` and select `Open Database`.  It will then look for the db file, where you can select your .db file.

We have SQLite Explorer in the sidebar now where we can see the `Users` and `EFMigrations` tables.

We can right click the `Users` table and say `New Query Insert` where we write the SQL commands to get the data in.

```sql
INSERT INTO `Users` (Id, UserName)
VALUES (1, "Bob")

INSERT INTO `Users` (Id, UserName)
VALUES (2, "Tom")

INSERT INTO `Users` (Id, UserName)
VALUES (3, "Jane")
```

Highlight the above code, right click, and say run selected query.  This executes the commands and updates the database.  You can close the file afterwards.

We can then right click the Users table, select "show table" and we can see the records in the database.

# Adding a New API Controller

We started by examining the features of the `WeatherForecast` controller.
1. [ApiController]
   - This signifies that this particular class is of the type `ApiController`
2. [Route("[controller]")] 
   - the route to get to the controller
3. The controller needs to derive from (using the `:` operator) `ControllerBase`

We created a new class in the `Controllers` folder called `UsersController.cs`.  This means our route will be `/users`

We added the derive from `ControllerBase`, which required us to bring in `Microsoft.AspNetCore.Mvc`.

We gave our controller both of the above attributes.  However, our route attribute is slightly different so that each of our routes will start with `api/` :
```c#
[Route("api/[controller]")]
```

Next, we want to get data from our database.  To do this, we will use **Dependancy Injection**.  We put our cursor in the `UsersController` and used the quick fix to generate the Constructor for the controller.

We added `DataContext context` to the constructor and used the quick fix again to initalize the context field from parameter.  Now, inside our class, we have access to the database by using `_context`.

We want to add 2 endpoints, one for getting all users, and one for getting a single user. 

Both need the `[HttpGet]` decorator to indicate they are `Get` endpoints

```c#
[HttpGet]
//Actionresult of type IEnumerable of Type AppUser

//IEnumerables allow us to use simple iteration over a collection of a specified type.
//List will do the same thing, but it offers many more methods than we will need for this case.
public ActionResult<IEnumerable<AppUser>> GetUsers() {
    return _context.Users.ToList();
}

//pass in the route parameter to the function
// api/users/3 (the api/ part of the route is specified above by the route decorator)
[HttpGet("{id}")]

//here, we are not returning an IEnumerable of type AppUser.
//we are simply returning an AppUser (since its a single user, not all)
public ActionResult<AppUser> GetUser(int id) {
    return _context.Users.Find(id);
}
```

We made a request in our browser to `localhost:5001/api/users` to get all users, and `localhost:5001/api/user/3` to get a single user.

We can also do this through PostMan by making a `GET` request to the same URLs.



# Making Our Code Asynchronous

Synchronous code means that when we make a request to the database, the thread that is handling the request is currently blocked until the database request is fulfilled.

If we have a multithreaded application, we can't wait on each thread to finish before starting another.  We can make our code asynchronous so that when each thread comes to an async piece of code, send that request to another thread.  This makes our application instantly more scalable.  Generally, if you are making database calls, always make that code asynchronous.  If you don't do this, you will need to refactor a lot of code to improve scalability.

```c#
[HttpGet]
//specify the async keyword
//wrap the ActionResult in a task
//when the await keyword is encountered, the request is passed to a Task
//when the Task comes back, we get the results out of the Task
public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers() {
    return await _context.Users.ToListAsync();
}

// api/users/3
[HttpGet("{id}")]
public async Task<ActionResult<AppUser>> GetUser(int id) {
    return await _context.Users.FindAsync(id);
}
```