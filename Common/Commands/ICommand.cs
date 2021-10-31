namespace Pathoschild.Stardew.Common.Commands
{
    /// <summary>A console command implemented by Content Patcher.</summary>
    internal interface ICommand
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The command's subname (e.g. the 'export' in 'patch export').</summary>
        string Name { get; }

        /// <summary>The command description for the 'patch help' command.</summary>
        string Description { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Handle a command invocation.</summary>
        /// <param name="args">The command arguments, excluding the name and subcommand.</param>
        void Handle(string[] args);
    }
}
