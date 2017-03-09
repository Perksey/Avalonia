// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml.Data;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.UnitTests;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Avalonia.Markup.Xaml.UnitTests.Xaml
{
    public class BasicTests
    {
        [Fact]
        public void Simple_Property_Is_Set()
        {
            var xaml = @"<ContentControl xmlns='https://github.com/avaloniaui' Content='Foo'/>";

            var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

            Assert.NotNull(target);
            Assert.Equal("Foo", target.Content);
        }

        [Fact]
        public void Default_Content_Property_Is_Set()
        {
            var xaml = @"<ContentControl xmlns='https://github.com/avaloniaui'>Foo</ContentControl>";

            var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

            Assert.NotNull(target);
            Assert.Equal("Foo", target.Content);
        }

        [Fact]
        public void AvaloniaProperty_Without_Getter_And_Setter_Is_Set()
        {
            var xaml =
 @"<local:NonControl xmlns='https://github.com/avaloniaui' 
    xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'
    Foo='55' />";

            var target = AvaloniaXamlLoader.Parse<NonControl>(xaml);

            Assert.Equal(55, target.GetValue(NonControl.FooProperty));
        }

        [Fact]
        public void AvaloniaProperty_With_Getter_And_No_Setter_Is_Set()
        {
            var xaml =
@"<local:NonControl xmlns='https://github.com/avaloniaui' 
    xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'
    Bar='bar' />";

            var target = AvaloniaXamlLoader.Parse<NonControl>(xaml);

            Assert.Equal("bar", target.Bar);
        }

        [Fact]
        public void Attached_Property_Is_Set()
        {
            var xaml =
        @"<ContentControl xmlns='https://github.com/avaloniaui' TextBlock.FontSize='21'/>";

            var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

            Assert.NotNull(target);
            Assert.Equal(21.0, TextBlock.GetFontSize(target));
        }

        [Fact]
        public void Attached_Property_Supports_Binding()
        {
            using (UnitTestApplication.Start(TestServices.MockWindowingPlatform))
            {
                var xaml =
            @"<Window xmlns='https://github.com/avaloniaui' TextBlock.FontSize='{Binding}'/>";

                var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

                target.DataContext = 21.0;

                Assert.Equal(21.0, TextBlock.GetFontSize(target));
            }
        }

        [Fact]
        public void Attached_Property_In_Panel_Is_Set()
        {
            var xaml = @"
<Panel xmlns='https://github.com/avaloniaui'>
    <ToolTip.Tip>Foo</ToolTip.Tip>
</Panel>";

            var target = AvaloniaXamlLoader.Parse<Panel>(xaml);

            Assert.Equal(0, target.Children.Count);

            Assert.Equal("Foo", ToolTip.GetTip(target));
        }

        [Fact]
        public void ContentControl_ContentTemplate_Is_Functional()
        {
            var xaml =
@"<ContentControl xmlns='https://github.com/avaloniaui'>
    <ContentControl.ContentTemplate>
        <DataTemplate>
            <TextBlock Text='Foo' />
        </DataTemplate>
    </ContentControl.ContentTemplate>
</ContentControl>";

            var contentControl = AvaloniaXamlLoader.Parse<ContentControl>(xaml);
            var target = contentControl.ContentTemplate;

            Assert.NotNull(target);

            var txt = (TextBlock)target.Build(null);

            Assert.Equal("Foo", txt.Text);
        }

        [Fact]
        public void Named_Control_Is_Added_To_NameScope_Simple()
        {
            var xaml = @"
<UserControl xmlns='https://github.com/avaloniaui'>
    <Button Name='button'>Foo</Button>
</UserControl>";

            var control = AvaloniaXamlLoader.Parse<UserControl>(xaml);
            var button = control.FindControl<Button>("button");

            Assert.Equal("Foo", button.Content);
        }

        [Fact]
        public void Direct_Content_In_ItemsControl_Is_Operational()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'>
     <ItemsControl Name='items'>
         <ContentControl>Foo</ContentControl>
         <ContentControl>Bar</ContentControl>
      </ItemsControl>
</Window>";

                var control = AvaloniaXamlLoader.Parse<Window>(xaml);

                var itemsControl = control.FindControl<ItemsControl>("items");

                Assert.NotNull(itemsControl);

                var items = itemsControl.Items.Cast<ContentControl>().ToArray();

                Assert.Equal("Foo", items[0].Content);
                Assert.Equal("Bar", items[1].Content);
            }
        }

        [Fact]
        public void Panel_Children_Are_Added()
        {
            var xaml = @"
<UserControl xmlns='https://github.com/avaloniaui'>
    <Panel Name='panel'>
        <ContentControl Name='Foo' />
        <ContentControl Name='Bar' />
    </Panel>
</UserControl>";

            var control = AvaloniaXamlLoader.Parse<UserControl>(xaml);

            var panel = control.FindControl<Panel>("panel");

            Assert.Equal(2, panel.Children.Count);

            var foo = control.FindControl<ContentControl>("Foo");
            var bar = control.FindControl<ContentControl>("Bar");

            Assert.Contains(foo, panel.Children);
            Assert.Contains(bar, panel.Children);
        }

        [Fact]
        public void ControlTemplate_With_Nested_Child_Is_Operational()
        {
            var xaml = @"
<ControlTemplate xmlns='https://github.com/avaloniaui'>
    <ContentControl Name='parent'>
        <ContentControl Name='child' />
    </ContentControl>
</ControlTemplate>
";
            var template = AvaloniaXamlLoader.Parse<ControlTemplate>(xaml);

            var parent = (ContentControl)template.Build(new ContentControl());

            Assert.Equal("parent", parent.Name);

            var child = parent.Content as ContentControl;

            Assert.NotNull(child);

            Assert.Equal("child", child.Name);
        }

        [Fact]
        public void ControlTemplate_With_Panel_Children_Are_Added()
        {
            var xaml = @"
<ControlTemplate xmlns='https://github.com/avaloniaui'>
    <Panel Name='panel'>
        <ContentControl Name='Foo' />
        <ContentControl Name='Bar' />
    </Panel>
</ControlTemplate>
";
            var template = AvaloniaXamlLoader.Parse<ControlTemplate>(xaml);

            var panel = (Panel)template.Build(new ContentControl());

            Assert.Equal(2, panel.Children.Count);

            var foo = panel.Children[0];
            var bar = panel.Children[1];

            Assert.Equal("Foo", foo.Name);
            Assert.Equal("Bar", bar.Name);
        }

        [Fact]
        public void Named_x_Control_Is_Added_To_NameScope_Simple()
        {
            var xaml = @"
<UserControl xmlns='https://github.com/avaloniaui'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Button x:Name='button'>Foo</Button>
</UserControl>";

            var control = AvaloniaXamlLoader.Parse<UserControl>(xaml);
            var button = control.FindControl<Button>("button");

            Assert.Equal("Foo", button.Content);
        }

        [Fact]
        public void Standart_TypeConverter_Is_Used()
        {
            var xaml = @"<UserControl xmlns='https://github.com/avaloniaui' Width='200.5' />";

            var control = AvaloniaXamlLoader.Parse<UserControl>(xaml);
            Assert.Equal(200.5, control.Width);
        }

        [Fact]
        public void Avalonia_TypeConverter_Is_Used()
        {
            var xaml = @"<UserControl xmlns='https://github.com/avaloniaui' Background='White' />";

            var control = AvaloniaXamlLoader.Parse<UserControl>(xaml);
            var bk = control.Background;
            Assert.IsType<SolidColorBrush>(bk);
            Assert.Equal(Colors.White, (bk as SolidColorBrush).Color);
        }

        [Fact]
        public void Simple_Style_Is_Parsed()
        {
            var xaml = @"
<Styles xmlns='https://github.com/avaloniaui'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Style Selector='TextBlock'>
        <Setter Property='Background' Value='White'/>
        <Setter Property='Width' Value='100'/>
    </Style>
</Styles>";

            var styles = AvaloniaXamlLoader.Parse<Styles>(xaml);

            Assert.Equal(1, styles.Count);

            var style = (Style)styles[0];

            var setters = style.Setters.Cast<Setter>().ToArray();

            Assert.Equal(2, setters.Length);

            Assert.Equal(TextBlock.BackgroundProperty, setters[0].Property);
            Assert.Equal(Brushes.White, setters[0].Value);

            Assert.Equal(TextBlock.WidthProperty, setters[1].Property);
            Assert.Equal(100.0, setters[1].Value);
        }

        [Fact]
        public void Style_Setter_With_AttachedProperty_Is_Parsed()
        {
            var xaml = @"
<Styles xmlns='https://github.com/avaloniaui'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Style Selector='ContentControl'>
        <Setter Property='TextBlock.FontSize' Value='21'/>
    </Style>
</Styles>";

            var styles = AvaloniaXamlLoader.Parse<Styles>(xaml);

            Assert.Equal(1, styles.Count);

            var style = (Style)styles[0];

            var setters = style.Setters.Cast<Setter>().ToArray();

            Assert.Equal(1, setters.Length);

            Assert.Equal(TextBlock.FontSizeProperty, setters[0].Property);
            Assert.Equal(21.0, setters[0].Value);
        }

        [Fact]
        public void Complex_Style_Is_Parsed()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Styles xmlns='https://github.com/avaloniaui'>
  <Style Selector='CheckBox'>
    <Setter Property='BorderBrush' Value='{StyleResource ThemeBorderMidBrush}'/>
    <Setter Property='BorderThickness' Value='{StyleResource ThemeBorderThickness}'/>
    <Setter Property='Template'>
      <ControlTemplate>
        <Grid ColumnDefinitions='Auto,*'>
          <Border Name='border'
                  BorderBrush='{TemplateBinding BorderBrush}'
                  BorderThickness='{TemplateBinding BorderThickness}'
                  Width='18'
                  Height='18'
                  VerticalAlignment='Center'>
            <Path Name='checkMark'
                  Fill='{StyleResource HighlightBrush}'
                  Width='11'
                  Height='10'
                  Stretch='Uniform'
                  HorizontalAlignment='Center'
                  VerticalAlignment='Center'
                  Data='M 1145.607177734375,430 C1145.607177734375,430 1141.449951171875,435.0772705078125 1141.449951171875,435.0772705078125 1141.449951171875,435.0772705078125 1139.232177734375,433.0999755859375 1139.232177734375,433.0999755859375 1139.232177734375,433.0999755859375 1138,434.5538330078125 1138,434.5538330078125 1138,434.5538330078125 1141.482177734375,438 1141.482177734375,438 1141.482177734375,438 1141.96875,437.9375 1141.96875,437.9375 1141.96875,437.9375 1147,431.34619140625 1147,431.34619140625 1147,431.34619140625 1145.607177734375,430 1145.607177734375,430 z'/>
          </Border>
          <ContentPresenter Name='PART_ContentPresenter'
                            Content='{TemplateBinding Content}'
                            ContentTemplate='{TemplateBinding ContentTemplate}'
                            Margin='4,0,0,0'
                            VerticalAlignment='Center'
                            Grid.Column='1'/>
        </Grid>
      </ControlTemplate>
    </Setter>
  </Style>
