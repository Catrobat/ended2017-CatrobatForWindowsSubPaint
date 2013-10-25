﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Catrobat.IDE.Core;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.Services.Storage;
using Catrobat.IDE.Core.UI.PortableUI;
using ToolStackPNGWriterLib;

namespace Catrobat.IDE.Phone.Services.Storage
{
    public class StoragePhone : IStorage
    {
        private static int _imageThumbnailDefaultMaxWidthHeight = 200;
        private readonly IsolatedStorageFile _iso = IsolatedStorageFile.GetUserStoreForApplication();
        private readonly List<Stream> _openedStreams = new List<Stream>();

        #region Synchron

        public bool FileExists(string path)
        {
            return _iso.FileExists(path);
        }

        public void CreateDirectory(string path)
        {
            _iso.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            return _iso.DirectoryExists(path);
        }

        public string[] GetDirectoryNames(string path)
        {
            return _iso.GetDirectoryNames(path + "/*");
        }

        public string[] GetFileNames(string path)
        {
            return _iso.GetFileNames(path + "/*.*");
        }

        public void DeleteDirectory(string path)
        {
            if (DirectoryExists(path))
            {
                DeleteDirectory(path, _iso);
            }
        }

        public void DeleteFile(string path)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                iso.DeleteFile(path);
            }
        }

        public void CopyDirectory(string sourcePath, string destinationPath)
        {
            if (DirectoryExists(destinationPath))
            {
                throw new Exception(string.Format("Destination directory {0} already exists.", destinationPath));
            }

            CreateFoldersIfNotExist(destinationPath, false);
            CopyDirectory(sourcePath, destinationPath, _iso);
        }

        public void MoveDirectory(string sourcePath, string destinationPath)
        {
            if (DirectoryExists(destinationPath))
            {
                throw new Exception(string.Format("Destination directory {0} already exists.", destinationPath));
            }

            CreateFoldersIfNotExist(destinationPath, false);
            MoveDirectory(sourcePath, destinationPath, _iso);
        }

        public void CopyFile(string sourcePath, string destinationPath)
        {
            CreateFoldersIfNotExist(destinationPath, true);
            _iso.CopyFile(sourcePath, destinationPath);
        }

        public void MoveFile(string sourcePath, string destinationPath)
        {
            CreateFoldersIfNotExist(destinationPath, true);
            _iso.MoveFile(sourcePath, destinationPath);
        }

        public Stream OpenFile(string path, StorageFileMode mode, StorageFileAccess access)
        {
            var fileMode = FileMode.Append;
            var fileAccess = FileAccess.Read;

            switch (mode)
            {
                case StorageFileMode.Append:
                    fileMode = FileMode.Append;
                    break;

                case StorageFileMode.Create:
                    fileMode = FileMode.Create;
                    break;

                case StorageFileMode.CreateNew:
                    fileMode = FileMode.CreateNew;
                    break;

                case StorageFileMode.Open:
                    fileMode = FileMode.Open;
                    break;

                case StorageFileMode.OpenOrCreate:
                    fileMode = FileMode.OpenOrCreate;
                    break;

                case StorageFileMode.Truncate:
                    fileMode = FileMode.Truncate;
                    break;
            }

            switch (access)
            {
                case StorageFileAccess.Read:
                    fileAccess = FileAccess.Read;
                    break;

                case StorageFileAccess.ReadWrite:
                    fileAccess = FileAccess.ReadWrite;
                    break;

                case StorageFileAccess.Write:
                    fileAccess = FileAccess.Write;
                    break;
            }

            if (access == StorageFileAccess.Write || access == StorageFileAccess.ReadWrite)
            {
                switch (mode)
                {
                    case StorageFileMode.Create:
                    case StorageFileMode.CreateNew:
                    case StorageFileMode.OpenOrCreate:
                        CreateFoldersIfNotExist(path, true);
                        break;
                }
            }

            var storageFileStream = _iso.OpenFile(path, fileMode, fileAccess);
            _openedStreams.Add(storageFileStream);

            return storageFileStream;
        }

        public void RenameDirectory(string directoryPath, string newDirectoryName)
        {
            var newDirectoryPath = directoryPath.Remove(directoryPath.LastIndexOf('/'));
            newDirectoryPath = Path.Combine(newDirectoryPath, newDirectoryName);

            _iso.CreateDirectory(newDirectoryPath);

            var folders = _iso.GetDirectoryNames(directoryPath + "/*");

            foreach (string folder in folders)
            {
                var tempFrom = Path.Combine(directoryPath, folder);
                var tempTo = Path.Combine(newDirectoryPath, folder);
                _iso.MoveDirectory(tempFrom, tempTo);
            }

            foreach (string file in _iso.GetFileNames(directoryPath + "/*"))
            {
                var source = Path.Combine(directoryPath, file);

                if (_iso.FileExists(source))
                {
                    var destination = Path.Combine(newDirectoryPath, file);
                    _iso.MoveFile(source, destination);
                }
            }

            _iso.DeleteDirectory(directoryPath);
        }


        public void WriteTextFile(string path, string content)
        {
            var writer = new StreamWriter(OpenFile(path, StorageFileMode.Create, StorageFileAccess.Write), Encoding.UTF8);
            writer.Write(content);
            writer.Close();
            writer.Dispose();
        }

        public object ReadSerializableObject(string path, Type type)
        {
            using (var fileStream = OpenFile(path, StorageFileMode.Open, StorageFileAccess.Read))
            {
                var serializer = new DataContractSerializer(type);
                var serializeableObject = serializer.ReadObject(fileStream); // TODO: does not work any more
                fileStream.Close();
                fileStream.Dispose();
                return serializeableObject;
            }
        }

        public void WriteSerializableObject(string path, object serializableObject)
        {
            using (var fileStream = OpenFile(path, StorageFileMode.Create, StorageFileAccess.Write))
            {
                var serializer = new DataContractSerializer(serializableObject.GetType());
                serializer.WriteObject(fileStream, serializableObject);
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        public string ReadTextFile(string path)
        {
            var fileStream = OpenFile(path, StorageFileMode.Open, StorageFileAccess.Read);
            var reader = new StreamReader(fileStream);
            var text = reader.ReadToEnd();
            fileStream.Close();
            fileStream.Dispose();
            return text;
        }

        public PortableImage LoadImage(string pathToImage)
        {
            pathToImage = pathToImage.Replace("\\", "/");

            if (FileExists(pathToImage))
            {
                try
                {
                    //var bitmapImage = new BitmapImage();

                    using (var storageFileStream = _iso.OpenFile(pathToImage,
                                                                 FileMode.Open,
                                                                 FileAccess.Read))
                    {
                        var image = new PortableImage();
                        var memoryStream = new MemoryStream();
                        storageFileStream.CopyTo(memoryStream);
                        image.EncodedData = memoryStream;
                        return image;

                        //bitmapImage.SetSource(storageFileStream);
                        //storageFileStream.Close();
                        //storageFileStream.Dispose();


                        //var writeableBitmap = new WriteableBitmap(bitmapImage);
                        //var portableImage = new PortableImage(writeableBitmap.ToByteArray(), writeableBitmap.PixelWidth,
                        //    writeableBitmap.PixelHeight);
                        //return portableImage;
                    }
                }
                catch {} //TODO: Exception message. Maybe logging?
            }

            return null;
        }

        public void DeleteImage(string pathToImage)
        {
            DeleteFile(pathToImage);
            DeleteFile(pathToImage + CatrobatContextBase.ImageThumbnailExtension);
        }

        public PortableImage LoadImageThumbnail(string pathToImage)
        {
            pathToImage = pathToImage.Replace("\\", "/");

            PortableImage retVal = null;
            var withoutExtension = Path.GetFileNameWithoutExtension(pathToImage);
            var imageBasePath = Path.GetDirectoryName(pathToImage);

            if (imageBasePath != null)
            {
                var thumbnailPath = Path.Combine(imageBasePath, string.Format("{0}{1}", 
                    withoutExtension, CatrobatContextBase.ImageThumbnailExtension));

                if (FileExists(thumbnailPath))
                {
                    retVal = LoadImage(thumbnailPath);
                }
                else
                {
                    var fullSizePortableImage = LoadImage(pathToImage);
                    return fullSizePortableImage;
                }
            }

            return retVal;
        }

        public void SaveImage(string path, PortableImage image, bool deleteExisting, ImageFormat format)
        {
            path = path.Replace("\\", "/");

            var withoutExtension = Path.GetFileNameWithoutExtension(path);
            var thumbnailPath = string.Format("{0}{1}", withoutExtension, CatrobatContextBase.ImageThumbnailExtension);

            if (deleteExisting)
            {
                if(FileExists(path))
                    _iso.DeleteFile(path);

                if (FileExists(thumbnailPath))
                    _iso.DeleteFile(thumbnailPath);
            }

            IsolatedStorageFileStream stream = null;

            try
            {
                stream = _iso.OpenFile(path, FileMode.CreateNew, FileAccess.Write);

                var writeableBitmap = new WriteableBitmap(image.Width, image.Height);
                writeableBitmap.FromByteArray(image.Data);

                switch (format)
                {
                    case ImageFormat.Png:
                        PNGWriter.WritePNG(writeableBitmap, stream, 95);
                        break;
                    case ImageFormat.Jpg:
                        writeableBitmap.SaveJpeg(stream, image.Width, image.Height, 0, 95);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("format");
                }
            }//TODO: Where should the error be handled?
            finally 
            {
                if (stream != null)
                {
                    stream.Flush();
                    stream.Dispose();
                }
            }

            stream.Close();
        }

        public void Dispose()
        {
            _iso.Dispose();

            foreach (Stream stream in _openedStreams)
            {
                stream.Dispose();
            }
        }

        public string BasePath
        {
            get { return ""; }
        }

        private void CreateFoldersIfNotExist(string path, bool isFilePath)
        {
            path = path.Replace("\\", "/");
            var splitPath = path.Split('/');
            var offset = isFilePath ? 1 : 0;
            for (var index = 0; index < splitPath.Length - 1; index++)
            {
                var subPath = "";

                for (var subIndex = 0; subIndex <= index; subIndex++)
                {
                    subPath = Path.Combine(subPath, splitPath[subIndex]);
                }

                if (!string.IsNullOrEmpty(subPath))
                {
                    _iso.CreateDirectory(subPath);
                }
            }
        }

        private void DeleteDirectory(string path, IsolatedStorageFile iso)
        {
            if (iso.DirectoryExists(path))
            {
                var directory = Path.Combine(path, "*.*");
                var folders = iso.GetDirectoryNames(directory);

                foreach (string folder in folders)
                {
                    var folderPath = Path.Combine(path, folder);
                    DeleteDirectory(folderPath, iso);
                }

                foreach (string file in iso.GetFileNames(directory))
                {
                    iso.DeleteFile(Path.Combine(path, file));
                }

                if (!string.IsNullOrEmpty(path))
                {
                    iso.DeleteDirectory(path);
                }
            }
        }

        public void CopyDirectory(string sourcePath, string destinationPath, IsolatedStorageFile iso)
        {
            if (iso.DirectoryExists(sourcePath))
            {
                var directory = Path.Combine(sourcePath, "*.*");
                var folders = iso.GetDirectoryNames(directory);

                foreach (string folder in folders)
                {
                    var sourceFolderPath = Path.Combine(sourcePath, folder);
                    var destinationFolderPath = Path.Combine(destinationPath, folder);

                    iso.CreateDirectory(destinationFolderPath);
                    CopyDirectory(sourceFolderPath, destinationFolderPath, iso);
                }

                var sourceDirectory = "";
                var destinationDirectory = "";

                try
                {
                    foreach (string file in iso.GetFileNames(directory))
                    {
                        if (file.StartsWith("."))
                        {
                            continue;
                        }

                        sourceDirectory = Path.Combine(sourcePath, file);
                        destinationDirectory = Path.Combine(destinationPath, file);
                        iso.CopyFile(sourceDirectory, destinationDirectory);
                    }
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("Cannot coppy {0} to {1}", sourceDirectory, destinationDirectory));
                }
            }
        }

        public void MoveDirectory(string sourcePath, string destinationPath, IsolatedStorageFile iso)
        {
            if (iso.DirectoryExists(sourcePath))
            {
                iso.MoveDirectory(sourcePath, destinationPath);
            }
        }

        public void SetImageMaxThumbnailWidthHeight(int maxWidthHeight)
        {
            _imageThumbnailDefaultMaxWidthHeight = maxWidthHeight;
        }

        public void SaveBitmapImage(string path, BitmapImage image)
        {
            var fileStream = OpenFile(path, StorageFileMode.Create, StorageFileAccess.Write);
            var wb = new WriteableBitmap(image);
            wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
            fileStream.Close();
            fileStream.Dispose();
        }

        #endregion


        #region Async

        public PortableImage CreateThumbnail(PortableImage image)
        {
            return ServiceLocator.ImageResizeService.ResizeImage(image, _imageThumbnailDefaultMaxWidthHeight);
        }


        public Task CreateDirectoryAsync(string path)
        {
            return Task.Run(() => CreateDirectory(path));
        }

        public Task<bool> DirectoryExistsAsync(string path)
        {
            return Task.Run(() => DirectoryExists(path));
        }

        public Task<bool> FileExistsAsync(string path)
        {
            return Task.Run(() => FileExists(path));
        }

        public Task<string[]> GetDirectoryNamesAsync(string path)
        {
            return Task.Run(() => GetDirectoryNames(path));
        }

        public Task<string[]> GetFileNamesAsync(string path)
        {
            return Task.Run(() => GetFileNames(path));
        }

        public Task DeleteDirectoryAsync(string path)
        {
            return Task.Run(() => DeleteDirectory(path));
        }

        public Task DeleteFileAsync(string path)
        {
            return Task.Run(() => DeleteFile(path));
        }

        public Task CopyDirectoryAsync(string sourcePath, string destinationPath)
        {
            return Task.Run(() => CopyDirectory(sourcePath, destinationPath));
        }

        public Task MoveDirectoryAsync(string sourcePath, string destinationPath)
        {
            return Task.Run(() => MoveDirectory(sourcePath, destinationPath));
        }

        public Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            return Task.Run(() => CopyFile(sourcePath, destinationPath));
        }

        public Task MoveFileAsync(string sourcePath, string destinationPath)
        {
            return Task.Run(() => MoveFile(sourcePath, destinationPath));
        }

        public Task<Stream> OpenFileAsync(string path, StorageFileMode mode, StorageFileAccess access)
        {
            return Task.Run(() => OpenFile(path, mode, access));
        }

        public Task RenameDirectoryAsync(string directoryPath, string newDirectoryName)
        {
            return Task.Run(() => RenameDirectory(directoryPath, newDirectoryName));
        }

        public Task<PortableImage> LoadImageAsync(string pathToImage)
        {
            return Task.Run(() => LoadImage(pathToImage));
        }

        public Task<PortableImage> LoadImageThumbnailAsync(string pathToImage)
        {
            return Task.Run(() => LoadImageThumbnail(pathToImage));
        }

        public Task<PortableImage> CreateThumbnailAsync(PortableImage image)
        {
            return Task.Run(() => CreateThumbnail(image));
        }

        public Task DeleteImageAsync(string pathToImage)
        {
            return Task.Run(() => DeleteImage(pathToImage));
        }

        public Task SaveImageAsync(string path, PortableImage image, bool deleteExisting, ImageFormat format)
        {
            return Task.Run(() => SaveImage(path, image, deleteExisting, format));
        }

        public Task<string> ReadTextFileAsync(string path)
        {
            return Task.Run(() => ReadTextFile(path));
        }

        public Task WriteTextFileAsync(string path, string content)
        {
            return Task.Run(() => WriteTextFile(path, content));
        }

        public Task<object> ReadSerializableObjectAsync(string path, Type type)
        {
            return Task.Run(() => ReadSerializableObject(path, type));
        }

        public Task WriteSerializableObjectAsync(string path, object serializableObject)
        {
            return Task.Run(() => WriteSerializableObject(path, serializableObject));
        }

        #endregion
    }
}