namespace PCAPModux
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button showMetricsButton;
        private System.Windows.Forms.TreeView packetTreeView;
        private System.Windows.Forms.ComboBox packetTypeComboBox;

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
            this.showMetricsButton = new System.Windows.Forms.Button();
            this.packetTreeView = new System.Windows.Forms.TreeView();
            this.packetTypeComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(12, 12);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Open File";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // showMetricsButton
            // 
            this.showMetricsButton.Location = new System.Drawing.Point(93, 12);
            this.showMetricsButton.Name = "showMetricsButton";
            this.showMetricsButton.Size = new System.Drawing.Size(100, 23);
            this.showMetricsButton.TabIndex = 1;
            this.showMetricsButton.Text = "Show Metrics";
            this.showMetricsButton.UseVisualStyleBackColor = true;
            this.showMetricsButton.Click += new System.EventHandler(this.showMetricsButton_Click);
            // 
            // packetTreeView
            // 
            this.packetTreeView.Location = new System.Drawing.Point(12, 70);
            this.packetTreeView.Name = "packetTreeView";
            this.packetTreeView.Size = new System.Drawing.Size(776, 368);
            this.packetTreeView.TabIndex = 2;
            // 
            // packetTypeComboBox
            // 
            this.packetTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.packetTypeComboBox.FormattingEnabled = true;
            this.packetTypeComboBox.Items.AddRange(new object[] {
            "ARP",
            "DNS"});
            this.packetTypeComboBox.Location = new System.Drawing.Point(199, 12);
            this.packetTypeComboBox.Name = "packetTypeComboBox";
            this.packetTypeComboBox.Size = new System.Drawing.Size(121, 24);
            this.packetTypeComboBox.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.packetTypeComboBox);
            this.Controls.Add(this.packetTreeView);
            this.Controls.Add(this.showMetricsButton);
            this.Controls.Add(this.startButton);
            this.Name = "Form1";
            this.Text = "PCAP Reader";
            this.ResumeLayout(false);
        }
    }
}
