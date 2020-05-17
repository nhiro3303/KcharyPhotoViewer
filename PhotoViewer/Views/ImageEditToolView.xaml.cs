﻿using System.Windows;
using PhotoViewer.ViewModels;

namespace PhotoViewer.Views
{
    /// <summary>
    /// Interaction logic for ExifDeleteToolView.xaml
    /// </summary>
    public partial class ImageEditToolView : Window
    {
        public ImageEditToolView()
        {
            InitializeComponent();

            DataContextChanged += (o, e) =>
            {
                var vm = this.DataContext as ImageEditToolViewModel;
                if (vm != null)
                {
                    vm.CloseView += (sender, args) => 
                    {
                        // 不要なメモリ解放
                        App.RunGC();
                        Close();
                    };
                }
            };
        }

        /// <summary>
        /// キャンセルボタン押下時
        /// </summary>
        /// <param name="sender">ImageEditWindow</param>
        /// <param name="e">引数情報</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 保存形式コンボボックスの選択状態が変化したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as ImageEditToolViewModel;
            if (vm == null) return;

            // 保存形式に応じて、品質設定の表示を切り替え
            vm.IsEnableImageSaveQuality = vm.SelectedForm.Form == ImageForm.ImageForms.Jpeg ? true : false;
        }
    }
}
