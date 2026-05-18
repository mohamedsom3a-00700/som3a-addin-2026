# Som3a Addin 2026 — Claude Code Session Prompts
**9 Sessions · كل session مستقلة وقابلة للتنفيذ بشكل منفصل**

---

## كيفية الاستخدام

1. افتح Claude Code في مجلد المشروع:
   ```bash
   cd "C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026"
   claude
   ```
2. انسخ الـ Session Prompt الخاص بالمرحلة المطلوبة بالكامل
3. الصقه كأول رسالة في الـ session
4. Claude Code سيطلب الملفات اللي يحتاجها ويبدأ التنفيذ

> **قاعدة ثابتة في كل session:** لا hardcoded colors، لا StaticResource للـ tokens، لا Styles داخل Window.Resources.

---

---

# SESSION 1 — Design Tokens & Base Resources

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project اسمه WpfApp2 وموجود في: WpfApp2\

## المهمة
إنشاء نظام Design Tokens كـ XAML ResourceDictionaries منفصلة.
هذه المرحلة هي الأساس الذي تعتمد عليه كل المراحل التالية.

## الملفات المطلوب إنشاؤها

### 1. WpfApp2\Theme\Base\Colors.xaml
ResourceDictionary يحتوي على:

الألوان (Color):
- AccentColor: #3A86FF
- BackgroundColor: #0E1720
- CardColor: #15202B
- SurfaceColor: #1C2B3A
- TextMainColor: #F2FFFFFF
- TextSubColor: #BFFFFFFF
- TextDisabledColor: #66FFFFFF
- SuccessColor: #2ED573
- WarningColor: #FFA502
- DangerColor: #FF4757
- InfoColor: #1E90FF
- BorderColor: #33FFFFFF
- ControlBgColor: #330E1720

الـ Brushes المشتقة منها (SolidColorBrush):
- AccentBrush ← AccentColor
- BackgroundBrush ← BackgroundColor
- CardBrush ← CardColor
- SurfaceBrush ← SurfaceColor
- TextMainBrush ← TextMainColor
- TextSubBrush ← TextSubColor
- TextDisabledBrush ← TextDisabledColor
- SuccessBrush ← SuccessColor
- WarningBrush ← WarningColor
- DangerBrush ← DangerColor
- InfoBrush ← InfoColor
- CardStrokeBrush ← BorderColor
- ControlBgBrush ← ControlBgColor
- ControlStrokeBrush ← BorderColor

### 2. WpfApp2\Theme\Base\Typography.xaml
ResourceDictionary يحتوي على:
- FontFamilyPrimary: "Segoe UI"
- TitleFontSize: 20 (System:Double)
- HeaderFontSize: 16
- SubHeaderFontSize: 14
- BodyFontSize: 13
- CaptionFontSize: 11
- FontWeightNormal: Normal (FontWeight)
- FontWeightMedium: Medium
- FontWeightBold: Bold

### 3. WpfApp2\Theme\Base\Spacing.xaml
ResourceDictionary يحتوي على:
- ButtonPadding: 12,6 (Thickness)
- CardPadding: 16,12
- InputPadding: 10,6
- SectionPadding: 20,16
- ControlHeight: 30 (System:Double)
- ButtonHeight: 32
- HeaderHeight: 48
- IconSize: 16

### 4. WpfApp2\Theme\Base\Radius.xaml
ResourceDictionary يحتوي على:
- SmallRadius: 6 (CornerRadius)
- MediumRadius: 10
- LargeRadius: 14
- CardRadius: 12
- WindowRadius: 16
- PillRadius: 20

## تعديل ملف موجود

### WpfApp2\Theme\App.xaml (أو ما يعادله في المشروع)
أضف MergedDictionaries بالترتيب التالي (الترتيب مهم جداً):
1. Colors.xaml أولاً
2. Typography.xaml
3. Spacing.xaml
4. Radius.xaml
5. ثم أي ملفات كانت موجودة مسبقاً (DarkTheme.xaml, ModernDashboard.xaml)

## قواعد صارمة
- كل قيمة Color تُعرَّف مرة واحدة فقط والـ Brush تشتق منها
- لا تستخدم StaticResource داخل هذه الملفات لأن الـ Brushes ستُستخدم كـ DynamicResource لاحقاً
- استخدم xmlns:sys="clr-namespace:System;assembly=mscorlib" للـ Double و FontWeight
- استخدم xmlns:sys أيضاً لـ CornerRadius أو System.Windows

## التحقق من النجاح
بعد الانتهاء:
1. شغّل Build وتأكد من 0 errors
2. تحقق أن App.xaml يحمّل الملفات بالترتيب الصحيح
3. أرسل لي محتوى كل ملف من الأربعة للمراجعة

## ممنوع
- لا تعدّل أي Window موجودة في هذه المرحلة
- لا تحذف أي ملف موجود
- لا تغير DarkTheme.xaml أو ModernDashboard.xaml
```

---

---

# SESSION 2 — Shared Converters

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
المرحلة 1 مكتملة — ملفات Base Tokens موجودة في Theme\Base\

## المشكلة الحالية
الـ Converters التالية معرّفة بشكل مكرر داخل Window.Resources في أكثر من Window:
- ProgressWidthConverter: موجود في MainWindow.xaml و LinksManagerWindow.xaml
- PercentToScaleConverter: موجود في MainWindow.xaml و LinksManagerWindow.xaml

## المهمة
إنشاء ملف Converters موحد وإزالة التعريفات المكررة.

## الخطوات المطلوبة

### الخطوة 1: اقرأ هذه الملفات أولاً
- WpfApp2\Converters\ (المجلد كاملاً)
- WpfApp2\MainWindow.xaml (Window.Resources فقط)
- WpfApp2\LinksManagerWindow.xaml (Window.Resources فقط)
- WpfApp2\Views\LinksManagerWindow.xaml (لو موجود)
- أي Window أخرى قد تحتوي Converters

### الخطوة 2: أنشئ WpfApp2\Converters\SharedConverters.cs
يحتوي على كل الـ Converters المجموعة من الملفات:

```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Som3a_WPF_UI.Converters
{
    // ProgressWidthConverter — يحسب عرض progress bar بناءً على النسبة المئوية
    public class ProgressWidthConverter : IMultiValueConverter { ... }

    // PercentToScaleConverter
    public class PercentToScaleConverter : IMultiValueConverter { ... }

    // BoolToVisibilityConverter
    public class BoolToVisibilityConverter : IValueConverter { ... }

    // InverseBoolToVisibilityConverter
    public class InverseBoolToVisibilityConverter : IValueConverter { ... }

    // NullToVisibilityConverter
    public class NullToVisibilityConverter : IValueConverter { ... }

    // InverseBoolConverter
    public class InverseBoolConverter : IValueConverter { ... }

    // StringToVisibilityConverter (تحويل string فارغ إلى Collapsed)
    public class StringToVisibilityConverter : IValueConverter { ... }
}
```

### الخطوة 3: سجّل الـ Converters في App.xaml
أضف في Application.Resources (بعد الـ MergedDictionaries):
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- الملفات من المرحلة 1 -->
        </ResourceDictionary.MergedDictionaries>

        <!-- Shared Converters -->
        <conv:ProgressWidthConverter x:Key="ProgressWidthConverter"/>
        <conv:PercentToScaleConverter x:Key="PercentToScaleConverter"/>
        <conv:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
        <conv:InverseBoolToVisibilityConverter x:Key="InverseBoolToVisibilityConverter"/>
        <conv:NullToVisibilityConverter x:Key="NullToVisibilityConverter"/>
        <conv:InverseBoolConverter x:Key="InverseBoolConverter"/>
        <conv:StringToVisibilityConverter x:Key="StringToVisibilityConverter"/>
    </ResourceDictionary>
</Application.Resources>
```

### الخطوة 4: احذف التعريفات المكررة من كل Window
في كل Window تجد فيها تعريف Converter:
- احذف تعريف الـ Converter من Window.Resources
- احذف السطر xmlns للـ Converter لو مش محتاج في حاجة تانية
- تأكد أن الـ Binding references لسا شغالة (مش محتاج تغيير لأن الـ Key نفسه)

## التحقق من النجاح
1. Build بدون أي خطأ
2. لا يوجد ProgressWidthConverter أو PercentToScaleConverter داخل أي Window.Resources
3. الـ Bindings في MainWindow و LinksManager لسا شغالة

