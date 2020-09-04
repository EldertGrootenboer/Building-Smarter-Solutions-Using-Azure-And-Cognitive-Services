﻿// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace IntelligentKioskSample.Views
{
    [KioskExperience(Title = "Vision API Explorer", ImagePath = "ms-appx:/Assets/VisionAPI.jpg")]
    public sealed partial class VisionApiExplorer : Page
    {
        public static bool ShowAgeAndGender { get { return SettingsHelper.Instance.ShowAgeAndGender; } }

        public VisionApiExplorer()
        {
            this.InitializeComponent();

            this.imagePicker.SetSuggestedImageList(
                "https://howoldkiosk.blob.core.windows.net/kiosksuggestedphotos/1.jpg",
                "https://howoldkiosk.blob.core.windows.net/kiosksuggestedphotos/2.jpg",
                "https://howoldkiosk.blob.core.windows.net/kiosksuggestedphotos/3.jpg",
                "https://howoldkiosk.blob.core.windows.net/kiosksuggestedphotos/4.jpg",
                "https://howoldkiosk.blob.core.windows.net/kiosksuggestedphotos/5.jpg",


                "https://intelligentkioskstore.blob.core.windows.net/visionapi/suggestedphotos/3.png",
                "https://intelligentkioskstore.blob.core.windows.net/visionapi/suggestedphotos/1.png",
                "https://intelligentkioskstore.blob.core.windows.net/visionapi/suggestedphotos/2.png"
            );
        }

        private void DisplayProcessingUI()
        {
            this.tagsGridView.ItemsSource = new[] { new { Name = "Analyzing..." } };
            this.descriptionGridView.ItemsSource = new[] { new { Description = "Analyzing..." } };
            this.celebritiesTextBlock.Text = "Analyzing...";
            this.landmarksTextBlock.Text = "Analyzing...";
            this.colorInfoListView.ItemsSource = new[] { new { Description = "Analyzing..." } };

            this.ocrToggle.IsEnabled = false;
            this.objectDetectionToggle.IsEnabled = false;
            this.ocrTextBox.Text = "";
        }

        private void UpdateResults(ImageAnalyzer img)
        {
            if (img.AnalysisResult.Tags == null || !img.AnalysisResult.Tags.Any())
            {
                this.tagsGridView.ItemsSource = new[] { new { Name = "No tags" } };
            }
            else
            {
                var tags = img.AnalysisResult.Tags.Select(t => new { Confidence = string.Format("({0}%)", Math.Round(t.Confidence * 100)), Name = t.Name });
                if (!ShowAgeAndGender)
                {
                    tags = tags.Where(t => !Util.ContainsGenderRelatedKeyword(t.Name));
                }

                this.tagsGridView.ItemsSource = tags;
            }

            if (img.AnalysisResult.Description == null || !img.AnalysisResult.Description.Captions.Any(d => d.Confidence >= 0.2))
            {
                this.descriptionGridView.ItemsSource = new[] { new { Description = "Not sure what that is" } };
            }
            else
            {
                var descriptions = img.AnalysisResult.Description.Captions.Select(d => new { Confidence = string.Format("({0}%)", Math.Round(d.Confidence * 100)), Description = d.Text });
                if (!ShowAgeAndGender)
                {
                    descriptions = descriptions.Where(t => !Util.ContainsGenderRelatedKeyword(t.Description));
                }

                if (descriptions.Any())
                {
                    this.descriptionGridView.ItemsSource = descriptions;
                }
                else
                {
                    this.descriptionGridView.ItemsSource = new[] { new { Description = "Please enable Age/Gender prediction in the Settings Page to see the results" } };
                }
            }

            var celebNames = this.GetCelebrityNames(img);
            if (celebNames == null || !celebNames.Any())
            {
                this.celebritiesTextBlock.Text = "None";
            }
            else
            {
                this.celebritiesTextBlock.Text = string.Join(", ", celebNames.OrderBy(name => name));
            }

            var landmarkNames = this.GetLandmarkNames(img);
            if (landmarkNames == null || !landmarkNames.Any())
            {
                this.landmarksTextBlock.Text = "None";
            }
            else
            {
                this.landmarksTextBlock.Text = string.Join(", ", landmarkNames.OrderBy(name => name).Distinct());
            }

            if (img.AnalysisResult.Color == null)
            {
                this.colorInfoListView.ItemsSource = new[] { new { Description = "Not available" } };
            }
            else
            { 
                this.colorInfoListView.ItemsSource = new[]
                {
                    new { Description = "Dominant background color:", Colors = new string[] { img.AnalysisResult.Color.DominantColorBackground } },
                    new { Description = "Dominant foreground color:", Colors = new string[] { img.AnalysisResult.Color.DominantColorForeground } },
                    new { Description = "Dominant colors:", Colors = img.AnalysisResult.Color.DominantColors?.ToArray() },
                    new { Description = "Accent color:", Colors = new string[] { "#" + img.AnalysisResult.Color.AccentColor } }
                };
            }

            this.ocrToggle.IsEnabled = true;
            this.objectDetectionToggle.IsEnabled = true;
        }

        private IEnumerable<string> GetCelebrityNames(ImageAnalyzer analyzer)
        {
            if (analyzer.AnalysisResult?.Categories != null)
            {
                foreach (var category in analyzer.AnalysisResult.Categories?.Where(c => c.Detail != null))
                {
                    if (category.Detail.Celebrities != null)
                    {
                        foreach (var celebrity in category.Detail.Celebrities)
                        {
                            yield return celebrity.Name;
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetLandmarkNames(ImageAnalyzer analyzer)
        {
            if (analyzer.AnalysisResult?.Categories != null)
            {
                foreach (var category in analyzer.AnalysisResult.Categories?.Where(c => c.Detail != null))
                {
                    if (category.Detail.Landmarks != null)
                    {
                        foreach (var landmark in category.Detail.Landmarks)
                        {
                            yield return landmark.Name;
                        }
                    }
                }
            }
        }

        private void UpdateActivePhoto(ImageAnalyzer img)
        {
            this.resultsDetails.Visibility = Visibility.Visible;

            if (img.AnalysisResult != null)
            {
                this.UpdateResults(img);
            }
            else
            {
                this.DisplayProcessingUI();
                img.ComputerVisionAnalysisCompleted += (s, args) =>
                {
                    this.UpdateResults(img);
                };
            }

            if (this.ocrToggle.IsOn)
            {
                if (img.TextOperationResult?.RecognitionResult != null)
                {
                    this.UpdateOcrTextBoxContent(img);
                }
                else
                {
                    img.TextRecognitionCompleted += (s, args) =>
                    {
                        this.UpdateOcrTextBoxContent(img);
                    };
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsHelper.Instance.VisionApiKey))
            {
                await new MessageDialog("Missing Computer Vision API Key. Please enter a key in the Settings page.", "Missing API Key").ShowAsync();
            }

            base.OnNavigatedTo(e);
        }

        private void OnImageSearchCompleted(object sender, IEnumerable<ImageAnalyzer> args)
        {
            ImageAnalyzer image = args.First();
            image.ShowDialogOnFaceApiErrors = true;

            this.imageWithFacesControl.Visibility = Visibility.Visible;

            this.UpdateActivePhoto(image);

            this.imageWithFacesControl.DataContext = image;
        }

        private void OnOCRToggled(object sender, RoutedEventArgs e)
        {
            this.printedOCRComboBoxItem.IsSelected = true;
            UpdateTextRecognition(TextRecognitionMode.Printed);
        }

        private void OcrModeSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (printedOCRComboBoxItem.IsSelected)
            {
                UpdateTextRecognition(TextRecognitionMode.Printed);
            }
            else if (handwrittigOCRComboBoxItem.IsSelected)
            {
                UpdateTextRecognition(TextRecognitionMode.Handwritten);
            }
        }

        private void UpdateTextRecognition(TextRecognitionMode textRecognitionMode)
        {
            imageWithFacesControl.TextRecognitionMode = textRecognitionMode;

            var currentImageDisplay = this.imageWithFacesControl;
            if (currentImageDisplay.DataContext != null)
            {
                var img = currentImageDisplay.DataContext;

                ImageAnalyzer analyzer = (ImageAnalyzer)img;

                if (analyzer.TextOperationResult?.RecognitionResult != null)
                {
                    UpdateOcrTextBoxContent(analyzer);
                }
                else
                {
                    analyzer.TextRecognitionCompleted += (s, args) =>
                    {
                        UpdateOcrTextBoxContent(analyzer);
                    };
                }

                currentImageDisplay.DataContext = null;
                currentImageDisplay.DataContext = img;
            }
        }

        private void UpdateOcrTextBoxContent(ImageAnalyzer imageAnalyzer)
        {
            this.ocrTextBox.Text = string.Empty;
            if (imageAnalyzer.TextOperationResult?.RecognitionResult?.Lines != null)
            {
                IEnumerable<string> lines = imageAnalyzer.TextOperationResult.RecognitionResult.Lines.Select(l => string.Join(" ", l?.Words?.Select(w => w.Text)));
                this.ocrTextBox.Text = string.Join("\n", lines);
            }
        }

        private void OnObjectDetectionToggled(object sender, RoutedEventArgs e)
        {
            var currentImageDisplay = this.imageWithFacesControl;
            if (currentImageDisplay.DataContext != null)
            {
                var img = currentImageDisplay.DataContext;

                ImageAnalyzer analyzer = (ImageAnalyzer)img;

                currentImageDisplay.DataContext = null;
                currentImageDisplay.DataContext = img;
            }
        }
    }
}
