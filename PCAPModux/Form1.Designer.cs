namespace PCAPModux
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TreeView packetTreeView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.startButton = new System.Windows.Forms.Button();
            this.packetTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(12, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // packetTreeView
            // 
            this.packetTreeView.Location = new System.Drawing.Point(12, 41);
            this.packetTreeView.Name = "packetTreeView";
            this.packetTreeView.Size = new System.Drawing.Size(776, 397);
            this.packetTreeView.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.packetTreeView);
            this.Controls.Add(this.startButton);
            this.Name = "Form1";
            this.Text = "PCAP Reader";
            this.ResumeLayout(false);
        }
    }
}