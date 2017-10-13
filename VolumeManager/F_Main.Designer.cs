namespace VolumeManager
{
    partial class F_Main
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_VolUp != null)
                    _VolUp.Dispose();

                if (_VolDown != null)
                    _VolDown.Dispose();

                if (_Mute != null)
                    _Mute.Dispose();

                if (_UdpClient != null)
                    _UdpClient.Close();

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F_Main));
            this.TB_Volume = new System.Windows.Forms.TrackBar();
            this.CMS_Main = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MI_Options = new System.Windows.Forms.ToolStripMenuItem();
            this.MI_Spacer1 = new System.Windows.Forms.ToolStripSeparator();
            this.MI_Placeholder = new System.Windows.Forms.ToolStripMenuItem();
            this.MI_Spacer2 = new System.Windows.Forms.ToolStripSeparator();
            this.MI_Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.UC_AI_Right = new UC_AnalogInstrument();
            this.UC_AI_Left = new UC_AnalogInstrument();
            this.L_ActiveDevice = new System.Windows.Forms.Label();
            this.L_ActiveSession = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.TB_Volume)).BeginInit();
            this.CMS_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // TB_Volume
            // 
            this.TB_Volume.BackColor = System.Drawing.Color.Black;
            this.TB_Volume.ContextMenuStrip = this.CMS_Main;
            this.TB_Volume.Location = new System.Drawing.Point(154, 15);
            this.TB_Volume.Maximum = 100;
            this.TB_Volume.Name = "TB_Volume";
            this.TB_Volume.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.TB_Volume.Size = new System.Drawing.Size(45, 152);
            this.TB_Volume.TabIndex = 2;
            this.TB_Volume.TabStop = false;
            this.TB_Volume.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.TB_Volume.ValueChanged += new System.EventHandler(this.TB_Volume_ValueChanged);
            // 
            // CMS_Main
            // 
            this.CMS_Main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MI_Options,
            this.MI_Spacer1,
            this.MI_Placeholder,
            this.MI_Spacer2,
            this.MI_Exit});
            this.CMS_Main.Name = "CMS_Main";
            this.CMS_Main.Size = new System.Drawing.Size(117, 82);
            this.CMS_Main.Opening += new System.ComponentModel.CancelEventHandler(this.CMS_Main_Opening);
            // 
            // MI_Options
            // 
            this.MI_Options.Name = "MI_Options";
            this.MI_Options.Size = new System.Drawing.Size(116, 22);
            this.MI_Options.Text = "Options";
            this.MI_Options.Click += new System.EventHandler(this.MI_Options_Click);
            // 
            // MI_Spacer1
            // 
            this.MI_Spacer1.Name = "MI_Spacer1";
            this.MI_Spacer1.Size = new System.Drawing.Size(113, 6);
            // 
            // MI_Placeholder
            // 
            this.MI_Placeholder.Name = "MI_Placeholder";
            this.MI_Placeholder.Size = new System.Drawing.Size(116, 22);
            this.MI_Placeholder.Text = "#-#";
            this.MI_Placeholder.Visible = false;
            // 
            // MI_Spacer2
            // 
            this.MI_Spacer2.Name = "MI_Spacer2";
            this.MI_Spacer2.Size = new System.Drawing.Size(113, 6);
            // 
            // MI_Exit
            // 
            this.MI_Exit.Name = "MI_Exit";
            this.MI_Exit.Size = new System.Drawing.Size(116, 22);
            this.MI_Exit.Text = "Exit";
            this.MI_Exit.Click += new System.EventHandler(this.MI_Exit_Click);
            // 
            // UC_AI_Right
            // 
            this.UC_AI_Right.AngelOffset = 90;
            this.UC_AI_Right.AngelStart = 180;
            this.UC_AI_Right.AngelStop = 270;
            this.UC_AI_Right.BackColor = System.Drawing.Color.Transparent;
            this.UC_AI_Right.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.UC_AI_Right.BorderIndicatorColor = System.Drawing.Color.Peru;
            this.UC_AI_Right.BorderWidth = 3;
            this.UC_AI_Right.ContextMenuStrip = this.CMS_Main;
            this.UC_AI_Right.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UC_AI_Right.Location = new System.Drawing.Point(197, 15);
            this.UC_AI_Right.Name = "UC_AI_Right";
            this.UC_AI_Right.Padding = new System.Windows.Forms.Padding(5);
            this.UC_AI_Right.PointerBaseColor = System.Drawing.Color.DimGray;
            this.UC_AI_Right.PointerColor = System.Drawing.Color.Crimson;
            this.UC_AI_Right.ScaleMainColor = System.Drawing.Color.LightSteelBlue;
            this.UC_AI_Right.ScaleMainLineLenght = 20;
            this.UC_AI_Right.ScaleMainLineWidth = 3;
            this.UC_AI_Right.ScaleMainStep = 20F;
            this.UC_AI_Right.ScaleMainTextColor = System.Drawing.Color.LightSteelBlue;
            this.UC_AI_Right.ScaleMainTextFont = new System.Drawing.Font("Mistral", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UC_AI_Right.ScaleMax = 100F;
            this.UC_AI_Right.ScaleMin = 0F;
            this.UC_AI_Right.ScaleReverse = true;
            this.UC_AI_Right.ScaleSecundaryColor = System.Drawing.Color.SteelBlue;
            this.UC_AI_Right.ScaleSecundaryLineLenght = 10;
            this.UC_AI_Right.ScaleSecundaryStep = 2F;
            this.UC_AI_Right.Size = new System.Drawing.Size(152, 152);
            this.UC_AI_Right.SmotherMaxUpdateValue = 1F;
            this.UC_AI_Right.SmotherUpdateInterval = 10;
            this.UC_AI_Right.StatisticHistoryCount = 20;
            this.UC_AI_Right.StatisticMaxColor = System.Drawing.Color.DarkRed;
            this.UC_AI_Right.StatisticMaxVisible = true;
            this.UC_AI_Right.StatisticMinColor = System.Drawing.Color.OliveDrab;
            this.UC_AI_Right.StatisticMinVisible = true;
            this.UC_AI_Right.TabIndex = 12;
            this.UC_AI_Right.TabStop = false;
            this.UC_AI_Right.Value = 0F;
            this.UC_AI_Right.Value2 = 0F;
            // 
            // UC_AI_Left
            // 
            this.UC_AI_Left.AngelStart = 180;
            this.UC_AI_Left.AngelStop = 270;
            this.UC_AI_Left.BackColor = System.Drawing.Color.Transparent;
            this.UC_AI_Left.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.UC_AI_Left.BorderIndicatorColor = System.Drawing.Color.Peru;
            this.UC_AI_Left.BorderWidth = 3;
            this.UC_AI_Left.ContextMenuStrip = this.CMS_Main;
            this.UC_AI_Left.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UC_AI_Left.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.UC_AI_Left.Location = new System.Drawing.Point(0, 15);
            this.UC_AI_Left.Name = "UC_AI_Left";
            this.UC_AI_Left.Padding = new System.Windows.Forms.Padding(5);
            this.UC_AI_Left.PointerBaseColor = System.Drawing.Color.DimGray;
            this.UC_AI_Left.PointerColor = System.Drawing.Color.Crimson;
            this.UC_AI_Left.ScaleMainColor = System.Drawing.Color.LightSteelBlue;
            this.UC_AI_Left.ScaleMainLineLenght = 20;
            this.UC_AI_Left.ScaleMainLineWidth = 3;
            this.UC_AI_Left.ScaleMainStep = 20F;
            this.UC_AI_Left.ScaleMainTextColor = System.Drawing.Color.LightSteelBlue;
            this.UC_AI_Left.ScaleMainTextFont = new System.Drawing.Font("Mistral", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UC_AI_Left.ScaleMax = 100F;
            this.UC_AI_Left.ScaleMin = 0F;
            this.UC_AI_Left.ScaleSecundaryColor = System.Drawing.Color.SteelBlue;
            this.UC_AI_Left.ScaleSecundaryLineLenght = 10;
            this.UC_AI_Left.ScaleSecundaryStep = 2F;
            this.UC_AI_Left.Size = new System.Drawing.Size(152, 152);
            this.UC_AI_Left.SmotherMaxUpdateValue = 1F;
            this.UC_AI_Left.SmotherUpdateInterval = 10;
            this.UC_AI_Left.StatisticHistoryCount = 20;
            this.UC_AI_Left.StatisticMaxColor = System.Drawing.Color.DarkRed;
            this.UC_AI_Left.StatisticMaxVisible = true;
            this.UC_AI_Left.StatisticMinColor = System.Drawing.Color.OliveDrab;
            this.UC_AI_Left.StatisticMinVisible = true;
            this.UC_AI_Left.TabIndex = 11;
            this.UC_AI_Left.TabStop = false;
            this.UC_AI_Left.Value = 0F;
            this.UC_AI_Left.Value2 = 0F;
            // 
            // L_ActiveDevice
            // 
            this.L_ActiveDevice.ForeColor = System.Drawing.Color.GreenYellow;
            this.L_ActiveDevice.Location = new System.Drawing.Point(12, 2);
            this.L_ActiveDevice.Name = "L_ActiveDevice";
            this.L_ActiveDevice.Size = new System.Drawing.Size(327, 15);
            this.L_ActiveDevice.TabIndex = 13;
            this.L_ActiveDevice.Text = "-.-";
            this.L_ActiveDevice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.L_ActiveDevice.DoubleClick += new System.EventHandler(this.L_ActiveDevice_DoubleClick);
            // 
            // L_ActiveSession
            // 
            this.L_ActiveSession.ForeColor = System.Drawing.Color.GreenYellow;
            this.L_ActiveSession.Location = new System.Drawing.Point(12, 173);
            this.L_ActiveSession.Name = "L_ActiveSession";
            this.L_ActiveSession.Size = new System.Drawing.Size(327, 15);
            this.L_ActiveSession.TabIndex = 14;
            this.L_ActiveSession.Text = "-.-";
            this.L_ActiveSession.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.L_ActiveSession.DoubleClick += new System.EventHandler(this.L_ActiveSession_DoubleClick);
            // 
            // F_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(357, 192);
            this.ContextMenuStrip = this.CMS_Main;
            this.Controls.Add(this.L_ActiveDevice);
            this.Controls.Add(this.L_ActiveSession);
            this.Controls.Add(this.UC_AI_Left);
            this.Controls.Add(this.UC_AI_Right);
            this.Controls.Add(this.TB_Volume);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::VolumeManager.Properties.Settings.Default, "MainWindowPosition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ExcludeList = new string[] {
        "TB_Volume"};
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Location = global::VolumeManager.Properties.Settings.Default.MainWindowPosition;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(357, 200);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(357, 176);
            this.Name = "F_Main";
            this.Text = "Volume Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_Main_FormClosing);
            this.Shown += new System.EventHandler(this.F_Main_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.TB_Volume)).EndInit();
            this.CMS_Main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar TB_Volume;
        private UC_AnalogInstrument UC_AI_Left;
        private UC_AnalogInstrument UC_AI_Right;
        private System.Windows.Forms.ContextMenuStrip CMS_Main;
        private System.Windows.Forms.ToolStripMenuItem MI_Options;
        private System.Windows.Forms.ToolStripMenuItem MI_Placeholder;
        private System.Windows.Forms.ToolStripMenuItem MI_Exit;
        private System.Windows.Forms.ToolStripSeparator MI_Spacer1;
        private System.Windows.Forms.ToolStripSeparator MI_Spacer2;
        private System.Windows.Forms.Label L_ActiveDevice;
        private System.Windows.Forms.Label L_ActiveSession;

    }
}


