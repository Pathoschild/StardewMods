using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands
{
    /// <summary>A console command which parses a given token string and shows the result.</summary>
    internal class ParseCommand : BaseCommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the current token context for a given mod ID, or the global context if given a null mod ID.</summary>
        private readonly Func<string, IContext> GetContext;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="getContext">Get the current token context.</param>
        public ParseCommand(IMonitor monitor, Func<string, IContext> getContext)
            : base(monitor, "parse")
        {
            this.GetContext = getContext;
        }

        /// <inheritdoc />
        public override string GetDescription()
        {
            return @"
                patch parse
                   usage: patch parse ""value"" [compact]
                   Parses the given token string and shows the result. For example, `patch parse ""assets/{{Season}}.png"" will show a value like ""assets/Spring.png"". If present, 'compact' returns less details.

                patch parse ""value"" ""content -pack.id"" [compact]
                   Parses the given token string and shows the result, using tokens available to the specified content pack (using the ID from the content pack's manifest.json). For example, `patch parse ""assets/{{CustomToken}}.png"" ""Pathoschild.ExampleContentPack"". If present, 'compact' returns less details.
            ";
        }

        /// <inheritdoc />
        public override void Handle(string[] args)
        {
            // parse arguments
            if (args.Length is < 1 or > 3)
            {
                this.Monitor.Log("The 'patch parse' command expects one to three arguments. See 'patch help parse' for more info.", LogLevel.Error);
                return;
            }
            string raw = args[0];
            string modID = args.Length >= 2 && args[1] != "compact" ? args[1] : null;
            bool compact = args.Skip(1).Contains("compact");

            // get context
            IContext context;
            try
            {
                context = this.GetContext(modID);
            }
            catch (KeyNotFoundException ex)
            {
                this.Monitor.Log(ex.Message, LogLevel.Error);
                return;
            }
            catch (Exception ex)
            {
                this.Monitor.Log(ex.ToString(), LogLevel.Error);
                return;
            }

            // parse value
            TokenString tokenStr;
            try
            {
                tokenStr = new TokenString(raw, context, new LogPathBuilder("console command"));
            }
            catch (LexFormatException ex)
            {
                this.Monitor.Log($"Can't parse that token value: {ex.Message}", LogLevel.Error);
                return;
            }
            catch (InvalidOperationException outerEx) when (outerEx.InnerException is LexFormatException ex)
            {
                this.Monitor.Log($"Can't parse that token value: {ex.Message}", LogLevel.Error);
                return;
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Can't parse that token value: {ex}", LogLevel.Error);
                return;
            }
            IContextualState state = tokenStr.GetDiagnosticState();

            // show result
            StringBuilder output = new StringBuilder();
            output.AppendLine();
            if (!compact)
            {
                output.AppendLine("Metadata");
                output.AppendLine("----------------");
                output.AppendLine($"   raw value:   {raw}");
                output.AppendLine($"   ready:       {tokenStr.IsReady}");
                output.AppendLine($"   mutable:     {tokenStr.IsMutable}");
                output.AppendLine($"   has tokens:  {tokenStr.HasAnyTokens}");
                if (tokenStr.HasAnyTokens)
                    output.AppendLine($"   tokens used: {string.Join(", ", tokenStr.GetTokensUsed().Distinct().OrderByHuman())}");
                output.AppendLine();

                output.AppendLine("Diagnostic state");
                output.AppendLine("----------------");
                output.AppendLine($"   valid:    {state.IsValid}");
                output.AppendLine($"   in scope: {state.IsValid}");
                output.AppendLine($"   ready:    {state.IsReady}");
                if (state.Errors.Any())
                    output.AppendLine($"   errors:         {string.Join(", ", state.Errors)}");
                if (state.InvalidTokens.Any())
                    output.AppendLine($"   invalid tokens: {string.Join(", ", state.InvalidTokens)}");
                if (state.UnreadyTokens.Any())
                    output.AppendLine($"   unready tokens: {string.Join(", ", state.UnreadyTokens)}");
                if (state.UnavailableModTokens.Any())
                    output.AppendLine($"   unavailable mod tokens: {string.Join(", ", state.UnavailableModTokens)}");
                output.AppendLine();

                output.AppendLine("Result");
                output.AppendLine("----------------");
            }
            output.AppendLine(!tokenStr.IsReady
                ? "The token string is invalid or unready."
                : $"   The token string is valid and ready. Parsed value: \"{tokenStr}\""
            );

            this.Monitor.Log(output.ToString(), LogLevel.Debug);
        }
    }
}
