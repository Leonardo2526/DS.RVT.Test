namespace DS.RevitLib.ExternalEvents
{
    partial class ExternalEventExampleDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.showMessageButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // showMessageButton
            // 
            this.showMessageButton.Location = new System.Drawing.Point(120, 118);
            this.showMessageButton.Name = "showMessageButton";
            this.showMessageButton.Size = new System.Drawing.Size(253, 89);
            this.showMessageButton.TabIndex = 0;
            this.showMessageButton.Text = "showMessage";
            this.showMessageButton.UseVisualStyleBackColor = true;
            this.showMessageButton.Click += new System.EventHandler(this.showMessageButton_Click);
            // 
            // ExternalEventExampleDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 363);
            this.Controls.Add(this.showMessageButton);
            this.Name = "ExternalEventExampleDialog";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button showMessageButton;
    }
}