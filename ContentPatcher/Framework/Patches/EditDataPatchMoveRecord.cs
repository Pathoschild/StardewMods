using System.Collections.Generic;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>An entry in a data file to move.</summary>
    internal class EditDataPatchMoveRecord : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The unique key for the entry in the data file.</summary>
        public ITokenString ID { get; }

        /// <summary>The ID of another entry this one should be inserted before.</summary>
        public ITokenString? BeforeID { get; }

        /// <summary>The ID of another entry this one should be inserted after.</summary>
        public ITokenString? AfterID { get; }

        /// <summary>The position to set.</summary>
        public MoveEntryPosition ToPosition { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;



        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="id">The unique key for the entry in the data file.</param>
        /// <param name="beforeID">The ID of another entry this one should be inserted before.</param>
        /// <param name="afterID">The ID of another entry this one should be inserted after.</param>
        /// <param name="toPosition">The position to set.</param>
        public EditDataPatchMoveRecord(IManagedTokenString id, IManagedTokenString? beforeID, IManagedTokenString? afterID, MoveEntryPosition toPosition)
        {
            this.ID = id;
            this.BeforeID = beforeID;
            this.AfterID = afterID;
            this.ToPosition = toPosition;

            this.Contextuals = new AggregateContextual()
                .Add(id)
                .Add(beforeID)
                .Add(afterID);
        }

        /// <inheritdoc />
        public bool UpdateContext(IContext context)
        {
            return this.Contextuals.UpdateContext(context);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetTokensUsed()
        {
            return this.Contextuals.GetTokensUsed();
        }

        /// <inheritdoc />
        public IContextualState GetDiagnosticState()
        {
            return this.Contextuals.GetDiagnosticState();
        }
    }
}
