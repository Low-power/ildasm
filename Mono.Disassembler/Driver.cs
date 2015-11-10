//
// Driver.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2005 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace Mono.Disassembler {

	using System;
	using System.IO;
	using System.Reflection;
	using System.Text;

	using Mono.Cecil;

	class Driver {

		enum Output {
			Gui,
			File,
			Console
		}

		enum OutputEncoding {
			ASCII,
			UTF8,
			Unicode
		}

		static void Main (string [] args)
		{
			Driver drv = new Driver (args);
			drv.Run ();
			Environment.Exit (0);
		}

		string [] m_args;
		Output m_output = Output.Console;
		OutputEncoding m_encoding = OutputEncoding.UTF8;
		string m_assembly;
		string m_outputFile;
		string m_error;
		bool m_no_alias = false;

		Driver (string [] args)
		{
			m_args = args;
		}

		void Run ()
		{
			try {
				Parse (m_args);
				switch (m_output) {
				case Output.Gui:
					throw new NotImplementedException ("GUI is not implemented");
				case Output.Console:
					WriteDisassembly (Console.Out);
					break;
				case Output.File:
					FileStream fs = new FileStream (m_outputFile, FileMode.Create, FileAccess.Write, FileShare.None);
					Encoding enc = null;
					if (m_encoding == OutputEncoding.Unicode)
						enc = Encoding.Unicode;
					else if (m_encoding == OutputEncoding.UTF8)
						enc = Encoding.UTF8;
					else
						enc = Encoding.ASCII;

					using (StreamWriter sw = new StreamWriter (fs, enc))
						WriteDisassembly (sw);

					break;
				}
			} catch (Exception e) {
				m_error = e.ToString ();
				Usage ();
			}
		}

		void WriteDisassembly (TextWriter writer)
		{
			CilWriter cw = new CilWriter (writer);
			StructureDisassembler sd = new StructureDisassembler ();
			sd.NoAlias = m_no_alias;
			sd.DisassembleAssembly (AssemblyFactory.GetAssembly (m_assembly), cw);
		}

		string AssemblyName {
			get { return m_assembly; }
			set {
				if (m_assembly != null) {
					m_error = "Multiple input files specified !";
					return;
				}

				m_assembly = value;
				if (!File.Exists (m_assembly)) {
					m_error = "Specified file does not exists !";
				}
			}
		}

		void SetOutputFile(String filename) {
			m_output = Output.File;
			m_outputFile = filename;
		}

		void Parse (string [] args)
		{
			//string cmd_arg;
			//foreach (string command in args) {
			for(int i=0; i<args.Length; i++) {
				String command = args[i];
/*
				if (cmd [0] != '-' && cmd [0] != '/') {
					AssemblyName = cmd;
					continue;
				}

				switch (GetCommand (cmd, out cmd_arg)) {
				case "text":
					m_output = Output.Console;
					break;
				case "output":
					m_output = Output.File;
					m_outputFile = cmd_arg;
					break;
				case "utf8":
					m_encoding = OutputEncoding.UTF8;
					break;
				case "unicode":
					m_encoding = OutputEncoding.Unicode;
					break;
				case "-about":
					About ();
					break;
				case "-version":
					Version ();
					break;
				default:
					if (cmd [0] == '/')
						//FIXME: This is only for unix
						AssemblyName = cmd;
					break;
				}
*/
				if(command.Length > 1 && command[0] == '-') {
					if(command[1] == '-') {
						if(command.Length == 2) {
							if(++i == args.Length) break;
							if(i + 1 < args.Length) {
								m_error = "Multiple input files specified !";
								break;
							}
							AssemblyName = args[i];
							return;
						}
						String long_option = command.Substring(2);
						switch(long_option) {
							case "about":
								About();
								break;
							case "version":
								Version();
								break;
							case "output":
								if(++i == args.Length) {
									Console.Error.WriteLine("Option --output require an argument");
									Environment.Exit(1);
								}
								SetOutputFile(args[i]);
								break;
							case "no-alias":
								m_no_alias = true;
								break;
							case "help":
							case "usage":
								Usage();
								return;
							default:
								Console.Error.WriteLine("Unknown option '{0}'", command);
								Environment.Exit(1);
								return;
						}
					} else {
						if(command.Length > 2) {
							Console.Error.WriteLine("Unknown option '{0}'", command);
							Environment.Exit(1);
						}
						switch(command[1]) {
							case 'o':
								if(++i == args.Length) {
									Console.Error.WriteLine("Option -o require an argument");
									Environment.Exit(1);
								}
								SetOutputFile(args[i]);
								break;
							default:
								Console.Error.WriteLine("Unknown option '{0}'", command);
								Environment.Exit(1);
								return;
						}
					}
				} else {
					AssemblyName = command;
				}
			}

			if(args.Length == 0 || m_error != null) {
				Usage ();
			} else if(AssemblyName == null) {
				Console.Error.WriteLine("No input file specified");
				Environment.Exit(1);
			}
		}

/*
		string GetCommand (string cmd, out string arg)
		{
			int sep = cmd.IndexOfAny (new char [] {':', '='}, 1);
			if (sep == -1) {
				arg = null;
				return cmd.Substring (1);
			}

			string command = cmd.Substring (1, sep - 1);
			arg = cmd.Substring (sep + 1);
			return command.ToLower ();
		}
*/
		void Usage ()
		{
			Console.WriteLine (
				"Mono CIL Disassembler");
			if (m_error != null)
				Console.WriteLine ("\n{0}\n", m_error);
			Console.WriteLine (
				"Usage: ildasm [<options>] <assembly>\n" +
				"   --about\t\t\tabout Mono CIL Disassembler\n" +
				"   --version\t\t\tprint version of the Mono CIL Disassembler\n" +
				"   --output <filename>\t\tprint disassembly into filename\n" +
				"   --no-alias			don't use type name aliases\n");
			Environment.Exit (255);
		}

		void Version ()
		{
			Console.WriteLine ("Mono CIL Disassembler version {0}",
				Assembly.GetExecutingAssembly ().GetName ().Version.ToString ());
			Environment.Exit (0);
		}

		void About ()
		{
			Console.WriteLine (
				"Mono CIL Disassembler\n" +
				"For more information on Mono, visit\n" +
				"   http://www.mono-project.com\n" +
				"");
			Environment.Exit (0);
		}
	}
}
