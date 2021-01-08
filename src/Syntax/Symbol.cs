/* Copyright (C) 2019-2021 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;
using Ligral.Component;

namespace Ligral.Syntax
{
    class Symbol
    {
        public string Name;
        public TypeSymbol Type;
        protected object Value;
        protected Logger logger = new Logger("Inspector");
        public Symbol(string name, TypeSymbol type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
        public virtual object GetValue()
        {
            return Value;
        }
        public override string ToString()
        {
            return $"{GetType().Name}({Name}:{Type.Name})";
        }
    }

    class DigitSymbol : Symbol
    {
        public DigitSymbol(string name, TypeSymbol type, double value) : base(name, type, value) {}
    }

    class MatrixSymbol : Symbol
    {
        public MatrixSymbol(string name, TypeSymbol type, Matrix<double> value) : base(name, type, value) {}
    }

    class ModelSymbol : Symbol
    {
        public ModelSymbol(string name, TypeSymbol type, ILinkable value) : base(name, type, value) {}
    }

    class TypeSymbol : Symbol
    {
        public TypeSymbol(string name, TypeSymbol type, object value) : base(name, type, value) {}
        public override object GetValue()
        {
            switch (Value)
            {
            case string modelType:
                return ModelManager.Create(modelType);
            case ScopedModelType scopedModelType:
                return ModelManager.Create(scopedModelType);
            case RouteConstructor routeConstructor:
                return routeConstructor.Construct();
            case Signature signature:
                return signature;
            default:
                throw logger.Error(new LigralException($"Invalid type symbol with value {Value.GetType()}"));
            }
        }
    }

    class ScopeSymbol : Symbol
    {
        public ScopeSymbol(string name, TypeSymbol type, ScopeSymbolTable value) : base(name, type, value) {}
    }

    class ScopeSymbolTable
    {
        public Dictionary<string,Symbol> Symbols = new Dictionary<string, Symbol>();
        private string name;
        public string ScopeName
        {
            get
            {
                if (enclosingScope != null && enclosingScope.ScopeName != "<global>")
                {
                    return enclosingScope.ScopeName + "." + name;
                }
                else
                {
                    return name;
                }
            }
            set
            {
                name = value;
            }
        }
        public int scopeLevel;
        private ScopeSymbolTable enclosingScope;
        public ScopeSymbolTable(string name, int level, ScopeSymbolTable enclosingScope=null)
        {
            this.ScopeName = name;
            this.scopeLevel = level;
            this.enclosingScope = enclosingScope;
            InitBuiltins();
        }

        private void InitBuiltins()
        {
            Insert(new TypeSymbol("DIGIT", null, null));
            Insert(new TypeSymbol("MATRIX", null, null));
            Insert(new TypeSymbol("SCOPE", null, null));
            Insert(new TypeSymbol("MODEL", null, null));
            Insert(new TypeSymbol("ROUTE", null, null));
            Insert(new TypeSymbol("SIGN", null, null));
            TypeSymbol modelType = Lookup("MODEL") as TypeSymbol;
            foreach (string modelName in ModelManager.ModelTypePool.Keys)
            {
                Insert(new TypeSymbol(modelName, modelType, modelName));
            }
            TypeSymbol digitType = Lookup("DIGIT") as TypeSymbol;
            Insert(new DigitSymbol("pi", digitType, Math.PI));
            Insert(new DigitSymbol("e", digitType, Math.E));
        }
        public bool Insert(Symbol symbol, bool canOverride=true)
        {
            if (!canOverride && Lookup(symbol.Name, false)!=null)
            {
                return false;
            }
            else
            {
                Symbols[symbol.Name] = symbol;
                return true;
            }
        }
        public Symbol Lookup(string name, bool recursively = true)
        {
            if (Symbols.ContainsKey(name))
            {
                return Symbols[name];
            }
            else if (recursively && enclosingScope!=null)
            {
                return enclosingScope.Lookup(name);
            }
            else
            {
                return null;
            }
        }
        public ScopeSymbolTable Merge(ScopeSymbolTable scope)
        {
            foreach(Symbol symbol in scope.Symbols.Values)
            {
                Insert(symbol);
            }
            return this;
        }
        public override string ToString()
        {
            List<string> lines = new List<string>
            {
                "\n",
                "SCOPE SYMBOL TABLE",
                "==================",
                string.Format("{0,20} {1}", "Scope Name", ScopeName),
                string.Format("{0,20} {1}", "Scope Level", scopeLevel),
                string.Format("{0,20} {1}", "Enclosing Scope", enclosingScope==null?"None":enclosingScope.ScopeName),
                "------------------"
            };
            foreach (Symbol symbol in Symbols.Values)
            {
                lines.Add(string.Format("{0,20} {1}", symbol.Name, symbol));
            }
            lines.Add(lines[2]);
            lines.Add(lines[0]);
            return string.Join('\n', lines);
        }
    }
}