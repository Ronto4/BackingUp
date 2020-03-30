using BackingUpConsole.Utilities.Commands;
using BackingUpConsole.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackingUpConsole.Utilities
{
    static class ArgumentHandler
    {
        public static (MessageHandler message, string[] args, UInt16 flags) ParseArguments(string[] args, UInt16 flags, MessagePrinter messagePrinter, Command command)
        {
            MessageHandler message;
            (message, flags) = flags.SetFromArgs(ref args);
            if (!message.IsSuccess(false, messagePrinter))
                return (message, args, flags);

            (message, args) = args.SortArgs(messagePrinter, command);
            if (!message.IsSuccess(false, messagePrinter))
                return (message, args, flags);

            return (MessageProvider.Success(), args, flags);
        }

        private static (MessageHandler message, string[] args) SortArgs(this string[] args, MessagePrinter messagePrinter, Command command)
        {
            if (command.ArgOrder.Length == 0)
                return (MessageProvider.Success(), args);

            var computedArgs = from arg in args where arg.StartsWith("+") select arg.Substring(1);
            if (computedArgs.Count() == 0)
                return (MessageProvider.Success(), args);

            if (computedArgs.Count() != args.Length)
                return (MessageProvider.MixedArguments(), args);

            var splitArgs = from arg in computedArgs select (arg.Split(':')[0], arg.Split(':')[1..], Array.IndexOf(command.ArgOrder, arg.Split(':')[0]));
            splitArgs = splitArgs.OrderBy((elem) => elem.Item3);

            var splittingArgs = splitArgs.ToArray();

            if (splittingArgs[0].Item3 < 0)
                return (MessageProvider.UnknownArgument(splittingArgs[0].Item1), args);

            string[] newArgs = new string[splitArgs.Count()];
            for (int i = 0; i < newArgs.Length; i++)
            {
                newArgs[i] = String.Empty;
                for (int j = 0; j < splittingArgs[i].Item2.Length - 1; j++)
                {
                    newArgs[i] += splittingArgs[i].Item2[j];
                    newArgs[i] += ":";
                }
                newArgs[i] += splittingArgs[i].Item2[^1];
            }
            return (MessageProvider.Success(), newArgs);
        }
    }
}
