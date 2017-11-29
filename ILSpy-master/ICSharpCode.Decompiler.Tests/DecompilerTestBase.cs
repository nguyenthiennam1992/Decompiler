﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.Tests.Helpers;
using Microsoft.CSharp;
using Mono.Cecil;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;

namespace ICSharpCode.Decompiler.Tests
{
	public abstract class DecompilerTestBase
	{
		public const string TestCasePath = "../../../TestCases";

		protected static void ValidateFileRoundtrip(string samplesFileName)
		{
			var fullPath = Path.Combine(TestCasePath, "..", samplesFileName);
			AssertRoundtripCode(fullPath);
		}

		static string RemoveIgnorableLines(IEnumerable<string> lines)
		{
			return CodeSampleFileParser.ConcatLines(lines.Where(l => !CodeSampleFileParser.IsCommentOrBlank(l)));
		}

		protected static void AssertRoundtripCode(string fileName, bool optimize = false, bool useDebug = false, int compilerVersion = 4)
		{
			var code = RemoveIgnorableLines(File.ReadLines(fileName));
			AssemblyDefinition assembly = CompileLegacy(code, optimize, useDebug, compilerVersion);

			CSharpDecompiler decompiler = new CSharpDecompiler(assembly.MainModule, new DecompilerSettings());

			decompiler.AstTransforms.Insert(0, new RemoveCompilerAttribute());

			var syntaxTree = decompiler.DecompileWholeModuleAsSingleFile();

			var options = FormattingOptionsFactory.CreateAllman();
			options.IndentSwitchBody = false;
			CodeAssert.AreEqual(code, syntaxTree.ToString(options));
		}

		protected static AssemblyDefinition CompileLegacy(string code, bool optimize, bool useDebug, int compilerVersion)
		{
			CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v" + new Version(compilerVersion, 0) } });
			CompilerParameters options = new CompilerParameters();
			options.CompilerOptions = "/unsafe /o" + (optimize ? "+" : "-") + (useDebug ? " /debug" : "");
			if (compilerVersion >= 4)
				options.ReferencedAssemblies.Add("System.Core.dll");
			CompilerResults results = provider.CompileAssemblyFromSource(options, code);
			try
			{
				if (results.Errors.Cast<CompilerError>().Any((arg) => !arg.IsWarning))
				{
					StringBuilder b = new StringBuilder("Compiler error:");
					foreach (var error in results.Errors)
					{
						b.AppendLine(error.ToString());
					}
					throw new Exception(b.ToString());
				}
				return AssemblyDefinition.ReadAssembly(results.PathToAssembly,
					new ReaderParameters { InMemory = true });
			}
			finally
			{
				File.Delete(results.PathToAssembly);
				results.TempFiles.Delete();
			}
		}
	}
}
