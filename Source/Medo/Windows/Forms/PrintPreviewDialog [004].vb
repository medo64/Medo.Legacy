'Josip Medved <jmedved@jmedved.com> http://www.jmedved.com 

'2008-01-03: New version. 
'2008-01-07: Fixed bug that caused all keys to close form instead of just Escape. 
'2008-01-22: Returns DialogResult for ShowDialog methods. 


Imports System.Windows.Forms

Namespace Medo.Windows.Forms

	''' <summary> 
	''' Represents a dialog box form that contains a System.Windows.Forms.PrintPreviewControl. 
	''' </summary> 
	Public Class PrintPreviewDialog
		Inherits System.Windows.Forms.PrintPreviewDialog

		''' <summary> 
		''' Initializes new instance. 
		''' </summary> 
		Public Sub New(ByVal document As System.Drawing.Printing.PrintDocument)
			Me.New()
			MyBase.Document = document
		End Sub

		''' <summary> 
		''' Initializes new instance. 
		''' </summary> 
		Public Sub New()
			MyBase.New()
			Dim ppdForm As System.Windows.Forms.Form = TryCast(Me, System.Windows.Forms.Form)
			If ppdForm IsNot Nothing Then
				AddHandler ppdForm.KeyPress, AddressOf ppdForm_KeyPress
				AddHandler ppdForm.Load, AddressOf ppdForm_Load
				ppdForm.Icon = Resources.TransparentIcon
				ppdForm.ShowIcon = True
				ppdForm.Text = Resources.Caption
				ppdForm.KeyPreview = True
				For i As Integer = 0 To ppdForm.Controls.Count - 1
					Dim iControl As System.Windows.Forms.Control = ppdForm.Controls(i)
					Dim iToolstrip As System.Windows.Forms.ToolStrip = TryCast(iControl, System.Windows.Forms.ToolStrip)
					If iToolstrip IsNot Nothing Then
						For j As Integer = 0 To iToolstrip.Items.Count - 1
							Dim jItem As System.Windows.Forms.ToolStripItem = iToolstrip.Items(j)
							Select Case jItem.Name
								Case "printToolStripButton"
									jItem.Text = Resources.Print
									Exit Select
								Case "zoomToolStripSplitButton"
									jItem.Text = Resources.Zoom
									Exit Select
								Case "onepageToolStripButton"
									jItem.Text = Resources.OnePage
									Exit Select
								Case "twopagesToolStripButton"
									jItem.Text = Resources.TwoPages
									Exit Select
								Case "threepagesToolStripButton"
									jItem.Text = Resources.ThreePages
									Exit Select
								Case "fourpagesToolStripButton"
									jItem.Text = Resources.FourPages
									Exit Select
								Case "sixpagesToolStripButton"
									jItem.Text = Resources.SixPages
									Exit Select
								Case "closeToolStripButton"
									jItem.Text = Resources.Close
									Exit Select
								Case "pageToolStripLabel"
									jItem.Text = Resources.Page
									Exit Select
								Case Else
									Exit Select
									'switch 
							End Select
							'for(j) 
						Next
						'if 
					End If
					'for(i) 
				Next
				'if 
			End If
		End Sub


		''' <summary> 
		''' Displays the control to the user. 
		''' </summary> 
		Public Shadows Sub Show()
			Me.Show(Nothing)
		End Sub

		''' <summary> 
		''' Shows the form with the specified owner to the user. 
		''' </summary> 
		''' <param name="owner">Any object that implements System.Windows.Forms.IWin32Window and represents the top-level window that will own this form.</param> 
		''' <exception cref="System.ArgumentException">The form specified in the owner parameter is the same as the form being shown.</exception> 
		Public Shadows Sub Show(ByVal owner As System.Windows.Forms.IWin32Window)
			Dim ppdForm As System.Windows.Forms.Form = TryCast(Me, System.Windows.Forms.Form)
			If owner IsNot Nothing Then
				If ppdForm IsNot Nothing Then
					ppdForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
				End If
				MyBase.Show(owner)
			Else
				If ppdForm IsNot Nothing Then
					ppdForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
				End If
				MyBase.Show()
			End If
		End Sub

		''' <summary> 
		''' Shows the form as a modal dialog box with the currently active window set as its owner. 
		''' </summary> 
		''' <return>One of the System.Windows.Forms.DialogResult values.</return> 
		''' <exception cref="System.InvalidOperationException">The form being shown is already visible.-or- The form being shown is disabled.-or- The form being shown is not a top-level window.-or- The form being shown as a dialog box is already a modal form.</exception> 
		''' <exception cref="System.InvalidOperationException">The current process is not running in user interactive mode. For more information, see System.Windows.Forms.SystemInformation.UserInteractive.</exception> 
		''' <exception cref="System.ArgumentException">The form specified in the owner parameter is the same as the form being shown.</exception> 
		Public Shadows Function ShowDialog() As DialogResult
			Return Me.ShowDialog(Nothing)
		End Function

		''' <summary> 
		''' Shows the form as a modal dialog with the specified owner. 
		''' </summary> 
		''' <param name="owner">Any object that implements System.Windows.Forms.IWin32Window that represents the top-level window that will own the modal dialog.</param> 
		''' <returns>One of the System.Windows.Forms.DialogResult values.</returns> 
		''' <exception cref="System.InvalidOperationException">The form being shown is already visible.-or- The form being shown is disabled.-or- The form being shown is not a top-level window.-or- The form being shown as a dialog box is already a modal form.</exception> 
		''' <exception cref="System.InvalidOperationException">The current process is not running in user interactive mode. For more information, see System.Windows.Forms.SystemInformation.UserInteractive.</exception> 
		''' <exception cref="System.ArgumentException">The form specified in the owner parameter is the same as the form being shown.</exception> 
		Public Shadows Function ShowDialog(ByVal owner As System.Windows.Forms.IWin32Window) As DialogResult
			Dim ppdForm As System.Windows.Forms.Form = TryCast(Me, System.Windows.Forms.Form)
			If owner IsNot Nothing Then
				If ppdForm IsNot Nothing Then
					ppdForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
				End If
				Return MyBase.ShowDialog(owner)
			Else
				If ppdForm IsNot Nothing Then
					ppdForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
				End If
				Return MyBase.ShowDialog()
			End If
		End Function


