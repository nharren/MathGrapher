using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MathGrapher
{
    public class SaveAsPngCommand : ICommand
    {
        private bool canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">A FrameworkElement to be rendered as a PNG.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            var result = parameter is FrameworkElement;

            if (result != canExecute)
            {
                canExecute = result;

                RaiseCanExecuteChanged();
            }

            return result;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">A FrameworkElement to be rendered as a PNG.</param>
        public void Execute(object parameter)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Graph";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".png";
            dialog.Filter = "Portable Network Graphics (.png)|*.png";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var frameworkElement = (FrameworkElement)parameter;
                
                var renderTargetBitmap = new RenderTargetBitmap((int)frameworkElement.ActualWidth, (int)frameworkElement.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(frameworkElement);

                var pngBitmapEncoder = new PngBitmapEncoder();
                pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                using (var fileStream = File.Create(dialog.FileName))
                {
                    pngBitmapEncoder.Save(fileStream);
                }
            }
        }

        /// <summary>
        /// Notifies subscribers to the CanExecuteChanged event when the result changes.
        /// </summary>
        /// <param name="result"></param>
        internal void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, null);
            }
        }
    }
}