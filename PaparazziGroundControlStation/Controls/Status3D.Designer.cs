namespace PaparazziGroundControlStation.Controls
{
    partial class Status3D
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.render_timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // render_timer
            // 
            this.render_timer.Interval = 200;
            this.render_timer.Tick += new System.EventHandler(this.render_timer_Tick);
            // 
            // Status3D
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.DoubleBuffered = true;
            this.Name = "Status3D";
            this.Size = new System.Drawing.Size(204, 197);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer render_timer;
    }
}
