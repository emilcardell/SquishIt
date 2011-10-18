using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Jurassic;

namespace SquishIt.Framework.Less
{
    public class LessCompiler
    {
        private static string _lessJs;
        private static ScriptEngine _engine;

        public string Compile(string input)
        {
            LessScriptEngine.SetGlobalValue("Source", input);

            // Errors go from here straight on to the rendered page; 
            // we don't want to hide them because they provide valuable feedback
            // on the location of the error
            var result = LessScriptEngine.Evaluate<string>(@"var less = require('less'); less.render(Source, function (e, css) { console.log(css);});");

            return result;
        }

        private static ScriptEngine LessScriptEngine
        {
            get
            {
                if (_engine == null)
                {
                    var engine = new ScriptEngine();
                    engine.ForceStrictMode = true;
                    engine.Execute(Compiler);
                    _engine = engine;
                }
                return _engine;
            }
        }


        private static IEnumerable<string> ParseImports(string lessFileName)
        {
            /* These are equivalent:
             * 
             *   @import "lib.less";
             *   @import "lib";
             * 
             * Ignore css though as they are not imported by LESS
             */
            string dir = Path.GetDirectoryName(lessFileName);

            var importRegex = new Regex(@"@import\s+[""'](.*)[""'];");

            return (from line in File.ReadAllLines(lessFileName)
                    let match = importRegex.Match(line)
                    let file = match.Groups[1].Value
                    where match.Success
                      && !file.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
                    select Path.Combine(dir, Path.ChangeExtension(file, ".less"))
            );
        }

        public static string Compiler
        {
            get
            {
                if (_lessJs == null)
                    _lessJs = LoadLessJs();

                return _lessJs;
            }
        }

        private static string LoadLessJs()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SquishIt.Framework.Less.less-1.1.3.min.js"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