</Styles>
";
                var styles = AvaloniaXamlLoader.Parse<Styles>(xaml);

                Assert.Equal(1, styles.Count);

                var style = (Style)styles[0];

                var setters = style.Setters.Cast<Setter>().ToArray();

                Assert.Equal(3, setters.Length);

                Assert.Equal(CheckBox.BorderBrushProperty, setters[0].Property);
                Assert.Equal(CheckBox.BorderThicknessProperty, setters[1].Property);
                Assert.Equal(CheckBox.TemplateProperty, setters[2].Property);

                Assert.IsType<StyleResourceBinding>(setters[0].Value);
                Assert.IsType<StyleResourceBinding>(setters[1].Value);
                Assert.IsType<ControlTemplate>(setters[2].Value);
            }
        }

        [Fact]
        public void Style_Resources_Are_Build()
        {
            var xaml = @"
<Style xmlns='https://github.com/avaloniaui'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
        xmlns:sys='clr-namespace:System;assembly=mscorlib'>
    <Style.Resources>
        <SolidColorBrush x:Key='Brush'>White</SolidColorBrush>
        <sys:Double x:Key='Double'>10</sys:Double>
    </Style.Resources>
</Style>";

            var style = AvaloniaXamlLoader.Parse<Style>(xaml);

            Assert.True(style.Resources.Count > 0);

            var brush = style.FindResource("Brush") as SolidColorBrush;

            Assert.NotNull(brush);

            Assert.Equal(Colors.White, brush.Color);

            var d = (double)style.FindResource("Double");

            Assert.Equal(10.0, d);
        }

        [Fact]
        public void StyleInclude_Is_Build()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Styles xmlns='https://github.com/avaloniaui'
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <StyleInclude Source='resm:Avalonia.Themes.Default.ContextMenu.xaml?assembly=Avalonia.Themes.Default'/>
</Styles>";

                var styles = AvaloniaXamlLoader.Parse<Styles>(xaml);

                Assert.True(styles.Count == 1);

                var styleInclude = styles.First() as StyleInclude;

                Assert.NotNull(styleInclude);

                var style = styleInclude.Loaded;

                Assert.NotNull(style);
            }
        }

        [Fact]
        public void Simple_Xaml_Binding_Is_Operational()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformWrapper
                                    .With(windowingPlatform: new MockWindowingPlatform())))
            {
                var xaml =
@"<Window xmlns='https://github.com/avaloniaui' Content='{Binding}'/>";

                var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

                Assert.Null(target.Content);

                target.DataContext = "Foo";

                Assert.Equal("Foo", target.Content);
            }
        }

        [Fact]
        public void Xaml_Binding_Is_Delayed()
        {
            using (UnitTestApplication.Start(TestServices.MockWindowingPlatform))
            {
                var xaml =
@"<ContentControl xmlns='https://github.com/avaloniaui' Content='{Binding}'/>";

                var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

                Assert.Null(target.Content);

                target.DataContext = "Foo";

                Assert.Null(target.Content);

                DelayedBinding.ApplyBindings(target);

                Assert.Equal("Foo", target.Content);
            }
        }

        [Fact]
        public void Double_Xaml_Binding_Is_Operational()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformWrapper
                                    .With(windowingPlatform: new MockWindowingPlatform())))
            {
                var xaml =
@"<Window xmlns='https://github.com/avaloniaui' Width='{Binding}'/>";

                var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

                Assert.Null(target.Content);

                target.DataContext = 55.0;

                Assert.Equal(55.0, target.Width);
            }
        }

        [Fact]
        public void Collection_Xaml_Binding_Is_Operational()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformWrapper
                                    .With(windowingPlatform: new MockWindowingPlatform())))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'>
    <ItemsControl Name='itemsControl' Items='{Binding}'>
    </ItemsControl>
