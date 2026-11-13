using AMSoluciones.Domain.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Errores;

/// <summary>
/// Traduce las excepciones de Domain a respuestas HTTP con ProblemDetails
/// (status, title, detail y una extensión "code" al nivel superior):
///   NoEncontradoException -> 404, ConflictoException (y derivadas) -> 409,
///   DominioException -> 400, cualquier otra -> 500.
/// </summary>
public class ManejadorExcepciones : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ManejadorExcepciones> _logger;

    public ManejadorExcepciones(IProblemDetailsService problemDetails, ILogger<ManejadorExcepciones> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (estado, titulo, codigo, detalle) = exception switch
        {
            NoEncontradoException e => (StatusCodes.Status404NotFound, "Recurso no encontrado", e.Codigo, e.Message),
            ConflictoException e => (StatusCodes.Status409Conflict, "Conflicto con el estado actual", e.Codigo, e.Message),
            DominioException e => (StatusCodes.Status400BadRequest, "Solicitud inválida", e.Codigo, e.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", "ERROR_INTERNO",
                  "Ocurrió un error inesperado.")
        };

        if (estado == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado");

        httpContext.Response.StatusCode = estado;

        var problema = new ProblemDetails
        {
            Status = estado,
            Title = titulo,
            Detail = detalle
        };
        problema.Extensions["code"] = codigo;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problema,
            Exception = exception
        });
    }
}
