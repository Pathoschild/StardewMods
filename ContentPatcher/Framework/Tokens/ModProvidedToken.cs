using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token provided by a mod.</summary>
    /// <remarks>To avoid breaking high-level token functionality (e.g. 'contains' handling), this must always call the base method.</remarks>
    internal class ModProvidedToken : Token
    {
        /*********
        ** Fields
        *********/
        /// <summary>Encapsulates monitoring and logging.</summary>
        private readonly IMonitor Monitor;


        /*********
        ** Accessors
        *********/
        /// <summary>The mod which registered the token.</summary>
        public IManifest Mod { get; }

        /// <summary>The mod prefix.</summary>
        public string NamePrefix { get; }

        /// <summary>The token name without the mod prefix.</summary>
        public string NameWithoutPrefix { get; }

        /// <inheritdoc />
        public override string Name => $"{this.NamePrefix}{this.NameWithoutPrefix}";


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="nameWithoutPrefix">The token name without the mod prefix.</param>
        /// <param name="mod">The mod which registered the token.</param>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public ModProvidedToken(string nameWithoutPrefix, IManifest mod, IValueProvider provider, IMonitor monitor)
            : base(null, provider)
        {
            this.NamePrefix = $"{mod.UniqueID}{InternalConstants.ModTokenSeparator}";
            this.NameWithoutPrefix = nameWithoutPrefix;
            this.Mod = mod;
            this.Monitor = monitor;
        }

        /// <inheritdoc />
        public override bool UpdateContext(IContext context)
        {
            try
            {
                return base.UpdateContext(context);
            }
            catch (Exception ex)
            {
                this.Log(ex);
                return false;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetTokensUsed()
        {
            try
            {
                return base.GetTokensUsed();
            }
            catch (Exception ex)
            {
                this.Log(ex);
                return Enumerable.Empty<string>();
            }
        }

        /// <inheritdoc />
        public override IContextualState GetDiagnosticState()
        {
            try
            {
                return base.GetDiagnosticState();
            }
            catch (Exception ex)
            {
                this.Log(ex);
                return new ContextualState();
            }
        }

        /// <inheritdoc />
        public override bool CanHaveMultipleValues(IInputArguments input)
        {
            try
            {
                return base.CanHaveMultipleValues(input);
            }
            catch (Exception ex)
            {
                this.Log(ex);
                return false;
            }
        }

        /// <inheritdoc />
        public override bool TryValidateInput(IInputArguments input, out string error)
        {
            try
            {
                return base.TryValidateInput(input, out error);
            }
            catch (Exception ex)
            {
                this.Log(ex);
                error = null;
                return true;
            }
        }

        /// <inheritdoc />
        public override bool TryValidateValues(IInputArguments input, InvariantHashSet values, IContext context, out string error)
        {
            try
            {
                return base.TryValidateValues(input, values, context, out error);
            }
            catch (Exception ex)
            {
                this.Log(ex);
                error = null;
                return true;
            }
        }

        /// <inheritdoc />
        public override InvariantHashSet GetAllowedInputArguments()
        {
            try
            {
                return base.GetAllowedInputArguments();
            }
            catch (Exception ex)
            {
                this.Log(ex);
                return null;
            }
        }

        /// <inheritdoc />
        public override bool HasBoundedValues(IInputArguments input, out InvariantHashSet allowedValues)
        {
            try
            {
                return base.HasBoundedValues(input, out allowedValues);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.Log(ex);
                allowedValues = null;
                return false;
            }
        }

        /// <inheritdoc />
        public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
        {
            try
            {
                return base.HasBoundedRangeValues(input, out min, out max);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.Log(ex);
                min = 0;
                max = 0;
                return false;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetValues(IInputArguments input)
        {
            try
            {
                return base.GetValues(input);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.Log(ex);
                return Enumerable.Empty<string>();
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Log an exception thrown by the underlying mod.</summary>
        /// <param name="ex">The error message.</param>
        private void Log(Exception ex)
        {
            this.Monitor.LogOnce($"The mod '{this.Mod.Name}' added custom token '{this.Name}', which failed and may not work correctly. See the log for details.", LogLevel.Warn);
            this.Monitor.Log($"Custom token '{this.Name}' failed:\n{ex}");
        }
    }
}