## ممنوع
- لا تغير منطق الـ Converters الموجودة، فقط انقلها
- لا تحذف Converter من Window.Resources قبل ما تتأكد أنه متسجل في App.xaml
- لا تغير أي XAML ليس له علاقة بالـ Converters
```

---

---

# SESSION 3 — Global Control Styles

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
المرحلتان 1 و 2 مكتملتان:
- Theme\Base\ فيها Colors, Typography, Spacing, Radius
- Converters\SharedConverters.cs موجود ومسجل في App.xaml

## المهمة
إنشاء Global Control Styles موحدة تعتمد على الـ Design Tokens.

## قبل البدء: اقرأ هذه الملفات
- WpfApp2\Theme\Base\Colors.xaml
- WpfApp2\Theme\Base\Spacing.xaml
- WpfApp2\Theme\Base\Radius.xaml
- WpfApp2\MainWindow.xaml (لفهم الـ Styles الحالية)
- WpfApp2\Theme\ModernDashboard.xaml (لو موجود)

## الملفات المطلوب إنشاؤها في Theme\Controls\

### 1. ButtonStyles.xaml
يحتوي على:

**BaseButton** (Style x:Key="BaseButton" TargetType="Button"):
- Foreground: {DynamicResource TextMainBrush}
- Background: #2FFFFFFF
- BorderBrush: {DynamicResource ControlStrokeBrush}
- BorderThickness: 1
- Padding: {DynamicResource ButtonPadding}
- Height: {DynamicResource ButtonHeight}
- Cursor: Hand
- Template: ControlTemplate مع Border بـ CornerRadius={DynamicResource MediumRadius}
- Triggers: IsMouseOver (أفتح قليلاً)، IsPressed (أغمق)، IsEnabled=False (Opacity 0.5)

**PrimaryButton** (BasedOn BaseButton):
- Background: {DynamicResource AccentBrush}
- BorderBrush: Transparent

**DangerButton** (BasedOn BaseButton):
- Background: {DynamicResource DangerBrush}
- BorderBrush: Transparent

**SuccessButton** (BasedOn BaseButton):
- Background: {DynamicResource SuccessBrush}
- BorderBrush: Transparent

**GhostButton** (BasedOn BaseButton):
- Background: Transparent
- BorderBrush: {DynamicResource ControlStrokeBrush}

**IconButton** (BasedOn BaseButton):
- Width: {DynamicResource ButtonHeight}
- Padding: 0
- Background: Transparent
- BorderBrush: Transparent

### 2. TextBoxStyles.xaml
**Default TextBox Style** (بدون x:Key لتطبيقه تلقائياً):
- Foreground: {DynamicResource TextMainBrush}
- Background: {DynamicResource ControlBgBrush}
- BorderBrush: {DynamicResource ControlStrokeBrush}
- BorderThickness: 1
- Padding: {DynamicResource InputPadding}
- Height: {DynamicResource ControlHeight}
- VerticalContentAlignment: Center
- CaretBrush: {DynamicResource AccentBrush}
- SelectionBrush: {DynamicResource AccentBrush}
- Template: ControlTemplate مع:
  - Border بـ CornerRadius={DynamicResource MediumRadius}
  - PART_ContentHost داخل ScrollViewer
  - Trigger على IsFocused: BorderBrush يتحول لـ AccentBrush

**ReadOnlyTextBox** (x:Key="ReadOnlyTextBox"):
- BasedOn Default
- IsReadOnly: True
- Opacity: 0.7
- Cursor: Arrow

### 3. ComboBoxStyles.xaml
**RoundComboBox** (x:Key="RoundComboBox"):
- نفس dimensions الـ TextBox (Height, Padding, CornerRadius)
- Foreground: {DynamicResource TextMainBrush}
- Background: {DynamicResource ControlBgBrush}
- BorderBrush: {DynamicResource ControlStrokeBrush}
- Template كامل مع:
  - ToggleButton مخصص (بدون الـ default arrow البشع)
  - Popup مع Border دائري
  - ScrollViewer داخل الـ Popup
- ComboBoxItem Style:
  - Hover: Background خفيف
  - Selected: {DynamicResource AccentBrush} بـ opacity منخفض

### 4. DataGridStyles.xaml
**ModernDataGrid** (x:Key="ModernDataGrid"):
- Background: Transparent
- BorderBrush: {DynamicResource ControlStrokeBrush}
- BorderThickness: 1
- RowBackground: Transparent
- AlternatingRowBackground: #0AFFFFFF (خفيف جداً)
- GridLinesVisibility: None (بدون grid lines)
- HeadersVisibility: Column
- ColumnHeaderStyle:
  - Background: {DynamicResource SurfaceBrush}
  - Foreground: {DynamicResource TextSubBrush}
  - BorderBrush: {DynamicResource ControlStrokeBrush}
  - Padding: 8,6
- DataGridRowStyle:
  - Hover: Background #15FFFFFF
  - Selected: Background {DynamicResource AccentBrush} بـ opacity 0.2

### 5. ListViewStyles.xaml
**Default ListView Style** (بدون x:Key):
- Background: Transparent
- BorderBrush: {DynamicResource ControlStrokeBrush}
- ListViewItem Style:
  - Hover: #15FFFFFF
  - Selected: {DynamicResource AccentBrush} بـ opacity 0.25
  - Padding: 6,4
- GridViewColumnHeader Style:
  - Background: {DynamicResource SurfaceBrush}
  - Foreground: {DynamicResource TextSubBrush}
  - Padding: 8,6
  - BorderBrush: {DynamicResource ControlStrokeBrush}

### 6. ScrollBarStyles.xaml
**Slim modern scrollbar** (بدون x:Key — يُطبق على كل ScrollBars):
- Width (Vertical): 6
- Height (Horizontal): 6
- Track: Transparent
- Thumb: #55FFFFFF rounded بـ CornerRadius=3
- Hover Thumb: #88FFFFFF
- بدون Arrow buttons (IsEnabled=False أو Collapsed)

## تعديل App.xaml
أضف في MergedDictionaries بعد Base Tokens:
```xml
<ResourceDictionary Source="Theme/Controls/ButtonStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/TextBoxStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/ComboBoxStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/DataGridStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/ListViewStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/ScrollBarStyles.xaml"/>
```

## قواعد صارمة
- كل قيمة لون أو size لازم تيجي من {DynamicResource} — مفيش hardcoded
- الاستثناء الوحيد: قيم opacity متغيرة زي #2FFFFFFF مقبولة لأنها مش tokens
- كل Style يكون TargetType محدد
- أي Style بـ x:Key مش بيطبق تلقائياً إلا لو استُدعي صراحةً

## التحقق من النجاح
1. Build بدون أي خطأ
2. افتح MainWindow — الـ Buttons والـ TextBoxes لازم تبدو نفس الشكل القديم (مش أحسن منه لأننا لسا في المرحلة دي)
3. تأكد إن الـ ComboBox بيشتغل صح (يفتح ويختار)
4. أرسل screenshot أو وصف للـ visual result

## ممنوع
- لا تمسح Window.Resources من أي Window في هذه المرحلة
- لا تغير ViewModels أو Code-behind
- لا تغير الـ MainWindow.xaml أو أي XAML موجود
```

---

---

# SESSION 3B — Missing Control Styles (الـ Controls الناقصة)
**يُنفَّذ مباشرة بعد Session 3 وقبل Session 4**

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
Session 3 مكتملة — عندنا في Theme\Controls\:
- ButtonStyles.xaml ✓
- TextBoxStyles.xaml ✓
- ComboBoxStyles.xaml ✓
- DataGridStyles.xaml ✓
- ListViewStyles.xaml ✓
- ScrollBarStyles.xaml ✓

## المشكلة
بعد مراجعة الكود الفعلي في كل Window، وُجد أن هذه الـ Controls لها Styles محلية
في كل Window لكن لا يوجد لها Global Style بعد:

| Control | موجود في | الـ Style المحلي |
|---|---|---|
| GroupBox | MainWindow + AssignTrade + SubDaily | Card-style بـ CornerRadius=14 |
| Label | كل الـ Windows | Foreground=TextSub, Padding=0 |
| GridViewColumnHeader | SubDailyReport + LinksManager | Header مخصص بـ Hover |
| ListViewItem | SubDailyReport | DarkListViewItem بـ Accent colors |
| ScrollBar (Accent) | SubDailyReport + LinksManager | DarkScrollBar بـ AccentBrush |
| ComboBoxItem | AssignTrade + MainWindow | CornerRadius=8 مع Hover |

## قبل البدء: اقرأ هذه الملفات
- WpfApp2\MainWindow.xaml (Window.Resources فقط — السطور 19 إلى 283)
- WpfApp2\SubDailyReportWindow.xaml (Window.Resources — السطور 18 إلى 370)
- WpfApp2\LinksManagerWindow.xaml (Window.Resources — السطور 15 إلى 72)
- WpfApp2\AssignTradeCodesWindow.xaml (Window.Resources — السطور 19 إلى 271)
- WpfApp2\Theme\Base\Colors.xaml
- WpfApp2\Theme\Base\Radius.xaml
- WpfApp2\Theme\Controls\ScrollBarStyles.xaml (لتجنب التعارض)

## الملفات المطلوب إنشاؤها في Theme\Controls\

### 1. GroupBoxStyles.xaml

هذا الـ Style موجود بشكل متطابق في MainWindow و AssignTrade و SubDaily.
يُحوَّل إلى Global Style بـ DynamicResource.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Default GroupBox — يُطبق تلقائياً على كل GroupBoxes -->
    <Style TargetType="{x:Type GroupBox}">
        <Setter Property="Foreground"   Value="{DynamicResource TextMainBrush}"/>
        <Setter Property="BorderBrush"  Value="{DynamicResource CardStrokeBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type GroupBox}">
                    <Border CornerRadius="{DynamicResource CardRadius}"
                            Background="#14000000"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Padding="12">
                        <Grid>
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="10"/>
                                <RowDefinition Height="*"/>
                            </Grid.RowDefinitions>

                            <!-- Header -->
                            <TextBlock Grid.Row="0"
                                       Text="{TemplateBinding Header}"
                                       Foreground="{DynamicResource TextMainBrush}"
                                       FontWeight="SemiBold"
                                       FontSize="{DynamicResource BodyFontSize}"/>

                            <!-- Content -->
                            <ContentPresenter Grid.Row="2"/>
                        </Grid>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

### 2. LabelStyles.xaml

