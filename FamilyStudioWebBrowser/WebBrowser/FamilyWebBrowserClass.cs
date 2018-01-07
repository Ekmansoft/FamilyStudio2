﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Security.Permissions;

namespace FamilyStudioFormsGui.WindowsGui.FamilyWebBrowser
{
  public class FamilyWebBrowserClass : Form
  {
    private static TraceSource trace = new TraceSource("FamilyWebBrowserClass", SourceLevels.Warning);
    public delegate void FamilyAuthenticationHandler(object sender, FamilyAuthenticationEvent e);
    public event FamilyAuthenticationHandler AuthenticationEvent;
    private WebBrowser webBrowser1;
    private bool eventDone;
    private string requestedAddress;

    public FamilyWebBrowserClass()
    {
      // Create the form layout. If you are using Visual Studio,  
      // you can replace this code with code generated by the designer. 

      eventDone = false;
      webBrowser1 = new WebBrowser();

      webBrowser1.Dock = DockStyle.Fill;
      webBrowser1.Navigated += new WebBrowserNavigatedEventHandler(webBrowser1_Navigated);

      webBrowser1.HandleDestroyed += webBrowser1_HandleDestroyed;

      //Controls.AddRange(new Control[] { webBrowser1, toolStrip2, toolStrip1, menuStrip1, statusStrip1, menuStrip1 });
      //Controls.AddRange(new Control[] { webBrowser1 });
      Controls.Add(webBrowser1);

      //trace.TraceInformation("GeniWebBrowserClass");

      this.SetBounds(20, 20, 700, 500);
      webBrowser1.GoHome();
    }

    void webBrowser1_HandleDestroyed(object sender, EventArgs e)
    {
      CheckNavigation();
    }

    public new void Hide()
    {
      CheckNavigation();
      base.Hide();
    }
    public new void Show()
    {
      eventDone = false;
      base.Show();
    }

    private void CheckNavigation()
    {
      if (!eventDone)
      {
        eventDone = true;
        if (AuthenticationEvent != null)
        {
          AuthenticationEvent(this, new FamilyAuthenticationEvent("#message=canceled&status=unauthorized access_token"));
        }
      }

    }

    // Navigates to the given URL if it is valid. 
    public void Navigate(String address)
    {
      //trace.TraceInformation("Navigate" + address);
      if (String.IsNullOrEmpty(address))
      {
        return;
      }
      if (address.Equals("about:blank"))
      {
        return;
      }
      if (!address.StartsWith("http://") && !address.StartsWith("https://"))
      {
        address = "http://" + address;
      }
      requestedAddress = address;
      try
      {
        webBrowser1.Navigate(new Uri(address));
      }
      catch (System.UriFormatException)
      {
        return;
      }
    }

    public static void ShowMessage(string message)
    {
      MessageBox.Show(message);
    }

    public static DialogResult ShowInputDialog(string title, ref string input)
    {
      System.Drawing.Size size = new System.Drawing.Size(500, 70);
      Form inputBox = new Form();

      inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      inputBox.ClientSize = size;
      inputBox.Text = "Name";

      inputBox.Text = title;

      System.Windows.Forms.TextBox textBox = new TextBox();
      textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
      textBox.Location = new System.Drawing.Point(5, 5);
      textBox.Text = input;
      inputBox.Controls.Add(textBox);

      Button okButton = new Button();
      okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      okButton.Name = "okButton";
      okButton.Size = new System.Drawing.Size(75, 23);
      okButton.Text = "&OK";
      okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
      inputBox.Controls.Add(okButton);

      Button cancelButton = new Button();
      cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      cancelButton.Name = "cancelButton";
      cancelButton.Size = new System.Drawing.Size(75, 23);
      cancelButton.Text = "&Cancel";
      cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
      inputBox.Controls.Add(cancelButton);

      inputBox.AcceptButton = okButton;
      inputBox.CancelButton = cancelButton;

      DialogResult result = inputBox.ShowDialog();
      input = textBox.Text;
      return result;
    }

    // Updates the URL in TextBoxAddress upon navigation. 
    public void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
    {
      trace.TraceInformation("FamilyWebBrowser navigated to:" + HttpUtility.UrlDecode(webBrowser1.Url.ToString()) + " at " + DateTime.Now.ToString());

      if (requestedAddress != null)
      {
        if (webBrowser1.Url.ToString() != requestedAddress)
        {
          eventDone = true;
          if (AuthenticationEvent != null)
          {
            AuthenticationEvent(this, new FamilyAuthenticationEvent(HttpUtility.UrlDecode(webBrowser1.Url.ToString())));
          }
        }
      }
    }

    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // FamilyWebBrowserClass
      // 
      this.ClientSize = new System.Drawing.Size(540, 291);
      this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Name = "FamilyWebBrowserClass";
      this.ResumeLayout(false);

    }
  }
  public class FamilyAuthenticationEvent : EventArgs
  {
    public string url;
    public FamilyAuthenticationEvent(string inUrl)
    {
      this.url = inUrl;
    }
  }

  public class FamilyTimer : System.Windows.Forms.Timer
  {
  }

}