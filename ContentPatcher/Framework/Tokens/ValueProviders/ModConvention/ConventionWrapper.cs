using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using StardewModdingAPI;

#pragma warning disable 649 // Fields are assigned through reflection.
namespace ContentPatcher.Framework.Tokens.ValueProviders.ModConvention
{
    /// <summary>Wraps a mod token implementation with optional methods matching <see cref="ConventionDelegates"/>.</summary>
    /// <remarks>Methods should be kept in sync with <see cref="ConventionDelegates"/>. This must not have public instance methods that don't match a convention delegate, since it's used for validation.</remarks>
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local", Justification = "Fields are assigned through reflection.")]
    internal class ConventionWrapper
    {
        /*********
        ** Fields
        *********/
        /****
        ** Metadata
        ****/
        /// <summary>Get whether the values may change depending on the context.</summary>
        private ConventionDelegates.IsMutable IsMutableImpl;

        /// <summary>The implementation for <see cref="AllowsInput"/>, if any.</summary>
        private ConventionDelegates.AllowsInput AllowsInputImpl;

        /// <summary>The implementation for <see cref="RequiresInput"/>, if any.</summary>
        private ConventionDelegates.RequiresInput RequiresInputImpl;

        /// <summary>The implementation for <see cref="CanHaveMultipleValues"/>, if any.</summary>
        private ConventionDelegates.CanHaveMultipleValues CanHaveMultipleValuesImpl;

        /// <summary>The implementation for <see cref="GetValidInputs"/>, if any.</summary>
        private ConventionDelegates.GetValidInputs GetValidInputsImpl;

        /// <summary>The implementation for <see cref="HasBoundedValues"/>, if any.</summary>
        private ConventionDelegates.HasBoundedValues HasBoundedValuesImpl;

        /// <summary>The implementation for <see cref="HasBoundedRangeValues"/>, if any.</summary>
        private ConventionDelegates.HasBoundedRangeValues HasBoundedRangeValuesImpl;

        /// <summary>The implementation for <see cref="TryValidateInput"/>, if any.</summary>
        private ConventionDelegates.TryValidateInput TryValidateInputImpl;

        /// <summary>The implementation for <see cref="TryValidateValues"/>, if any.</summary>
        private ConventionDelegates.TryValidateValues TryValidateValuesImpl;

        /****
        ** State
        ****/
        /// <summary>The implementation for <see cref="IsReady"/>, if any.</summary>
        private ConventionDelegates.IsReady IsReadyImpl;

        /// <summary>The implementation for <see cref="GetValues"/>, if any.</summary>
        private ConventionDelegates.GetValues GetValuesImpl;

        /// <summary>The implementation for <see cref="UpdateContext"/>, if any.</summary>
        private ConventionDelegates.UpdateContext UpdateContextImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Try to create a convention wrapper for a given token instance.</summary>
        /// <param name="instance">The token instance.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="wrapper">The wrapper, if created successfully.</param>
        /// <param name="error">The error indicating why creating the wrapper failed, if applicable.</param>
        public static bool TryCreate(object instance, IReflectionHelper reflection, out ConventionWrapper wrapper, out string error)
        {
            error = null;
            var result = new ConventionWrapper();

            // Map a delegate type to a token method and wrapper field by convention.
            // This assumes the delegate type name matches the method name, and the wrapper has a
            // field of the delegate type in the form {name}Impl.
            bool TryMap<TDelegate>(out string mapError) where TDelegate : Delegate
            {
                string methodName = typeof(TDelegate).Name;
                string fieldName = $"{methodName}Impl";

                if (ConventionWrapper.TryWrapMethod(instance, methodName, out TDelegate mapped, out mapError))
                    reflection.GetField<TDelegate>(result, fieldName).SetValue(mapped);

                return mapError == null;
            }

            // detect unknown methods
            bool succeeded = true;
            {
                string[] unknownMethods =
                    (
                        from MethodInfo method in instance.GetType().GetMethods()
                        where typeof(ConventionWrapper).GetMethod(method.Name) == null
                        select method.Name
                    )
                    .ToArray();
                if (unknownMethods.Any())
                {
                    succeeded = false;
                    error = unknownMethods.Length == 1
                        ? $"found unsupported '{unknownMethods[0]}' method"
                        : $"found unsupported methods: {string.Join(", ", unknownMethods)}";
                }
            }

            // map implemented fields
            if (succeeded)
            {
                succeeded =
                    // metadata
                    TryMap<ConventionDelegates.IsMutable>(out error)
                    && TryMap<ConventionDelegates.AllowsInput>(out error)
                    && TryMap<ConventionDelegates.RequiresInput>(out error)
                    && TryMap<ConventionDelegates.CanHaveMultipleValues>(out error)
                    && TryMap<ConventionDelegates.GetValidInputs>(out error)
                    && TryMap<ConventionDelegates.HasBoundedValues>(out error)
                    && TryMap<ConventionDelegates.HasBoundedRangeValues>(out error)
                    && TryMap<ConventionDelegates.TryValidateInput>(out error)
                    && TryMap<ConventionDelegates.TryValidateValues>(out error)

                    // state
                    && TryMap<ConventionDelegates.IsReady>(out error)
                    && TryMap<ConventionDelegates.GetValues>(out error)
                    && TryMap<ConventionDelegates.UpdateContext>(out error);
            }

            wrapper = succeeded ? result : null;
            return succeeded;
        }

