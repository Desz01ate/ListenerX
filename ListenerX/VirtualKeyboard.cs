﻿using Listener.ImageProcessing;
using ListenerX.ChromaExtension;
using ListenerX.Extensions;
using ListenerX.Components;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Listener.Plugin.ChromaEffect.Enums;

namespace ListenerX
{
    public partial class VirtualKeyboard : Form
    {
        private readonly VirtualKeyboardComponent virtualKeyboard;
        private readonly ToolTip tt = new ToolTip();
        public VirtualKeyboard(VirtualKeyboardComponent component)
        {
            InitializeComponent();
            this.Width = 1520;
            this.Height = 440;
            this.pictureBox1.Width = this.Width;
            this.pictureBox1.Height = this.Height;
            this.pictureBox1.MouseMove += PictureBox1_MouseMove;
            this.virtualKeyboard = component;
            this.virtualKeyboard.OnImageChanged += Timer_Tick;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int x = 1520 * e.X / 1520;
            int y = 450 * e.Y / 450;
            var posX = (x / 50) - 1;
            var posY = (y / 50) - 1;
            if ((0 <= posX && posX < ChromaWorker.Instance.FullGridArray.ColumnCount) && (0 <= posY && posY < ChromaWorker.Instance.FullGridArray.RowCount))
            {
                var key = ChromaWorker.Instance.FullGridArray.Single(x => x.Index == (posX, posY));
                if (key.Type != KeyType.Invalid)
                {
                    tt.SetToolTip(pictureBox1, $"({key.Index.X},{key.Index.Y}) {key.FriendlyName}\nType = {key.Type}\n{key.Color.ToHex()}");
                    return;
                }
            }
            tt.RemoveAll();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = virtualKeyboard.Image;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.virtualKeyboard.OnImageChanged -= Timer_Tick;
            this.tt.Dispose();
            base.Dispose();
            base.OnClosing(e);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
