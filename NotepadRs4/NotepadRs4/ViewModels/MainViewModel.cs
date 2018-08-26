﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using NotepadRs4.Helpers;
using NotepadRs4.Models;
using NotepadRs4.Services;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NotepadRs4.ViewModels
{
    public class MainViewModel : NotificationBase
    {
        // Properties
        private TextDataModel _data;
        public TextDataModel Data
        {
            get { return _data; }
            set { SetProperty(ref _data, value); }
        }

        private StorageFile _file;
        public StorageFile File
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }


        // Main
        public MainViewModel()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_data == null)
            {
                TextDataModel data = new TextDataModel();
                data.DocumentTitle = "Untitled";
                Data = data;
                RefreshTitlebarTitle();
            }
        }
        


        // Commands
        private ICommand _newFileCommand;
        public ICommand NewFileCommand
        {
            get
            {
                if (_newFileCommand == null)
                {
                    _newFileCommand = new RelayCommand(
                        async () =>
                        {
                            NewFile();
                        });
                }
                return _newFileCommand;
            }
        }

        private ICommand _saveFileCommand;
        public ICommand SaveFileCommand
        {
            get
            {
                if (_saveFileCommand == null)
                {
                    _saveFileCommand = new RelayCommand(
                        async () =>
                        {
                            await SaveFileAs();
                        });
                }
                return _saveFileCommand;
            }
        }

        private ICommand _saveFileAsCommand;
        public ICommand SaveFileAsCommand
        {
            get
            {
                if (_saveFileAsCommand == null)
                {
                    _saveFileAsCommand = new RelayCommand(
                        async () =>
                        {
                            await SaveFileAs();
                        });
                }
                return _saveFileAsCommand;
            }
        }

        private ICommand _loadFileCommand;
        public ICommand LoadFileCommand
        {
            get
            {
                if (_loadFileCommand == null)
                {
                    _loadFileCommand = new RelayCommand(
                        async () =>
                        {
                            await LoadFile();
                        });
                }
                return _loadFileCommand;
            }
        }

        private ICommand _printCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand(
                        async () =>
                        {
                            // Print stuff
                            //throw new NotImplementedException();
                        });
                }
                return _printCommand;
            }
        }

        private ICommand _settingsCommand;
        public ICommand SettingsCommand
        {
            get
            {
                if (_settingsCommand == null)
                {
                    _settingsCommand = new RelayCommand(
                        async () =>
                        {
                            GoToSettings();
                        });
                }
                return _settingsCommand;
            }
        }



        // Methods
        // New
        public async void NewFile()
        {
            // #TODO: Ask the user to save the current file if there is one open
            if (_data.Text != "")
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Content = "Would you like to save your work?";
                dialog.Title = "You have unsaved work";
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Don't save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Background = Application.Current.Resources["SystemControlAcrylicElementMediumHighBrush"] as AcrylicBrush;

                var answer = await dialog.ShowAsync();

                if (answer == ContentDialogResult.Primary)
                {
                    // Save & then open a new file
                    Debug.WriteLine("New File: Save & open new file");

                    // TODO: Change between Save and SaveAs methods when it has been saved before

                    bool saveSuccessful = await SaveFileAs();

                    if (saveSuccessful == true)
                    {
                        TextDataModel emptyData = new TextDataModel();
                        emptyData.DocumentTitle = "Untitled";
                        Data = emptyData;
                        File = null;
                        RefreshTitlebarTitle();
                    }
                    else
                    {
                        Debug.WriteLine("New File: Saving Failed");
                        // TODO: Give a message back that saving has failed

                    }


                }
                if (answer == ContentDialogResult.Secondary)
                {
                    // Discard changes and open a new file
                    Debug.WriteLine("New File: Discard changes");

                    TextDataModel emptyData = new TextDataModel();
                    emptyData.DocumentTitle = "Untitled";
                    Data = emptyData;
                    File = null;
                    RefreshTitlebarTitle();
                }
                else
                {
                    // Close the dialog and do nothing
                    Debug.WriteLine("New File: Dialog cancelled");
                }

                // or
                // #TODO: Create a new window with an empty file if a file is currently open
            }
        }

        // Save
        public async Task<bool> SaveFile()
        {
            if (File == null)
            {
                bool success = await SaveFileAs();
                return success;
            }
            else
            {

                bool success = await FileDataService.Save(_data, _file);
                return success;
            }
        }


        // Save As
        public async Task<bool> SaveFileAs()
        {
            StorageFile tempFile = await FileDataService.SaveAs(_data);

            // TODO - Expose the proper check here for the UI to react to
            if (tempFile != null)
            {
                File = tempFile;
                // Create a temp TextDataModel to make the changes in
                TextDataModel data = new TextDataModel();
                data.Text = Data.Text;
                data.DocumentTitle = File.DisplayName + File.FileType;
                // Write the changes back to the Data property since it doesn't register single changed items otherwise
                Data = data;
                RefreshTitlebarTitle();

                return true;
            }
            else
            {
                return false;
            }
        }

        // Load
        public async Task<bool> LoadFile()
        {
            // TODO: Ask if the user wants to save their work before loading another file
            TextDataModel data = await FileDataService.Load();
            if (data != null)
            {
                Data = data;
            }
            else
            {
                Data = new TextDataModel();
                Data.DocumentTitle = "Loading Failed";
                RefreshTitlebarTitle();
            }

            return true;
        }

        // Go to Settings Page
        public void GoToSettings()
        {
            NavigationService.Navigate(typeof(Views.SettingsPage));
        }




        // Set title of the app
        private void RefreshTitlebarTitle()
        {
            ApplicationView.GetForCurrentView().Title = Data.DocumentTitle;
        }
    }
}