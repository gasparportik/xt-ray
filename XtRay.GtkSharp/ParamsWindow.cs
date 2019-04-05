/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace XtRay.GtkSharp
{
    public class ParamsWindow : GladyWindow
    {
        private Gtk.ListStore treeModel;

        [Glade.Widget]
        public Gtk.TreeView ParamsTree;

        public ParamsWindow(string[] parameters) : base(Gtk.WindowType.Toplevel, "ParamsWindow")
        {
            ((Gtk.Dialog)Window).AddButton("gtk-close", Gtk.ResponseType.Cancel);


            treeModel = new Gtk.ListStore(typeof(int), typeof(string), typeof(string), typeof(string));
            ParamsTree.Model = treeModel;


            var indexColumn = new Gtk.TreeViewColumn { Title = "Index" };
            ParamsTree.AppendColumn(indexColumn);

            var nameColumn = new Gtk.TreeViewColumn { Title = "Name" };
            ParamsTree.AppendColumn(nameColumn);

            var typeColumn = new Gtk.TreeViewColumn { Title = "Type" };
            ParamsTree.AppendColumn(typeColumn);

            var dataColumn = new Gtk.TreeViewColumn { Title = "Data" };
            ParamsTree.AppendColumn(dataColumn);


            var indexCell = new Gtk.CellRendererText();
            indexColumn.PackStart(indexCell, true);

            var nameCell = new Gtk.CellRendererText();
            nameColumn.PackStart(nameCell, true);

            var typeCell = new Gtk.CellRendererText();
            typeColumn.PackStart(typeCell, true);

            var dataCell = new Gtk.CellRendererText();
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

            Window.SetSizeRequest(500, System.Math.Min(500, 100 + index * 20));

        }

        internal int Run()
        {
            return ((Gtk.Dialog)Window).Run();
        }
    }
}