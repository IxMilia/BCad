﻿using System;
using System.Windows.Input;

namespace BCad
{
    internal class UserCommand : ICommand
    {
        private string commandName = null;
        private IWorkspace workspace = null;

        public UserCommand(IWorkspace workspace, string command)
        {
            this.workspace = workspace;
            this.commandName = command;

            this.workspace.CommandExecuting += workspace_CommandExecuting;
            this.workspace.CommandExecuted += workspace_CommandExecuted;
        }

        void workspace_CommandExecuting(object sender, EventArguments.CommandExecutingEventArgs e)
        {
            OnCanExecuteChanged(new EventArgs());
        }

        void workspace_CommandExecuted(object sender, EventArguments.CommandExecutedEventArgs e)
        {
            OnCanExecuteChanged(new EventArgs());
        }

        protected void OnCanExecuteChanged(EventArgs e)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, e);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return this.workspace.CanExecute();
        }

        public void Execute(object parameter)
        {
            this.workspace.ExecuteCommand(this.commandName);
        }
    }
}
