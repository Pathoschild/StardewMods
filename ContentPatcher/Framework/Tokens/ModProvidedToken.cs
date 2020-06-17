using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens
{
    /// <summary>A token provided by a mod.</summary>
    internal class ModProvidedToken : GenericToken
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


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="mod">The mod which registered the token.</param>
        /// <param name="provider">The underlying value provider.</param>
        /// <param name="monitor">Encapsulates monitoring and logging.</param>
        public ModProvidedToken(string name, IManifest mod, IValueProvider provider, IMonitor monitor)
            : base(name, provider)
        {
            this.Mod = mod;
            this.Monitor = monitor;
        }

        /// <summary>Update the token data when the context changes.</summary>
        /// <param name="context">The condition context.</param>
        /// <returns>Returns whether the token data changed.</returns>
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

        /// <summary>Get the token names used by this patch in its fields.</summary>
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

        /// <summary>Get diagnostic info about the contextual instance.</summary>
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

        /// <summary>Whether the token may return multiple values for the given name.</summary>
        /// <param name="input">The input argument, if any.</param>
        public override bool CanHaveMultipleValues(ITokenString input)
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

        /// <summary>Validate that the provided input argument is valid.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryValidateInput(ITokenString input, out string error)
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

        /// <summary>Validate that the provided values are valid for the input argument (regardless of whether they match).</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="context">Provides access to contextual tokens.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        public override bool TryValidateValues(ITokenString input, InvariantHashSet values, IContext context, out string error)
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

        /// <summary>Get the allowed input arguments, if supported and restricted to a specific list.</summary>
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

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="IToken.HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input argument, if applicable.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        public override bool HasBoundedValues(ITokenString input, out InvariantHashSet allowedValues)
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

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="IToken.HasBoundedValues"/>.</summary>
        /// <param name="input">The input argument, if any.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <exception cref="InvalidOperationException">The input argument doesn't match this value provider, or does not respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        public override bool HasBoundedRangeValues(ITokenString input, out int min, out int max)
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

        /// <summary>Get the current token values.</summary>
        /// <param name="input">The input to check, if any.</param>
        /// <exception cref="InvalidOperationException">The input does not respect <see cref="IToken.CanHaveInput"/> or <see cref="IToken.RequiresInput"/>.</exception>
        public override IEnumerable<string> GetValues(ITokenString input)
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