موجود بشكل متطابق في كل الـ Windows الأربعة.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Default Label — يُطبق تلقائياً -->
    <Style TargetType="{x:Type Label}">
        <Setter Property="Foreground"        Value="{DynamicResource TextSubBrush}"/>
        <Setter Property="VerticalAlignment" Value="Center"/>
        <Setter Property="Padding"           Value="0"/>
        <Setter Property="FontSize"          Value="{DynamicResource BodyFontSize}"/>
    </Style>

    <!-- HeaderLabel — للعناوين الكبيرة -->
    <Style x:Key="HeaderLabel" TargetType="{x:Type Label}" BasedOn="{StaticResource {x:Type Label}}">
        <Setter Property="Foreground"  Value="{DynamicResource TextMainBrush}"/>
        <Setter Property="FontSize"    Value="{DynamicResource SubHeaderFontSize}"/>
        <Setter Property="FontWeight"  Value="SemiBold"/>
    </Style>

    <!-- CaptionLabel — للنصوص الصغيرة -->
    <Style x:Key="CaptionLabel" TargetType="{x:Type Label}" BasedOn="{StaticResource {x:Type Label}}">
        <Setter Property="FontSize"  Value="{DynamicResource CaptionFontSize}"/>
        <Setter Property="Opacity"   Value="0.7"/>
    </Style>

</ResourceDictionary>
```

### 3. ListViewItemStyles.xaml

هذا الملف يكمل ListViewStyles.xaml ويضيف:
- ListViewItem Style الموحد (من DarkListViewItem في SubDaily)
- GridViewColumnHeader Style الموحد (من SubDaily + LinksManager)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- ListViewItem — Default (بدون Key — يُطبق تلقائياً) -->
    <Style TargetType="{x:Type ListViewItem}">
        <Setter Property="Foreground"               Value="{DynamicResource TextMainBrush}"/>
        <Setter Property="Background"               Value="Transparent"/>
        <Setter Property="Padding"                  Value="6,4"/>
        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
        <Setter Property="BorderThickness"          Value="0"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#223A86FF"/>
            </Trigger>
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#443A86FF"/>
                <Setter Property="Foreground" Value="White"/>
            </Trigger>
            <Trigger Property="IsEnabled" Value="False">
                <Setter Property="Opacity" Value="0.55"/>
            </Trigger>
        </Style.Triggers>
    </Style>

    <!-- Named variant — لو محتاج ListViewItem مختلف في Window معينة -->
    <Style x:Key="AccentListViewItem" TargetType="{x:Type ListViewItem}"
           BasedOn="{StaticResource {x:Type ListViewItem}}"/>

    <!-- GridViewColumnHeader — Default (بدون Key) -->
    <Style TargetType="{x:Type GridViewColumnHeader}">
        <Setter Property="Foreground"                Value="{DynamicResource TextMainBrush}"/>
        <Setter Property="Background"                Value="#1AFFFFFF"/>
        <Setter Property="BorderBrush"               Value="{DynamicResource CardStrokeBrush}"/>
        <Setter Property="BorderThickness"           Value="0,0,0,1"/>
        <Setter Property="Padding"                   Value="8,6"/>
        <Setter Property="HorizontalContentAlignment" Value="Center"/>
        <Setter Property="FontWeight"                Value="SemiBold"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type GridViewColumnHeader}">
                    <Border x:Name="HdrBd"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter VerticalAlignment="Center"
                                          HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="HdrBd" Property="Background" Value="#223A86FF"/>
                        </Trigger>
                        <Trigger Property="IsPressed" Value="True">
                            <Setter TargetName="HdrBd" Property="Background" Value="#333A86FF"/>
                        </Trigger>
                        <!-- Gripper (سهم الـ resize) — إخفاؤه -->
                        <Trigger Property="Role" Value="Padding">
                            <Setter Property="Visibility" Value="Collapsed"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

### 4. تحديث ScrollBarStyles.xaml (إضافة Accent Variant)

الـ ScrollBar الحالي في Session 3 بيستخدم #55FFFFFF (أبيض شفاف).
لكن LinksManager و SubDaily بيستخدمان #553A86FF (Accent شفاف).
نضيف الـ Accent variant بـ DynamicResource حتى يتغير مع الثيم.

أضف في نهاية ScrollBarStyles.xaml الموجود:

```xml
<!-- Accent ScrollBar — يستخدم AccentBrush بدل الأبيض -->
<!-- يُستخدم داخل ComboBox Popup و ListView المخصصة -->
<Style x:Key="AccentScrollBar" TargetType="{x:Type ScrollBar}">
    <Setter Property="Width"      Value="6"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type ScrollBar}">
                <Grid Background="Transparent">
                    <Track x:Name="PART_Track">
                        <Track.DecreaseRepeatButton>
                            <RepeatButton Command="ScrollBar.LineUpCommand"
                                          Opacity="0" Focusable="False"/>
                        </Track.DecreaseRepeatButton>
                        <Track.Thumb>
                            <Thumb Focusable="False">
                                <Thumb.Template>
                                    <ControlTemplate TargetType="{x:Type Thumb}">
                                        <Border x:Name="ThumbBd"
                                                CornerRadius="3">
                                            <Border.Background>
                                                <!-- Accent بـ opacity 55 -->
                                                <SolidColorBrush Color="{Binding Source={DynamicResource AccentColor}}"
                                                                 Opacity="0.35"/>
                                            </Border.Background>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Setter TargetName="ThumbBd" Property="Opacity" Value="0.6"/>
                                            </Trigger>
                                            <Trigger Property="IsDragging" Value="True">
                                                <Setter TargetName="ThumbBd" Property="Opacity" Value="0.8"/>
                                            </Trigger>
                                            <Trigger Property="IsEnabled" Value="False">
                                                <Setter TargetName="ThumbBd" Property="Opacity" Value="0.2"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Thumb.Template>
                            </Thumb>
                        </Track.Thumb>
                        <Track.IncreaseRepeatButton>
                            <RepeatButton Command="ScrollBar.LineDownCommand"
                                          Opacity="0" Focusable="False"/>
                        </Track.IncreaseRepeatButton>
                    </Track>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### 5. ComboBoxItemStyles.xaml

ComboBoxItem له style مستقل في AssignTrade وداخل ComboBox template في MainWindow.
نوحدهم في ملف منفصل.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Default ComboBoxItem — يُطبق تلقائياً على كل الـ ComboBoxes -->
    <Style TargetType="{x:Type ComboBoxItem}">
        <Setter Property="Foreground"               Value="{DynamicResource TextMainBrush}"/>
        <Setter Property="Background"               Value="Transparent"/>
        <Setter Property="Padding"                  Value="10,6"/>
        <Setter Property="HorizontalContentAlignment" Value="Stretch"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type ComboBoxItem}">
                    <Border x:Name="Bd"
                            Background="{TemplateBinding Background}"
                            CornerRadius="{DynamicResource SmallRadius}">
                        <ContentPresenter Margin="{TemplateBinding Padding}"
                                          VerticalAlignment="Center"
                                          TextElement.Foreground="{TemplateBinding Foreground}"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsHighlighted" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="#2FFFFFFF"/>
                        </Trigger>
                        <Trigger Property="IsSelected" Value="True">
                            <Setter TargetName="Bd" Property="Background" Value="#334A90E2"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter Property="Opacity" Value="0.55"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

### 6. ProgressBarStyles.xaml

ProgressBar مستخدم في كل الـ Windows لكن بـ Custom Template (مش الـ WPF default).
نضيف Style للـ ProgressBar الـ WPF الكلاسيكي مع الـ tokens، ونترك الـ Custom
Progress Border في الـ Windows كما هي — لكن نحوّل ألوانها للـ Tokens.

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Standard ProgressBar (لو استُخدم في المستقبل) -->
    <Style TargetType="{x:Type ProgressBar}">
        <Setter Property="Height"      Value="8"/>
        <Setter Property="Foreground"  Value="{DynamicResource AccentBrush}"/>
        <Setter Property="Background"  Value="#22000000"/>
        <Setter Property="BorderBrush" Value="{DynamicResource CardStrokeBrush}"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type ProgressBar}">
                    <Border CornerRadius="{DynamicResource PillRadius}"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}">
                        <Border x:Name="PART_Indicator"
                                HorizontalAlignment="Left"
                                CornerRadius="{DynamicResource PillRadius}"
                                Background="{TemplateBinding Foreground}"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- ProgressTrack Style — للـ Border المستخدم كـ progress track في الـ Windows -->
    <!-- هذا Key يُستخدم كـ reference في Session 6 migration -->
    <Style x:Key="ProgressTrackBorder" TargetType="{x:Type Border}">
        <Setter Property="Height"        Value="20"/>
        <Setter Property="CornerRadius"  Value="{DynamicResource PillRadius}"/>
        <Setter Property="Background"    Value="#22000000"/>
        <Setter Property="BorderBrush"   Value="{DynamicResource CardStrokeBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
    </Style>

    <!-- ProgressFill Style — للـ Border الداخلي اللي بيتحرك -->
    <Style x:Key="ProgressFillBorder" TargetType="{x:Type Border}">
        <Setter Property="CornerRadius" Value="{DynamicResource PillRadius}"/>
        <Setter Property="Background"   Value="{DynamicResource AccentBrush}"/>
    </Style>

