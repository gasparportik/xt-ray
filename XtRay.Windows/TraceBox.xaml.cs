/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace XtRay.Windows
{
    using Common;
    using Common.Abstractions;

    public partial class TraceBox : UserControl, ITraceUiNode
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Trace
        public static readonly DependencyProperty TraceProperty = DependencyProperty.Register("Trace", typeof(object), typeof(TraceBox), new UIPropertyMetadata(null, TraceChangedCallback));


        public ITrace Trace
        {
            get { return (ITrace)GetValue(TraceProperty); }
            set { SetValue(TraceProperty, value); }
        }

        private static void TraceChangedCallback(DependencyObject owner, DependencyPropertyChangedEventArgs args)
        {
            var window = (TraceBox)owner;
            var x = (ITrace)args.NewValue;
        }
        #endregion

        public string Call => Trace.Call.Name;

        public bool HasParameters => Trace.Parameters != null && Trace.Parameters.Length > 0;

        // Minimize parameters
        public string Parameters
        {
            get
            {
                if (Trace.ReferencedFile == null)
                {
                    return Trace.Parameters != null && Trace.Parameters.Length > 0 ? ParseParameters(Trace.Parameters) : string.Empty;
                }
                return "'" + Trace.ReferencedFile.Path + "'";
            }
        }

        // if parameters not too long ,just show in tooltip
        public string TooltipParameters
        {
            get
            {
                return FullParameters.Length > 80 ? "Dobule Click to Show All Parameters" : FullParameters;
            }
        }

        public string FullParameters
        {
            get
            {
                if (Trace.ReferencedFile == null)
                {
                    return Trace.Parameters != null && Trace.Parameters.Length > 0 ? string.Join(", ", Trace.Parameters) : "No Parameters";
                }
                return "'" + Trace.ReferencedFile.Path + "'";
            }
        }

        public string ParseParameters(string[] parameters)
        {
            string[] parsed = new string[parameters.Length];
            for (var i = parameters.Length - 1; i >= 0; --i)
            {
                var _tmp = parameters[i].Split(' ');
                parsed[i] = _tmp[0];
            }
            return string.Join(",",parsed);
        }
        
        public string TooltipReturnValue
        {
            get
            {
                return ReturnValue.Length > 50 ? "Dobule Click to Show Return Value" : ReturnValue;
            }
        }

        public string ReturnValueVisibility => Trace.ReturnValue?.Length > 0 ? "Visible" : "Hidden";

        public string ReturnValue => Trace.ReturnValue ?? "";
        public string FileInfo => Trace.File.Path + " @ L" + Trace.FileLine;

        public bool IsExpandable => Trace.Children.Length > 0;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                OnPropertyChanged(nameof(Expanded));
            }
        }
        public Visibility Expanded => IsExpandable && IsExpanded ? Visibility.Visible : Visibility.Collapsed;

        private bool _profileDataVisible = false;
        public bool ProfileInfoVisible
        {
            get => _profileDataVisible;
            set
            {
                _profileDataVisible = value;
                if (ChildrenPanel != null)
                {
                    foreach (TraceBox child in ChildrenPanel.Children)
                    {
                        child.ProfileInfoVisible = _profileDataVisible;
                    }
                }
                OnPropertyChanged("ProfileInfoVisibility");
            }
        }
        public Visibility ProfileInfoVisibility => _profileDataVisible ? Visibility.Visible : Visibility.Collapsed;

        public string TimingInfo => string.Format("{0:0.000}ms / {1:0.000}ms", Trace.SelfTime * 1000, Trace.CumulativeTime * 1000);

        public double TotalTimingPercent => Trace.TimePercent;

        public double ParentTimingPercent => Trace.ParentTimePercent;

        public bool IsUserDefined => Trace.IsUserDefined;

        public Brush BackColor => Trace.IsUserDefined ? Brushes.Aquamarine : Brushes.Beige;

        public bool FilterMatched
        {
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    Dispatcher.InvokeAsync(() => { Visibility = value ? Visibility.Visible : Visibility.Collapsed; });
                }
            }
        }

        public TraceBox(ITrace trace)
        {
            Trace = trace;
            InitializeComponent();
            DataContext = this;
            IsExpanded = false;

        }

        public void ShowChildren(IEnumerable<FlexibleTraceNode> traces)
        {
            Action action = () => {
                ChildrenPanel.Children.Clear();
                foreach (var trace in traces)
                {
                    var childNode = new TraceBox(trace.Trace) { ProfileInfoVisible = _profileDataVisible };
                    trace.UiNode = childNode;
                    ChildrenPanel.Children.Add(childNode);
                }
            };
            if (ChildrenPanel.Dispatcher.CheckAccess())
            {
                action();
            } else
            {
                ChildrenPanel.Dispatcher.Invoke(action);
            }
        }

        private void ExpandButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                IsExpanded = e.Key == Key.Right;
                e.Handled = true;
            }
        }

        private void ParamsLabelAction(object sender, MouseButtonEventArgs e)
        {
            if (Trace.ReferencedFile == null)
            {
                var dialog = new ParamsDialog(Trace.Parameters)
                {
                    Owner = Window.GetWindow(this)
                };
                dialog.ShowDialog();
            }
        }

        private void ValueLabelAction(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ReturnValueDialog(Trace.ReturnValue)
            {
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TraceBox() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
