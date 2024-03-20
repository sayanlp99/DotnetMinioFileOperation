using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Shapes;
using System.IO.Pipes;

namespace WpfApp3
{
    public partial class MainWindow : Window
    {
        private readonly MinioClient minio;
        private const string Endpoint = "192.168.3.120:9000"; // e.g., "play.min.io"
        private const string AccessKey = "ld1BdPBOvqxULvuoPRPE";
        private const string SecretKey = "AUgaMaVwTMqGKNdC3bJ74BOl8eUVCh2tyJ5j5b6s";
        private const string BucketName = "firmware";

        public MainWindow()
        {
            InitializeComponent();

            minio = (MinioClient)new MinioClient()
                .WithEndpoint(Endpoint)
                .WithCredentials(AccessKey, SecretKey)
                .Build();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "firmware",
                DefaultExt = ".bin"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var filePath = saveFileDialog.FileName;
                var fileName = System.IO.Path.GetFileName(filePath);

                try
                {
                    StatObjectArgs statObjectArgs = new StatObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(fileName);
                    await minio.StatObjectAsync(statObjectArgs);

                    using (var fileStream = File.Create(filePath))
                    {
                        GetObjectArgs getObjectArgs = new GetObjectArgs()
                            .WithBucket(BucketName)
                            .WithObject(fileName)
                            .WithCallbackStream((stream) =>
                            {
                                stream.CopyTo(fileStream);
                            });
                        await minio.GetObjectAsync(getObjectArgs);
                    }

                    MessageBox.Show("File downloaded successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading file: {ex.Message}");
                }
            }
        }
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var fileExtension = System.IO.Path.GetExtension(filePath);

                // Generate a unique filename with datetime
                var newObjectName = $"{fileName}{DateTime.Now:yyyyMMddHHmmss}.{fileExtension}";

                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        PutObjectArgs putObjectArgs = new PutObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(newObjectName)
                        .WithStreamData(fileStream)
                        .WithObjectSize(fileStream.Length)
                        .WithContentType("application/octet-stream");
                        await minio.PutObjectAsync(putObjectArgs);

                        StatObjectArgs statObjectArgs = new StatObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(newObjectName);
                        await minio.StatObjectAsync(statObjectArgs);

                        MessageBox.Show("File uploaded successfully!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error uploading file: {ex.Message}");
                }
            }
        }

    }
}
