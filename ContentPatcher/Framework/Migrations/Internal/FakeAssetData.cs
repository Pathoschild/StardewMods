using System;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations.Internal
{
    /// <summary>An implementation of <see cref="IAssetData"/> used to apply edits to temporary data during runtime content migrations.</summary>
    internal class FakeAssetData : IAssetData
    {
        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public string? Locale { get; }

        /// <inheritdoc />
        public IAssetName Name { get; }

        /// <inheritdoc />
        public IAssetName NameWithoutLocale { get; }

        /// <inheritdoc />
        public Type DataType { get; }

        /// <inheritdoc />
        public object Data { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="realData">The real asset being edited.</param>
        /// <param name="assetName">The asset name that should be shown to the edit patch.</param>
        /// <param name="data">The asset data that should be affected by the edit patch.</param>
        public FakeAssetData(IAssetData realData, IAssetName assetName, object data)
        {
            this.Locale = realData.Locale;
            this.Name = assetName;
            this.NameWithoutLocale = assetName.GetBaseAssetName();
            this.DataType = data.GetType();
            this.Data = data;
        }

        /// <remarks>This implementation does nothing, since Content Patcher's edit patches don't use it.</remarks>
        /// <inheritdoc />
        public void ReplaceWith(object value)
        {
            throw new NotImplementedException();
        }

        /// <remarks>This isn't implemented since edit patches don't use it.</remarks>
        /// <inheritdoc />
        public IAssetDataForDictionary<TKey, TValue> AsDictionary<TKey, TValue>()
        {
            throw new NotImplementedException();
        }

        /// <remarks>This isn't implemented since edit patches don't use it.</remarks>
        /// <inheritdoc />
        public IAssetDataForImage AsImage()
        {
            throw new NotImplementedException();
        }

        /// <remarks>This isn't implemented since edit patches don't use it.</remarks>
        /// <inheritdoc />
        public IAssetDataForMap AsMap()
        {
            throw new NotImplementedException();
        }

        /// <remarks>This isn't implemented since edit patches don't use it.</remarks>
        /// <inheritdoc />
        public TData GetData<TData>()
        {
            throw new NotImplementedException();
        }
    }
}
