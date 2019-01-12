﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamCompiler.RoslynCompile
{
    using Antlr4.Runtime.Tree;
    using Antlr4.Runtime;
    using DreamCompiler.Grammar;
    using DreamCompiler.Visitors;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Diagnostics;

    class GenerateBinaryExpression
    {
        private LocalDeclarationStatementSyntax statementSyntax;
        private ParserRuleContext currentKeyWord;
        private Stack<ParserRuleContext> operators = new Stack<ParserRuleContext>();
        private List<ParserRuleContext> postfix = new List<ParserRuleContext>();
        private Stack<ContextExpressionUnion> unionStack = new Stack<ContextExpressionUnion>();
        private const string LeftParen = "(";
        private const string RightParen = ")";
        private const int DefaultNumberOfChildren = 1;

        static internal CSharpSyntaxNode CreateBinaryExpression(ParserRuleContext context, DreamRoslynVisitor visitor)
        {
            /*
            var nodeList = new List<CSharpSyntaxNode>();

            foreach (var expression in context.children)
            {
                var item = expression.Accept(visitor);
                nodeList.Add(item);
            }

            String unaryOp = context..GetText();

            var expressionArray = nodeList.ToArray();
            var left = (ExpressionSyntax)expressionArray[0];
            var right = (ExpressionSyntax)expressionArray[1];

            if (unaryOp.Equals("+") || unaryOp.Equals("*"))
            {
                SyntaxKind kind = unaryOp.Equals("+") ? SyntaxKind.AddExpression : SyntaxKind.MultiplyExpression;
                return SyntaxFactory.BinaryExpression(kind, left, right);
            }

            if (unaryOp.Equals("-") || unaryOp.Equals("/"))
            {
                SyntaxKind kind = unaryOp.Equals("-") ? SyntaxKind.SubtractExpression : SyntaxKind.DivideExpression;
                return SyntaxFactory.BinaryExpression(kind, left, right);
            }

            throw new Exception("Invalid expression");
            */

            return null;
             
        }

        internal LocalDeclarationStatementSyntax GetLocalDeclarationStatementSyntax()
        {
            return statementSyntax;
        }


        internal GenerateBinaryExpression Walk(ParserRuleContext node)
        {
            try
            {
                if (node != null)
                {
                    /*
                    if (node is DreamGrammarParser.ParenthesizedBinaryExpressionContext)
                    {
                        if (node.children[0] is TerminalNodeImpl && node.children[node.ChildCount-1] is TerminalNodeImpl)
                        {
                            ParserRuleContext singleExpression = node.children[1] as ParserRuleContext;
                            if (singleExpression != null)
                            {
                                WalkChildren(singleExpression.children);
                            }
                        }
                        return this;
                    }
                    else
                    {
                    */
                    
                    WalkChildren(node.children);
                    if (node is DreamGrammarParser.BinaryExpressionsContext || 
                        node is DreamGrammarParser.MultiplyBinaryExpressionContext)
                    {
                        WalkChildren(node.children);
                    }
                    else if (node is DreamGrammarParser.VariableContext)
                    {
                        //currentVariable = node as DreamGrammarParser.VariableContext;
                        postfix.Add(node);
                        Trace.WriteLine(node.GetText());
                        return this;
                    }
                    else if (node is DreamGrammarParser.KeywordsContext)
                    {
                        currentKeyWord = node;
                        return this;
                    }
                    else if (node is DreamGrammarParser.VariableDeclarationContext)
                    {
                        Trace.WriteLine(node.GetText());
                        return this;
                    }
                    else if (node is DreamGrammarParser.AssignmentOperatorContext)
                    {
                        if (operators.Count == 0)
                        {
                            Trace.WriteLine(node.GetText());
                            operators.Push(node);
                            return this;
                        }

                        throw new ArgumentException("Invalid expressions");
                    }
                    else if (node is DreamGrammarParser.DecimalValueContext)
                    {
                        postfix.Add(node);
                        Trace.WriteLine(node.GetText());
                        return this;
                    }
                    /*
                    
                    else if (!(node is DreamGrammarParser.BinaryOperatorContext))
                    {
                        // other non binary operands will need to be tested here.
                        if (node.children.Count() == DefaultNumberOfChildren)
                        {
                            if (node is DreamGrammarParser.FunctionCallExpressionContext)
                            {
                                postfix.Add(node);
                            }
                        }
                        return this;
                    }
                    */
                    else if (
                        node is DreamGrammarParser.AddSubtractOpContext || 
                        node is DreamGrammarParser.BinaryOperatorContext ||
                        node is DreamGrammarParser.MultiplyDivideOpContext )
                    {
                        if (operators.Count == 0)
                        {
                            operators.Push(node);
                            return this;
                        }
                        if (Precedence(operators.Peek()) > Precedence(node) ||
                            Precedence(operators.Peek()) == Precedence(node))
                        {
                            Trace.WriteLine(operators.Peek().GetText());
                            postfix.Add(operators.Pop());
                            operators.Push(node);
                            return this;
                        }

                        if (Precedence(operators.Peek()) < Precedence(node))
                        {
                            operators.Push(node);
                            return this;
                        }

                    }
                    //}
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return this;
        }

        private int Precedence(IParseTree op)
        {
            int precedence =  operaterLookup[op.GetChild(0).GetText()];
            return precedence;
        }

        private void WalkChildren(IList<IParseTree> children)
        {
            foreach (var child in children)
            {
                if (child is ParserRuleContext)
                {
                    Walk(child as ParserRuleContext);
                }
            }
        }

        internal GenerateBinaryExpression PostWalk()
        {
            while (operators.Count > 0)
            {
                postfix.Add(operators.Pop());
            }

            return this;
        }

        internal GenerateBinaryExpression PrintPostFix()
        {
            foreach(var p in postfix)
            {
                Trace.WriteLine(p.GetText());
            }

            return this;
        }

        internal void Eval()
        {
            foreach (var token in postfix)
            {
                if (token is DreamGrammarParser.AssignmentOperatorContext)
                {
                    CreateBinaryLeftAndRight(unionStack);

                }
                if (token is DreamGrammarParser.BinaryOperatorContext)
                {
                    CreateBinaryLeftAndRight(unionStack);

                    string binaryOp = token.GetChild(0).GetText();

                    SyntaxKind op = syntaxKindLookup[operaterLookup[binaryOp]];

                    unionStack.Push(new ContextExpressionUnion()
                    {
                        context = null,
                        syntax = SyntaxFactory.BinaryExpression(op,
                        unionStack.Pop().syntax,
                        unionStack.Pop().syntax)
                    });
                }
                else
                {
                    unionStack.Push(new ContextExpressionUnion() { context = token, syntax = null });
                }
            }
        }

        private void CreateBinaryLeftAndRight(Stack<ContextExpressionUnion> stack)
        {
            var right = stack.Pop();
            var left = stack.Pop();

            if (right.context != null)
            {
                CheckContextType(right.context, stack);
            }
            else if (right.syntax != null)
            {
                stack.Push(right);
            }

            if (left.context != null)
            { 
                CheckContextType(left.context, stack);
            }
            else if (left.syntax != null)
            {
                stack.Push(left);
            }
        }

        private void CheckContextType(ParserRuleContext context, Stack<ContextExpressionUnion> stack) {
            if (context != null)
            {
                if (context is DreamGrammarParser.DecimalValueContext)
                {
                    stack.Push(new ContextExpressionUnion()
                    {
                        context = null,
                        syntax = CreateNumericLiteralExpression(context)
                    });
                }
                else if (context is DreamGrammarParser.StringValueContext)
                {
                    stack.Push(new ContextExpressionUnion()
                    {
                        context = null,
                        syntax = CreateStringLiteralExpression(context)
                    });
                }
                else if (context is DreamGrammarParser.VariableContext)
                {
                    CreateVariableDeclarator(context, stack.Pop().syntax as BinaryExpressionSyntax);
                }
            }
        }
        

        private LiteralExpressionSyntax CreateNumericLiteralExpression(ParserRuleContext context)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                CreateNumericLiteral(context));
        }

        private LiteralExpressionSyntax CreateStringLiteralExpression(ParserRuleContext context)
        {
            return SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                CreateStringLiteral(context));
        }

        private void CreateVariableDeclarator(ParserRuleContext context, BinaryExpressionSyntax binaryExpression)
        {
            if (!string.IsNullOrEmpty(currentKeyWord.GetText()))
            {
                statementSyntax = SyntaxFactory.LocalDeclarationStatement(
                  declaration: SyntaxFactory.VariableDeclaration(
                     SyntaxFactory.PredefinedType(GetKeywordTokenType(currentKeyWord.GetText())))
                 .WithVariables(
                     SyntaxFactory.SingletonSeparatedList(
                         SyntaxFactory.VariableDeclarator(
                             SyntaxFactory.Identifier(context.GetText())).WithInitializer(
                         SyntaxFactory.EqualsValueClause(
                           binaryExpression)))));
            }
        }

        private SyntaxToken CreateNumericLiteral(ParserRuleContext context)
        {
            int value = Int16.Parse(context.GetChild(0).GetText());
            return SyntaxFactory.Literal(value);
        }

        private SyntaxToken CreateStringLiteral(ParserRuleContext context)
        {
            return SyntaxFactory.Literal(context.GetChild(0).GetText());
        }

        private enum SrEval
        {
            Sht,
            Red,
            Err,
            End,
        }


        private SyntaxToken GetKeywordTokenType(String keyword)
        {
            switch (keyword)
            {
                case "int":
                    return SyntaxFactory.Token(SyntaxKind.IntKeyword);
                case "string":
                    break;
                case "double":
                    break;
            }

            return SyntaxFactory.Token(SyntaxKind.ErrorKeyword);
        }

        private readonly Dictionary<string, int> operaterLookup = new Dictionary<string, int>()
        {
            { "=",  0 },
            { "||", 1 },
            { "&&", 2 },
            { "==", 3 },
            { "!=", 4 },
            { ">=", 5 },
            { "<=", 6 },
            { ">",  7 },
            { "<",  8 },
            { "+",  9 },
            { "-",  10 },
            { "/",  11 },
            { "%",  12 },
            { "*",  13 },
            { "()", 14 },
            { ".",  15 }
        };

        private readonly Dictionary<int, SyntaxKind> syntaxKindLookup = new Dictionary<int, SyntaxKind>()
        {
            { 0 , SyntaxKind.SimpleAssignmentExpression},
            { 1 , SyntaxKind.LogicalOrExpression},
            { 2 , SyntaxKind.LogicalAndExpression},
            { 3 , SyntaxKind.EqualsEqualsToken},
            { 4 , SyntaxKind.NotEqualsExpression},
            { 5 , SyntaxKind.GreaterThanOrEqualExpression},
            { 6 , SyntaxKind.LessThanOrEqualExpression},
            { 7 , SyntaxKind.GreaterThanExpression},
            { 8 , SyntaxKind.LessThanExpression },
            { 9 , SyntaxKind.AddExpression},
            { 10 , SyntaxKind.SubtractExpression},
            { 11 , SyntaxKind.DivideExpression},
            { 12 , SyntaxKind.ModuloExpression},
            { 13 , SyntaxKind.MultiplyExpression},
            { 14 , SyntaxKind.LocalFunctionStatement},
            { 15 , SyntaxKind.DotToken}
        };

        private class OperatorState
        {
            public SrEval[,] shiftReduceOperationTable = new SrEval[,]
            { /*             assign      OR          AND         EQEQ        NOTEQ       GTEQ        LTEQ        GT          LT          PLUS        MINUS       DIV         MOD         MULT        FUNC        DOT        /*
              /* assn */  {  SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* OR   */  {  SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* AND  */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* EQEQ */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* NEQ  */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* GTEQ */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* LTEQ */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* GT   */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* LT   */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* PLUS */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* MINUS*/  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht, SrEval.Sht},
              /* DIV  */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht},
              /* MULT */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht},
              /* MOD  */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht, SrEval.Sht},
              /* FUNC */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Sht},
              /* DOT  */  {  SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red, SrEval.Red},
            };
        }

        private class ContextExpressionUnion
        {
            public ParserRuleContext context;
            public ExpressionSyntax syntax;
        }
    }
}