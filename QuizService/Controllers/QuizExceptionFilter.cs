using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Refit;

namespace QuizService.Controllers
{
    public class QuizExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var (status, message) = context.Exception switch
            {
                KeyNotFoundException ex => (404, ex.Message),
                ArgumentException ex => (400, ex.Message),
                InvalidOperationException ex => (409, ex.Message),
                DbUpdateException { InnerException: PostgresException { SqlState: "23505" } }
                    => (409, "Question is already in this quiz."),
                ApiException { StatusCode: HttpStatusCode.NotFound }
                    => (404, "Question not found in QuestionService."),
                ApiException { StatusCode: HttpStatusCode.Unauthorized }
                    => (401, "QuestionService rejected the authorization token."),
                ApiException { StatusCode: HttpStatusCode.Forbidden }
                    => (403, "You do not have permission to access these questions."),
                ApiException => (502, "QuestionService returned an unexpected response."),
                HttpRequestException => (502, "Unable to connect to QuestionService."),
                TaskCanceledException => (504, "QuestionService request timed out."),
                _ => (0, string.Empty)
            };

            if (status == 0)
                return;

            context.Result = new ObjectResult(new { message }) { StatusCode = status };
            context.ExceptionHandled = true;
        }
    }
}
