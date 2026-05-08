using System;

namespace SharedModels.Exceptions
{
  public class NotFoundException(string message) : Exception(message)
  {
  }

  public class ValidationException(string message) : Exception(message)
  {
  }
}