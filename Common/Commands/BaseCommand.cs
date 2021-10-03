using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands
{
    /// <summary>The base implementation for a console command implemented by Content Patcher.</summary>
    internal abstract class BaseCommand : ICommand
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        protected readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description => this.StripCommonIndentation(this.GetDescription());


        /*********
        ** Public methods
        *********/
        /// <summary>Get a description for the command shown by the 'patch help' command.</summary>
        public abstract string GetDescription();

        /// <inheritdoc />
        public abstract void Handle(string[] args);


        /*********
        ** Protected methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        /// <param name="name">The command's subname (e.g. the 'export' in 'patch export').</param>
        protected BaseCommand(IMonitor monitor, string name)
        {
            this.Monitor = monitor;
            this.Name = name;
        }

        /// <summary>Trim newlines from a block of text, and remove an equal amount of indentation from each line so the least-indented text starts is unindented.</summary>
        /// <param name="text">The text to process.</param>
        private string StripCommonIndentation(string text)
        {
            // preprocess
            string[] lines = text.Split('\n');

            // get minimum indentation
            int minIndentation = int.MaxValue;
            foreach (string line in lines)
            {
                string trimmed = line.TrimStart();
                if (trimmed.Length == 0)
                    continue;

                int indents = line.Length - trimmed.Length;
                if (indents < minIndentation)
                    minIndentation = indents;
            }

            // strip common indentation
            if (minIndentation != int.MaxValue)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string trimmed = line.TrimStart();

                    if (trimmed.Length == 0)
                        lines[i] = trimmed;
                    else
                        lines[i] = line.Substring(minIndentation);
                }
            }

            return string.Join("\n", lines).Trim('\r', '\n');
        }
    }
}
