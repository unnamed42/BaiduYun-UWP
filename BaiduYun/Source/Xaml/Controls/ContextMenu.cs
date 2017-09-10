using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;

namespace BaiduYun.Xaml.Controls {

    public class ContextMenu : DependencyObject {

        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.RegisterAttached("Menu", typeof(MenuFlyout), typeof(ContextMenu), new PropertyMetadata(null));

        public static MenuFlyout GetMenu(UIElement elem) {
            return (MenuFlyout)elem.GetValue(MenuProperty);
        }

        public static void SetMenu(UIElement elem, MenuFlyout menu) {
            elem.SetValue(MenuProperty, menu);
            elem.IsRightTapEnabled = true;
            elem.RightTapped += ContextMenuPopup;
        }

        private static void ContextMenuPopup(object sender, RightTappedRoutedEventArgs e) {
            var elem = (UIElement)sender;
            var menu = ContextMenu.GetMenu(elem);
            menu.ShowAt(elem, e.GetPosition(elem));
        }
    }
}
