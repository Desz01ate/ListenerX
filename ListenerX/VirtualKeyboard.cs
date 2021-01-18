﻿using Listener.ImageProcessing;
using ListenerX.Classes;
using ListenerX.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListenerX
{
    public partial class VirtualKeyboard : Form
    {
        private readonly VirtualKeyboardComponent virtualKeyboard;
        private bool SampleInit = false;
        public VirtualKeyboard()
        {
            InitializeComponent();

            var sample = new { Width = 1780, Height = 450 };
            this.Width = sample.Width;
            this.Height = sample.Height;
            this.pictureBox1.Width = this.Width;
            this.pictureBox1.Height = this.Height;
            this.virtualKeyboard = new VirtualKeyboardComponent(50);
            this.virtualKeyboard.OnImageChanged += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = virtualKeyboard.Image;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            virtualKeyboard?.Dispose();
            base.OnClosing(e);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
