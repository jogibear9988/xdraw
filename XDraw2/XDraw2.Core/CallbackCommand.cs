using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace XDraw
{
   /// <summary>
   /// Basci implementation of a command
   /// </summary>
   public class CallbackCommand : ICommand
   {
      /// <summary>
      /// Creates a basic command.
      /// </summary>
      /// <param name="executeCallback">Callback to be called on command Execute.</param>
      /// <param name="canExecuteCallback">Callback to be called on command CanExecute (optional)</param>
      public CallbackCommand(Action<object> executeCallback, Func<object, bool> canExecuteCallback = null)
      {
         _ExecuteCallback = executeCallback;
         _CanExecuteCallback = canExecuteCallback;
      }

      private Action<object> _ExecuteCallback;
      private Func<object, bool> _CanExecuteCallback;

      /// <summary>
      /// Defines the method to be called when the command is invoked.
      /// </summary>
      /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
      public void Execute(object parameter)
      {
         if (_ExecuteCallback != null && CanExecute(parameter))
         {
            _ExecuteCallback(parameter);
         }
      }

      /// <summary>
      /// Defines the method that determines whether the command can execute in its current state.
      /// </summary>
      /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
      /// <returns>true if this command can be executed; otherwise, false.</returns>
      public bool CanExecute(object parameter)
      {
         if (_CanExecuteCallback != null)
         {
            return _CanExecuteCallback(parameter);
         }
         return true;
      }

      /// <summary>
      /// Notifies all sources whether or not the command should execute has changed.
      /// </summary>
      public void NotifyCanExecuteChanged()
      {
         var temp = CanExecuteChanged;
         if (temp != null)
         {
            temp(this, new EventArgs());
         }
      }

      /// <summary>
      /// Occurs when changes occur that affect whether or not the command should execute.
      /// </summary>
      public event EventHandler CanExecuteChanged;
   }
}
