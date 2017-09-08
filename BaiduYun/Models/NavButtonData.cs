using System;

using Windows.UI.Xaml;

namespace BaiduYun.Models {

    class NavButtonData : DependencyObject {

        public string Glyph {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        public static DependencyProperty GlyphProperty =
            DependencyProperty.RegisterAttached(nameof(Glyph), typeof(string), typeof(NavButtonData), new PropertyMetadata(""));

        public string Label {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        
        public static DependencyProperty LabelProperty =
            DependencyProperty.RegisterAttached(nameof(Label), typeof(string), typeof(NavButtonData), new PropertyMetadata(""));

        public Type Page {
            get { return (Type)GetValue(PageProperty); }
            set { SetValue(PageProperty, value); }
        }

        public static DependencyProperty PageProperty =
            DependencyProperty.RegisterAttached(nameof(Page), typeof(Type), typeof(NavButtonData), new PropertyMetadata(null));
    }

}
