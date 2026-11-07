using Microsoft.AspNetCore.Mvc;

namespace Server.Utilities;

public static class ApiResponse
{
    public static IActionResult Success(object? data)
    {
        return new ObjectResult(new
        {
            success = true,
            data = data ?? null
        })
        {
            StatusCode = 200
        };
    }

    public static IActionResult UserError(string message)
    {
        return new ObjectResult(new
        {
            success = false,
            message
        })
        {
            StatusCode = 404
        };
    }

    public static IActionResult ServerError(string message, Exception? ex = null)
    {
        Console.WriteLine($"[500] {message}\n{ex}");

        return new ObjectResult(new
        {
            success = false,
            message
        })
        {
            StatusCode = 500
        };
    }
}