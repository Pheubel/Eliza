using Eliza.Bot;
using Eliza.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Buffers;
using System.IO;
using System.Text.Json;

namespace Eliza.Server.Controllers
{
    [Route("api/elizabot/[controller]")]
    [ApiController]
    [Authorize(Policy = Eliza.Shared.Constants.IsBotOwner)]
    public class SettingsController : ControllerBase
    {
        private static readonly object _settingsLock = new object();
        private static readonly object _fileLock = new object();

        private static readonly byte[] BOTSETTINGS = System.Text.Encoding.UTF8.GetBytes(nameof(BotSettings));
        private const int BUFFERSIZE = 32;
        private static readonly byte[] _buffer = new byte[BUFFERSIZE];

        private readonly BotSettings _settings;

        public SettingsController(BotSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        [HttpPost]
        public IActionResult PostSettings([FromBody] SettingsDTO settings)
        {
            if (settings.Prefix.Length == 0)
                return BadRequest("prefix must be at least one character.");
            if (settings.Prefix[0] == ' ')
                return BadRequest("prefix can not start with a space character.");

            lock (_settingsLock)
            {
                _settings.Prefix = settings.Prefix;
                _settings.UseMentionPrefix = settings.UseMentionPrefix;
                _settings.CaseSensitiveComands = settings.CaseSensitiveComands;
            }

            // todo: write to appsettings.json
            OverwriteAppsettings();

            return Ok();
        }

        [HttpGet]
        public ActionResult<SettingsDTO> GetSettings()
        {
            return Ok(new SettingsDTO
            {
                Prefix = _settings.Prefix,
                CaseSensitiveComands = _settings.CaseSensitiveComands,
                UseMentionPrefix = _settings.UseMentionPrefix
            });
        }

        private void OverwriteAppsettings()
        {
            const string APPSETTINGS = "appsettings.json";
            const string TEMP = "appsettings.tmp";


            lock (_fileLock)
            {
                using (Stream ofStream = System.IO.File.OpenRead(APPSETTINGS))
                using (Utf8JsonStreamReader.Utf8JsonStreamReader reader = new Utf8JsonStreamReader.Utf8JsonStreamReader(ofStream, BUFFERSIZE))
                using (Stream nfStream = System.IO.File.Create(TEMP))
                using (Utf8JsonWriter writer = new Utf8JsonWriter(nfStream, new JsonWriterOptions { Indented = true }))
                {
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.None:
                                break;
                            case JsonTokenType.StartObject:
                                writer.WriteStartObject();
                                break;
                            case JsonTokenType.EndObject:
                                writer.WriteEndObject();
                                break;
                            case JsonTokenType.StartArray:
                                writer.WriteStartArray();
                                break;
                            case JsonTokenType.EndArray:
                                writer.WriteEndArray();
                                break;
                            case JsonTokenType.Comment:
                                writer.WriteCommentValue(reader.GetComment());
                                break;
                            case JsonTokenType.String:
                                writer.WriteStringValue(reader.GetString());
                                break;
                            case JsonTokenType.Number:
                                writer.WriteNumberValue(reader.GetDecimal());
                                break;
                            case JsonTokenType.True:
                                writer.WriteBooleanValue(true);
                                break;
                            case JsonTokenType.False:
                                writer.WriteBooleanValue(false);
                                break;
                            case JsonTokenType.Null:
                                writer.WriteNullValue();
                                break;

                            case JsonTokenType.PropertyName:
                                if (reader.HasValueSequence)
                                {
                                    Span<byte> buffer;

                                    if (reader.ValueSequence.Length <= BUFFERSIZE)
                                    {
                                        buffer = new Span<byte>(_buffer, 0, (int)reader.ValueSequence.Length);
                                    }
                                    else
                                    {
                                        buffer = new Span<byte>(new byte[reader.ValueSequence.Length]);
                                    }

                                    reader.ValueSequence.CopyTo(buffer);

                                    if (!buffer.SequenceEqual(BOTSETTINGS))
                                    {
                                        writer.WritePropertyName(buffer);
                                        break;
                                    }
                                }
                                else
                                {
                                    if (!reader.ValueSpan.SequenceEqual(BOTSETTINGS))
                                    {
                                        writer.WritePropertyName(reader.ValueSpan);
                                        break;
                                    }
                                }

                                writer.WritePropertyName(BOTSETTINGS);
                                JsonSerializer.Serialize(writer, _settings);

                                // skip botsettigns
                                int startDepth = reader.CurrentDepth;
                                while (reader.TokenType != JsonTokenType.EndObject && reader.CurrentDepth != startDepth)
                                    reader.Read();

                                break;
                            default:
                                break;
                        }
                    }
                }

                System.IO.File.Move(TEMP, APPSETTINGS);
            }
        }
    }
}
