# SESSION 5B — Ribbon Button + Settings Window
**يُنفَّذ بعد Session 5 (Theme Manager) وقبل Session 6 (Migration)**

---

```
أنت تعمل على مشروع VSTO Add-in اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
المشروع فيه جزئين:
- VSTO Project: الـ Add-in نفسه (فيه الـ Ribbon و ThisAddIn.cs)
- WpfApp2: الـ WPF UI Project

## السياق
المراحل 1-5 مكتملة:
- Design Tokens جاهزة في Theme\Base\
- ThemeManager.cs موجود في WpfApp2\Services\
- ThemeSettings.cs موجود مع حفظ JSON

## المهمة
3 خطوات مترابطة:
1. إنشاء زر "addin_settings" في الـ Ribbon
2. إنشاء SettingsWindow في WpfApp2
3. ربط الزر بالـ Window

---

## قبل البدء: اقرأ هذه الملفات أولاً

اقرأ كل الملفات دي قبل ما تكتب أي كود:
- الـ VSTO Project كاملاً (ابحث عن ملفات .xml أو .cs فيها كلمة Ribbon)
- الـ Ribbon XML file (ممكن اسمه Ribbon1.xml أو CustomUI.xml أو مشابه)
- ThisAddIn.cs
- WpfApp2\Services\ThemeManager.cs
- WpfApp2\Services\ThemeSettings.cs
- WpfApp2\Theme\Base\Colors.xaml (للـ Token names)
- WpfApp2\App.xaml

---

## الخطوة 1: إضافة زر في الـ Ribbon (VSTO Project)

### ابحث عن ملف الـ Ribbon أولاً
في VSTO، الـ Ribbon ممكن يكون:
- ملف XML (Ribbon1.xml أو CustomUI.xml) → نضيف فيه button
- ملف Designer (Ribbon1.cs + Ribbon1.Designer.cs) → نضيف control
- أو فئة ترث من RibbonBase

### لو الـ Ribbon XML:
أضف في الـ Group المناسب (أو أنشئ Group جديد "Som3a"):

```xml
<group id="grpSom3aMain" label="Som3a">

  <!-- الأزرار الموجودة — لا تمسها -->

  <!-- زر جديد -->
  <button id="btnAddinSettings"
          label="الإعدادات"
          screentip="إعدادات Som3a Addin"
          supertip="تغيير المظهر وإعدادات الأداة"
          size="large"
          imageMso="PropertySheet"
          onAction="btnAddinSettings_Click"/>
</group>
```

### لو الـ Ribbon Designer:
أضف RibbonButton بـ:
- Name: btnAddinSettings
- Label: "الإعدادات"
- OfficeImageId: "PropertySheet"
- Size: RibbonControlSize.RibbonControlSizeLarge

### في Ribbon1.cs (أو الـ Code-behind للـ Ribbon):
```csharp
private void btnAddinSettings_Click(Office.IRibbonControl control)
{
    Som3aAddinBridge.OpenSettings();
}
```

---

## الخطوة 2: إنشاء Bridge بين VSTO و WPF

### VSTO Project \ Som3aAddinBridge.cs
(لأن VSTO مش بيعرف WPF مباشرة)

```csharp
using System.Windows;
using WpfApp2.Views; // أو الـ namespace الصحيح لـ SettingsWindow

namespace Som3a_Addin_2026 // نفس namespace الـ VSTO project
{
    public static class Som3aAddinBridge
    {
        private static System.Windows.Application _wpfApp;

        /// <summary>
        /// يُستدعى مرة واحدة من ThisAddIn_Startup
        /// </summary>
        public static void Initialize()
        {
            // WPF Application بيحتاج STA thread
            // لو WpfApp2 مش initialized بعد، نعمل Application object
            if (System.Windows.Application.Current == null)
            {
                _wpfApp = new System.Windows.Application();
                _wpfApp.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }

            // حمّل الـ Theme settings المحفوظة
            WpfApp2.Services.ThemeManager.LoadSettings();
        }

