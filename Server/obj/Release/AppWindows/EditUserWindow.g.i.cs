﻿#pragma checksum "..\..\..\AppWindows\EditUserWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "1A0E391333B16207C7834B3E9DBF785422EF92BD"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.42000
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using DataServerGUI.AppWindows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace DataServerGUI.AppWindows {
    
    
    /// <summary>
    /// EditUserWindow
    /// </summary>
    public partial class EditUserWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 28 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox IDTextBox;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox NameTextBox;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SurnameTextBox;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton radioButtonPasswor;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox Password1;
        
        #line default
        #line hidden
        
        
        #line 43 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.PasswordBox Password2;
        
        #line default
        #line hidden
        
        
        #line 46 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox PrivilegesComboBox;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox Space_available;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\AppWindows\EditUserWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Zatwierdz;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/DataServerGUI;component/appwindows/edituserwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\AppWindows\EditUserWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.IDTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 28 "..\..\..\AppWindows\EditUserWindow.xaml"
            this.IDTextBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.IDTextBox_TextChanged);
            
            #line default
            #line hidden
            
            #line 28 "..\..\..\AppWindows\EditUserWindow.xaml"
            this.IDTextBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.IDTextBox_PreviewTextInput);
            
            #line default
            #line hidden
            return;
            case 2:
            this.NameTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.SurnameTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.radioButtonPasswor = ((System.Windows.Controls.RadioButton)(target));
            
            #line 37 "..\..\..\AppWindows\EditUserWindow.xaml"
            this.radioButtonPasswor.Checked += new System.Windows.RoutedEventHandler(this.Password_Change_RadioButton);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Password1 = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 6:
            this.Password2 = ((System.Windows.Controls.PasswordBox)(target));
            return;
            case 7:
            this.PrivilegesComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 8:
            this.Space_available = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.Zatwierdz = ((System.Windows.Controls.Button)(target));
            
            #line 56 "..\..\..\AppWindows\EditUserWindow.xaml"
            this.Zatwierdz.Click += new System.Windows.RoutedEventHandler(this.Edit_User_Save);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

