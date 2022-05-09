using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>A map tile to change when editing a map.</summary>
    internal class EditMapPatchTile : IContextual
    {
        /*********
        ** Fields
        *********/
        /// <summary>The underlying contextual values.</summary>
        private readonly AggregateContextual Contextuals;


        /*********
        ** Accessors
        *********/
        /// <summary>The tile position to edit, relative to the top-left corner.</summary>
        public TokenPosition Position { get; }

        /// <summary>The map layer name to edit.</summary>
        public ITokenString? Layer { get; }

        /// <summary>The tilesheet index to apply, the string <c>false</c> to remove it, or null to leave it as-is.</summary>
        public ITokenString? SetIndex { get; }

        /// <summary>The tilesheet ID to set.</summary>
        public ITokenString? SetTilesheet { get; }

        /// <summary>The tile properties to set.</summary>
        public IDictionary<ITokenString, ITokenString?> SetProperties { get; }

        /// <summary>Whether to remove the current tile and all its properties.</summary>
        public ITokenString? Remove { get; }

        /// <inheritdoc />
        public bool IsMutable => this.Contextuals.IsMutable;

        /// <inheritdoc />
        public bool IsReady => this.Contextuals.IsReady;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="position">The tile position to edit, relative to the top-left corner.</param>
        /// <param name="layer">The map layer name to edit.</param>
        /// <param name="setIndex">The tilesheet index to apply, the string <c>false</c> to remove it, or null to leave it as-is.</param>
        /// <param name="setTilesheet">The tilesheet ID to set.</param>
        /// <param name="setProperties">The tile properties to set.</param>
        /// <param name="remove">Whether to remove the current tile and all its properties.</param>
        public EditMapPatchTile(TokenPosition position, IManagedTokenString layer, IManagedTokenString? setIndex, IManagedTokenString? setTilesheet, IDictionary<IManagedTokenString, IManagedTokenString?>? setProperties, IManagedTokenString? remove)
        {
            this.Position = position;
            this.Layer = layer;
            this.SetIndex = setIndex;
            this.SetTilesheet = setTilesheet;
            this.SetProperties = setProperties?.ToDictionary(p => (ITokenString)p.Key, p => (ITokenString?)p.Value) ?? new();
            this.Remove = remove;

            this.Contextuals = new AggregateContextual()
                .Add(position)
                .Add(layer)
                .Add(setIndex)
                .Add(setTilesheet)
                .Add(remove);

            if (setProperties != null)
            {
                foreach ((IManagedTokenString key, IManagedTokenString? value) in setProperties)
                    this.Contextuals.Add(key).Add(value);
            }
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
