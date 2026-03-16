namespace EventCalendarAPI.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, int id)
            : base($"{entityName} with ID {id} was not found.") { }

        public EntityNotFoundException(string message) : base(message) { }
    }

    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message = "You are not authorized to perform this action.")
            : base(message) { }
    }

    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(List<string> errors) : base("Validation failed.")
        {
            Errors = errors;
        }
    }
}
