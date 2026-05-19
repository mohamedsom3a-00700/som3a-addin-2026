using Microsoft.Office.Tools.Ribbon;
using Som3a_Addin_2026;
using Som3a_WPF_UI;
using System;
namespace Som3a_Addin_2026
{
    partial class Ribbon1 : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon1()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.comparsion = this.Factory.CreateRibbonButton();
            this.Com_Xer = this.Factory.CreateRibbonButton();
            this.button1 = this.Factory.CreateRibbonButton();
            this.group2 = this.Factory.CreateRibbonGroup();
            this.btnDailyReport = this.Factory.CreateRibbonButton();
            this.button22 = this.Factory.CreateRibbonButton();
            this.Tools = this.Factory.CreateRibbonGroup();
            this.btnProjectAnalysis = this.Factory.CreateRibbonButton();
            this.group3 = this.Factory.CreateRibbonGroup();
            this.Coloringwbs = this.Factory.CreateRibbonButton();
            this.GroupWbs = this.Factory.CreateRibbonButton();
            this.group12 = this.Factory.CreateRibbonGroup();
            this.addin_setting = this.Factory.CreateRibbonButton();
            this.color_setting = this.Factory.CreateRibbonButton();
            this.group5 = this.Factory.CreateRibbonGroup();
            this.Float_Path = this.Factory.CreateRibbonButton();
            this.Xer_Editor = this.Factory.CreateRibbonButton();
            this.button10 = this.Factory.CreateRibbonButton();
            this.button17 = this.Factory.CreateRibbonButton();
            this.button16 = this.Factory.CreateRibbonButton();
            this.group6 = this.Factory.CreateRibbonGroup();
            this.button6 = this.Factory.CreateRibbonButton();
            this.button7 = this.Factory.CreateRibbonButton();
            this.button8 = this.Factory.CreateRibbonButton();
            this.button15 = this.Factory.CreateRibbonButton();
            this.button25 = this.Factory.CreateRibbonButton();
            this.button26 = this.Factory.CreateRibbonButton();
            this.button27 = this.Factory.CreateRibbonButton();
            this.button28 = this.Factory.CreateRibbonButton();
            this.button29 = this.Factory.CreateRibbonButton();
            this.group7 = this.Factory.CreateRibbonGroup();
            this.button9 = this.Factory.CreateRibbonButton();
            this.button14 = this.Factory.CreateRibbonButton();
            this.group10 = this.Factory.CreateRibbonGroup();
            this.button30 = this.Factory.CreateRibbonButton();
            this.button31 = this.Factory.CreateRibbonButton();
            this.button32 = this.Factory.CreateRibbonButton();
            this.button33 = this.Factory.CreateRibbonButton();
            this.group8 = this.Factory.CreateRibbonGroup();
            this.button12 = this.Factory.CreateRibbonButton();
            this.button13 = this.Factory.CreateRibbonButton();
            this.button20 = this.Factory.CreateRibbonButton();
            this.button21 = this.Factory.CreateRibbonButton();
            this.button23 = this.Factory.CreateRibbonButton();
            this.button24 = this.Factory.CreateRibbonButton();
            this.group9 = this.Factory.CreateRibbonGroup();
            this.button3 = this.Factory.CreateRibbonButton();
            this.button4 = this.Factory.CreateRibbonButton();
            this.button18 = this.Factory.CreateRibbonButton();
            this.button19 = this.Factory.CreateRibbonButton();
            this.group4 = this.Factory.CreateRibbonGroup();
            this.PreForReload = this.Factory.CreateRibbonButton();
            this.tab2 = this.Factory.CreateRibbonTab();
            this.group11 = this.Factory.CreateRibbonGroup();
            this.btnUnmergeFillDown = this.Factory.CreateRibbonButton();
            this.ExportPaletteHex = this.Factory.CreateRibbonButton();
            this.button2 = this.Factory.CreateRibbonButton();
            this.btnLinksManager = this.Factory.CreateRibbonButton();
            this.btnSafeClone = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.group1.SuspendLayout();
            this.group2.SuspendLayout();
            this.Tools.SuspendLayout();
            this.group3.SuspendLayout();
            this.group12.SuspendLayout();
            this.group5.SuspendLayout();
            this.group6.SuspendLayout();
            this.group7.SuspendLayout();
            this.group10.SuspendLayout();
            this.group8.SuspendLayout();
            this.group9.SuspendLayout();
            this.group4.SuspendLayout();
            this.tab2.SuspendLayout();
            this.group11.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.Groups.Add(this.group1);
            this.tab1.Groups.Add(this.group2);
            this.tab1.Groups.Add(this.Tools);
            this.tab1.Groups.Add(this.group3);
            this.tab1.Groups.Add(this.group12);
            this.tab1.Groups.Add(this.group5);
            this.tab1.Groups.Add(this.group6);
            this.tab1.Groups.Add(this.group7);
            this.tab1.Groups.Add(this.group10);
            this.tab1.Groups.Add(this.group8);
            this.tab1.Groups.Add(this.group9);
            this.tab1.Groups.Add(this.group4);
            this.tab1.Label = "Som3a Planning Tools";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Items.Add(this.comparsion);
            this.group1.Items.Add(this.Com_Xer);
            this.group1.Items.Add(this.button1);
            this.group1.Label = "Comparsion";
            this.group1.Name = "group1";
            // 
            // comparsion
            // 
            this.comparsion.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.comparsion.Image = global::Som3a_Addin_2026.Properties.Resources.stock_vector_comparison_icon_and_symbol_438145465;
            this.comparsion.Label = "Comparsion";
            this.comparsion.Name = "comparsion";
            this.comparsion.ShowImage = true;
            this.comparsion.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.comparsion_Click);
            // 
            // Com_Xer
            // 
            this.Com_Xer.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.Com_Xer.Image = global::Som3a_Addin_2026.Properties.Resources._3781852;
            this.Com_Xer.Label = "Comparsion by Xer";
            this.Com_Xer.Name = "Com_Xer";
            this.Com_Xer.ShowImage = true;
            this.Com_Xer.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Com_Xer_Click);
            // 
            // button1
            // 
            this.button1.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.button1.Image = global::Som3a_Addin_2026.Properties.Resources._3502688;
            this.button1.Label = "Trades Codes";
            this.button1.Name = "button1";
            this.button1.ShowImage = true;
            this.button1.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button1_Click);
            // 
            // group2
            // 
            this.group2.Items.Add(this.btnDailyReport);
            this.group2.Items.Add(this.button22);
            this.group2.Label = "Reporting";
            this.group2.Name = "group2";
            // 
            // btnDailyReport
            // 
            this.btnDailyReport.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnDailyReport.Image = global::Som3a_Addin_2026.Properties.Resources._199_1998376_email_marketing_icon_daily_report_icon_png;
            this.btnDailyReport.Label = "Daily Report";
            this.btnDailyReport.Name = "btnDailyReport";
            this.btnDailyReport.ShowImage = true;
            this.btnDailyReport.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnDailyReport_Click);
            // 
            // button22
            // 
            this.button22.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.button22.Image = global::Som3a_Addin_2026.Properties.Resources._8276278;
            this.button22.Label = "Weekly Report Generator";
            this.button22.Name = "button22";
            this.button22.ShowImage = true;
            // 
            // Tools
            // 
            this.Tools.Items.Add(this.btnProjectAnalysis);
            this.Tools.Label = "Analysis Tools";
            this.Tools.Name = "Tools";
            // 
            // btnProjectAnalysis
            // 
            this.btnProjectAnalysis.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnProjectAnalysis.Image = global::Som3a_Addin_2026.Properties.Resources.cf4c1cb044c54bfc856f2f370df8e395;
            this.btnProjectAnalysis.Label = "Revised Baseline (Split Activity)";
            this.btnProjectAnalysis.Name = "btnProjectAnalysis";
            this.btnProjectAnalysis.ShowImage = true;
            this.btnProjectAnalysis.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnProjectAnalysis_Click);
            // 
            // group3
            // 
            this.group3.Items.Add(this.Coloringwbs);
            this.group3.Items.Add(this.GroupWbs);
            this.group3.Label = "Formats";
            this.group3.Name = "group3";
            // 
            // Coloringwbs
            // 
            this.Coloringwbs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.Coloringwbs.Image = global::Som3a_Addin_2026.Properties.Resources.colour_wheel;
            this.Coloringwbs.Label = "Coloring WBS";
            this.Coloringwbs.Name = "Coloringwbs";
            this.Coloringwbs.ShowImage = true;
            this.Coloringwbs.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Coloringwbs_Click);
            // 
            // GroupWbs
            // 
            this.GroupWbs.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.GroupWbs.Image = global::Som3a_Addin_2026.Properties.Resources._1302070;
            this.GroupWbs.Label = "Group WBS";
            this.GroupWbs.Name = "GroupWbs";
            this.GroupWbs.ShowImage = true;
            this.GroupWbs.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.GroupWbs_Click);
            // 
            // group12
            // 
            this.group12.Items.Add(this.addin_setting);
            this.group12.Items.Add(this.color_setting);
            this.group12.Label = "Setting";
            this.group12.Name = "group12";
            // 
            // addin_setting
            // 
            this.addin_setting.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.addin_setting.Image = global::Som3a_Addin_2026.Properties.Resources.settings;
            this.addin_setting.Label = "Add in Setting";
            this.addin_setting.Name = "addin_setting";
            this.addin_setting.ShowImage = true;
            this.addin_setting.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.addin_setting_Click);
            // 
            // color_setting
            // 
            this.color_setting.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.color_setting.Image = global::Som3a_Addin_2026.Properties.Resources._6915924;
            this.color_setting.Label = "Color WBS Setting";
            this.color_setting.Name = "color_setting";
            this.color_setting.ShowImage = true;
            this.color_setting.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.color_setting_Click);
            // 
            // group5
            // 
            this.group5.Items.Add(this.Float_Path);
            this.group5.Items.Add(this.Xer_Editor);
            this.group5.Items.Add(this.button10);
            this.group5.Items.Add(this.button17);
            this.group5.Items.Add(this.button16);
            this.group5.Label = "Schedule Health";
            this.group5.Name = "group5";
            // 
            // Float_Path
            // 
            this.Float_Path.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.Float_Path.Image = global::Som3a_Addin_2026.Properties.Resources._7171906;
            this.Float_Path.Label = "Float Path Analyzer";
            this.Float_Path.Name = "Float_Path";
            this.Float_Path.ShowImage = true;
            this.Float_Path.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Float_Path_Click);
            // 
            // Xer_Editor
            // 
            this.Xer_Editor.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.Xer_Editor.Image = global::Som3a_Addin_2026.Properties.Resources._800a0207_9614_4cb2_8c0b_8daf79416ee0;
            this.Xer_Editor.Label = "Xer Editor";
            this.Xer_Editor.Name = "Xer_Editor";
            this.Xer_Editor.ShowImage = true;
            this.Xer_Editor.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.Xer_Editor_Click);
            // 
            // button10
            // 
            this.button10.Label = "Schedule Health Check";
            this.button10.Name = "button10";
            this.button10.ShowImage = true;
            // 
            // button17
            // 
            this.button17.Label = "Critical Event Tracker";
            this.button17.Name = "button17";
            this.button17.ShowImage = true;
            // 
            // button16
            // 
            this.button16.Label = "Windows Analyzer";
            this.button16.Name = "button16";
            this.button16.ShowImage = true;
            // 
            // group6
            // 
            this.group6.Items.Add(this.button6);
            this.group6.Items.Add(this.button7);
            this.group6.Items.Add(this.button8);
            this.group6.Items.Add(this.button15);
            this.group6.Items.Add(this.button25);
            this.group6.Items.Add(this.button26);
            this.group6.Items.Add(this.button27);
            this.group6.Items.Add(this.button28);
            this.group6.Items.Add(this.button29);
            this.group6.Label = "Delay & Claims";
            this.group6.Name = "group6";
            // 
            // button6
            // 
            this.button6.Label = "Project Analysis";
            this.button6.Name = "button6";
            this.button6.ShowImage = true;
            // 
            // button7
            // 
            this.button7.Label = "Delay Analysis";
            this.button7.Name = "button7";
            this.button7.ShowImage = true;
            // 
            // button8
            // 
            this.button8.Label = "Event Milestone";
            this.button8.Name = "button8";
            this.button8.ShowImage = true;
            // 
            // button15
            // 
            this.button15.Label = "Fragnet Builder";
            this.button15.Name = "button15";
            this.button15.ShowImage = true;
            // 
            // button25
            // 
            this.button25.Label = "Risk Register Generator";
            this.button25.Name = "button25";
            this.button25.ShowImage = true;
            // 
            // button26
            // 
            this.button26.Label = "Variation Tracker";
            this.button26.Name = "button26";
            this.button26.ShowImage = true;
            // 
            // button27
            // 
            this.button27.Label = "Payment Certificate Generator";
            this.button27.Name = "button27";
            this.button27.ShowImage = true;
            // 
            // button28
            // 
            this.button28.Label = "What-if Simulator";
            this.button28.Name = "button28";
            this.button28.ShowImage = true;
            // 
            // button29
            // 
            this.button29.Label = "Power BI Connector";
            this.button29.Name = "button29";
            this.button29.ShowImage = true;
            // 
            // group7
            // 
            this.group7.Items.Add(this.button9);
            this.group7.Items.Add(this.button14);
            this.group7.Label = "Data Cleaning";
            this.group7.Name = "group7";
            // 
            // button9
            // 
            this.button9.Label = "P6 Data Validator";
            this.button9.Name = "button9";
            this.button9.ShowImage = true;
            // 
            // button14
            // 
            this.button14.Label = "Excel Schedule Sanitizer";
            this.button14.Name = "button14";
            this.button14.ShowImage = true;
            // 
            // group10
            // 
            this.group10.Items.Add(this.button30);
            this.group10.Items.Add(this.button31);
            this.group10.Items.Add(this.button32);
            this.group10.Items.Add(this.button33);
            this.group10.Label = "Management";
            this.group10.Name = "group10";
            // 
            // button30
            // 
            this.button30.Label = "Executive Dashboard";
            this.button30.Name = "button30";
            this.button30.ShowImage = true;
            this.button30.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button30_Click);
            // 
            // button31
            // 
            this.button31.Label = "Risk Summary";
            this.button31.Name = "button31";
            this.button31.ShowImage = true;
            // 
            // button32
            // 
            this.button32.Label = "Forecast Completion";
            this.button32.Name = "button32";
            this.button32.ShowImage = true;
            // 
            // button33
            // 
            this.button33.Label = "Key Decisions Log";
            this.button33.Name = "button33";
            this.button33.ShowImage = true;
            // 
            // group8
            // 
            this.group8.Items.Add(this.button12);
            this.group8.Items.Add(this.button13);
            this.group8.Items.Add(this.button20);
            this.group8.Items.Add(this.button21);
            this.group8.Items.Add(this.button23);
            this.group8.Items.Add(this.button24);
            this.group8.Label = "Lookahead";
            this.group8.Name = "group8";
            // 
            // button12
            // 
            this.button12.Label = "Monthly Dashboard";
            this.button12.Name = "button12";
            this.button12.ShowImage = true;
            // 
            // button13
            // 
            this.button13.Label = "Lookahead Consistency Checker";
            this.button13.Name = "button13";
            this.button13.ShowImage = true;
            // 
            // button20
            // 
            this.button20.Label = "E1 Auto Generator";
            this.button20.Name = "button20";
            this.button20.ShowImage = true;
            // 
            // button21
            // 
            this.button21.Label = "E2 Forecast Engine";
            this.button21.Name = "button21";
            this.button21.ShowImage = true;
            // 
            // button23
            // 
            this.button23.Label = "Resource Histogram Generator";
            this.button23.Name = "button23";
            this.button23.ShowImage = true;
            // 
            // button24
            // 
            this.button24.Label = "Productivity Tracker";
            this.button24.Name = "button24";
            this.button24.ShowImage = true;
            // 
            // group9
            // 
            this.group9.Items.Add(this.button3);
            this.group9.Items.Add(this.button4);
            this.group9.Items.Add(this.button18);
            this.group9.Items.Add(this.button19);
            this.group9.Label = "AI Tools";
            this.group9.Name = "group9";
            // 
            // button3
            // 
            this.button3.Label = " Ai Agent";
            this.button3.Name = "button3";
            this.button3.ShowImage = true;
            // 
            // button4
            // 
            this.button4.Label = "Connect DB P6";
            this.button4.Name = "button4";
            this.button4.ShowImage = true;
            // 
            // button18
            // 
            this.button18.Label = "AI Progress Explainer";
            this.button18.Name = "button18";
            this.button18.ShowImage = true;
            // 
            // button19
            // 
            this.button19.Label = "AI Risk Detector";
            this.button19.Name = "button19";
            this.button19.ShowImage = true;
            // 
            // group4
            // 
            this.group4.Items.Add(this.PreForReload);
            this.group4.Label = "Info";
            this.group4.Name = "group4";
            // 
            // PreForReload
            // 
            this.PreForReload.Label = "Prepare for Reload";
            this.PreForReload.Name = "PreForReload";
            this.PreForReload.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.PreForReload_Click);
            // 
            // tab2
            // 
            this.tab2.Groups.Add(this.group11);
            this.tab2.Label = "Som3a Helper Tools";
            this.tab2.Name = "tab2";
            // 
            // group11
            // 
            this.group11.Items.Add(this.btnUnmergeFillDown);
            this.group11.Items.Add(this.ExportPaletteHex);
            this.group11.Items.Add(this.button2);
            this.group11.Items.Add(this.btnLinksManager);
            this.group11.Items.Add(this.btnSafeClone);
            this.group11.Label = "Tools";
            this.group11.Name = "group11";
            // 
            // btnUnmergeFillDown
            // 
            this.btnUnmergeFillDown.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnUnmergeFillDown.Image = global::Som3a_Addin_2026.Properties.Resources.editable_unmerge_table_cell_vector_260nw_2568331085;
            this.btnUnmergeFillDown.Label = "Unmerage and Fill Down";
            this.btnUnmergeFillDown.Name = "btnUnmergeFillDown";
            this.btnUnmergeFillDown.ShowImage = true;
            this.btnUnmergeFillDown.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnUnmergeFillDown_Click);
            // 
            // ExportPaletteHex
            // 
            this.ExportPaletteHex.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.ExportPaletteHex.Image = global::Som3a_Addin_2026.Properties.Resources._9604087;
            this.ExportPaletteHex.Label = "Get Cell Color";
            this.ExportPaletteHex.Name = "ExportPaletteHex";
            this.ExportPaletteHex.ShowImage = true;
            this.ExportPaletteHex.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ExportPaletteHex_Click);
            // 
            // button2
            // 
            this.button2.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.button2.Image = global::Som3a_Addin_2026.Properties.Resources._3892368;
            this.button2.Label = "Fix Colors Charts";
            this.button2.Name = "button2";
            this.button2.ShowImage = true;
            this.button2.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.button2_Click);
            // 
            // btnLinksManager
            // 
            this.btnLinksManager.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnLinksManager.Image = global::Som3a_Addin_2026.Properties.Resources.links;
            this.btnLinksManager.Label = "Links Mangers";
            this.btnLinksManager.Name = "btnLinksManager";
            this.btnLinksManager.ShowImage = true;
            this.btnLinksManager.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnLinksManager_Click);
            // 
            // btnSafeClone
            // 
            this.btnSafeClone.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.btnSafeClone.Image = global::Som3a_Addin_2026.Properties.Resources.png_clipart_computer_icons_copying_icon_design_transfer_icon_text_logo;
            this.btnSafeClone.Label = "Safe Clone Workbook";
            this.btnSafeClone.Name = "btnSafeClone";
            this.btnSafeClone.ShowImage = true;
            // 
            // Ribbon1
            // 
            this.Name = "Ribbon1";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tab1);
            this.Tabs.Add(this.tab2);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon1_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.group1.ResumeLayout(false);
            this.group1.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();
            this.Tools.ResumeLayout(false);
            this.Tools.PerformLayout();
            this.group3.ResumeLayout(false);
            this.group3.PerformLayout();
            this.group12.ResumeLayout(false);
            this.group12.PerformLayout();
            this.group5.ResumeLayout(false);
            this.group5.PerformLayout();
            this.group6.ResumeLayout(false);
            this.group6.PerformLayout();
            this.group7.ResumeLayout(false);
            this.group7.PerformLayout();
            this.group10.ResumeLayout(false);
            this.group10.PerformLayout();
            this.group8.ResumeLayout(false);
            this.group8.PerformLayout();
            this.group9.ResumeLayout(false);
            this.group9.PerformLayout();
            this.group4.ResumeLayout(false);
            this.group4.PerformLayout();
            this.tab2.ResumeLayout(false);
            this.tab2.PerformLayout();
            this.group11.ResumeLayout(false);
            this.group11.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton comparsion;
        internal RibbonGroup group2;
        internal RibbonButton button1;
        internal RibbonButton btnDailyReport;
        internal RibbonGroup Tools;
        internal RibbonButton btnProjectAnalysis;
        internal RibbonGroup group3;
        internal RibbonButton GroupWbs;
        internal RibbonButton Coloringwbs;
        internal RibbonGroup group4;
        internal RibbonButton PreForReload;
        internal RibbonButton button3;
        internal RibbonButton button4;
        internal RibbonButton Xer_Editor;
        internal RibbonButton button6;
        internal RibbonButton button7;
        internal RibbonButton button8;
        internal RibbonButton button9;
        internal RibbonButton button10;
        internal RibbonButton Float_Path;
        internal RibbonButton button12;
        internal RibbonButton button13;
        internal RibbonButton button15;
        internal RibbonButton button16;
        internal RibbonButton button17;
        internal RibbonGroup group5;
        internal RibbonGroup group6;
        internal RibbonGroup group7;
        internal RibbonGroup group8;
        internal RibbonGroup group9;
        internal RibbonButton button14;
        internal RibbonButton button18;
        internal RibbonButton button19;
        internal RibbonButton button22;
        internal RibbonButton button25;
        internal RibbonButton button26;
        internal RibbonButton button27;
        internal RibbonButton button28;
        internal RibbonButton button20;
        internal RibbonButton button21;
        internal RibbonButton button23;
        internal RibbonButton button24;
        internal RibbonButton button29;
        internal RibbonGroup group10;
        internal RibbonButton button30;
        internal RibbonButton button31;
        internal RibbonButton button32;
        internal RibbonButton button33;
        private RibbonTab tab2;
        internal RibbonGroup group11;
        internal RibbonButton btnUnmergeFillDown;
        internal RibbonButton ExportPaletteHex;
        internal RibbonButton button2;
        internal RibbonButton btnLinksManager;
        internal RibbonButton color_setting;
        internal RibbonButton btnSafeClone;
        internal RibbonButton Com_Xer;
        internal RibbonGroup group12;
        internal RibbonButton addin_setting;
    }
    partial class ThisRibbonCollection
    {
        internal Ribbon1 Ribbon1
        {
            get { return this.GetRibbon<Ribbon1>(); }
        }
    }
}

