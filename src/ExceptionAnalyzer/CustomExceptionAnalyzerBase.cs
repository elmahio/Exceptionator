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
    }
}