        /****
        ** Metadata
        ****/
        /// <summary>Get whether the values may change depending on the context.</summary>
        public bool IsMutable()
        {
            return this.IsMutableImpl?.Invoke() ?? true;
        }

        /// <summary>Get whether the value provider allows input arguments (e.g. an NPC name for a relationship token).</summary>
        /// <remarks>Default false.</remarks>
        public bool AllowsInput()
        {
            return this.AllowsInputImpl?.Invoke() ?? false;
        }

        /// <summary>Whether the value provider requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
        /// <remarks>Default false.</remarks>
        public bool RequiresInput()
        {
            return this.RequiresInputImpl?.Invoke() ?? false;
        }

        /// <summary>Whether the value provider may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <remarks>Default true.</remarks>
        public bool CanHaveMultipleValues(string input = null)
        {
            return this.CanHaveMultipleValuesImpl?.Invoke() ?? true;
        }

        /// <summary>Get the set of valid input arguments if restricted, or an empty collection if unrestricted.</summary>
        /// <remarks>Default unrestricted.</remarks>
        public IEnumerable<string> GetValidInputs()
        {
            return this.GetValidInputsImpl != null
                ? this.GetValidInputsImpl()
                : Enumerable.Empty<string>();
        }

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <remarks>Default unrestricted.</remarks>
        public bool HasBoundedValues(string input, out IEnumerable<string> allowedValues)
        {
            allowedValues = null;

            return this.HasBoundedValuesImpl?.Invoke(input, out allowedValues) ?? false;
        }

        /// <summary>Get whether the token always returns a value within a bounded numeric range for the given input. Mutually exclusive with <see cref="HasBoundedValues"/>.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="min">The minimum value this token may return.</param>
        /// <param name="max">The maximum value this token may return.</param>
        /// <remarks>Default false.</remarks>
        public bool HasBoundedRangeValues(string input, out int min, out int max)
        {
            min = 0;
            max = 0;

            return this.HasBoundedRangeValuesImpl?.Invoke(input, out min, out max) ?? false;
        }

        /// <summary>Validate that the provided input arguments are valid.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        public bool TryValidateInput(string input, out string error)
        {
            error = null;

            return this.TryValidateInputImpl?.Invoke(input, out error) ?? true;
        }

        /// <summary>Validate that the provided values are valid for the given input arguments (regardless of whether they match).</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="values">The values to validate.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        public bool TryValidateValues(string input, IEnumerable<string> values, out string error)
        {
            error = null;

            return this.TryValidateValuesImpl?.Invoke(input, values, out error) ?? true;
        }


        /****
        ** State
        ****/
        /// <summary>Get whether the token is available for use. This should always be called after <see cref="UpdateContext"/>.</summary>
        public bool IsReady()
        {
            return this.IsReadyImpl?.Invoke() ?? true;
        }

        /// <summary>Get the current values. This method is required.</summary>
        /// <param name="input">The input arguments, if any.</param>
        public IEnumerable<string> GetValues(string input)
        {
            return this.GetValuesImpl != null
                ? this.GetValuesImpl(input)
                : Enumerable.Empty<string>();
        }

