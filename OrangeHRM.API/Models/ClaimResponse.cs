namespace OrangeHRM.API.Models;

/// <summary>
/// Ответ после создания претензии
/// </summary>
public class ClaimResponse
{
    public bool Success { get; set; }

    public string? ReferenceId { get; set; }

    public string? ErrorMessage { get; set; }

    public static ClaimResponse CreateSuccess(string referenceId)
    {
        return new ClaimResponse
        {
            Success = true,
            ReferenceId = referenceId
        };
    }

    public static ClaimResponse CreateError(string errorMessage)
    {
        return new ClaimResponse
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}