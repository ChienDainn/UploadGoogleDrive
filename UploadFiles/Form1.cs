using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadFiles
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		// luu ten cac files da chon
		ArrayList listFiles = new ArrayList();
    ArrayList listFiles2 = new ArrayList();

    private void selectFiles_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				listFiles.Add(openFileDialog1.FileName);
        listFiles2.Add(openFileDialog1.FileName);
        filesSelected.Text += "- " + openFileDialog1.FileName + Environment.NewLine;
			}
		}

    private void uploadFile(string path, DriveService service, string folderUpload)
    {
      var fileMetadata = new Google.Apis.Drive.v3.Data.File();
      fileMetadata.Name = Path.GetFileName(path);
      fileMetadata.MimeType = "application/vnd.google-apps.file"; // application/vnd.google-apps.file

      fileMetadata.Parents = new List<string> { folderUpload };

      FilesResource.CreateMediaUpload request;
      using (var stream = new FileStream(path, FileMode.Open))
      {
        request = service.Files.Create(fileMetadata, stream, "*/*");
        request.Fields = "id";
        request.Upload();
      }
    }

    [Obsolete]
		private void upload_Click(object sender, EventArgs e)
		{
      // xac dinh pham vi va cap quyen truy cap
      string[] Scopes = { DriveService.Scope.Drive };
      string ApplicationName = "Upload Files";
      UserCredential credential;

      using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
      {
        string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        credPath = Path.Combine(credPath, "credentials.json");

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.Load(stream).Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(credPath, true)).Result;
      }

      // Create Drive API service.
      var service = new DriveService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential,
        ApplicationName = ApplicationName,
      });

      // khai bao mot doi tuong folder nho thuoc tin MimeType
      var fileMetadatas = new Google.Apis.Drive.v3.Data.File()
      {
        Name = folderName.Text,
        MimeType = "application/vnd.google-apps.folder"
      };

      // tao folder tren gg drive bang phuong thuc create
      var requests = service.Files.Create(fileMetadatas);
      requests.Fields = "id";

      // nhan id cua folder da tao do gg drive cung cap
      var file = requests.Execute();
      var folderId = file.Id;

      // upload
      while(listFiles.Count > 0)
      {
        uploadFile(listFiles[0].ToString(), service, folderId);
        listFiles.Remove(listFiles[0]);
      }

      if (listFiles.Count == 0)
      {
        folderName.Text = string.Empty;
        filesSelected.Text = string.Empty;
        MessageBox.Show("File đã được upload toàn bộ", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.Show("File chưa được upload toàn bộ", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}
