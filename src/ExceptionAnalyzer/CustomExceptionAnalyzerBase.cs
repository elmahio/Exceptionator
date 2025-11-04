using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExceptionAnalyzer
{
    public abstract class CustomExceptionAnalyzerBase : DiagnosticAnalyzer
    {
        protected static bool InheritsFromException(ITypeSymbol? symbol)
        {
            while (symbol != null)
            {
                if (symbol.ToDisplayString() == "System.Exception")
                    return true;
                symbol = symbol.BaseType;
            }
            return false;
        }

        protected static bool InheritsFrom(ITypeSymbol type, ITypeSymbol potentialBase)
        {
            var current = type;
            while (current.BaseType is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(current.BaseType, potentialBase))
                    return true;

                current = current.BaseType;
            }

            return false;
        }
    }
}
