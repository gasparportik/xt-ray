/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace XtRay.GtkSharp
{
    public class ParamsWindow : Dialog
    {
        private ListStore treeModel;

        [UI]
        public TreeView ParamsTree;

        private ParamsWindow(Builder builder) : base(builder.GetObject("ParamsWindow").Handle)
        {
            builder.Autoconnect(this);
        }

        public ParamsWindow(string[] parameters) : this(new Builder("ParamsWindow.glade"))
        {
            AddButton("gtk-close", ResponseType.Cancel);

            treeModel = new ListStore(typeof(int), typeof(string), typeof(string), typeof(string));
            ParamsTree.Model = treeModel;


            var indexColumn = new TreeViewColumn { Title = "Index" };
            ParamsTree.AppendColumn(indexColumn);

            var nameColumn = new TreeViewColumn { Title = "Name" };
            ParamsTree.AppendColumn(nameColumn);

            var typeColumn = new TreeViewColumn { Title = "Type" };
            ParamsTree.AppendColumn(typeColumn);

            var dataColumn = new TreeViewColumn { Title = "Data" };
            ParamsTree.AppendColumn(dataColumn);


            var indexCell = new CellRendererText();
            indexColumn.PackStart(indexCell, true);

            var nameCell = new CellRendererText();
            nameColumn.PackStart(nameCell, true);

            var typeCell = new CellRendererText();
            typeColumn.PackStart(typeCell, true);

            var dataCell = new CellRendererText();
            dataColumn.PackStart(dataCell, true);


            indexColumn.AddAttribute(indexCell, "text", 0);
            nameColumn.AddAttribute(nameCell, "text", 1);
            typeColumn.AddAttribute(nameCell, "text", 2);
            dataColumn.AddAttribute(dataCell, "text", 3);


            var index = 0;
            foreach (var param in parameters)
            {
                treeModel.AppendValues(index, "#" + index, "unknown", param);
                index++;
            }

            SetSizeRequest(500, System.Math.Min(500, 100 + index * 20));

        }
    }
}