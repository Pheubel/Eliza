namespace ElizaBot
{
    /// <summary> The settings to apply to the client.</summary>
    public class BotConfig
    {
        /// <summary> The token the bot will use for authentication.</summary>
        public string BotToken { get; set; }
        /// <summary> The cache size for messages posted in a channel.</summary>
        public int MessageCacheSize { get; set; }
        /// <summary> The character the bot should respond to.</summary>
        public string Prefix { get; set; }
        public bool UseMentionPrefix { get; set; }
    }
}