#Region "Events"

		Private Sub ppdForm_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
			If e.KeyChar = System.Convert.ToChar(27) Then
				Dim form As System.Windows.Forms.Form = TryCast(sender, System.Windows.Forms.Form)
				If form IsNot Nothing Then
					form.Close()
				End If
			End If
		End Sub

		Private Sub ppdForm_Load(ByVal sender As Object, ByVal e As System.EventArgs)
			Dim form As System.Windows.Forms.Form = TryCast(sender, System.Windows.Forms.Form)
			If form IsNot Nothing Then
				Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
			End If
		End Sub

#End Region


		Private NotInheritable Class Resources
			Private Sub New()
			End Sub

			Friend Shared ReadOnly Property Caption() As String
				Get
					Return GetInCurrentLanguage("Print preview...", "Pregled ispisa...")
				End Get
			End Property

			Friend Shared ReadOnly Property Print() As String
				Get
					Return GetInCurrentLanguage("Print", "Ispis")
				End Get
			End Property

			Friend Shared ReadOnly Property Zoom() As String
				Get
					Return GetInCurrentLanguage("Zoom", "Poveæanje")
				End Get
			End Property

			Friend Shared ReadOnly Property OnePage() As String
				Get
					Return GetInCurrentLanguage("One page", "Jedna stranica")
				End Get
			End Property

			Friend Shared ReadOnly Property TwoPages() As String
				Get
					Return GetInCurrentLanguage("Two pages", "Dvije stranice")
				End Get
			End Property

			Friend Shared ReadOnly Property ThreePages() As String
				Get
					Return GetInCurrentLanguage("Three pages", "Tri stranice")
				End Get
			End Property

			Friend Shared ReadOnly Property FourPages() As String
				Get
					Return GetInCurrentLanguage("Four pages", "Èetiri stranice")
				End Get
			End Property

			Friend Shared ReadOnly Property SixPages() As String
				Get
					Return GetInCurrentLanguage("Six pages", "Šest stranica")
				End Get
			End Property

			Friend Shared ReadOnly Property Close() As String
				Get
					Return GetInCurrentLanguage("&Close", "&Zatvori")
				End Get
			End Property

			Friend Shared ReadOnly Property Page() As String
				Get
					Return GetInCurrentLanguage("&Page", "&Stranica")
				End Get
			End Property

			Friend Shared ReadOnly Property TransparentIcon() As System.Drawing.Icon
				Get
					Dim iconBuffer As Byte() = System.Convert.FromBase64String("AAABAAEAEBAQAAAABAAoAQAAFgAAACgAAAAQAAAAIAAAAAEABAAAAAAAgAAAAAAAAAAAAAAAEAAAABAAAAAAAAAAAACAAACAAAAAgIAAgAAAAIAAgACAgAAAgICAAMDAwAAAAP8AAP8AAAD//wD/AAAA/wD/AP//AAD///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA//8AAP//AAD//wAA")
					Return New System.Drawing.Icon(New System.IO.MemoryStream(iconBuffer))
				End Get
			End Property


			Private Shared Function GetInCurrentLanguage(ByVal en_US As String, ByVal hr_HR As String) As String
				Select Case System.Threading.Thread.CurrentThread.CurrentUICulture.Name.ToLower(System.Globalization.CultureInfo.InvariantCulture)
					Case "en", "en-us", "en-gb"
						Return en_US
					Case "hr", "hr-hr", "hr-ba"

						Return hr_HR
					Case Else

						Return en_US
				End Select
			End Function

		End Class

	End Class

End Namespace