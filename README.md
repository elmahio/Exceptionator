# The Exceptionator

![The Exceptionator](https://raw.githubusercontent.com/elmahio/Exceptionator/refs/heads/main/src/ExceptionAnalyzer.Vsix/icon.png)

Roslyn analyzers for improving exception handling in C# code.

This project includes a set of diagnostics aimed at encouraging better exception practices. Each rule detects common pitfalls and helps improve reliability, maintainability, and clarity of your exception logic.

## Rules

### EX001: Exception should include a message

Ensures that exceptions are instantiated with a meaningful message.

❌ Bad:
```csharp
throw new InvalidOperationException();
```

✅ Good:
```csharp
throw new InvalidOperationException("Operation failed due to ...");
```

### EX002: Avoid throwing base exceptions

Avoids throwing base exceptions like System.Exception or System.SystemException.

❌ Bad:
```csharp
throw new Exception("Something went wrong");
```

✅ Good:
```csharp
throw new InvalidOperationException("Something went wrong");
```

### EX003: Missing inner exception

Ensures that newly thrown exceptions inside catch blocks include the original exception as inner exception.

❌ Bad:
```csharp
catch (Exception ex)
{
    throw new CustomException("Something failed");
}
```

✅ Good:
```csharp
catch (Exception ex)
{
    throw new CustomException("Something failed", ex);
}
```

### EX004: Use 'throw;' instead of 'throw ex;'

Preserves the original stack trace when rethrowing exceptions.

❌ Bad:
```csharp
catch (Exception ex)
{
    throw ex;
}
```

✅ Good:
```csharp
catch (Exception ex)
{
    throw;
}
```

### EX005: Exception variable is unused

Detects catch variables that are never used.

❌ Bad:
```csharp
catch (Exception ex) { LogError(); }
```

✅ Good:
```csharp
catch (Exception ex) { LogError(ex); }
```

### EX006: Unreachable code after throw

Detects code written after a throw statement that will never execute.

❌ Bad:
```csharp
throw new Exception();
DoSomething();
```

✅ Good:
```csharp
DoSomething();
throw new Exception();
```

### EX007: Pointless try/catch block

Detects try/catch blocks that don’t add meaningful handling logic.

❌ Bad:
```csharp
try { DoSomething(); } catch (Exception) { throw; }
```

✅ Good:
```csharp
DoSomething();
```

### EX008: ThreadAbortException must not be swallowed

Ensures ThreadAbortException is either rethrown or reset.

❌ Bad:
```csharp
catch (ThreadAbortException ex) { LogError(); }
```

✅ Good:
```csharp
catch (ThreadAbortException ex) { Thread.ResetAbort(); }
```

### EX009: Empty try block with catch

Detects try blocks that are empty while having a catch.

❌ Bad:
```csharp
try { } catch (Exception ex) { Log(ex); }
```

✅ Good:
```csharp
try { DoSomething(); } catch (Exception ex) { Log(ex); }
```

### EX010: Task.WaitAll should be wrapped with AggregateException catch

Ensures proper exception handling for Task.WaitAll by requiring AggregateException catch or using Task.WhenAll.

❌ Bad:
```csharp
try { Task.WaitAll(tasks); } catch (Exception ex) { Log(ex); }
```

✅ Good:
```csharp
try { Task.WaitAll(tasks); } catch (AggregateException ex) { Log(ex); }
```

### EX011: Empty catch block

Detects catch blocks that are completely empty without even a comment.

❌ Bad:
```csharp
try { DoSomething(); } catch (Exception) { }
```

✅ Good:
```csharp
try { DoSomething(); } catch (Exception) { /* intentionally ignored */ }
```

### EX012: Don't throw exceptions from property getters

Discourages throwing exceptions directly from property getters.

❌ Bad:
```csharp
public string Name => throw new Exception();
```

✅ Good:
```csharp
public string Name => _name ?? "";
```

### EX013: Avoid throwing ex.InnerException

Detects when ex.InnerException is thrown directly, which may cause null reference issues and loses the stack trace.

❌ Bad:
```csharp
catch (Exception ex) { throw ex.InnerException; }
```

✅ Good:
```csharp
catch (Exception ex) { throw; }
```

### EX014: Avoid logging only ex.Message

Suggests logging the full exception instead of just the message to retain full context.

❌ Bad:
```csharp
LogError(ex.Message);
```

✅ Good:
```csharp
LogError(ex);
```

### EX015: Avoid logging ex.ToString()

Recommends logging the exception directly rather than calling ToString.

❌ Bad:
```csharp
LogError("Error: " + ex.ToString());
```

✅ Good:
```csharp
LogError(ex);
```

### EX016: Avoid empty catch when throwing new exception without message

Detects cases where a new exception is thrown in a catch block without message or inner exception.

❌ Bad:
```csharp
catch (Exception ex) { throw new Exception(); }
```

✅ Good:
```csharp
catch (Exception ex) { throw new Exception("Something went wrong", ex); }
```

### EX017: Avoid when clauses that always evaluate to true

Detects `when` filters on catch blocks that always return true and are thus redundant.

❌ Bad:
```csharp
catch (Exception ex) when (true) { Handle(ex); }
```

✅ Good:
```csharp
catch (Exception ex) when (ex is IOException) { Handle(ex); }
```

### EX018: NotImplementedException left in code

Detects `throw new NotImplementedException()` left in methods or properties.

❌ Bad:
```csharp
public void DoWork() => throw new NotImplementedException();
```

✅ Good:
```csharp
public void DoWork() => ActualImplementation();
```

### EX019: Avoid general catch-all without any handling

Detects general catch blocks that don’t include logging, rethrow, or even a comment.

❌ Bad:
```csharp
try { ... } catch { }
```

✅ Good:
```csharp
try { ... } catch { /* intentionally blank */ }
```

### EX020: Exception class should be public

Ensures that exception types are declared `public` to be visible when thrown or caught across assemblies.

❌ Bad:
```csharp
class CustomException : Exception
{
}
```

✅ Good:
```csharp
public class CustomException : Exception
{
}
```

### EX021: Missing expected constructors on custom exception

Ensures that custom exceptions implement the expected constructors with message and inner exception parameters.

❌ Bad:
```csharp
public class MyCustomException : Exception
{
}
```

✅ Good:
```csharp
public class MyCustomException : Exception
{
    public MyCustomException(string message) : base(message) { }

    public MyCustomException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

### EX022: Exception constructors must call base

Ensures that exception constructors pass their parameters (message, innerException) to the base constructor.

❌ Bad:
```csharp
public MyCustomException(string message) { }
public MyCustomException(string message, Exception inner) { }
```

✅ Good:
```csharp
public MyCustomException(string message) : base(message) { }
public MyCustomException(string message, Exception inner) : base(message, inner) { }
```

### EX023: Exception class name must end with 'Exception'

Ensures consistency and clarity by requiring exception classes to follow the naming convention of ending with 'Exception'.

❌ Bad:
```csharp
public class MyCustomError : Exception
{
}
```

✅ Good:
```csharp
public class MyCustomException : Exception
{
}
```

### EX024: Avoid catching fatal exceptions like StackOverflowException or ExecutionEngineException

Flags catch blocks that handle fatal exceptions which should not be caught or are uncatchable.

❌ Bad:
```csharp
try { ... }
catch (StackOverflowException ex) { Log(ex); }
```

✅ Good:
```csharp
try { ... }
catch (Exception ex) { Log(ex); }
```

### EX025: Catching Exception and checking its type inside the catch block

Flags catch blocks that handle `Exception` (or `System.Exception`) and then immediately check the exception's type with an is expression.
If the code already distinguishes a specific type, a dedicated catch clause should be introduced instead.

❌ Bad:
```csharp
try
{
    DoSomething();
}
catch (Exception ex)
{
    if (ex is InvalidOperationException)
    {
        HandleInvalidOperation();
    }
    else
    {
        Log(ex);
    }
}
```

✅ Good:
```csharp
try
{
    DoSomething();
}
catch (InvalidOperationException ex)
{
    HandleInvalidOperation();
}
catch (Exception ex)
{
    Log(ex);
}
```

## Acknowledgments

* [Davide Bellone](https://github.com/bellons91)
* [Andrew Lock](https://github.com/andrewlock)
* [Marc Jacobi](https://github.com/obiwanjacobi)

---

Sponsored by [elmah.io](https://elmah.io).