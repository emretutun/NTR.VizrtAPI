using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Entities
{
    public class CommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static CommandResult Ok(string message = "OK", object? data = null)
            => new CommandResult { Success = true, Message = message, Data = data };

        public static CommandResult Fail(string message)
            => new CommandResult { Success = false, Message = message };
    }
}