</Window>
";

                var target = AvaloniaXamlLoader.Parse<Window>(xaml);

                Assert.NotNull(target.Content);

                var itemsControl = target.FindControl<ItemsControl>("itemsControl");

                var items = new string[] { "Foo", "Bar" };

                //DelayedBinding.ApplyBindings(itemsControl);

                target.DataContext = items;

                Assert.Equal(items, itemsControl.Items);
            }
        }

        [Fact]
        public void Multi_Xaml_Binding_Is_Parsed()
        {
            var xaml =
@"<MultiBinding xmlns='https://github.com/avaloniaui'
        Converter='{Static BoolConverters.And}'>
     <Binding Path='Foo' />
     <Binding Path='Bar' />
</MultiBinding>";

            var target = AvaloniaXamlLoader.Parse<MultiBinding>(xaml);

            Assert.Equal(2, target.Bindings.Count);

            Assert.Equal(BoolConverters.And, target.Converter);

            var bindings = target.Bindings.Cast<Binding>().ToArray();

            Assert.Equal("Foo", bindings[0].Path);
            Assert.Equal("Bar", bindings[1].Path);
        }

        [Fact]
        public void Control_Template_Is_Operational()
        {
            using (UnitTestApplication.Start(TestServices.MockPlatformWrapper
                                    .With(windowingPlatform: new MockWindowingPlatform())))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Window.Template>
        <ControlTemplate>
            <ContentPresenter Name='PART_ContentPresenter'
                        Content='{TemplateBinding Content}'/>
        </ControlTemplate>
    </Window.Template>
</Window>";

                var target = AvaloniaXamlLoader.Parse<ContentControl>(xaml);

                Assert.NotNull(target.Template);

                Assert.Null(target.Presenter);

                target.ApplyTemplate();

                Assert.NotNull(target.Presenter);

                target.Content = "Foo";

                Assert.Equal("Foo", target.Presenter.Content);
            }
        }

        [Fact]
        public void Style_ControlTemplate_Is_Build()
        {
            var xaml = @"
<Style xmlns='https://github.com/avaloniaui' Selector='ContentControl'>
  <Setter Property='Template'>
     <ControlTemplate>
        <ContentPresenter Name='PART_ContentPresenter'
                       Content='{TemplateBinding Content}'
                       ContentTemplate='{TemplateBinding ContentTemplate}' />
      </ControlTemplate>
  </Setter>
</Style> ";

            var style = AvaloniaXamlLoader.Parse<Style>(xaml);

            Assert.Equal(1, style.Setters.Count());

            var setter = (Setter)style.Setters.First();

            Assert.Equal(ContentControl.TemplateProperty, setter.Property);

            Assert.IsType<ControlTemplate>(setter.Value);

            var template = (ControlTemplate)setter.Value;

            var control = new ContentControl();

            var result = (ContentPresenter)template.Build(control);

            Assert.NotNull(result);
        }

        [Fact]
        public void Named_Control_Is_Added_To_NameScope()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
    <Button Name='button'>Foo</Button>
