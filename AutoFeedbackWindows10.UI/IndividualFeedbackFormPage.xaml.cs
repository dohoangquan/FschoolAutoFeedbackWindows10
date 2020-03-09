﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using System.Numerics;
using AutoFeedbackWindows10.UI.DataProviders;
using AutoFeedbackWindows10.UI.Models;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AutoFeedbackWindows10.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IndividualFeedbackFormPage : Page
    {
        private FeedbackEntryProvider Provider => ((App)Application.Current).FeedbackProvider;

        public IndividualFeedbackFormPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var ca = ConnectedAnimationService.GetForCurrentView().GetAnimation("individualClick");
            ca?.TryStart(headerPS, new List<UIElement>() { txblHeaderName, txblHeaderClassName });

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () => DataContext = await Provider.GetFeedbackData(e.Parameter as FeedbackTeacherModel));

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var propSet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollRoot);
            var compositor = propSet.Compositor;
            var props = compositor.CreatePropertySet();
            props.InsertScalar("progress", 0);
            props.InsertScalar("clampSize", 150);

            var propScroll = propSet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var propGet = props.GetReference();
            var progressionNode = propGet.GetScalarProperty("progress");
            var clampSizeNode = propGet.GetScalarProperty("clampSize");

            ExpressionNode progressNode = ExpressionFunctions.Clamp(-propScroll.Translation.Y / clampSizeNode, 0, 1);
            props.StartAnimation("progress", progressNode);

            ExpressionNode opacityNode = ExpressionFunctions.Lerp(1, 0, progressionNode);
            var pnlDetailVisual = ElementCompositionPreview.GetElementVisual(pnlDetail);
            var pnlFeedbackForVisual = ElementCompositionPreview.GetElementVisual(pnlFeedbackFor);
            pnlDetailVisual.StartAnimation("Opacity", opacityNode);
            pnlFeedbackForVisual.StartAnimation("Opacity", opacityNode);

            ExpressionNode opacityInvertedNode = ExpressionFunctions.Lerp(0, 1, progressionNode);
            var headerBackgroundVisual = ElementCompositionPreview.GetElementVisual(recHeaderBackground);
            headerBackgroundVisual.StartAnimation("Opacity", opacityInvertedNode);

            ExpressionNode scaleNode = ExpressionFunctions.Lerp(1, 0, progressionNode / 2);
            var headerPSVisual = ElementCompositionPreview.GetElementVisual(headerPS);
            headerPSVisual.StartAnimation("Scale.X", scaleNode);
            headerPSVisual.StartAnimation("Scale.Y", scaleNode);


            ExpressionNode headerTextNode = ExpressionFunctions.Lerp(new Vector3(0, 0, 0), new Vector3(-50f, 0, 0), progressionNode);
            var pnlBasicInfoVisual = ElementCompositionPreview.GetElementVisual(pnlBasicInfo);
            pnlBasicInfoVisual.StartAnimation("Offset", headerTextNode);

            ExpressionNode actionButtonNode = ExpressionFunctions.Lerp(new Vector2(0, 0), new Vector2(0, 3.3f), progressionNode);
            var sendButtonViusal = ElementCompositionPreview.GetElementVisual(btnSend);
            sendButtonViusal.StartAnimation("AnchorPoint", actionButtonNode);
        }

        private async void btnSend_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            await Provider.SendFeedbackAsync(DataContext as FeedbackTeacherModel);
        }
    }
}
