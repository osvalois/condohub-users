using System;

namespace AuthService.Application.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public Guid UserId { get; set; }
        public string? Message { get; set; }

        // Constructor público
        public AuthResult() { }

        public static AuthResult SuccessResult(string token, Guid userId)
        {
            return new AuthResult
            {
                Success = true,
                Token = token,
                UserId = userId,
                Message = "Authentication successful"
            };
        }

        public static AuthResult FailureResult(string message)
        {
            return new AuthResult
            {
                Success = false,
                Token = null,
                UserId = Guid.Empty,
                Message = message
            };
        }

        // Método adicional para crear un resultado de éxito con un mensaje personalizado
        public static AuthResult SuccessResultWithMessage(string token, Guid userId, string message)
        {
            return new AuthResult
            {
                Success = true,
                Token = token,
                UserId = userId,
                Message = message
            };
        }
    }
}