        /// <summary>Update the instance when the context changes.</summary>
        /// <returns>Returns whether the instance changed.</returns>
        public bool UpdateContext()
        {
            return this.UpdateContextImpl?.Invoke() ?? false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Try to get a method which matches the given delegate.</summary>
        /// <param name="instance">The token value provider.</param>
        /// <param name="name">The method name on <see cref="ConventionWrapper"/> being mapped.</param>
        /// <param name="methodDelegate">The created method delegate, if any.</param>
        /// <param name="error">The error indicating why creating the wrapper failed, if applicable.</param>
        private static bool TryWrapMethod<TDelegate>(object instance, string name, out TDelegate methodDelegate, out string error)
            where TDelegate : Delegate
        {
            methodDelegate = null;
            error = null;

            // check target method exists
            {
                MethodInfo[] candidates = instance.GetType().GetMethods().Where(p => p.Name == name).ToArray();
                if (candidates.Length == 0)
                    return false;
                if (candidates.Length > 1)
                    return Fail($"method {name} has multiple implementations.", out error);
            }

            // map to delegate
            try
            {
                methodDelegate = (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), instance, name);
                error = null;
                return true;
            }
            catch (ArgumentException ex)
            {
                methodDelegate = null;
                return Fail(ConventionWrapper.GetMethodMismatchError(instance, name, ex), out error);
            }
        }

        /// <summary>Get an error indicating why a method could not be mapped by convention.</summary>
        /// <param name="instance">The token value provider.</param>
        /// <param name="name">The method name to find.</param>
        /// <param name="ex">The mapping exception.</param>
        private static string GetMethodMismatchError(object instance, string name, Exception ex)
        {
            string GetTypeName(Type type)
            {
                if (type == typeof(bool))
                    return "bool";
                if (type == typeof(int))
                    return "int";
                if (type == typeof(string))
                    return "string";
                if (type.IsPrimitive)
                    return type.Name;
                return type.FullName;
            }


            // get interface method
            MethodInfo source = typeof(ConventionWrapper).GetMethod(name);
            if (source == null)
                throw new InvalidOperationException($"The {nameof(ConventionWrapper)} class has no '{name}' method."); // should never happen

            // get target method
            MethodInfo target = instance.GetType().GetMethod(name);
            if (target == null)
                throw new InvalidOperationException($"The token has no '{name}' method."); // should never happen, we check if the target method exists before we get here

            // check return type
            if (source.ReturnType != target.ReturnType)
                return $"method {name} has return type {GetTypeName(target.ReturnType)}, but expected {GetTypeName(source.ReturnType)}.";

            // check generic parameters (not supported)
            if (source.ContainsGenericParameters)
                return $"method {name} is a generic method, which isn't supported.";

            // check arguments
            ParameterInfo[] sourceParams = source.GetParameters().ToArray();
            ParameterInfo[] targetParams = target.GetParameters().ToArray();
            if (sourceParams.Length != targetParams.Length)
                return $"method {name} has {targetParams.Length} arguments, but expected {sourceParams.Length}.";

            for (int i = 0; i < sourceParams.Length; i++)
            {
                ParameterInfo sourceParam = sourceParams[i];
                ParameterInfo targetParam = targetParams[i];

                string paramErrorPrefix = $"method {name} > parameter {i + 1} ({targetParam.Name}) ";
                if (sourceParam.ParameterType != targetParam.ParameterType)
                    return $"{paramErrorPrefix} has type {GetTypeName(targetParam.ParameterType)}, but expected {GetTypeName(sourceParam.ParameterType)}.";
                if (sourceParam.Attributes != targetParam.Attributes)
                    return $"{paramErrorPrefix} has attributes {targetParam.Attributes}, but expected {sourceParam.Attributes}.";
            }

            // unknown reason
            return $"method {name} could not be mapped:\n{ex}";
        }

        /// <summary>A convenience wrapper which enables failing in a Try* method in one line.</summary>
        /// <param name="inError">The error message to output.</param>
        /// <param name="outError">A copy of the <paramref name="inError"/>.</param>
        /// <returns>Return false.</returns>
        private static bool Fail(string inError, out string outError)
        {
            outError = inError;
            return false;
        }
    }
}
