using Deeparteffects.Sdk;
using Deeparteffects.Sdk.Model;
using System;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace DeepArtEffectsExample
{
    public partial class Form1 : Form
    {
        static readonly string AccessKey = ConfigurationManager.AppSettings["AccessKey"];
        static readonly string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
        static readonly string ApiKey = ConfigurationManager.AppSettings["ApiKey"];

        static readonly int CheckResultInMs = 2500;

        private DeepArtEffectsClient client;
        private Timer timer;
        private OpenFileDialog openFileDialog;
        private string submissionId;

        public Form1()
        {
            InitializeComponent();

            client = new DeepArtEffectsClient(AccessKey, SecretKey, ApiKey);
            openFileDialog = new OpenFileDialog();
            timer = new Timer();
            timer.Interval = CheckResultInMs;
            timer.Tick += checkInterval_Tick;
        }

        private void checkInterval_Tick(object sender, EventArgs e)
        {
            //get response
            Result result = client.getResult(submissionId);
            if (result.Status=="finished")
            {
                timer.Stop();
                pictureBox1.ImageLocation = result.Url;
                label1.Text = "Finished";
            } else if (result.Status=="error")
            {
                timer.Stop();
                label1.Text = "Error";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {         
                // Insert code to read the stream here.
                var filePath = openFileDialog.FileName;
 
                pictureBox1.Image = Image.FromFile(filePath);

                label1.Text = "Get Styles...";

                Styles styles = client.getStyles();

                //Get first style
                Style style = styles[0];

                label1.Text = "Upload image...";

                byte[] imageArray = System.IO.File.ReadAllBytes(filePath);
                string base64Image = Convert.ToBase64String(imageArray);

                //upload image
                UploadRequest uploadRequest = new UploadRequest();
                uploadRequest.StyleId = style.Id;
                uploadRequest.ImageBase64Encoded = base64Image;

                UploadResponse response = client.uploadImage(uploadRequest);

                label1.Text = "Processing image...";

                //get submission id
                submissionId = response.SubmissionId;

                //get the result
                timer.Start();
            }
        }
    }
}
