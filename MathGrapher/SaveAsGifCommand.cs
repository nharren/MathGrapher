using System;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace MathGrapher
{
    internal class SaveAsGifCommand : ICommand
    {
        private bool canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">A Graph to be rendered as a GIF.</param>
        /// <returns>True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            var result = parameter is Graph;

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
        /// <param name="parameter">A Graph to be rendered as a GIF.</param>
        public void Execute(object parameter)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "Graph";
            dialog.AddExtension = true;
            dialog.DefaultExt = ".gif";
            dialog.Filter = "Graphics Interchange Format (.gif)|*.gif";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var graph = (Graph)parameter;
                graph.BeginAnimation(Graph.XProperty, null);

                using (var fileStream = File.Create(dialog.FileName))
                using (var encoder = new AnimatedGifEncoder(fileStream, 0))
                {
                    if (graph.DomainAnimation is DoubleAnimation)
                    {
                        var frameCount = graph.DomainAnimation.Duration.TimeSpan.TotalSeconds * 50; // frames per second.

                        var xSkip = (graph.Domain.Y - graph.Domain.X) / frameCount;

                        for (int i = 0; i < frameCount; i++)
                        {
                            graph.X = graph.Domain.X + i * xSkip;
                            graph.UpdateLayout();

                            var renderTargetBitmap = new RenderTargetBitmap((int)graph.ActualWidth, (int)graph.ActualHeight, 96, 96, PixelFormats.Default);
                            renderTargetBitmap.Render(graph);

                            encoder.AddFrame(renderTargetBitmap, TimeSpan.FromMilliseconds(20));
                        }
                    }
                    else if (graph.DomainAnimation is DoubleAnimationUsingKeyFrames)
                    {
                        var animation = (DoubleAnimationUsingKeyFrames)graph.DomainAnimation;

                        for (int i = 0; i < animation.KeyFrames.Count; i++)
                        {
                            graph.X = animation.KeyFrames[i].Value;
                            graph.UpdateLayout();

                            var renderTargetBitmap = new RenderTargetBitmap((int)graph.ActualWidth, (int)graph.ActualHeight, 96, 96, PixelFormats.Default);
                            renderTargetBitmap.Render(graph);

                            var delay = i + 1 < animation.KeyFrames.Count ? animation.KeyFrames[i + 1].KeyTime.TimeSpan - animation.KeyFrames[i].KeyTime.TimeSpan : (animation.Duration - animation.KeyFrames[i].KeyTime.TimeSpan).TimeSpan;

                            encoder.AddFrame(renderTargetBitmap, delay);
                        }
                    }
                    
                }

                graph.BeginAnimation(Graph.XProperty, graph.DomainAnimation);
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