</ResourceDictionary>
```

## تعديل App.xaml
أضف بعد ملفات Session 3، بالترتيب:

```xml
<!-- Session 3B — Missing Controls -->
<ResourceDictionary Source="Theme/Controls/GroupBoxStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/LabelStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/ListViewItemStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/ComboBoxItemStyles.xaml"/>
<ResourceDictionary Source="Theme/Controls/ProgressBarStyles.xaml"/>
```

> ملاحظة: ScrollBarStyles.xaml تم تعديله في مكانه (إضافة AccentScrollBar variant فقط).

## جدول استبدال الـ Keys في Sessions 6A/6B/6C

هذا الجدول يُستخدم في Sessions 6 كمرجع للاستبدال:

| الـ Key القديم (محلي) | الـ Key الجديد (Global) | ملاحظة |
|---|---|---|
| `TextMain` Brush | `{DynamicResource TextMainBrush}` | |
| `TextSub` Brush | `{DynamicResource TextSubBrush}` | |
| `Accent` Brush | `{DynamicResource AccentBrush}` | |
| `ControlBg` Brush | `{DynamicResource ControlBgBrush}` | |
| `ControlStroke` Brush | `{DynamicResource ControlStrokeBrush}` | |
| `CardStroke` Brush | `{DynamicResource CardStrokeBrush}` | |
| `Style="{StaticResource RoundButton}"` | `Style="{DynamicResource BaseButton}"` | |
| `Style="{StaticResource PrimaryButton}"` | `Style="{DynamicResource PrimaryButton}"` | |
| `Style="{StaticResource RoundComboBox}"` | `Style="{DynamicResource RoundComboBox}"` | |
| `Style="{StaticResource DarkScrollBar}"` | `Style="{DynamicResource AccentScrollBar}"` | |
| `Style="{StaticResource DarkListViewItem}"` | لا شيء — Default يُطبق تلقائياً | احذف الـ Key |
| `GroupBox` Style محلي | احذف — Global يُطبق تلقائياً | |
| `Label` Style محلي | احذف — Global يُطبق تلقائياً | |
| `GridViewColumnHeader` Style محلي | احذف — Global يُطبق تلقائياً | |
| `ComboBoxItem` Style محلي | احذف — Global يُطبق تلقائياً | |
| `ProgressWidthConverter` محلي | احذف — في App.xaml من Session 2 | |
| `PercentToScaleConverter` محلي | احذف — في App.xaml من Session 2 | |
| Background LinearGradientBrush | يبقى كما هو — decorative | لا تحوّله لـ Token |

## قائمة التحقق من النجاح

```
□ Build بدون أي خطأ
□ GroupBox يظهر بـ CornerRadius=CardRadius ويأخذ AccentBrush على الـ border
□ كل الـ Labels في الـ Windows تأخذ TextSubBrush تلقائياً
□ ListViewItem يُظلل بالـ Accent عند Hover
□ GridViewColumnHeader يظهر بـ Hover effect أزرق
□ ComboBox Dropdown يعرض Items بـ CornerRadius صح
□ ScrollBar داخل ListView و ComboBox يأخذ AccentScrollBar style
□ ProgressBar Style موجود وجاهز للاستخدام المستقبلي
```

## ممنوع
- لا تعدّل Window.Resources في أي Window في هذه المرحلة
- لا تحذف الـ Custom Progress Border في الـ Windows — يتم تحويله في Session 6
- لا تغير ListViewStyles.xaml الموجود من Session 3 — فقط أضف ListViewItemStyles.xaml جديد
- لا تدمج ComboBoxItemStyles.xaml داخل ComboBoxStyles.xaml (يسبب circular dependency)
```

---

---

# SESSION 4 — Base Window Class (ModernWindow)

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\
الـ Namespace الأساسي: Som3a_WPF_UI

## السياق
المراحل 1 و 2 و 3 مكتملة:
- Design Tokens جاهزة في Theme\Base\
- SharedConverters.cs موجود
- Control Styles جاهزة في Theme\Controls\

## قبل البدء: اقرأ هذه الملفات أولاً
- WpfApp2\MainWindow.xaml (كاملاً — لفهم الـ Window structure الحالية)
- WpfApp2\Views\LinksManagerWindow.xaml أو WpfApp2\LinksManagerWindow.xaml
- أي Window أخرى لفهم الـ Pattern المشترك

## المهمة
إنشاء ModernWindow كـ Base Class لكل الـ Windows.

### الملف 1: WpfApp2\Controls\ModernWindow.cs

```csharp
using System.Windows;
using System.Windows.Input;

namespace Som3a_WPF_UI.Controls
{
    public class ModernWindow : Window
    {
        static ModernWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ModernWindow),
                new FrameworkPropertyMetadata(typeof(ModernWindow)));
        }

        public ModernWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            // ESC يغلق الـ Window
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) Close();
            };
        }

        // Drag support
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // Commands
        public ICommand CloseCommand => new RelayCommand(_ => Close());
        public ICommand MinimizeCommand => new RelayCommand(_ =>
            WindowState = WindowState.Minimized);
        public ICommand MaximizeCommand => new RelayCommand(_ =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized);
    }
}
```

> إذا RelayCommand غير موجود في المشروع، أنشئه في WpfApp2\Helpers\RelayCommand.cs

### الملف 2: WpfApp2\Theme\Controls\WindowStyles.xaml

Style للـ ModernWindow يشمل:
- Background: {DynamicResource BackgroundBrush}
- ControlTemplate مع:
  - Border خارجي بـ CornerRadius={DynamicResource WindowRadius}
  - Shadow effect خفيف
  - Header بارتفاع {DynamicResource HeaderHeight} يحتوي:
    - Title TextBlock (يسار)
    - Minimize / Close buttons (يمين) — بدون Maximize في الـ Windows الحالية لأنها ResizeMode=NoResize
  - ContentPresenter للـ Content

Header Buttons Style:
- مربعات 32x32
- Background: Transparent
- Hover: #22FFFFFF
- Close button Hover: {DynamicResource DangerBrush}
- بدون Border

### الملف 3: WpfApp2\Controls\ModernWindow.xaml (Generic.xaml)
لو WPF بيطلب Generic.xaml لتسجيل الـ Default Style، أنشئه في:
WpfApp2\Themes\Generic.xaml

## تعديل App.xaml
أضف:
```xml
<ResourceDictionary Source="Theme/Controls/WindowStyles.xaml"/>
```

## التحقق من النجاح
1. Build بدون أي خطأ
2. أنشئ Window تجريبية ترث من ModernWindow وتأكد أنها:
   - بتفتح بدون Chrome الافتراضي
   - الـ Title يظهر في الـ Header المخصص
   - ESC بيغلقها
   - Drag بيشتغل
3. احذف الـ Window التجريبية بعد التأكد

## ممنوع
- لا تعدّل أي Window موجودة لترث من ModernWindow في هذه المرحلة
- لا تغير WindowStyle أو AllowsTransparency في أي Window موجودة
- المرحلة دي لإنشاء الـ Class فقط — التطبيق في المرحلة 6
```

---

---

# SESSION 5 — Theme Manager

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\
الـ Namespace: Som3a_WPF_UI

## السياق
المراحل 1-4 مكتملة. عندنا:
- Design Tokens في Theme\Base\
- Control Styles في Theme\Controls\
- ModernWindow جاهز

## قبل البدء: اقرأ
- WpfApp2\Theme\App.xaml (لفهم الـ ResourceDictionaries المسجلة)
- WpfApp2\Theme\Base\Colors.xaml

## المهمة
إنشاء نظام Theme Management كامل.

### الملف 1: WpfApp2\Services\ThemeSettings.cs

```csharp
using System;
using System.IO;

namespace Som3a_WPF_UI.Services
{
    public enum ThemeType
    {
        FluentDarkBlue,
        FluentWhite      // للمستقبل
    }

    public class ThemeSettings
    {
        public ThemeType CurrentTheme { get; set; } = ThemeType.FluentDarkBlue;
        public string AccentColor     { get; set; } = "#3A86FF";
        public bool   HighPerformance { get; set; } = false; // تعطيل Animations للأجهزة الضعيفة

        public static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "theme.json");
    }
}
```

### الملف 2: WpfApp2\Services\ThemeManager.cs

```csharp
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json; // أو System.Text.Json لو .NET 5+

namespace Som3a_WPF_UI.Services
{
    public static class ThemeManager
    {
        private static ThemeSettings _current = new ThemeSettings();

        // تطبيق ثيم كامل
        public static void ApplyTheme(ThemeType theme)
        {
            _current.CurrentTheme = theme;
            // لو FluentWhite: override Colors.xaml brushes
            // لو FluentDarkBlue: الـ defaults
            // ... التنفيذ
            SaveSettings();
        }

        // تغيير لون الـ Accent فقط (بدون restart)
        public static void ChangeAccent(string hexColor)
        {
            if (Application.Current?.Resources == null) return;
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            Application.Current.Resources["AccentColor"] = color;
            Application.Current.Resources["AccentBrush"] = new SolidColorBrush(color);
            _current.AccentColor = hexColor;
            SaveSettings();
        }

        // تغيير DynamicResource واحد بالاسم
        public static void SetResource(string key, object value)
        {
            if (Application.Current?.Resources != null)
                Application.Current.Resources[key] = value;
        }

        public static ThemeSettings GetCurrentSettings() => _current;

        public static void SaveSettings()
        {
            try
            {
                var dir = Path.GetDirectoryName(ThemeSettings.SettingsPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(ThemeSettings.SettingsPath,
                    JsonConvert.SerializeObject(_current, Formatting.Indented));
            }
            catch { /* silent fail */ }
        }

        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(ThemeSettings.SettingsPath)) return;
                var json = File.ReadAllText(ThemeSettings.SettingsPath);
                _current = JsonConvert.DeserializeObject<ThemeSettings>(json)
                           ?? new ThemeSettings();
                ApplyTheme(_current.CurrentTheme);
                ChangeAccent(_current.AccentColor);
            }
            catch { _current = new ThemeSettings(); }
        }
    }
}
```

> إذا Newtonsoft.Json غير موجود في المشروع، استخدم System.Text.Json أو أضف NuGet package.

### تعديل App.xaml.cs (أو نقطة البداية المناسبة)
أضف في Application Startup:
```csharp
ThemeManager.LoadSettings();
```

## التحقق من النجاح
1. Build بدون أي خطأ
2. اختبر في الـ Immediate Window أو وقت التشغيل:
   ```csharp
   ThemeManager.ChangeAccent("#FF5722"); // المفروض يغير الـ Accent فوراً
   ThemeManager.ChangeAccent("#3A86FF"); // يرجع للأصلي
   ```
3. تأكد أن Settings بتتحفظ في AppData\Som3a\theme.json

## ممنوع
- لا تضيف UI لاختيار الثيم في هذه المرحلة (ده اختياري في المستقبل)
- لا تغير أي Window
```

