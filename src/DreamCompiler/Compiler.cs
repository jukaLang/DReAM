﻿//java org.antlr.v4.Tool -Dlanguage= CSharp DreamGrammar.g4 -no-listener

using DreamCompiler.Lexer;
using DreamCompiler.Scanner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DreamCompiler
{

    interface ICompilerInterface
    {
        String Go(String ouputFileName, String raw_string);
    }

    public class Compiler
    {
        public Compiler(string[] args)
        {
            var hostBuilder = new HostBuilder()
             .ConfigureServices((context, services) =>
             {
                 services.AddSingleton<IScanner, Scanner.Scanner>();
                 services.AddSingleton<ILexicalAnalysis, LexicalAnalysis>();
                 services.AddSingleton(s => CommandLineProvider.InputArgs(args, s));
                 services.AddHostedService<HostedService>();
             });

            hostBuilder.RunConsoleAsync().Wait();
        }

        public String Go(String ouputFileName, String path)
        {
            try
            {
                Console.WriteLine("Starting the compiler...");

                //var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(raw_string, null, "");
                //return CompileRoslyn.CompileSyntaxTree(parsedSyntaxTree, ouputFileName);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}

