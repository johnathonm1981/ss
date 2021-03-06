﻿using Interfaces.Delegates.Correct;
using Interfaces.Input;
using System.Threading.Tasks;

using Interfaces.Status;

namespace Delegates.Correct
{
    public class CorrectUsernamePasswordAsyncDelegate : ICorrectAsyncDelegate<string[]>
    {
        readonly IInputController<string> inputController;

        public CorrectUsernamePasswordAsyncDelegate(IInputController<string> inputController)
        {
            this.inputController = inputController;
        }

        public async Task<string[]> CorrectAsync(string[] usernamePassword, IStatus status)
        {
            if (usernamePassword == null ||
                usernamePassword.Length < 2)
                usernamePassword = new string[2];

            var emptyUsername = string.IsNullOrEmpty(usernamePassword[0]);
            var emptyPassword = string.IsNullOrEmpty(usernamePassword[1]);

            if (emptyUsername)
                usernamePassword[0] =
                    await inputController.RequestInputAsync("Please enter your GOG.com username (email):");

            if (emptyPassword)
                usernamePassword[1] =
                    await inputController.RequestPrivateInputAsync(
                        string.Format(
                            "Please enter password for {0}:", 
                            usernamePassword[0]));

            return usernamePassword;
        }
    }
}