        public static void OpenSettings()
        {
            var win = new SettingsWindow();
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            win.ShowDialog();
        }

        public static void Shutdown()
        {
            _wpfApp?.Shutdown();
        }
    }
}
```

### تعديل ThisAddIn.cs:
```csharp
private void ThisAddIn_Startup(object sender, EventArgs e)
{
    // الكود الموجود — لا تمسحه
    // ...

    // إضافة في نهاية الـ Startup:
    Som3aAddinBridge.Initialize();
}

private void ThisAddIn_Shutdown(object sender, EventArgs e)
{
    // الكود الموجود — لا تمسحه
    // ...

    Som3aAddinBridge.Shutdown();
}
```

> **ملاحظة:** لو WpfApp2 هو نفسه موجود كـ Referenced Project في الـ VSTO Solution، فالـ Bridge بسيط ومباشر. لو منفصل تماماً كـ DLL، أخبرني وسنتعامل معه بشكل مختلف.

---

## الخطوة 3: إنشاء SettingsWindow في WpfApp2

### WpfApp2\Views\SettingsWindow.xaml

Window properties:
- Width: 480
- Height: 560
- WindowStartupLocation: CenterScreen
- ResizeMode: NoResize
- WindowStyle: None (ترث من ModernWindow لو المرحلة 4 اكتملت)

#### هيكل الـ UI:

```
┌─────────────────────────────────────┐
│  ⚙  إعدادات Som3a Addin        ✕   │  ← Header (ModernWindow chrome)
├─────────────────────────────────────┤
│                                     │
│  المظهر                             │  ← Section Header
│  ┌─────────────────────────────┐    │
│  │  ◉ داكن (Dark Blue)         │    │  ← RadioButton
│  │  ○ فاتح (White)             │    │  ← RadioButton
│  └─────────────────────────────┘    │
│                                     │
│  لون التمييز (Accent)               │  ← Section Header
│  ┌──┐ ┌──┐ ┌──┐ ┌──┐ ┌──┐         │  ← 5 Color Circles
│  │  │ │  │ │  │ │  │ │  │         │
│  └──┘ └──┘ └──┘ └──┘ └──┘         │
│                                     │
│  معاينة                             │  ← Section Header
│  ┌─────────────────────────────┐    │
│  │  [Primary Button] [Ghost]   │    │  ← Preview card
│  │  [_________________]        │    │  ← TextBox preview
│  └─────────────────────────────┘    │
│                                     │
│  الأداء                             │  ← Section Header
│  □ وضع الأداء العالي               │  ← CheckBox (تعطيل animations)
│    (مناسب للأجهزة البطيئة)          │
│                                     │
│         [حفظ]      [إلغاء]          │  ← Footer buttons
└─────────────────────────────────────┘
```

### WpfApp2\Views\SettingsWindow.xaml (الكود الكامل):

```xml
<controls:ModernWindow
    x:Class="Som3a_WPF_UI.Views.SettingsWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:Som3a_WPF_UI.Controls"
    Title="الإعدادات"
    Width="480" Height="560"
    ResizeMode="NoResize"
    FlowDirection="RightToLeft">

    <Grid Margin="{DynamicResource SectionPadding}">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>  <!-- المظهر -->
            <RowDefinition Height="Auto"/>  <!-- Accent -->
            <RowDefinition Height="Auto"/>  <!-- Preview -->
            <RowDefinition Height="Auto"/>  <!-- الأداء -->
            <RowDefinition Height="*"/>     <!-- Spacer -->
            <RowDefinition Height="Auto"/>  <!-- Buttons -->
        </Grid.RowDefinitions>

        <!-- ===== Section 1: المظهر ===== -->
        <StackPanel Grid.Row="0" Margin="0,0,0,20">
            <TextBlock Text="المظهر"
                       FontSize="{DynamicResource SubHeaderFontSize}"
                       FontWeight="SemiBold"
                       Foreground="{DynamicResource TextMainBrush}"
                       Margin="0,0,0,10"/>

            <Border Background="{DynamicResource CardBrush}"
                    BorderBrush="{DynamicResource CardStrokeBrush}"
                    BorderThickness="1"
                    CornerRadius="{DynamicResource MediumRadius}"
                    Padding="16,12">
                <StackPanel>
                    <RadioButton x:Name="rbDark"
                                 Content="داكن (Dark Blue)"
                                 GroupName="ThemeGroup"
                                 Foreground="{DynamicResource TextMainBrush}"
                                 Margin="0,0,0,8"
                                 IsChecked="True"/>
                    <RadioButton x:Name="rbWhite"
                                 Content="فاتح (White)"
                                 GroupName="ThemeGroup"
                                 Foreground="{DynamicResource TextMainBrush}"/>
                </StackPanel>
            </Border>
        </StackPanel>

        <!-- ===== Section 2: Accent Color ===== -->
        <StackPanel Grid.Row="1" Margin="0,0,0,20">
            <TextBlock Text="لون التمييز"
                       FontSize="{DynamicResource SubHeaderFontSize}"
                       FontWeight="SemiBold"
                       Foreground="{DynamicResource TextMainBrush}"
                       Margin="0,0,0,10"/>

            <Border Background="{DynamicResource CardBrush}"
                    BorderBrush="{DynamicResource CardStrokeBrush}"
                    BorderThickness="1"
                    CornerRadius="{DynamicResource MediumRadius}"
                    Padding="16,12">
                <ItemsControl x:Name="AccentColors">
                    <ItemsControl.ItemsPanel>
                        <ItemsPanelTemplate>
                            <WrapPanel Orientation="Horizontal" ItemWidth="44"/>
                        </ItemsPanelTemplate>
                    </ItemsControl.ItemsPanel>
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <!-- كل دايرة لون -->
                            <Button Width="36" Height="36"
                                    Margin="4"
                                    Tag="{Binding Hex}"
                                    Click="AccentColor_Click"
                                    Style="{DynamicResource IconButton}"
                                    ToolTip="{Binding Name}">
                                <Button.Template>
                                    <ControlTemplate TargetType="Button">
                                        <Grid>
                                            <Ellipse Fill="{Binding Brush}"
                                                     Width="32" Height="32"/>
                                            <!-- Check mark لو selected -->
                                            <TextBlock Text="✓"
                                                       Foreground="White"
                                                       FontSize="14"
                                                       HorizontalAlignment="Center"
                                                       VerticalAlignment="Center"
                                                       Visibility="{Binding IsSelected,
                                                           Converter={DynamicResource BoolToVisibilityConverter}}"/>
                                        </Grid>
                                    </ControlTemplate>
                                </Button.Template>
                            </Button>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </Border>
        </StackPanel>

        <!-- ===== Section 3: Preview ===== -->
        <StackPanel Grid.Row="2" Margin="0,0,0,20">
            <TextBlock Text="معاينة"
                       FontSize="{DynamicResource SubHeaderFontSize}"
                       FontWeight="SemiBold"
                       Foreground="{DynamicResource TextMainBrush}"
                       Margin="0,0,0,10"/>

            <Border Background="{DynamicResource CardBrush}"
                    BorderBrush="{DynamicResource CardStrokeBrush}"
                    BorderThickness="1"
                    CornerRadius="{DynamicResource MediumRadius}"
                    Padding="16,12">
                <StackPanel>
                    <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                        <Button Content="حفظ" Style="{DynamicResource PrimaryButton}" Margin="0,0,8,0"/>
                        <Button Content="إلغاء" Style="{DynamicResource GhostButton}"/>
                    </StackPanel>
                    <TextBox Text="مثال على حقل النص..."
                             IsReadOnly="True"/>
                </StackPanel>
            </Border>
        </StackPanel>

        <!-- ===== Section 4: الأداء ===== -->
        <StackPanel Grid.Row="3" Margin="0,0,0,20">
            <TextBlock Text="الأداء"
                       FontSize="{DynamicResource SubHeaderFontSize}"
                       FontWeight="SemiBold"
                       Foreground="{DynamicResource TextMainBrush}"
                       Margin="0,0,0,10"/>

            <Border Background="{DynamicResource CardBrush}"
                    BorderBrush="{DynamicResource CardStrokeBrush}"
                    BorderThickness="1"
                    CornerRadius="{DynamicResource MediumRadius}"
                    Padding="16,12">
                <StackPanel>
                    <CheckBox x:Name="chkHighPerf"
                              Content="وضع الأداء العالي"
                              Foreground="{DynamicResource TextMainBrush}"/>
                    <TextBlock Text="يعطّل التأثيرات البصرية لأجهزة بطيئة"
                               Foreground="{DynamicResource TextSubBrush}"
                               FontSize="{DynamicResource CaptionFontSize}"
                               Margin="20,4,0,0"/>
                </StackPanel>
            </Border>
        </StackPanel>

        <!-- ===== Footer Buttons ===== -->
        <StackPanel Grid.Row="5"
                    Orientation="Horizontal"
                    HorizontalAlignment="Left"
                    Margin="0,8,0,0">
            <Button Content="حفظ وإغلاق"
                    Style="{DynamicResource PrimaryButton}"
                    Width="120"
                    Click="Save_Click"
                    Margin="0,0,12,0"/>
            <Button Content="إلغاء"
                    Style="{DynamicResource GhostButton}"
                    Width="80"
                    Click="Cancel_Click"/>
        </StackPanel>
    </Grid>