---

---

# SESSION 5B — Ribbon Button + Settings Window
**يُنفَّذ بعد Session 5 (Theme Manager) وقبل Session 6A (Migration)**

> الملف الكامل لهذه الـ Session موجود في: `session_5b_settings.md`
> انسخ محتواه كاملاً وألصقه في Claude Code.

---

---

# SESSION 6A — Migration: XerEditor & AssignTradeCodes

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
المراحل 1-5 مكتملة. الـ Global Styles والـ Tokens والـ ThemeManager جاهزين.

## المهمة
تحويل أبسط Windows للنظام الجديد (بدون تغيير في الـ functionality).

## قبل البدء: اقرأ هذه الملفات كاملاً
- WpfApp2\XerEditorWindow.xaml
- WpfApp2\AssignTradeCodesWindow.xaml (أو المسار الصحيح)
- WpfApp2\Theme\Base\Colors.xaml (للـ Token names)

## عملية التحويل لكل Window

### الخطوة 1: حذف Window.Resources
احذف كل محتوى <Window.Resources> ... </Window.Resources>
(بما فيه MergedDictionaries المحلية وأي Styles أو Converters معرّفة هناك)

### الخطوة 2: تحديث الـ xmlns
احذف أي xmlns للـ Converters المحلية
أضف لو محتاج:
```xml
xmlns:controls="clr-namespace:Som3a_WPF_UI.Controls"
```

### الخطوة 3: استبدال الـ References
- كل `StaticResource Accent` → `DynamicResource AccentBrush`
- كل `StaticResource TextMain` → `DynamicResource TextMainBrush`
- كل `StaticResource TextSub` → `DynamicResource TextSubBrush`
- كل `StaticResource ControlBg` → `DynamicResource ControlBgBrush`
- كل `StaticResource ControlStroke` → `DynamicResource ControlStrokeBrush`
- كل `StaticResource CardStroke` → `DynamicResource CardStrokeBrush`
- كل `StaticResource BgBrush` → `DynamicResource BackgroundBrush`
- كل hardcoded colors في Background/BorderBrush → المقابل من الـ Tokens

### الخطوة 4 (اختياري في هذه المرحلة): تغيير Window إلى ModernWindow
إذا كانت Window تستخدم WindowStyle=None و AllowsTransparency=True:
```xml
<!-- قبل -->
<Window x:Class="..." WindowStyle="None" AllowsTransparency="True" ...>

<!-- بعد -->
<controls:ModernWindow x:Class="..." ...>
```
وفي الـ Code-behind:
```csharp
// قبل
public partial class XerEditorWindow : Window

// بعد
public partial class XerEditorWindow : ModernWindow
```

> ملاحظة: الخطوة 4 اختياري لو ModernWindow معقدة الـ Header — اتركها Window عادية وعدّل Styling فقط.

## قائمة التحقق لكل Window
بعد تحويل كل Window:
□ Build بدون أي خطأ
□ لا يوجد Binding Warning في Output Window
□ لا يوجد StaticResource يشير لـ key محذوف
□ لا يوجد أي لون hardcoded في Background أو BorderBrush
□ الـ Window بتفتح وبتشتغل بشكل طبيعي
□ الـ Buttons والـ TextBoxes تأخذ الـ Global Styles

## ممنوع
- لا تغير أي منطق في Code-behind
- لا تغير أي ViewModel
- لا تغير Bindings أو Commands
- لا تضيف أي UI جديد
```

---

---

# SESSION 6B — Migration: LinksManager & SubDailyReport

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
Session 6A مكتملة — XerEditor و AssignTradeCodes تم تحويلهم بنجاح.

## قبل البدء: اقرأ
- WpfApp2\Views\LinksManagerWindow.xaml (أو مسارها الصحيح) — كاملاً
- WpfApp2\SubDailyReportWindow.xaml — كاملاً
- WpfApp2\Theme\Base\Colors.xaml (للـ Token names)

## المهمة
تحويل LinksManagerWindow و SubDailyReportWindow.

## تنبيهات خاصة بهذه الـ Windows

### LinksManagerWindow
- Window كبيرة (604+ سطر) — كن دقيقاً
- تحتوي ScrollBar Style مخصص (DarkScrollBar) — استبدلها بـ Global ScrollBar Style
- تحتوي ListView Style مخصص — استبدله بـ Global ListView Style
- تحتوي ProgressBar مخصص — تأكد أنه لسا شغال بعد حذف Window.Resources

### SubDailyReportWindow
- قد تحتوي Charts أو DataGrid — تعامل معها بحذر
- أي Style مخصص للـ DataGrid → استبدل بـ ModernDataGrid Style

## عملية التحويل
نفس خطوات Session 6A:
1. احذف Window.Resources
2. حدّث الـ xmlns
3. استبدل كل Resource references
4. اختياري: حوّل لـ ModernWindow

## جدول استبدال الـ Resources (LinksManager بالتحديد)
- DarkScrollBar Style → محذوف (الـ Global ScrollBar Style بيغطيه)
- كل x:Key="..." في ListView → استبدل بـ Global Style أو احذف
- ProgressWidthConverter → موجود في App.xaml (مش محتاج تعريف محلي)

## قائمة التحقق
□ Build بدون أي خطأ
□ LinksManager بتفتح وبتشتغل
□ SubDailyReport بتفتح وبتشتغل
□ ScrollBar الـ slim شايفه في LinksManager
□ DataGrid أو ListView شغالة صح
□ لا يوجد أي خطأ في Output Window

## ممنوع
- لا تغير أي منطق
- لا تحذف أي Control من الـ UI
- لو مش متأكد من حاجة، اتركها وأخبرني
```

---

---

# SESSION 6C — Migration: MainWindow (الأصعب)

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
Sessions 6A و 6B مكتملتان. كل الـ Windows الأخرى تم تحويلها.
MainWindow هي أعقد Window في المشروع.

## قبل البدء: اقرأ هذه الملفات كاملاً
- WpfApp2\MainWindow.xaml (كاملاً — مهم جداً)
- WpfApp2\MainWindow.xaml.cs
- WpfApp2\Theme\Base\Colors.xaml
- WpfApp2\Theme\Base\Spacing.xaml
- WpfApp2\Converters\SharedConverters.cs

## تنبيهات خاصة بـ MainWindow

### Progress Bar المخصص
MainWindow فيها Custom Progress Bar بـ MultiBinding وConverters:
```xml
<Border.Width>
    <MultiBinding Converter="{StaticResource ProgressWidthConverter}">
        <Binding RelativeSource="..." Path="ActualWidth"/>
        <Binding Path="ProgressPercent"/>
    </MultiBinding>
</Border.Width>
```
- غيّر StaticResource → DynamicResource للـ Converter مش محتاج
- الـ Converter نفسه موجود في App.xaml فمش هيتغير
- فقط تأكد إن الـ Key name صح (ProgressWidthConverter)

### الـ Styles المحلية الكتير
MainWindow فيها:
- RoundButton Style
- PrimaryButton Style
- TextBox Style (بدون Key — يطبق تلقائياً)
- Label Style
- RoundComboBox Style

كل دول هيتحذفوا وهياخدوا الـ Global Styles. تحقق إن:
- كل Button عندها Style مناسب (PrimaryButton, RoundButton=BaseButton)
- لو Button مش عندها Style explicit → هتاخد الـ Default لو موجود، أو حدد لها Style

### الـ GroupBox
لو MainWindow فيها GroupBox Style مخصص، تأكد من وجود Global Style ليه أو اتركه.

## عملية التحويل
1. احذف Window.Resources كاملاً
2. حدّث xmlns
3. استبدل كل Resource references حسب الجدول ده:
   - StaticResource Accent → DynamicResource AccentBrush
   - StaticResource TextMain → DynamicResource TextMainBrush
   - StaticResource TextSub → DynamicResource TextSubBrush
   - StaticResource ControlBg → DynamicResource ControlBgBrush
   - StaticResource ControlStroke → DynamicResource ControlStrokeBrush
   - Style="{StaticResource RoundButton}" → Style="{DynamicResource BaseButton}" (أو PrimaryButton)
   - Style="{StaticResource PrimaryButton}" → Style="{DynamicResource PrimaryButton}"
   - Style="{StaticResource RoundComboBox}" → Style="{DynamicResource RoundComboBox}"
   - أي hardcoded hex في Background → Token مناسب أو اتركه لو decorative

4. اختياري: حوّل لـ ModernWindow

## قائمة التحقق
□ Build بدون أي خطأ
□ لا يوجد أي Binding Warning
□ Progress Bar يعمل صح (يتحرك مع ProgressPercent)
□ كل الـ ComboBoxes تعمل (تفتح وتختار)
□ كل الـ Buttons شغالة
□ ListView يعرض البيانات
□ الـ Window تفتح وتُغلق بشكل طبيعي

## لو حصل خطأ في الـ Bindings
أرسل لي الـ Binding Errors من Output Window وسأساعدك في حلها قبل الاستمرار.

