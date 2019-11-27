namespace ElizaBot
{
    /// <summary> The settings to apply to the client.</summary>
    public class AppSettings
    {
        /// <summary> The token the bot will use for authentication.</summary>
        public string BotToken { get; set; }
        /// <summary> The connectionstring used to connect to database of the bot.</summary>
        public string ConnectionString { get; set; }
        /// <summary> The character the bot should respond to.</summary>
        public string Prefix { get; set; }
    }
}