</controls:ModernWindow>
```

---

### WpfApp2\Views\SettingsWindow.xaml.cs

```csharp
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.Views
{
    public partial class SettingsWindow : ModernWindow // أو : Window لو ModernWindow مش جاهز
    {
        // الألوان المتاحة للاختيار
        private readonly List<AccentColorOption> _accentOptions = new List<AccentColorOption>
        {
            new AccentColorOption { Name = "أزرق",    Hex = "#3A86FF" },
            new AccentColorOption { Name = "سماوي",   Hex = "#00B4D8" },
            new AccentColorOption { Name = "أخضر",    Hex = "#2ED573" },
            new AccentColorOption { Name = "بنفسجي",  Hex = "#9B59B6" },
            new AccentColorOption { Name = "برتقالي", Hex = "#FF6B35" },
            new AccentColorOption { Name = "وردي",    Hex = "#FF4D8D" },
            new AccentColorOption { Name = "أصفر",    Hex = "#FFA502" },
            new AccentColorOption { Name = "رمادي",   Hex = "#6C757D" },
        };

        private string _selectedAccent;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var settings = ThemeManager.GetCurrentSettings();

            // تحديد الثيم الحالي
            rbDark.IsChecked  = settings.CurrentTheme == ThemeType.FluentDarkBlue;
            rbWhite.IsChecked = settings.CurrentTheme == ThemeType.FluentWhite;

            // تحديد الـ Accent الحالي
            _selectedAccent = settings.AccentColor;

            // تحديث IsSelected لكل لون
            foreach (var opt in _accentOptions)
                opt.IsSelected = opt.Hex.Equals(_selectedAccent,
                    System.StringComparison.OrdinalIgnoreCase);

            // ربط الألوان بالـ ItemsControl
            AccentColors.ItemsSource = _accentOptions;

            // الأداء
            chkHighPerf.IsChecked = settings.HighPerformance;
        }

        private void AccentColor_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn?.Tag is string hex)
            {
                _selectedAccent = hex;

                // تحديث IsSelected
                foreach (var opt in _accentOptions)
                    opt.IsSelected = opt.Hex.Equals(hex,
                        System.StringComparison.OrdinalIgnoreCase);

                AccentColors.Items.Refresh();

                // معاينة فورية
                ThemeManager.ChangeAccent(hex);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // تطبيق الثيم
            var theme = rbWhite.IsChecked == true
                ? ThemeType.FluentWhite
                : ThemeType.FluentDarkBlue;

            ThemeManager.ApplyTheme(theme);
            ThemeManager.ChangeAccent(_selectedAccent);

            // الأداء
            ThemeManager.SetHighPerformance(chkHighPerf.IsChecked == true);

            // حفظ
            ThemeManager.SaveSettings();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // إرجاع الـ Accent للقديم لو تغيّر في المعاينة
            var settings = ThemeManager.GetCurrentSettings();
            ThemeManager.ChangeAccent(settings.AccentColor);

            DialogResult = false;
            Close();
        }
    }

    // Model للألوان
    public class AccentColorOption : System.ComponentModel.INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Hex  { get; set; }

        public SolidColorBrush Brush =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(Hex));

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
```

---

## الخطوة 4: تحديث ThemeManager

أضف method مفقودة في ThemeManager.cs:

```csharp
public static void SetHighPerformance(bool enabled)
{
    _current.HighPerformance = enabled;

    if (enabled)
    {
        // تعطيل الـ Effects
        SetResource("CardShadow",   null);
        SetResource("FocusGlow",    null);
        SetResource("WindowShadow", null);
    }
    else
    {
        // إعادة تفعيل الـ Effects (لو Session 7 اكتملت)
        // يعيد تحميل FluentEffects.xaml
    }
}
```

---

## قائمة التحقق الكاملة

### الـ Ribbon:
```
□ زر "الإعدادات" ظاهر في الـ Excel Ribbon
□ الزر فيه Icon مناسب (PropertySheet أو SettingsPage)
□ الضغط عليه بيفتح SettingsWindow
□ لو Excel مش شغال (أي وضع تصميم) البناء يكمل بدون خطأ
```

### الـ SettingsWindow:
```
□ الـ Window بتفتح وبتتغلق
□ الـ RadioButtons بيعكسوا الثيم الحالي صح
□ الـ Accent circles كلها ظاهرة بألوانها
□ الـ Circle المختارة فيها checkmark ✓
□ الضغط على circle يغير الـ Accent فوراً (معاينة)
□ الـ Preview section بيتحدث مع التغيير
□ زر "حفظ" بيحفظ ويغلق
□ زر "إلغاء" بيرجع الأكسنت للقديم ويغلق
□ الإعدادات بتتحفظ في AppData\Som3a\theme.json
□ بعد Restart، الإعدادات المحفوظة بتتطبق تلقائياً
```

### الـ Bridge:
```
□ ThemeManager.LoadSettings() بيتشغل في ThisAddIn_Startup
□ لا يوجد NullReferenceException عند فتح الـ Window
□ Build بدون أي خطأ في الـ VSTO Project والـ WPF Project
```

---

## مشاكل شائعة وحلولها

### مشكلة: "Cannot find resource 'ModernWindow'"
**الحل:** في السطر الأول من SettingsWindow.xaml، غير `controls:ModernWindow` إلى `Window` مؤقتاً لو Session 4 مش اكتملت بعد.

### مشكلة: STA Thread Exception
**الحل:** في Som3aAddinBridge.OpenSettings:
```csharp
public static void OpenSettings()
{
    var thread = new System.Threading.Thread(() =>
    {
        var win = new SettingsWindow();
        win.ShowDialog();
    });
    thread.SetApartmentState(System.Threading.ApartmentState.STA);
    thread.Start();
}
```

### مشكلة: SettingsWindow مش شايف WpfApp2
**الحل:** تأكد إن WpfApp2 مضاف كـ Project Reference في الـ VSTO Project.
في Solution Explorer: VSTO Project → References → Add Reference → Projects → WpfApp2

### مشكلة: Ribbon XML مش بيعرف يلاقي الـ Method
**الحل:** تأكد إن اسم الـ method في `onAction="..."` مطابق تماماً للـ method في الـ Ribbon class.

---

## ملاحظة مهمة للـ VSTO + WPF

الـ VSTO بيشتغل على STA COM Thread، والـ WPF بيحتاج STA Thread كمان.
في الغالب هيشتغل مباشرة. لو حصل أي مشكلة في الـ Threading، أخبرني بالـ Exception الكاملة.

---

*Session 5B · Som3a Addin 2026 · Ribbon + Settings*
```