</Window>";

                var window = AvaloniaXamlLoader.Parse<Window>(xaml);
                var button = window.FindControl<Button>("button");

                Assert.Equal("Foo", button.Content);
            }
        }

#if OMNIXAML
        [Fact]
#else
        [Fact(Skip =
@"Doesn't work with Portable.xaml, it's working in different creation order -
Handled in test 'Control_Is_Added_To_Parent_Before_Final_EndEdit'
do we need it?")]
#endif
        public void Control_Is_Added_To_Parent_Before_Properties_Are_Set()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
             xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'>
    <local:InitializationOrderTracker Width='100'/>
</Window>";

                var window = AvaloniaXamlLoader.Parse<Window>(xaml);
                var tracker = (InitializationOrderTracker)window.Content;

                var attached = tracker.Order.IndexOf("AttachedToLogicalTree");
                var widthChanged = tracker.Order.IndexOf("Property Width Changed");

                Assert.NotEqual(-1, attached);
                Assert.NotEqual(-1, widthChanged);
                Assert.True(attached < widthChanged);
            }
        }

        [Fact]
        public void Control_Is_Added_To_Parent_Before_Final_EndEdit()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
             xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'>
    <local:InitializationOrderTracker Width='100'/>
</Window>";

                var window = AvaloniaXamlLoader.Parse<Window>(xaml);
                var tracker = (InitializationOrderTracker)window.Content;

                var attached = tracker.Order.IndexOf("AttachedToLogicalTree");
                var endInit = tracker.Order.IndexOf("EndInit 0");

                Assert.NotEqual(-1, attached);
                Assert.NotEqual(-1, endInit);
                Assert.True(attached < endInit);
            }
        }

        [Fact]
        public void All_Properties_Are_Set_Before_Final_EndEdit()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'>
    <Window.Styles>
        <Style>
            <Style.Resources>
                <x:Double x:Key='Double'>100</x:Double>
            </Style.Resources>
        </Style>
    </Window.Styles>
    <local:InitializationOrderTracker Width='100' Height='{StyleResource Double}'
                     Tag='{Binding Height, RelativeSource={RelativeSource Self}}' />
