/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using Gtk;

namespace XtRay.GtkSharp
{
    using Common.Parsers;
    using Common.Abstractions;

    class TraceBox : VBox
    {
        private static readonly Gdk.Cursor handCursor = new Gdk.Cursor(Gdk.CursorType.Hand1);

        #region Glade widget instances
#pragma warning disable 0649
        [Glade.Widget]
        protected HBox TraceContainer;

        [Glade.Widget]
        protected Label CallLabel;

        [Glade.Widget]
        protected Label ParametersLabel;

        [Glade.Widget]
        protected EventBox ParamsEventBox;

        [Glade.Widget]
        protected Label FileLabel;

        [Glade.Widget]
        protected ToggleButton ExpandButton;

        [Glade.Widget]
        protected new VBox Children;
#pragma warning restore 0649
        #endregion

        private ITrace _trace;
        public ITrace Trace
        {
            get
            {
                return _trace;
            }
            set
            {
                _trace = value;
                ShowTrace();
            }
        }

        public bool IsExpandable
        {
            get
            {
                return Trace?.Children.Length > 0;
            }
        }
        private bool _childrenLoaded;
        private bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (value && !_childrenLoaded)
                {
                    foreach (var child in Trace.Children)
                    {
                        Children.PackStart(new TraceBox(child) { ProfileInfoVisible = _profileDataVisible }, false, true, 0);
                    }
                    _childrenLoaded = true;
                }
                _isExpanded = value;
            }
        }

        #region Profiling data view(not yet used)
        private bool _profileDataVisible = false;
        public bool ProfileInfoVisible
        {
            get
            {
                return _profileDataVisible;
            }
            set
            {
                _profileDataVisible = value;
                if (_childrenLoaded)
                {
                    foreach (TraceBox child in Children.Children)
                    {
                        child.ProfileInfoVisible = _profileDataVisible;
                    }
                }
            }
        }

        public string TimingInfo
        {
            get
            {
                return string.Format("{0:0.000}ms / {1:0.000}ms", Trace.SelfTime * 1000, Trace.CumulativeTime * 1000);
            }
        }

        public double TotalTimingPercent
        {
            get
            {
                return Trace.TimePercent;
            }
        }

        public double ParentTimingPercent
        {
            get
            {
                return Trace.ParentTimePercent;
            }
        }
        #endregion

        public TraceBox() : base(false, 0)
        {
            using (var file = App.GetEmbeddedResourceStream("glade.TraceBox.glade"))
            {
                Glade.XML gxml = new Glade.XML(file, "TraceContainer", "");
                gxml.Autoconnect(this);
                this.PackStart(TraceContainer, false, true, 0);
                this.ShowAll();
            }

            IsExpanded = false;
        }

        private void ShowTrace()
        {
            CallLabel.Markup = (Trace.IsUserDefined ? "<b>" : "") + Trace.Call.Name + (Trace.IsUserDefined ? "</b>" : "") + "(";
            if (Trace.ReferencedFile == null)
            {
                ParametersLabel.Markup = Trace.Parameters != null ? "<u>" + string.Join(", ", Trace.Parameters) + "</u>" : string.Empty;
            }
            else
            {
                ParametersLabel.Markup = Trace.ReferencedFile.Path;
            }
            FileLabel.Text = Trace.File.Path + " @ L" + Trace.FileLine;
            ExpandButton.Sensitive = IsExpandable;
        }


        private void ParamsRealize(object sender, EventArgs e)
        {
            if (Trace.Parameters != null)
            {
                ParamsEventBox.GdkWindow.Cursor = handCursor;
            }
        }

        public TraceBox(ITrace trace) : this()
        {
            Trace = trace;
        }

        private void ToggleButton_Clicked(object sender, EventArgs e)
        {
            IsExpanded = !IsExpanded;
            ExpandButton.Label = IsExpanded ? "-" : "+";
            Children.Visible = IsExpanded;
        }

        private void ParamsLabelAction(object sender, ButtonPressEventArgs e)
        {
            if (Trace.ReferencedFile == null)
            {
                var dialog = new ParamsWindow(Trace.Parameters);
                var result = (ResponseType)Enum.ToObject(typeof(ResponseType), dialog.Run());
                switch (result)
                {
                    case ResponseType.DeleteEvent:
                    case ResponseType.Cancel:
                        dialog.Destroy();
                        break;
                }
            }
        }
    }
}