## ممنوع
- لا تغير أي Code-behind
- لا تغير أي ViewModel
- لا تغير منطق الـ Progress Bar أو أي Converter
```

---

---

# SESSION 7 — Fluent Visual Effects

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
المراحل 1-6 مكتملة. كل الـ Windows تعتمد على الـ Token system.
الآن نضيف الـ Visual Polish.

## قبل البدء: اقرأ
- WpfApp2\Theme\Controls\ButtonStyles.xaml
- WpfApp2\Theme\Controls\WindowStyles.xaml
- WpfApp2\Controls\ModernWindow.cs

## المهمة
إضافة Fluent animations وeffects على الـ Styles الموجودة.

### 1. Button Hover Animation
في ButtonStyles.xaml، أضف Storyboard Trigger بدل Setter:

```xml
<Style.Triggers>
    <Trigger Property="IsMouseOver" Value="True">
        <Trigger.EnterActions>
            <BeginStoryboard>
                <Storyboard>
                    <DoubleAnimation
                        Storyboard.TargetProperty="Opacity"
                        To="1.0" Duration="0:0:0.12"
                        EasingFunction.../>
                </Storyboard>
            </BeginStoryboard>
        </Trigger.EnterActions>
    </Trigger>
</Style.Triggers>
```

### 2. Window Open Animation
في ModernWindow.cs:
```csharp
protected override void OnContentRendered(EventArgs e)
{
    base.OnContentRendered(e);
    // Fade in + slight scale up
    var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
    BeginAnimation(OpacityProperty, anim);
}
```

### 3. Card Shadow Effect
أنشئ ملف Theme\Fluent\FluentEffects.xaml:
```xml
<DropShadowEffect x:Key="CardShadow"
                  Color="#000000" Opacity="0.25"
                  BlurRadius="16" ShadowDepth="4"
                  Direction="270"/>

<DropShadowEffect x:Key="FocusGlow"
                  Color="#3A86FF" Opacity="0.35"
                  BlurRadius="10" ShadowDepth="0"/>

<DropShadowEffect x:Key="WindowShadow"
                  Color="#000000" Opacity="0.4"
                  BlurRadius="30" ShadowDepth="8"
                  Direction="270"/>
```

أضف CardShadow على الـ Cards في WindowStyles:
```xml
<Border Effect="{DynamicResource CardShadow}" .../>
```

أضف FocusGlow على TextBox عند Focus في TextBoxStyles:
```xml
<Trigger Property="IsFocused" Value="True">
    <Setter Property="Effect" Value="{DynamicResource FocusGlow}"/>
    <Setter Property="BorderBrush" Value="{DynamicResource AccentBrush}"/>
</Trigger>
```

### 4. High Performance Fallback
في كل Effect، أضف:
```csharp
// في ThemeManager
if (_current.HighPerformance)
{
    Application.Current.Resources["CardShadow"] = null;
    Application.Current.Resources["FocusGlow"] = null;
    Application.Current.Resources["WindowShadow"] = null;
}
```

## قواعد للـ Animations
- Duration: 100-200ms للـ Hover، 200-300ms للـ Window open
- EasingFunction: CubicEase أو QuadraticEase
- لا تستخدم Animations على DataGrid أو ListView items (بطيء)
- كل Animation تكون على Opacity أو RenderTransform فقط — مش على Size أو Position

## التحقق من النجاح
□ Button Hover يبدو سلس (مش مفاجئ)
□ Window بتظهر بـ Fade in
□ Cards عندها shadow واضح
□ TextBox عند Focus يظهر glow خفيف
□ الـ Application مش بطيء أو laggy
□ Build بدون أي خطأ

## ممنوع
- لا تضيف Effects على DataGrid rows (performance issue)
- لا تستخدم RenderTransform3D
- لا تضيف Blur effect (مش مدعوم كويس في .NET 4.8 WPF بدون libs)
```

---

---

# SESSION 8 — Enterprise Components

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
المراحل 1-7 مكتملة. النظام مستقر والـ Theme system شغال.

## المهمة
إنشاء Enterprise UI Components جاهزة للاستخدام في الـ Windows الجديدة.

## Component 1: Toast Notification System

### WpfApp2\Controls\Toast\ToastModel.cs
```csharp
public enum ToastType { Success, Warning, Error, Info }

public class ToastModel
{
    public string Message  { get; set; }
    public ToastType Type  { get; set; }
    public int DurationMs  { get; set; } = 3000;
}
```

### WpfApp2\Services\ToastService.cs
```csharp
public static class ToastService
{
    public static void Show(string message, ToastType type = ToastType.Info, int durationMs = 3000)
    {
        // يعرض Toast في أسفل يمين الـ Application
        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastWindow(new ToastModel
            {
                Message = message, Type = type, DurationMs = durationMs
            });
            toast.Show();
        });
    }

    public static void Success(string message) => Show(message, ToastType.Success);
    public static void Error(string message)   => Show(message, ToastType.Error);
    public static void Warning(string message) => Show(message, ToastType.Warning);
}
```

### WpfApp2\Controls\Toast\ToastWindow.xaml
- WindowStyle=None, AllowsTransparency=True
- الـ Toast يظهر في أسفل يمين الشاشة
- Animation: Slide up + fade in عند الظهور، fade out عند الإغلاق
- يغلق تلقائياً بعد DurationMs
- Icon حسب الـ Type: ✓ ✗ ⚠ ℹ
- Width: 300px تقريباً، Height: Auto
- Colors من الـ Tokens: Success/Danger/Warning/InfoBrush

## Component 2: Loading Overlay

### WpfApp2\Controls\LoadingOverlay.xaml (UserControl)
```xml
<!-- Overlay يغطي الـ Window بالكامل -->
<Grid Background="#88000000">
    <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <!-- Spinning circle animation -->
        <Ellipse .../>
        <!-- Message Text -->
        <TextBlock Text="{Binding Message}" .../>
    </StackPanel>
</Grid>
```

الاستخدام:
```xml
<Grid>
    <!-- محتوى الـ Window -->
    <controls:LoadingOverlay
        Visibility="{Binding IsBusy, Converter={DynamicResource BoolToVisibilityConverter}}"
        Message="{Binding StatusText}"/>
</Grid>
```

## Component 3: ModernCard UserControl

### WpfApp2\Controls\ModernCard.xaml
```xml
<UserControl>
    <Border Background="{DynamicResource CardBrush}"
            BorderBrush="{DynamicResource CardStrokeBrush}"
            BorderThickness="1"
            CornerRadius="{DynamicResource CardRadius}"
            Effect="{DynamicResource CardShadow}"
            Padding="{DynamicResource CardPadding}">
        <ContentPresenter/>
    </Border>
</UserControl>
```

Dependency Property:
- `Header` (string) — عنوان الكارت في أعلاه
- `IsLoading` (bool) — يعرض loading spinner

## Component 4: EmptyState Control

### WpfApp2\Controls\EmptyState.xaml
يُعرض لما الـ ListView أو DataGrid فارغ:
```xml
<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
    <TextBlock Text="📋" FontSize="48" HorizontalAlignment="Center"/>
    <TextBlock Text="{Binding Title}" Style="Header"/>
    <TextBlock Text="{Binding Subtitle}" Style="Caption"/>
</StackPanel>
```

## تعديل App.xaml
أضف xmlns للـ Controls:
```xml
xmlns:controls="clr-namespace:Som3a_WPF_UI.Controls"
```

## التحقق من النجاح
□ ToastService.Success("تم بنجاح") يعرض Toast أخضر
□ ToastService.Error("حدث خطأ") يعرض Toast أحمر
□ Toast يختفي تلقائياً بعد 3 ثوانٍ
□ LoadingOverlay يظهر ويختفي مع IsBusy
□ ModernCard يعرض محتواه صح
□ Build بدون أي خطأ

## ممنوع
- لا تدمج الـ Components في الـ Windows الموجودة في هذه المرحلة
- اللي بنبنيه هنا جاهز للاستخدام في الـ Windows الجديدة مستقبلاً
```

---

---

# SESSION 9 — White Theme (اختياري)

```
أنت تعمل على مشروع WPF اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
الـ WPF project: WpfApp2\

## السياق
المراحل 1-8 مكتملة. النظام مستقر تماماً.
هذه المرحلة اختيارية وتضيف ثيم فاتح.

## قبل البدء: اقرأ
- WpfApp2\Theme\Base\Colors.xaml
- WpfApp2\Services\ThemeManager.cs

## المهمة
إنشاء White Theme بأقل قدر من التغيير (override الـ Tokens فقط).

### WpfApp2\Theme\Fluent\FluentWhite.xaml
```xml
<ResourceDictionary>
    <!-- Override الألوان فقط — الباقي يبقى كما هو -->
    <Color x:Key="AccentColor">#0078D4</Color>
    <Color x:Key="BackgroundColor">#F5F5F5</Color>
    <Color x:Key="CardColor">#FFFFFF</Color>
    <Color x:Key="SurfaceColor">#EFEFEF</Color>
    <Color x:Key="TextMainColor">#1A1A2E</Color>
    <Color x:Key="TextSubColor">#555566</Color>
    <Color x:Key="TextDisabledColor">#AAAAAA</Color>
    <Color x:Key="BorderColor">#E0E0E0</Color>
    <Color x:Key="ControlBgColor">#F9F9F9</Color>

    <!-- Brushes المشتقة -->
    <SolidColorBrush x:Key="AccentBrush"      Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush"  Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="CardBrush"        Color="{StaticResource CardColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush"     Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="TextMainBrush"    Color="{StaticResource TextMainColor}"/>
    <SolidColorBrush x:Key="TextSubBrush"     Color="{StaticResource TextSubColor}"/>
    <SolidColorBrush x:Key="TextDisabledBrush" Color="{StaticResource TextDisabledColor}"/>
    <SolidColorBrush x:Key="CardStrokeBrush"  Color="{StaticResource BorderColor}"/>
    <SolidColorBrush x:Key="ControlBgBrush"   Color="{StaticResource ControlBgColor}"/>
    <SolidColorBrush x:Key="ControlStrokeBrush" Color="{StaticResource BorderColor}"/>
</ResourceDictionary>
```

