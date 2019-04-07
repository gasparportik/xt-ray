/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using XtRay.ParserLib.Abstractions;

namespace XtRay.GtkSharp
{
    class TraceBox : HBox
    {
        private static readonly Gdk.Cursor handCursor = new Gdk.Cursor(Gdk.CursorType.Hand1);

        #region Glade widget instances
        [UI]
        protected Label CallLabel;

        [UI]
        protected Label ParametersLabel;

        [UI]
        protected EventBox ParamsEventBox;

        [UI]
        protected Label FileLabel;

        [UI]
        protected ToggleButton ExpandButton;

        [UI]
        protected Image ExpandButtonIcon;

        [UI]
        protected new VBox Children;
        #endregion

        private ITrace _trace;
        private readonly CssProvider css;

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
                        Children.PackStart(new TraceBox(css, child) { ProfileInfoVisible = _profileDataVisible }, false, true, 0);
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

        private TraceBox(Builder builder) : base(builder.GetObject("TraceBox").Handle)
        {
            builder.Autoconnect(this);
        }

        public TraceBox(CssProvider css, ITrace trace) : this(new Builder("TraceBox.glade"))
        {
            this.css = css;
            Trace = trace;
            ShowAll();
            this.ApplyCss(css);
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
                ParamsEventBox.Window.Cursor = handCursor;
            }
        }

        private void ToggleButton_Clicked(object sender, EventArgs e)
        {
            IsExpanded = !IsExpanded;
            ExpandButtonIcon.Stock = IsExpanded ? "gtk-remove" : "gtk-add";
            Children.Visible = IsExpanded;
        }

        private void ParamsLabelAction(object sender, ButtonPressEventArgs e)
        {
            if (Trace.ReferencedFile == null)
            {
                var dialog = new ParamsWindow(Trace.Parameters)
                {
                    Parent = this,
                    Icon = Program.Logo
                };
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
