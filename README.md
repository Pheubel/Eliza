# Eliza
A web server for serving pages using ASP.NET Core with a Blazor front end. The name for the server stems from remnants of the original discord bot project.

# ElizaBot
A small Discord bot backed by an ASP.Net Core web server providing a web interface for some simple interactions, such as reading commands and checking tags.

This project was made with blazor preview 5 to provide front-end functionality with the power of web assembly.

## Contributing
Anyone is free to contribute to this project. It is assumed that you follow C# naming conventions when writing code.
When setting up your own enviroment, it is adviced to run `git update-index --skip-worktree source/Eliza/Server/appsettings.Development.json` in your git terminal to avoid accidentally committing sensitive data used for development.

### Setting up
To get started you first need to make an application and bot user on [the discord developer portal](https://discordapp.com/developers/applications). you will then need to copy and paste the following values into `ElizaBot/Eliza/Server/appsettings.Development.json`:
- Client Id
- Client Secret
- Bot Token

As a final step, you need to allow a redirect to the application page in the OAuth2 tab, since the application is hosted on your machine during development it will look something like this: `https://localhost:44353/signin-discord`. Keep in mind that the port number can differ. You can find out on which port the application is listening to in the URL bar when hosting it ia Visual Studio.

Optionally you can set yourself as bot owner by setting `OwnerId` to your discord user ID. This allows you to access parts of the application which should only be exposed by the owner of the bot.

### Note
I am not too familiar with graphical design, hence why the front-end design can be lackluster at times.
