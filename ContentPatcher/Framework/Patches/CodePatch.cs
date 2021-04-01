using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using Mono.CSharp;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches
{
    /// <summary>Metadata for code that runs when the patch is triggered.</summary>
    internal class CodePatch : Patch
    {
        /*********
        ** Fields
        *********/
        private readonly IManagedTokenString[] CodeLines;

        private CompiledMethod compiledFunc;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="indexPath">The path of indexes from the root <c>content.json</c> to this patch; see <see cref="IPatch.IndexPath"/>.</param>
        /// <param name="path">The path to the patch from the root content file.</param>
        /// <param name="assetName">The normalized asset name to intercept.</param>
        /// <param name="conditions">The conditions which determine whether this patch should be applied.</param>
        /// <param name="codeLines">The lines of code to run.</param>
        /// <param name="updateRate">When the patch should be updated.</param>
        /// <param name="contentPack">The content pack which requested the patch.</param>
        /// <param name="parentPatch">The parent patch for which this patch was loaded, if any.</param>
        /// <param name="normalizeAssetName">Normalize an asset name.</param>
        public CodePatch(int[] indexPath, LogPathBuilder path, IEnumerable<Condition> conditions, IEnumerable<IManagedTokenString> codeLines, UpdateRate updateRate, IContentPack contentPack, IPatch parentPatch, Func<string, string> normalizeAssetName)
            : base(
                indexPath: indexPath,
                path: path,
                type: PatchType.Load,
                assetName: new TokenString( "DummyTarget", new GenericTokenContext( (s) => false ), path ),
                conditions: conditions,
                updateRate: updateRate,
                contentPack: contentPack,
                parentPatch: parentPatch,
                normalizeAssetName: normalizeAssetName
            )
        {
            this.CodeLines = codeLines?.ToArray() ?? new IManagedTokenString[0];

            this.Contextuals.Add( this.CodeLines );
        }

        public override IEnumerable<string> GetTokensUsed()
        {
            var ret = new List<string>();
            ret.AddRange( base.GetTokensUsed() );
            foreach ( var line in this.CodeLines )
            {
                ret.AddRange( line.GetTokensUsed() );
            }
            return ret;
        }

        public override bool UpdateContext( IContext context )
        {
            if ( base.UpdateContext( context ) )
            {
                this.compiledFunc = null;
                return true;
            }
            return false;
        }

        public void Run()
        {
            if ( this.compiledFunc == null )
            {
                var settings = new CompilerSettings()
                {
                    Unsafe = true,
                };

                var libs = new List<string>();
                foreach ( var asm in AppDomain.CurrentDomain.GetAssemblies() )
                {
                    try
                    {
                        settings.AssemblyReferences.Add( asm.CodeBase );
                    }
                    catch ( Exception e )
                    {
                        //Log.trace("Couldn't add assembly " + asm + ": " + e);
                    }
                }

                var eval = new Evaluator(new CompilerContext(settings, new ConsoleReportPrinter()));
                //eval.ReferenceAssembly( typeof( StardewValley.Game1 ).Assembly );
                string code = @"using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using StardewModdingAPI;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using xTile;";
                eval.Compile( code );
                var lines = new List<string>();
                foreach ( var line in this.CodeLines )
                {
                    if ( !line.IsReady )
                    {
                        return;
                    }
                    lines.Add( line.Value );
                }
                this.compiledFunc = eval.Compile( string.Join( "\n", lines ) );
            }
            object ret = null;
            this.compiledFunc.Invoke( ref ret );
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetChangeLabels()
        {
            yield return "ran code";
        }
    }
}
