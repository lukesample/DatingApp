# Section 4: Authentication Basics

## Introduction

To start an app, we need requirements!  Some typical requirements for a dating app may involve the following:
- Users should be able to log in
- Users should be able to register
- Users should be able to view other users
- Users should be able to privately message other users

Everything starts with the users!

# Safe storage of Passwords

We have a few options for storing passwords.

Option 1 - Storing in clear text.  This is a terrible idea because if the database is compromised, all of the information is out in the open.  This should absolutely never be done in a real application.

Option 2 is `Hashing` the Password.  This means we take their password, apply a `hashing` alogorithm to it, and we store the password hash in the database.  `Hashing` is one-way only.  You cannot calculate what a password was before it was hashed.  If two passwords are matching, that means they will be stored as the same hash in the database (i.e. Bob and Tom using the password "letmein").  If the database were to be compromised, they will know Bob and Tom have the same password, and if the password is gotten, they will know both of their passwords.

Only `hashing` a password is dangerous because there are dictionaries of hashes online of popular passwords, in many algorithms.  Hackers could potentially find out passwords relatively easy.

Option 3 is `hashing` and `salting` the password.  A `Salt` applied to a `hashing` algorithm will scramble the Hash.  This means that two people with the same passwords will not have the same hash.  We will also store the password `Salt`.

FAQs:
1. Why don't you use `ASP.NET Identity`?
2. Why are you storing the password `Salt` in the DB? Isn't this less secure?

We will use `ASP.NET Identity` Later after some refactoring.  We are essentially doing it ourselves this way for now to see how it actually works.

# Updating the User Entity

In this section are are working on adding authentication.

We added two additional properties to our `AppUser` entity, the `PasswordHash` and `PasswordSalt`.  We will need to add these to our migration.

Make sure the application is stopped before adding a migration!

we use the command `dotnet ef migrations add UserPasswordAdded` to add this migration.  Inside the Migrations folder, we can see the code for adding these two properties.  Notice that the type is `blob`, only because SQLite does not support Byte Arrays.  However, it will be passed back to us in the correct datatype.

Now that we have our columns, we need to update our database.  We can use the command `dotnet ef database update` to do so.

# Creating a Base API Controller

This will be used to manage our User login and register.

Every contoller shares similar properties, i.e. the `ApiController` and `Route` decorators as well as inheriting from `ControllerBase`.  We can create a Base API controller to keep from repeating ourselves in each controller.  This is done through inheritance.

We will create the `BaseApiController` to accomplish this.  We will decorate the class with the `[ApiContoller]` and `[Route]` attributes so that we no longer need to add these in each new class.  In the `UsersController`, we can now inherit from `BaseApiController` and remove the `ApiController` and `Route` attributes, since we inherit all the attributes, methods, and properties when inheriting from another class.

The course includes a Postman collection, so this was imported for ease of use purposes.

# Creating an Account Controller with a Register Endpoint

We need to create a new `AccountController`.  This will derive from our `BaseApiController` we created earlier.  We added a constructor and injected the `DataContext` (use the `Ctrl + .` method to initialize from parameter).

The `ApiController` attribute is solely for improving the experience of developers, not for end users.

We added a new HTTP endpoint using `[HttpPost("register)]`.  We use `Post` if we want to add a resource through our API Endpoint.  Since we're creating a new user, `HttpPost` is the correct method to use.

An additional thing that the `ApiController` attribute does for us is that it automatically binds to any parameters it finds in the parameters of our method.  If we weren't using this, we would need to specify a `[From]` parameter for each of our method parameters. 

`HMACSHA512` is what will give us the hashing algorithm for creating the password hash.  

The `Using` statement means that, whenever we are finished with our class, the code block will be disposed of correctly.  Whenever we use the `using` statement, it will call the `Dispose` method so that it will clean up as it should.  Every class