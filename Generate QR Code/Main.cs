using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace Generate_QR_Code
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

        }
        Bitmap bitmapLogo;
        Bitmap QrCode;
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbMessage.Text))
            {
                int height = int.Parse(string.IsNullOrEmpty(tbHeight.Text)?"0": tbHeight.Text);
                int width = int.Parse(string.IsNullOrEmpty(tbWidth.Text) ? "0" : tbWidth.Text);
                if (height <= 20 || width <= 20)
                {
                    // Set default size
                    height = 300;
                    width = 300;
                }

                QrCode = GenerateQRCode(tbMessage.Text, bitmapLogo, height, width);
                bool a = IsDecodeQR(QrCode);
                //Console.WriteLine("A :"+a);
                if (a)
                {
                    pictureBox1.Image = QrCode;
                }
                else
                {
                    MessageBox.Show("QR Code is not generated");
                    pictureBox1.Image = null;
                }
                
            }
            else
            {
                MessageBox.Show("Please enter text to generate QR Code");
            }
        }
        private Bitmap GenerateQRCode(string text, Bitmap Logo = null, int height = 300, int width = 300, int margin = 1)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter();
            EncodingOptions options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = width,
                Height = height,
                Margin = margin,
                PureBarcode = false
            };
            options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
            barcodeWriter.Renderer = new BitmapRenderer();
            barcodeWriter.Options = options;
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            Bitmap bitmap = barcodeWriter.Write(text);
            if (Logo != null)
            {                
                Bitmap logo = Logo;
                Graphics g = Graphics.FromImage(bitmap);
                g.DrawImage(logo, new Point((bitmap.Width - logo.Width) / 2, (bitmap.Height - logo.Height) / 2));
            }

            return bitmap;
        }

        private bool IsDecodeQR(Bitmap bitmap)
        {
            try
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                Result result = barcodeReader.Decode(bitmap);
                if (result != null)
                {
                    if (result.Text != tbMessage.Text)
                    {
                        throw new Exception("Decode for inocomplete information!");
                    }                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
        }

        private void openLogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                // Filter icon
                fileDialog.Filter = "Image Files(*.ICO; *.JPG; *.PNG; *.BMP;)|*.ICO; *.JPG; *.PNG; *.BMP;";
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = fileDialog.FileName;
                    // if file is icon
                    if (filePath.EndsWith(".ico"))
                    {
                        Icon icon = new Icon(filePath);
                        bitmapLogo = icon.ToBitmap();
                    }
                    else
                    {
                        bitmapLogo = new Bitmap(filePath);
                    }

                    int height = int.Parse(string.IsNullOrEmpty(tbHeight.Text) ? "0" : tbHeight.Text);
                    int width = int.Parse(string.IsNullOrEmpty(tbWidth.Text) ? "0" : tbWidth.Text);
                    if (height <= 20 || width <= 20)
                    {
                        // Set default size
                        height = 300;
                        width = 300;
                    }
                    
                    if (bitmapLogo.Height > (height/4) || bitmapLogo.Width > (width/4))
                    {
                        // Resize
                        bitmapLogo = (Bitmap)resizeImage(bitmapLogo, new Size((height / 4), (width / 4)));
                    }
                    pictureBoxLogo.Image = bitmapLogo;
                    pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            

        }

        private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }
        

        private void tb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }            
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save file
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Images|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;                    
                }
                QrCode.Save(sfd.FileName, format);
            }
        }

        private void removeLogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bitmapLogo = null;
            pictureBoxLogo.Image = null;
            btnGenerate.PerformClick();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
