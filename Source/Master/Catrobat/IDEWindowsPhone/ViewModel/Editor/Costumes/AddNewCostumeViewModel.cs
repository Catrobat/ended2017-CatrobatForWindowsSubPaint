﻿using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Catrobat.Core.Objects;
using Catrobat.IDECommon.Resources.Editor;
using Catrobat.IDEWindowsPhone.Misc;
using Catrobat.IDEWindowsPhone.Views.Editor.Costumes;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Tasks;

namespace Catrobat.IDEWindowsPhone.ViewModel.Editor.Costumes
{
    public class AddNewCostumeViewModel : ViewModelBase
    {
        #region Private Members

        private string _costumeName;
        private CostumeBuilder _builder;
        private Sprite _receivedSelectedSprite;
        private ImageDimention _dimention;
        private ImageSizeEntry _selectedSize;

        #endregion

        #region Properties

        public string CostumeName
        {
            get { return _costumeName; }
            set
            {
                if (value == _costumeName)
                {
                    return;
                }
                _costumeName = value;
                RaisePropertyChanged("CostumeName");
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public ImageDimention Dimention
        {
            get { return _dimention; }
            set
            {
                _dimention = value;
                RaisePropertyChanged("Dimention");
            }
        }

        public ImageSizeEntry SelectedSize
        {
            get { return _selectedSize; }
            set
            {
                _selectedSize = value;
                RaisePropertyChanged("SelectedSize");
            }
        }

        public ObservableCollection<ImageSizeEntry> ImageSizes { get; set; }

        #endregion

        #region Commands

        public RelayCommand OpenGalleryCommand { get; private set; }

        public RelayCommand OpenCameraCommand { get; private set; }

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand CancelCommand { get; private set; }

        public RelayCommand ResetViewModelCommand { get; private set; }

        #endregion

        #region CommandCanExecute

        private bool SaveCommand_CanExecute()
        {
            return CostumeName != null && CostumeName.Length >= 2;
        }

        #endregion

        #region Actions

        private void OpenGalleryAction()
        {
            lock (this)
            {
                var photoChooserTask = new PhotoChooserTask();
                photoChooserTask.Completed -= Task_Completed;
                photoChooserTask.Completed += Task_Completed;
                photoChooserTask.Show();
            }
        }

        private void OpenCameraAction()
        {
            lock (this)
            {
                var cameraCaptureTask = new CameraCaptureTask();
                cameraCaptureTask.Completed -= Task_Completed;
                cameraCaptureTask.Completed += Task_Completed;
                cameraCaptureTask.Show();
            }
        }

        private void SaveAction()
        {
            var costume = _builder.Save(CostumeName, Dimention);
            _receivedSelectedSprite.Costumes.Costumes.Add(costume);

            Navigation.RemoveBackEntry();
            Navigation.NavigateBack();
        }

        private void CancelAction()
        {
            Navigation.NavigateBack();
        }

        private void ReceiveSelectedSpriteMessageAction(GenericMessage<Sprite> message)
        {
            _receivedSelectedSprite = message.Content;
        }

        private void ResetViewModelAction()
        {
            ResetViewModel();
        }

        #endregion

        public AddNewCostumeViewModel()
        {
            OpenGalleryCommand = new RelayCommand(OpenGalleryAction);
            OpenCameraCommand = new RelayCommand(OpenCameraAction);
            SaveCommand = new RelayCommand(SaveAction, SaveCommand_CanExecute);
            CancelCommand = new RelayCommand(CancelAction);
            ResetViewModelCommand = new RelayCommand(ResetViewModelAction);

            Messenger.Default.Register<GenericMessage<Sprite>>(this, ViewModelMessagingToken.SelectedSpriteListener, ReceiveSelectedSpriteMessageAction);

            InitImageSizes();
            if (IsInDesignMode)
                InitDesignData();
        }

        private void InitDesignData()
        {
            _dimention = new ImageDimention { Width = 500, Height = 500 };
            _selectedSize = ImageSizes[1];
        }

        private void Task_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                CostumeName = EditorResources.Image;

                _builder = new CostumeBuilder();
                _builder.LoadCostumeSuccess += LoadCostumeSuccess;
                _builder.LoadCostumeFailed += LoadCostumeFailed;

                _builder.StartCreateCostumeAsync(_receivedSelectedSprite, e.ChosenPhoto);
            }
        }

        private void LoadCostumeSuccess(ImageDimention dimention)
        {
            this.Dimention = dimention;
            Deployment.Current.Dispatcher.BeginInvoke(() => Navigation.NavigateTo(typeof(CostumeNameChooserView)));
        }

        private void LoadCostumeFailed()
        {
            var message = new DialogMessage(EditorResources.MessageBoxWrongImageFormatText, WrongImageFormatResult)
            {
                Button = MessageBoxButton.OK,
                Caption = EditorResources.MessageBoxWrongImageFormatHeader
            };
            Messenger.Default.Send(message);
        }

        private void WrongImageFormatResult(MessageBoxResult result)
        {
            Navigation.NavigateBack();
        }

        private void InitImageSizes()
        {
            ImageSizes = new ObservableCollection<ImageSizeEntry>
            {
                new ImageSizeEntry {Size = ImageSize.Small},
                new ImageSizeEntry {Size = ImageSize.Medium},
                new ImageSizeEntry {Size = ImageSize.Large},
                new ImageSizeEntry {Size = ImageSize.FullSize}
            };

            SelectedSize = ImageSizes[1];
        }

        private void ResetViewModel()
        {
            CostumeName = null;

            InitImageSizes();

            if (_builder != null)
            {
                _builder.LoadCostumeSuccess -= LoadCostumeSuccess;
                _builder.LoadCostumeFailed -= LoadCostumeFailed;
                _builder = null;
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}