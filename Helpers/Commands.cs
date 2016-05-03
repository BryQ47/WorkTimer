using System;
using System.Windows.Input;

namespace WorkTimer.Helpers
{
    public class ButtonCommand : ICommand
    {
        private Action WhattoExecute;
        private Func<bool> WhentoExecute;

        public ButtonCommand(Action What , Func<bool> When)
        {
            WhattoExecute = What;
            WhentoExecute = When;
        }

        public bool CanExecute(object parameter)
        {
            return WhentoExecute();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            WhattoExecute();
        }
    }
}
