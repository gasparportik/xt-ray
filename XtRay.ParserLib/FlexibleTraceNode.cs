using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtRay.Common.Abstractions;

namespace XtRay.Common
{
    public class FlexibleTraceNode
    {

        public IDictionary<FlexibleTraceNode, bool> Children { get; private set; }
        public ITrace Trace { get; private set; }
        private ITraceUiNode _uiNode;
        public ITraceUiNode UiNode
        {
            get => _uiNode;
            set
            {
                if (_uiNode != null)
                {
                    _uiNode.PropertyChanged -= UiNode_PropertyChanged;
                }
                _uiNode = value;
                if (_uiNode != null)
                {
                    _uiNode.PropertyChanged += UiNode_PropertyChanged;
                }
            }
        }
        public ITraceFilter Filter { get; set; }
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            private set
            {
                if (!value && Children != null)
                {
                    foreach (var child in Children.Keys)
                    {
                        child.IsExpanded = false;
                    }
                }
                _isExpanded = value;
            }
        }

        //

        public FlexibleTraceNode(ITrace trace)
        {
            Trace = trace;
            if (Trace.Children.Length > 0)
            {
                Children = new Dictionary<FlexibleTraceNode, bool>(Trace.Children.Length);
                foreach (var childTrace in Trace.Children)
                {
                    Children.Add(new FlexibleTraceNode(childTrace), true);
                }
            }
        }

        private void UiNode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExpanded")
            {
                IsExpanded = _uiNode.IsExpanded;
                if (_uiNode.IsExpanded)
                {
                    Expand();
                }
                else
                {
                    Contract();
                }
            }
        }

        private void Expand()
        {
            if (Children != null)
            {
                var visibleChildren = Children.Where(c => c.Value).Select(c => c.Key);
                _uiNode.ShowChildren(visibleChildren);
                foreach (var child in visibleChildren)
                {
                    if (child.IsExpanded)
                    {
                        child.UiNode.IsExpanded = true;
                    }
                }
            }
        }

        private void Contract()
        {
            if (Children != null)
            {
                foreach (var child in Children.Keys)
                {
                    child.Contract();
                    if (child.UiNode != null)
                    {
                        child.UiNode.Dispose();
                        child.UiNode = null;
                    }
                }
            }
        }

        public bool ApplyFilter(ITraceFilter filter)
        {
            Filter = filter;
            var matched = Filter?.Apply(Trace) ?? true;
            if (Children != null)
            {
                var childrenChanged = false;
                foreach (var child in Children.Keys.ToArray())
                {
                    var childMatched = child.ApplyFilter(filter);
                    if (childMatched != Children[child])
                    {
                        childrenChanged = true;
                    }
                    Children[child] = childMatched;
                    if (childMatched)
                    {
                        matched = true;
                    }
                }
                if (childrenChanged && IsExpanded && _uiNode != null)
                {
                    Expand();
                }
            }
            if (_uiNode != null)
            {
                _uiNode.FilterMatched = matched;
            }
            return matched;
        }

    }
}
