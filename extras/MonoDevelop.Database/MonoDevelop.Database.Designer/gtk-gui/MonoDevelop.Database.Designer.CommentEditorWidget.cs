
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.Database.Designer
{
	public partial class CommentEditorWidget
	{
		private global::Gtk.ScrolledWindow scrolledwindow;

		private global::Gtk.TextView textComment;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.Database.Designer.CommentEditorWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "MonoDevelop.Database.Designer.CommentEditorWidget";
			// Container child MonoDevelop.Database.Designer.CommentEditorWidget.Gtk.Container+ContainerChild
			this.scrolledwindow = new global::Gtk.ScrolledWindow ();
			this.scrolledwindow.CanFocus = true;
			this.scrolledwindow.Name = "scrolledwindow";
			this.scrolledwindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledwindow.Gtk.Container+ContainerChild
			this.textComment = new global::Gtk.TextView ();
			this.textComment.CanFocus = true;
			this.textComment.Name = "textComment";
			this.scrolledwindow.Add (this.textComment);
			this.Add (this.scrolledwindow);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Show ();
		}
	}
}
