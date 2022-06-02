namespace RORZE
{
    partial class MainForm
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
            this.picDisp = new System.Windows.Forms.PictureBox();
            this.btnGrabImg = new System.Windows.Forms.Button();
            this.lblTime = new System.Windows.Forms.Label();
            this.btnInfinitelyLoop = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.hswDisplay = new HalconDotNet.HSmartWindowControl();
            ((System.ComponentModel.ISupportInitialize)(this.picDisp)).BeginInit();
            this.SuspendLayout();
            // 
            // picDisp
            // 
            this.picDisp.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.picDisp.Location = new System.Drawing.Point(26, 46);
            this.picDisp.Name = "picDisp";
            this.picDisp.Size = new System.Drawing.Size(365, 394);
            this.picDisp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDisp.TabIndex = 0;
            this.picDisp.TabStop = false;
            // 
            // btnGrabImg
            // 
            this.btnGrabImg.Location = new System.Drawing.Point(424, 46);
            this.btnGrabImg.Name = "btnGrabImg";
            this.btnGrabImg.Size = new System.Drawing.Size(148, 33);
            this.btnGrabImg.TabIndex = 1;
            this.btnGrabImg.Text = "Call Server Grab Image";
            this.btnGrabImg.UseVisualStyleBackColor = true;
            this.btnGrabImg.Click += new System.EventHandler(this.btnGrabImg_Click);
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(24, 19);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(68, 12);
            this.lblTime.TabIndex = 2;
            this.lblTime.Text = "Elapse Time: ";
            // 
            // btnInfinitelyLoop
            // 
            this.btnInfinitelyLoop.Location = new System.Drawing.Point(424, 101);
            this.btnInfinitelyLoop.Name = "btnInfinitelyLoop";
            this.btnInfinitelyLoop.Size = new System.Drawing.Size(148, 39);
            this.btnInfinitelyLoop.TabIndex = 3;
            this.btnInfinitelyLoop.Text = "Infinitely Grab Image";
            this.btnInfinitelyLoop.UseVisualStyleBackColor = true;
            this.btnInfinitelyLoop.Click += new System.EventHandler(this.btnInfinitelyLoop_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(451, 167);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // hswDisplay
            // 
            this.hswDisplay.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.hswDisplay.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.hswDisplay.HDoubleClickToFitContent = true;
            this.hswDisplay.HDrawingObjectsModifier = HalconDotNet.HSmartWindowControl.DrawingObjectsModifier.None;
            this.hswDisplay.HImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hswDisplay.HKeepAspectRatio = true;
            this.hswDisplay.HMoveContent = true;
            this.hswDisplay.HZoomContent = HalconDotNet.HSmartWindowControl.ZoomContent.WheelForwardZoomsIn;
            this.hswDisplay.Location = new System.Drawing.Point(608, 46);
            this.hswDisplay.Margin = new System.Windows.Forms.Padding(0);
            this.hswDisplay.Name = "hswDisplay";
            this.hswDisplay.Size = new System.Drawing.Size(381, 394);
            this.hswDisplay.TabIndex = 5;
            this.hswDisplay.WindowSize = new System.Drawing.Size(381, 394);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 464);
            this.Controls.Add(this.hswDisplay);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnInfinitelyLoop);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.btnGrabImg);
            this.Controls.Add(this.picDisp);
            this.Name = "MainForm";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.picDisp)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picDisp;
        private System.Windows.Forms.Button btnGrabImg;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Button btnInfinitelyLoop;
        private System.Windows.Forms.Button button1;
        private HalconDotNet.HSmartWindowControl hswDisplay;
    }
}

