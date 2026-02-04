using POS.UI.Core.Exceptions;
using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace POS.UI.Core.Services
{
    /// <summary>
    /// Base API service class providing common functionality for all API service classes.
    /// Handles HTTP response validation, error extraction, exception throwing, and structured logging.
    /// </summary>
    public abstract class BaseApiService
    {
        protected readonly HttpClient _http;
        protected readonly ILogger _logger;

        protected BaseApiService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            
            // Get logger for this service type
            _logger = Log.ForContext(this.GetType());
        }

        /// <summary>
        /// Ensures the HTTP response is successful. If not, extracts and throws appropriate exceptions.
        /// Includes detailed structured logging for diagnostics.
        /// </summary>
        /// <param name="response">The HTTP response to validate.</param>
        /// <param name="operationName">Optional name of the operation for logging context.</param>
        /// <exception cref="ApiValidationException">Thrown when response is 400 Bad Request with validation errors.</exception>
        /// <exception cref="HttpRequestException">Thrown for other non-success responses.</exception>
        protected async Task EnsureSuccessAsync(HttpResponseMessage response, string operationName = null)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            // Success - log and return
            if (response.IsSuccessStatusCode)
            {
                _logger.Information(
                    "API request successful [{Operation}]: {Method} {RequestUri} -> {StatusCode}",
                    operationName ?? "Unknown",
                    response.RequestMessage?.Method,
                    response.RequestMessage?.RequestUri,
                    (int)response.StatusCode);
                return;
            }

            // Log failed request details
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.Warning(
                "API request failed [{Operation}]: {Method} {RequestUri} -> {StatusCode}. Response: {ResponseContent}",
                operationName ?? "Unknown",
                response.RequestMessage?.Method,
                response.RequestMessage?.RequestUri,
                (int)response.StatusCode,
                responseContent);

            // Handle specific error codes
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    // Extract message from response (content already read above; do not read stream again)
                    try
                    {
                        var error = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);
                        if (error.TryGetProperty("message", out var msgProp))
                        {
                            var msg = msgProp.GetString();
                            if (!string.IsNullOrEmpty(msg))
                            {
                                _logger.Warning("API BadRequest [{Operation}]: {Message}", operationName ?? "Unknown", msg);
                                throw new HttpRequestException(msg);
                            }
                        }
                        if (error.TryGetProperty("errors", out var errors) && errors.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var apiError = System.Text.Json.JsonSerializer.Deserialize<ApiValidationError>(responseContent);
                            throw new ApiValidationException(apiError ?? new ApiValidationError { Errors = new() });
                        }
                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }
                    catch (ApiValidationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "Could not parse BadRequest response for operation [{Operation}]", operationName ?? "Unknown");
                    }
                    throw new HttpRequestException(string.IsNullOrEmpty(responseContent) ? "Request was rejected by the server." : responseContent);

                case HttpStatusCode.Unauthorized:
                    _logger.Warning("Authentication required (401) for operation [{Operation}] at {RequestUri}", 
                        operationName ?? "Unknown",
                        response.RequestMessage?.RequestUri);
                    throw new HttpRequestException("Authentication required. Please log in.");

                case HttpStatusCode.Forbidden:
                    _logger.Warning("Access forbidden (403) for operation [{Operation}] at {RequestUri}",
                        operationName ?? "Unknown",
                        response.RequestMessage?.RequestUri);
                    throw new HttpRequestException("You do not have permission to perform this action.");

                case HttpStatusCode.NotFound:
                    _logger.Warning("Resource not found (404) for operation [{Operation}] at {RequestUri}",
                        operationName ?? "Unknown",
                        response.RequestMessage?.RequestUri);
                    throw new HttpRequestException("The requested resource was not found.");

                case HttpStatusCode.InternalServerError:
                    _logger.Error("Internal server error (500) for operation [{Operation}] at {RequestUri}",
                        operationName ?? "Unknown",
                        response.RequestMessage?.RequestUri);
                    throw new HttpRequestException("An internal server error occurred. Please try again later.");

                case HttpStatusCode.ServiceUnavailable:
                    _logger.Error("Service unavailable (503) for operation [{Operation}] at {RequestUri}",
                        operationName ?? "Unknown",
                        response.RequestMessage?.RequestUri);
                    throw new HttpRequestException("The service is temporarily unavailable. Please try again later.");
            }

            // For any other non-success status, throw with detailed message (use already-read responseContent)
            var message = $"HTTP {(int)response.StatusCode} {response.StatusCode}";
            if (!string.IsNullOrEmpty(responseContent))
                message += $": {responseContent}";

            _logger.Error("API error [{Operation}] ({StatusCode}): {Message}",
                operationName ?? "Unknown",
                (int)response.StatusCode, 
                message);
            
            throw new HttpRequestException(message);
        }

        protected async Task<string?> TryGetJsonAsync(params string[] urls)
        {
            foreach (var url in urls)
            {
                try
                {
                    var resp = await _http.GetAsync(url);
                    if (resp.IsSuccessStatusCode)
                    {
                        return await resp.Content.ReadAsStringAsync();
                    }
                }
                catch
                {
                }
            }
            return null;
        }
    }
}