</Window>";


                var window = AvaloniaXamlLoader.Parse<Window>(xaml);
                var tracker = (InitializationOrderTracker)window.Content;

                //ensure binding is set and operational first
                Assert.Equal(100.0, tracker.Tag);

                Assert.Equal("EndInit 0", tracker.Order.Last());
            }
        }

        [Fact]
        public void BeginInit_Matches_EndInit()
        {
            using (UnitTestApplication.Start(TestServices.StyledWindow))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'
             xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
             xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'>
    <local:InitializationOrderTracker />
</Window>";

                var window = AvaloniaXamlLoader.Parse<Window>(xaml);
                var tracker = (InitializationOrderTracker)window.Content;

                Assert.Equal(0, tracker.InitState);
            }
        }

        [Fact]
        public void DeferedXamlLoader_Should_Preserve_NamespacesContext()
        {
            var xaml =
@"<ContentControl xmlns='https://github.com/avaloniaui'
            xmlns:local='clr-namespace:Avalonia.Markup.Xaml.UnitTests.Xaml;assembly=Avalonia.Markup.Xaml.UnitTests'>
    <ContentControl.ContentTemplate>
        <DataTemplate>
            <TextBlock  Tag='{Static local:NonControl.StringProperty}'/>
        </DataTemplate>
    </ContentControl.ContentTemplate>
</ContentControl>";

            var contentControl = AvaloniaXamlLoader.Parse<ContentControl>(xaml);
            var template = contentControl.ContentTemplate;

            Assert.NotNull(template);

            var txt = (TextBlock)template.Build(null);

            Assert.Equal((object)NonControl.StringProperty, txt.Tag);
        }

        [Fact]
        public void Binding_To_List_AvaloniaProperty_Is_Operational()
        {
            using (UnitTestApplication.Start(TestServices.MockWindowingPlatform))
            {
                var xaml = @"
<Window xmlns='https://github.com/avaloniaui'>
    <ListBox Items='{Binding Items}' SelectedItems='{Binding SelectedItems}'/>
</Window>";

                var window = AvaloniaXamlLoader.Parse<Window>(xaml);
                var listBox = (ListBox)window.Content;

                var vm = new SelectedItemsViewModel()
                {
                    Items = new string[] { "foo", "bar", "baz" }
                };

                window.DataContext = vm;

                Assert.Equal(vm.Items, listBox.Items);

                Assert.Equal(vm.SelectedItems, listBox.SelectedItems);
            }
        }

        private class SelectedItemsViewModel : INotifyPropertyChanged
        {
            public string[] Items { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            private IList _selectedItems = new AvaloniaList<string>();

            public IList SelectedItems
            {
                get { return _selectedItems; }
                set
                {
                    _selectedItems = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItems)));
                }
            }
        }
    }
}