### تحديث ThemeManager.cs
```csharp
public static void ApplyTheme(ThemeType theme)
{
    var uri = theme == ThemeType.FluentWhite
        ? new Uri("Theme/Fluent/FluentWhite.xaml", UriKind.Relative)
        : null; // DarkBlue هو الـ Default في Colors.xaml

    // امسح الـ Theme Override القديم وحمّل الجديد
    var dicts = Application.Current.Resources.MergedDictionaries;
    var existing = dicts.FirstOrDefault(d => d.Source?.ToString().Contains("Fluent") == true);
    if (existing != null) dicts.Remove(existing);

    if (uri != null)
    {
        var dict = new ResourceDictionary { Source = uri };
        dicts.Add(dict);
    }

    _current.CurrentTheme = theme;
    SaveSettings();
}
```

### اختبار التبديل
```csharp
// Dark Blue
ThemeManager.ApplyTheme(ThemeType.FluentDarkBlue);

// White
ThemeManager.ApplyTheme(ThemeType.FluentWhite);

// المفروض يتغير فوراً بدون restart
```

## التحقق من النجاح
□ White Theme يغير الـ Background للفاتح
□ Text يقرأ بوضوح على الخلفية الفاتحة
□ Buttons والـ Controls تبدو صح
□ التبديل بين الثيمين يعمل بدون Restart
□ الإعدادات تتحفظ وترجع بعد إعادة التشغيل

## ممنوع
- لا تغير أي Control Style (الألوان بس هي اللي بتتغير)
- لا تضيف خيار في الـ UI في هذه المرحلة
```

---

---

## ملخص الـ Sessions (الترتيب النهائي)

| Session | المرحلة | المدخل | المخرج |
|---|---|---|---|
| 1 | Design Tokens | مشروع جديد | 4 ملفات XAML |
| 2 | Shared Converters | Session 1 | SharedConverters.cs |
| 3 | Control Styles | Sessions 1-2 | 6 ملفات XAML |
| 4 | ModernWindow | Sessions 1-3 | ModernWindow.cs |
| 5 | Theme Manager | Sessions 1-4 | ThemeManager.cs |
| **5B** | **Ribbon Button + Settings Window** | **Session 5** | **addin_setting زر + SettingsWindow** |
| 6A | Migration (بسيطة) | Sessions 1-5B | XerEditor + AssignTrade محوّلين |
| 6B | Migration (متوسطة) | Session 6A | LinksManager + SubDaily محوّلين |
| 6C | Migration (MainWindow) | Session 6B | MainWindow محوّل |
| 7 | Fluent Effects | Session 6C | Animations + Shadows |
| 8 | Enterprise Components | Session 7 | Toast + Loading + Card |
| 9 | White Theme | Session 8 | FluentWhite.xaml |

---

*Som3a Addin 2026 · UI Architecture Session Prompts v2.0*

<!-- Session 5B الكامل موجود في ملف session_5b_settings.md -->

```
أنت تعمل على مشروع VSTO Add-in اسمه Som3a Addin 2026.
الـ Solution path: C:\Users\mohamedabdelsamea\source\repos\Som3a Addin 2026\
المشروع يحتوي على:
- VSTO project (الـ Add-in نفسه) — فيه الـ Ribbon و ThisAddIn.cs
- WpfApp2 — الـ WPF project (الـ UI)

## السياق
- ThemeManager.cs موجود في WpfApp2\Services\ (من Session 5)
- FluentDarkBlue هو الثيم الافتراضي
- FluentWhite موجود في WpfApp2\Theme\Fluent\ (من Session 9)
- ModernWindow موجود في WpfApp2\Controls\ (من Session 4)

## قبل البدء: اقرأ هذه الملفات أولاً
- كل الملفات في مجلد الـ VSTO project (خصوصاً Ribbon*.cs أو Ribbon*.xml)
- ThisAddIn.cs
- WpfApp2\Services\ThemeManager.cs
- WpfApp2\Services\ThemeSettings.cs

## المهمة — 3 أجزاء

---

## الجزء 1: إضافة زر addin_setting في الـ Ribbon

### لو الـ Ribbon بيستخدم XML (Ribbon.xml):
أضف في الـ Group المناسب:
```xml
<button id="addin_setting"
        label="Settings"
        screentip="Addin Settings"
        supertip="Open theme and display settings"
        imageMso="PropertySheet"
        size="large"
        onAction="OnAddinSettingClick"/>
```

### لو الـ Ribbon بيستخدم Designer (Ribbon.cs / RibbonBase):
أضف RibbonButton في الـ Group المناسب:
- Name: addinSettingButton
- Label: "Settings"
- OfficeImageId: "PropertySheet" (أيقونة ترس/إعدادات)
- Size: RibbonControlSize.RibbonControlSizeLarge
- Click event: addinSettingButton_Click

### الـ Click Handler (في Ribbon.cs أو ThisAddIn.cs):
```csharp
private void OnAddinSettingClick(Office.IRibbonControl control)
// أو
private void addinSettingButton_Click(object sender, RibbonControlEventArgs e)
{
    AddinSettingsBridge.OpenSettingsWindow();
}
```

---

## الجزء 2: إنشاء Bridge بين VSTO و WPF

### المشكلة
VSTO يعمل في COM context — مش ممكن يفتح WPF Window مباشرة من الـ Ribbon بدون Dispatcher.

### الملف: WpfApp2\Services\AddinSettingsBridge.cs
```csharp
using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Som3a_WPF_UI.Services
{
    public static class AddinSettingsBridge
    {
        private static Thread _wpfThread;
        private static Dispatcher _wpfDispatcher;

        /// <summary>
        /// يُستدعى من الـ VSTO Ribbon لفتح Settings Window
        /// </summary>
        public static void OpenSettingsWindow()
        {
            // لو الـ WPF Application موجودة (WpfApp2 شغال)
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowSettingsWindow();
                });
                return;
            }

            // لو الـ WPF Application مش موجودة، شغّلها في Thread منفصل
            if (_wpfThread == null || !_wpfThread.IsAlive)
            {
                _wpfThread = new Thread(() =>
                {
                    var app = new Application();
                    app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    _wpfDispatcher = Dispatcher.CurrentDispatcher;
                    ThemeManager.LoadSettings();
                    ShowSettingsWindow();
                    Dispatcher.Run();
                });
                _wpfThread.SetApartmentState(ApartmentState.STA);
                _wpfThread.IsBackground = true;
                _wpfThread.Start();
            }
            else
            {
                _wpfDispatcher?.Invoke(ShowSettingsWindow);
            }
        }

        private static void ShowSettingsWindow()
        {
            var existing = Application.Current?.Windows
                .OfType<SettingsWindow>()
                .FirstOrDefault();

            if (existing != null)
            {
                // لو الـ Window مفتوحة بالفعل، اجيبها للأمام
                existing.Activate();
                existing.Focus();
                return;
            }

            var win = new SettingsWindow();
            win.Show();
        }
    }
}
```

> ملاحظة: لو WpfApp2 بيتشغل كـ Standalone App (مش embedded) اقرأ ThisAddIn.cs وأخبرني كيف بيتشغل الـ WPF حالياً لتعديل الـ Bridge وفقاً لذلك.

---

## الجزء 3: إنشاء SettingsWindow

### WpfApp2\Views\SettingsWindow.xaml

الـ Window ترث من ModernWindow وتعرض:
1. **Theme Section** — Dark Blue / White toggle
2. **Accent Color Section** — color picker بسيط (3-5 ألوان جاهزة + custom)
3. **Preview Section** — معاينة مباشرة للتغيير
4. **Save/Cancel Buttons**

