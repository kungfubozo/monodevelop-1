// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MonoDevelop.Ide.Gui.Dialogs {
    
    
    public partial class DeleteFileDialog {
        
        private Gtk.HBox hbox1;
        
        private Gtk.Image image1;
        
        private Gtk.VBox vbox3;
        
        private Gtk.Label QuestionLabel;
        
        private Gtk.CheckButton cbDeleteFromDisk;
        
        private Gtk.Button NoButton;
        
        private Gtk.Button YesButton;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize();
            // Widget MonoDevelop.Ide.Gui.Dialogs.DeleteFileDialog
            this.Events = ((Gdk.EventMask)(256));
            this.Name = "MonoDevelop.Ide.Gui.Dialogs.DeleteFileDialog";
            this.Title = Mono.Unix.Catalog.GetString("Delete File");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.BorderWidth = ((uint)(6));
            // Internal child MonoDevelop.Ide.Gui.Dialogs.DeleteFileDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Events = ((Gdk.EventMask)(256));
            w1.Name = "dialog_VBox";
            w1.Spacing = 6;
            // Container child dialog_VBox.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 12;
            this.hbox1.BorderWidth = ((uint)(6));
            // Container child hbox1.Gtk.Box+BoxChild
            this.image1 = new Gtk.Image();
            this.image1.Name = "image1";
            this.image1.Pixbuf = Stetic.IconLoader.LoadIcon("gtk-dialog-question", 48);
            this.hbox1.Add(this.image1);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox1[this.image1]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 12;
            // Container child vbox3.Gtk.Box+BoxChild
            this.QuestionLabel = new Gtk.Label();
            this.QuestionLabel.Name = "QuestionLabel";
            this.QuestionLabel.LabelProp = Mono.Unix.Catalog.GetString("Are you sure you want to delete from project ?");
            this.QuestionLabel.Wrap = true;
            this.vbox3.Add(this.QuestionLabel);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.vbox3[this.QuestionLabel]));
            w3.Position = 0;
            w3.Expand = false;
            w3.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.cbDeleteFromDisk = new Gtk.CheckButton();
            this.cbDeleteFromDisk.CanFocus = true;
            this.cbDeleteFromDisk.Name = "cbDeleteFromDisk";
            this.cbDeleteFromDisk.Label = Mono.Unix.Catalog.GetString("_Delete from disk");
            this.cbDeleteFromDisk.DrawIndicator = true;
            this.cbDeleteFromDisk.UseUnderline = true;
            this.vbox3.Add(this.cbDeleteFromDisk);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox3[this.cbDeleteFromDisk]));
            w4.Position = 1;
            w4.Expand = false;
            w4.Fill = false;
            this.hbox1.Add(this.vbox3);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.hbox1[this.vbox3]));
            w5.Position = 1;
            w5.Expand = false;
            w5.Fill = false;
            w1.Add(this.hbox1);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(w1[this.hbox1]));
            w6.Position = 0;
            w6.Expand = false;
            w6.Fill = false;
            // Internal child MonoDevelop.Ide.Gui.Dialogs.DeleteFileDialog.ActionArea
            Gtk.HButtonBox w7 = this.ActionArea;
            w7.Name = "MonoDevelop.Ide.DeleteFileDialog_ActionArea";
            w7.Spacing = 10;
            w7.BorderWidth = ((uint)(5));
            w7.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child MonoDevelop.Ide.DeleteFileDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.NoButton = new Gtk.Button();
            this.NoButton.CanDefault = true;
            this.NoButton.CanFocus = true;
            this.NoButton.Name = "NoButton";
            this.NoButton.UseStock = true;
            this.NoButton.UseUnderline = true;
            this.NoButton.Label = "gtk-no";
            this.AddActionWidget(this.NoButton, -9);
            Gtk.ButtonBox.ButtonBoxChild w8 = ((Gtk.ButtonBox.ButtonBoxChild)(w7[this.NoButton]));
            w8.Expand = false;
            w8.Fill = false;
            // Container child MonoDevelop.Ide.DeleteFileDialog_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.YesButton = new Gtk.Button();
            this.YesButton.CanDefault = true;
            this.YesButton.CanFocus = true;
            this.YesButton.Name = "YesButton";
            this.YesButton.UseStock = true;
            this.YesButton.UseUnderline = true;
            this.YesButton.Label = "gtk-yes";
            this.AddActionWidget(this.YesButton, -8);
            Gtk.ButtonBox.ButtonBoxChild w9 = ((Gtk.ButtonBox.ButtonBoxChild)(w7[this.YesButton]));
            w9.Position = 1;
            w9.Expand = false;
            w9.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 373;
            this.DefaultHeight = 161;
            this.Show();
            this.NoButton.Clicked += new System.EventHandler(this.OnNoButtonClicked);
            this.YesButton.Clicked += new System.EventHandler(this.OnYesButtonClicked);
        }
    }
}
