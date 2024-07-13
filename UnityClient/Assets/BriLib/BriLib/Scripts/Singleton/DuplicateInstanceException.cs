using System;

namespace BriLib
{
  public class DuplicateInstanceException : Exception
  {
    public DuplicateInstanceException(string exception) : base(exception) { }
  }
}
