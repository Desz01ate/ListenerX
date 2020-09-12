﻿using Microsoft.Win32;
using SpotifyListener.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyListener
{
    public class Wallpaper : IDisposable
    {
        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }
        string OriginalBackgroundImagePath { get; }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SystemParametersInfo(UInt32 action, UInt32 uParam, String vParam, UInt32 winIni);

        public static readonly uint SPI_SETDESKWALLPAPER = 0x14;
        public static readonly uint SPIF_UPDATEINIFILE = 0x01;
        public static readonly uint SPIF_SENDWININICHANGE = 0x02;
        private static string BAK_IMAGE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IMGBAK.bak");
        public readonly float FontSize;
        public readonly string FontFamily;
        public IMusic Player;
        private string temporaryWaitForDeleteFiles = "";
        public Wallpaper(float fontSize, string fontFamily)
        {
            OriginalBackgroundImagePath = BAK_IMAGE;
            FontSize = fontSize;
            FontFamily = fontFamily;
            Disable();
        }
        public Wallpaper(string backgroundImagePath, float fontSize, string fontFamily)
        {
            OriginalBackgroundImagePath = backgroundImagePath;
            FontSize = fontSize;
            FontFamily = fontFamily;
            try
            {
                File.Copy(backgroundImagePath, BAK_IMAGE, true);
            }
            catch
            {

            }
        }
        public static bool TryGetWallpaper(out string imagePath)
        {
            try
            {
                byte[] SliceMe(byte[] source, int pos)
                {
                    byte[] dest = new byte[source.Length - pos];
                    Array.Copy(source, pos, dest, 0, dest.Length);
                    return dest;
                };
                byte[] path = (byte[])Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop").GetValue("TranscodedImageCache");
                var wallpaper_file_path = System.Text.Encoding.Unicode.GetString(SliceMe(path, 24)).TrimEnd("\0".ToCharArray());
                imagePath = wallpaper_file_path;
                return File.Exists(wallpaper_file_path);
            }
            catch
            {
                imagePath = null;
                return false;
            }
        }
        public void Enable()
        {
            using var image = CalculateBackgroundImage();
            Set(image, Wallpaper.Style.Stretched);
        }
        public void Disable()
        {
            Set(null, Wallpaper.Style.Stretched, OriginalBackgroundImagePath);
        }
        private bool Set(Image image, Style style, string path = "")
        {
            DeleteTempFile();
            bool success = false;
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
            RegistryKey desktopKey = default, lockScreenKey = default;
            temporaryWaitForDeleteFiles = tempPath;
            try
            {
                if (image != null)
                {
                    File.Create(tempPath).Close();
                    image.Save(tempPath, ImageFormat.Bmp);
                }
                desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                lockScreenKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", true);
                if (lockScreenKey == null)
                {
                    lockScreenKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", true);
                    lockScreenKey.SetValue("LockScreenImagePath", "");
                    lockScreenKey.SetValue("LockScreenImageUrl", "");
                    lockScreenKey.SetValue("LockScreenImageStatus", 0, RegistryValueKind.DWord);
                }
                switch (style)
                {
                    case Style.Stretched:
                        desktopKey.SetValue(@"WallpaperStyle", "2");
                        desktopKey.SetValue(@"TileWallpaper", "0");
                        break;
                    case Style.Centered:
                        desktopKey.SetValue(@"WallpaperStyle", "1");
                        desktopKey.SetValue(@"TileWallpaper", "0");
                        break;
                    default:
                    case Style.Tiled:
                        desktopKey.SetValue(@"WallpaperStyle", "1");
                        desktopKey.SetValue(@"TileWallpaper", "1");
                        break;
                }
                if (!string.IsNullOrWhiteSpace(path))
                {
                    lockScreenKey?.SetValue("LockScreenImagePath", path);
                    lockScreenKey?.SetValue("LockScreenImageUrl", path);
                    lockScreenKey.SetValue("LockScreenImageStatus", 1, RegistryValueKind.DWord);
                    Task.Run(() => SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE));
                }
                else
                {
                    lockScreenKey?.SetValue("LockScreenImagePath", tempPath);
                    lockScreenKey?.SetValue("LockScreenImageUrl", tempPath);
                    lockScreenKey.SetValue("LockScreenImageStatus", 1, RegistryValueKind.DWord);
                    Task.Run(() => SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE));
                }
                success = true;
            }
            catch (Exception ex)
            {
                //ex.HandleException();
            }
            finally
            {
                desktopKey?.Close();
                lockScreenKey?.Close();
                image?.Dispose();

            }
            return success;
        }
        internal void SetPlayerBase(IMusic music)
        {
            Player = music;
        }
        private Image CalculateBackgroundImage(Image highlightImg, Image backgroundImg, string track, string album, string artist)
        {
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            backgroundImg = backgroundImg.Resize((int)screenWidth, (int)screenHeight);
            using var g = Graphics.FromImage(backgroundImg);

            var highlightX = (int)((screenWidth - highlightImg.Width) / 2);
            var highlightY = (int)((screenHeight - highlightImg.Height) / 2) - (int)(screenHeight * 0.12);
            g.DrawImage(highlightImg, highlightX, highlightY);

            using var font = new Font(FontFamily, FontSize, FontStyle.Regular);
            using var trackFont = new Font(FontFamily, FontSize * 1.3f, FontStyle.Bold);

            var trackMeasure = g.MeasureString(track, trackFont);
            var albumMeasure = g.MeasureString(album, font);
            var artistMeasure = g.MeasureString(artist, font);

            g.DrawString(track, trackFont, Brushes.White, (int)((screenWidth - trackMeasure.Width) / 2), highlightY + highlightImg.Height + (int)(screenHeight * 0.07));
            g.DrawString(album, font, Brushes.White, (int)((screenWidth - albumMeasure.Width) / 2), highlightY + highlightImg.Height + (int)(screenHeight * 0.15));
            g.DrawString(artist, font, Brushes.White, (int)((screenWidth - artistMeasure.Width) / 2), highlightY + highlightImg.Height + (int)(screenHeight * 0.2));

            return backgroundImg;
        }
        private Image CalculateBackgroundImage()
        {
            var highlightSize = (int)Math.Round(System.Windows.SystemParameters.PrimaryScreenHeight * 0.555);

            var artwork = Player.AlbumArtwork;
            using var background = Effects.BitmapHelper.CalculateBackgroundSource(artwork);
            using var highlight = artwork.Resize(highlightSize, highlightSize);
            var image = CalculateBackgroundImage(
                highlight,
                background,
                Player.Track, Player.Album, Player.Artist);

            return image;
        }
        public void SaveWallpaperToFile(string filePath)
        {
            using (var image = CalculateBackgroundImage())
            {
                image.Save(filePath);
            }
        }
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Disable();
                DeleteTempFile();
            }
        }
        private void DeleteTempFile()
        {
            if (File.Exists(temporaryWaitForDeleteFiles))
            {
                File.Delete(temporaryWaitForDeleteFiles);
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
