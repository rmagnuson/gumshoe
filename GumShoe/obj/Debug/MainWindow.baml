   M S B A M L   `   `   `  ����  GumShoe    GumShoe.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  
MainWindow��5   	    � SizeToContent$ Height=�6   $	��525��63   .��5      ]� .+�5   
   o� $	��Top=�6   .@�5	      m� $��_Spider��6   .@�5
      m� -   5        $��	_Settings��5
      6.   m� 5      -   5        $	��_Go��5      6(   5      5      �� 5   
   .+�5      &   System.Windows.Controls.DataGrid  -   5          Name$ WebNodesDataGrid��5         AutoGenerateColumns .  $��Stretch=�5      $��Stretch=�61   $	��200��6M   $��
10,10,10,0q�6Z      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$	 Url��6)   1  +System.Windows.Controls.DataGridBoundColumn   Binding 66   �� *NodeUri.AbsoluteUri+  5      $ Crawled��6)    6:   �� *	IsCrawled+  5      $ Parsed��6)    69   �� *
IsParsed+5        5      -   5        $ ChatterVolumeDataGrid��5       . $��Stretch=�5      $��Stretch=�61   $	��100��6M   $��
10,5,10,10q�6Z   5      5      5      