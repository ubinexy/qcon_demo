﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    using VBF.Compilers;
    using VBF.Compilers.Parsers;
    using VBF.Compilers.Scanners;
    using RE = VBF.Compilers.Scanners.RegularExpression;

    class GLRCombinatorsTestParser : ParserBase<int>
    {
        public GLRCombinatorsTestParser(CompilationErrorManager em) : base(em) { }

        private Token PLUS;
        private Token ASTERISK;
        private Token LEFT_PARENTHESIS;
        private Token RIGHT_PARENTHESIS;
        private Token NUMBER;
        private Token SPACE;

        protected override void OnDefineLexer(Lexicon lexicon, ICollection<Token> triviaTokens)
        {
            var lexer = lexicon.Lexer;

            PLUS = lexer.DefineToken(RE.Symbol('+'));
            ASTERISK = lexer.DefineToken(RE.Symbol('*'));
            LEFT_PARENTHESIS = lexer.DefineToken(RE.Symbol('('));
            RIGHT_PARENTHESIS = lexer.DefineToken(RE.Symbol(')'));
            NUMBER = lexer.DefineToken(RE.Range('0', '9').Many1(), "number");
            SPACE = lexer.DefineToken(RE.Symbol(' ').Many1());

            triviaTokens.Add(SPACE);
        }

        protected override ProductionBase<int> OnDefineGrammar()
        {
            var T = new Production<int>();

            ProductionBase<int> Num = from n in NUMBER select ParseInt32AnyWay(n);

            //U → ‘[0..9]+’ | ‘(’ T ‘)’
            ProductionBase<int> U =
                Num |
                from lp in LEFT_PARENTHESIS
                from exp in T
                from rp in RIGHT_PARENTHESIS
                select exp;

            //F → U | F ‘*’ U
            var F = new Production<int>();
            F.Rule =
                U |
                from f in F
                from op in ASTERISK
                from u in U
                select f * u;

            //T → F | T ‘+’ F
            T.Rule =
                F |
                from t in T
                from op in PLUS
                from f in F
                select t + f;

            //E → T$
            ProductionBase<int> E = from t in T
                                    from eos in Grammar.Eos()
                                    select t;

            return E;
        }

        int ParseInt32AnyWay(Lexeme str)
        {
            int a = 0;
            Int32.TryParse(str.Value.Content, out a);

            return a;
        }
    }

    class GLRCombinatorsTest
    {
        public void Test(SourceReader sr)
        {
            Console.WriteLine("=============== GLR Parser Combinators ==================");

            CompilationErrorManager em = new CompilationErrorManager();
            var parser = new GLRCombinatorsTestParser(em);

            var errList = em.CreateErrorList();

            var result = parser.Parse(sr, errList);

            if (errList.Count == 0)
            {
                Console.WriteLine("Result: {0}", result);
            }
            else
            {
                Console.WriteLine("Parse Errors:");
                foreach (var err in
                                from e in errList orderby e.ErrorPosition.StartLocation select e)
                {
                    Console.WriteLine(err.ToString());
                }
            }


            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