```xml
<controls:ModernWindow
    x:Class="Som3a_WPF_UI.Views.SettingsWindow"
    xmlns:controls="clr-namespace:Som3a_WPF_UI.Controls"
    Title="Addin Settings"
    Width="480" Height="520"
    ResizeMode="NoResize"
    WindowStartupLocation="CenterScreen">

    <Grid Margin="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>  <!-- Theme Section -->
            <RowDefinition Height="16"/>    <!-- Spacer -->
            <RowDefinition Height="Auto"/>  <!-- Accent Section -->
            <RowDefinition Height="16"/>    <!-- Spacer -->
            <RowDefinition Height="Auto"/>  <!-- Preview -->
            <RowDefinition Height="*"/>     <!-- Filler -->
            <RowDefinition Height="Auto"/>  <!-- Buttons -->
        </Grid.RowDefinitions>

        <!-- ── Section 1: Theme ── -->
        <StackPanel Grid.Row="0">
            <TextBlock Text="الثيم"
                       Foreground="{DynamicResource TextSubBrush}"
                       FontSize="{DynamicResource CaptionFontSize}"
                       Margin="0,0,0,10"/>

            <!-- Dark Blue / White Toggle Buttons -->
            <UniformGrid Columns="2" Height="72">

                <!-- Dark Blue Option -->
                <Border x:Name="DarkOption"
                        Background="{DynamicResource CardBrush}"
                        BorderBrush="{DynamicResource AccentBrush}"
                        BorderThickness="2"
                        CornerRadius="{DynamicResource MediumRadius}"
                        Margin="0,0,8,0"
                        Cursor="Hand">
                    <Border.InputBindings>
                        <MouseBinding MouseAction="LeftClick"
                                      Command="{Binding SelectDarkCommand}"/>
                    </Border.InputBindings>
                    <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                        <TextBlock Text="🌙" FontSize="20" HorizontalAlignment="Center"/>
                        <TextBlock Text="Dark Blue"
                                   Foreground="{DynamicResource TextMainBrush}"
                                   HorizontalAlignment="Center"
                                   FontSize="{DynamicResource CaptionFontSize}"/>
                    </StackPanel>
                </Border>

                <!-- White Option -->
                <Border x:Name="WhiteOption"
                        Background="{DynamicResource CardBrush}"
                        BorderBrush="{DynamicResource ControlStrokeBrush}"
                        BorderThickness="2"
                        CornerRadius="{DynamicResource MediumRadius}"
                        Margin="8,0,0,0"
                        Cursor="Hand">
                    <Border.InputBindings>
                        <MouseBinding MouseAction="LeftClick"
                                      Command="{Binding SelectWhiteCommand}"/>
                    </Border.InputBindings>
                    <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
                        <TextBlock Text="☀️" FontSize="20" HorizontalAlignment="Center"/>
                        <TextBlock Text="White"
                                   Foreground="{DynamicResource TextMainBrush}"
                                   HorizontalAlignment="Center"
                                   FontSize="{DynamicResource CaptionFontSize}"/>
                    </StackPanel>
                </Border>
            </UniformGrid>
        </StackPanel>

        <!-- ── Section 2: Accent Color ── -->
        <StackPanel Grid.Row="2">
            <TextBlock Text="لون التمييز"
                       Foreground="{DynamicResource TextSubBrush}"
                       FontSize="{DynamicResource CaptionFontSize}"
                       Margin="0,0,0,10"/>

            <WrapPanel>
                <!-- 5 ألوان جاهزة -->
                <!-- كل دائرة: Ellipse بـ Fill = اللون، Width=32, Height=32, Margin=4 -->
                <!-- Selected: Border بـ Stroke من AccentBrush -->
                <!-- الألوان: #3A86FF / #0078D4 / #FF5722 / #2ED573 / #9C27B0 -->

                <!-- Custom Color Button -->
                <Border Width="32" Height="32" Margin="4"
                        CornerRadius="16"
                        BorderBrush="{DynamicResource ControlStrokeBrush}"
                        BorderThickness="1"
                        Cursor="Hand"
                        ToolTip="Custom color">
                    <TextBlock Text="+" FontSize="18"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               Foreground="{DynamicResource TextSubBrush}"/>
                </Border>
            </WrapPanel>
        </StackPanel>

        <!-- ── Section 3: Live Preview ── -->
        <Border Grid.Row="4"
                Background="{DynamicResource CardBrush}"
                BorderBrush="{DynamicResource CardStrokeBrush}"
                BorderThickness="1"
                CornerRadius="{DynamicResource CardRadius}"
                Padding="16,12">
            <StackPanel>
                <TextBlock Text="معاينة"
                           Foreground="{DynamicResource TextSubBrush}"
                           FontSize="{DynamicResource CaptionFontSize}"
                           Margin="0,0,0,8"/>
                <StackPanel Orientation="Horizontal">
                    <Button Content="Primary" Style="{DynamicResource PrimaryButton}" Margin="0,0,8,0"/>
                    <Button Content="Default" Style="{DynamicResource BaseButton}" Margin="0,0,8,0"/>
                    <Button Content="Danger"  Style="{DynamicResource DangerButton}"/>
                </StackPanel>
            </StackPanel>
        </Border>

        <!-- ── Buttons ── -->
        <StackPanel Grid.Row="6" Orientation="Horizontal" HorizontalAlignment="Right">
            <Button Content="إلغاء"
                    Style="{DynamicResource GhostButton}"
                    Width="90" Margin="0,0,8,0"
                    Command="{Binding CancelCommand}"/>
            <Button Content="حفظ"
                    Style="{DynamicResource PrimaryButton}"
                    Width="90"
                    Command="{Binding SaveCommand}"/>
        </StackPanel>
    </Grid>
</controls:ModernWindow>
```

### WpfApp2\ViewModels\SettingsViewModel.cs
```csharp
using System.Windows.Input;
using Som3a_WPF_UI.Services;

namespace Som3a_WPF_UI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private ThemeType _selectedTheme;
        private string    _selectedAccent;

        // حفظ القيم الأصلية للـ Cancel
        private readonly ThemeType _originalTheme;
        private readonly string    _originalAccent;

        public SettingsViewModel()
        {
            var settings   = ThemeManager.GetCurrentSettings();
            _selectedTheme = settings.CurrentTheme;
            _selectedAccent = settings.AccentColor;

            // احفظ الأصل للـ Cancel
            _originalTheme  = _selectedTheme;
            _originalAccent = _selectedAccent;
        }

        public ThemeType SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                _selectedTheme = value;
                OnPropertyChanged();
                // Live preview — طبّق فوراً بدون حفظ
                ThemeManager.ApplyTheme(value);
            }
        }

        public string SelectedAccent
        {
            get => _selectedAccent;
            set
            {
                _selectedAccent = value;
                OnPropertyChanged();
                // Live preview
                ThemeManager.ChangeAccent(value);
            }
        }

        // Commands
        public ICommand SelectDarkCommand  => new RelayCommand(_ =>
            SelectedTheme = ThemeType.FluentDarkBlue);

        public ICommand SelectWhiteCommand => new RelayCommand(_ =>
            SelectedTheme = ThemeType.FluentWhite);

        public ICommand SelectAccentCommand => new RelayCommand(hex =>
            SelectedAccent = hex?.ToString() ?? "#3A86FF");

        public ICommand SaveCommand => new RelayCommand(_ =>
        {
            ThemeManager.SaveSettings();
            CloseWindow();
        });

        public ICommand CancelCommand => new RelayCommand(_ =>
        {
            // ارجع للقيم الأصلية
            ThemeManager.ApplyTheme(_originalTheme);
            ThemeManager.ChangeAccent(_originalAccent);
            CloseWindow();
        });

        public Action CloseAction { get; set; }
        private void CloseWindow() => CloseAction?.Invoke();
    }
}
```

### WpfApp2\Views\SettingsWindow.xaml.cs
```csharp
public partial class SettingsWindow : ModernWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        var vm = new SettingsViewModel();
        vm.CloseAction = () => Close();
        DataContext = vm;
    }
}
```

---

## الـ Accent Color Circles (تفاصيل التنفيذ)

في الـ WrapPanel، أنشئ 5 دوائر بهذه الألوان:

| اللون | الـ Hex | الاسم |
|---|---|---|
| 🔵 | #3A86FF | Blue (افتراضي) |
| 🟦 | #0078D4 | Microsoft Blue |
| 🟠 | #FF5722 | Orange |
| 🟢 | #2ED573 | Green |
| 🟣 | #9C27B0 | Purple |

كل دائرة:
```xml
<Border Width="36" Height="36" Margin="4"
        CornerRadius="18"
        Background="#3A86FF"
        Cursor="Hand">
    <Border.InputBindings>
        <MouseBinding MouseAction="LeftClick"
                      Command="{Binding DataContext.SelectAccentCommand,
                                RelativeSource={RelativeSource AncestorType=Window}}"
                      CommandParameter="#3A86FF"/>
    </Border.InputBindings>
    <!-- Checkmark لما يكون selected -->
    <TextBlock Text="✓"
               Foreground="White"
               HorizontalAlignment="Center"
               VerticalAlignment="Center"
               Visibility="{Binding DataContext.SelectedAccent,
                            Converter=..., ConverterParameter='#3A86FF',
                            RelativeSource={RelativeSource AncestorType=Window}}"/>
</Border>
```

---

## ترتيب التنفيذ داخل الـ Session

1. اقرأ الملفات المذكورة أولاً
2. أنشئ SettingsViewModel.cs
3. أنشئ SettingsWindow.xaml + .cs
4. أنشئ AddinSettingsBridge.cs
5. أضف الزر في الـ Ribbon
6. اربط الزر بـ AddinSettingsBridge.OpenSettingsWindow()
7. Build وتأكد من 0 errors
8. اختبر: اضغط الزر من الـ Ribbon → الـ Window بتفتح؟

## قائمة التحقق النهائية
□ زر "Settings" ظاهر في الـ Ribbon بأيقونة مناسبة
□ الزر يفتح SettingsWindow
□ لو SettingsWindow مفتوحة بالفعل، يجيبها للأمام (مش يفتح نسخة تانية)
□ اختيار Dark Blue يغير الثيم فوراً (Live preview)
□ اختيار White يغير الثيم فوراً
□ اختيار Accent color يغير اللون فوراً
□ Cancel يرجع للإعدادات الأصلية
□ Save يحفظ في theme.json ويغلق الـ Window
□ بعد إعادة تشغيل Excel، الإعدادات المحفوظة بترجع

## تنبيهات VSTO محددة
- لا تستخدم ShowDialog() من الـ Ribbon Thread — استخدم Show() فقط
- لو ظهر خطأ "The calling thread must be STA" — الـ Bridge بيحله
- لو الـ WPF Application مش موجودة (Application.Current == null)، الـ Bridge بيشغّلها في Thread منفصل
- ممنوع استخدام Dispatcher.Invoke من داخل الـ Ribbon callback مباشرة بدون الـ Bridge
```

---

*Som3a Addin 2026 · UI Architecture Session Prompts v1.0*
