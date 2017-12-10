using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using FamilyStudio1.WindowsGui.Controls;
using FamilyStudio1.FamilyData;
using FamilyStudio1.FamilyTreeStore;
using System.Collections;

namespace FamilyStudio1.WindowsGui.Panels
{
  class ImageViewPanel1 : TreeViewPanelBaseClass //, FlowLayoutPanel
  {
    private FamilyTreeStoreBaseClass familyTree;
    private IndividualClass selectedIndividual;
    private IList<Control> controlList;
    private bool layoutDone;
    private FamilyForm2 parentForm;
    //private bool visible;
    private bool printMode;


    public ImageViewPanel1()
    {
      controlList = new List<Control>();

      //this.Dock = DockStyle.Fill;

      layoutDone = false;

      parentForm = null;
      printMode = false;

      this.VisibleChanged += ImageViewPanel1_VisibleChanged;
      //this.Layout += ImageViewPanel1_Layout;
      if (printMode)
      {
        Debug.WriteLine("ImageViewPanel1::ImageViewPanel1()");
      }

    }

    void ImageViewPanel1_VisibleChanged(object sender, EventArgs e)
    {
      ShowActiveFamily();
    }
    public override void SetParentForm(FamilyForm2 parentForm)
    {
      this.parentForm = parentForm;

      parentForm.SelectedPersonChanged += OnSelectedPersonChangedEvent;
    }

    public override string GetTitle()
    {
      return "ImageView1";
    }

    /*void ImageViewPanel1_Layout(object sender, LayoutEventArgs e)
    {
      if (printMode)
      {
        Debug.WriteLine("ImageViewPanel1::ImageViewPanel1_Layout()");
      }

      //visible = this.CanFocus;

      ShowActiveFamily();

    }*/

    /*void ImageViewPanel1_GotFocus(object sender, EventArgs e)
    {
      Debug.WriteLine("ImageViewPanel1::ImageViewPanel1_GotFocus()");
    }

    void ImageViewPanel1_Enter(object sender, EventArgs e)
    {
      Debug.WriteLine("ImageViewPanel1::ImageViewPanel1_Enter()");
      
    }*/

    private void OnSelectedPersonChangedEvent(object sender, SelectedPersonChangedEvent e)
    {
      if (familyTree != null)
      {
        selectedIndividual = e.selectedPerson;
        //visible = this.CanFocus;

        layoutDone = false;
        ShowActiveFamily();
      }

    }


    public override void SetFamilyTree(FamilyTreeStoreBaseClass inFamilyTree)
    {
      if (printMode)
      {
        Debug.WriteLine("ImageViewPanel1::SetFamilyTree()");
      }

      familyTree = inFamilyTree;

    }
    public override void SetSelectedIndividual(String xrefName)
    {
      if (printMode)
      {
        Debug.WriteLine("ImageViewPanel1::SetSelectedIndividual(" + xrefName + ")");
      }
      /*if (familyTree != null)
      {
        selectedIndividual = (IndividualClass)familyTree.GetIndividual(xrefName);
        //ShowActiveFamily();
      }*/

      //layoutDone = false;
      //ShowActiveFamily();


    }

    /*protected override void OnInvalidated(InvalidateEventArgs e)
    {
      base.OnInvalidated(e);

      //ShowActiveFamily();
    }*/

    private Image GetImage(String url)
    {

      if (printMode)
      {
        Debug.WriteLine("GetImage: " + url);
      }
      // Create a request for the URL. 		
      WebRequest request = WebRequest.Create(url);
      // If required by the server, set the credentials.
      request.Credentials = CredentialCache.DefaultCredentials;
      // Get the response.

      HttpWebResponse response = null;

      try
      {
        response = (HttpWebResponse)request.GetResponse();
      }

      catch(WebException e)
      {
        Debug.WriteLine("GetImage: " + url + " failed");
        Debug.WriteLine(e.ToString());

        return null;
      }
      // Display the status.
      Debug.WriteLine(response.StatusDescription);
      // Get the stream containing content returned by the server.
      Stream dataStream = response.GetResponseStream();
      // Open the stream using a StreamReader for easy access.
      //StreamReader reader = new StreamReader(dataStream);
      // Read the content.
      //string responseFromServer = reader.ReadToEnd();
      // Display the content.
      //Debug.WriteLine(responseFromServer);
      // Cleanup the streams and the response.

      Image image = Image.FromStream(dataStream);

      //reader.Close();
      dataStream.Close();
      response.Close();

      return image;
    }

    /*protected override void OnInvalidated()
    {
      ShowActiveFamily();
    }*/

    private void ReadImages()
    {
      if (selectedIndividual != null)
      {
        IList<MultimediaLinkClass> mmLinklList;
        mmLinklList = selectedIndividual.GetMultimediaLinkList();
        if (printMode)
        {
          Debug.WriteLine("Images:" + mmLinklList.Count);
        }

        Label textLabel = new Label();

        textLabel.Text = "Loading " + mmLinklList.Count + " images";
        textLabel.AutoSize = true;
        textLabel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        this.Controls.Add(textLabel);
        //controlList.Add(textLabel);
        if (mmLinklList.Count > 0)
        {
          int cnt = 0;
          foreach (MultimediaLinkClass link in mmLinklList)
          {
            if (printMode)
            {
              Debug.WriteLine("link + " + link.GetLink() + " (" + link.GetFormat() + ") " + cnt++ + "/" + mmLinklList.Count);
            }

            if (link.GetLink().Contains(".jpg"))
            {

              PictureBox picture = new PictureBox();

              //picture.Top = pos;
              picture.AutoSize = true;

              picture.Image = GetImage(link.GetLink());

              if (picture.Image != null)
              {

                picture.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                controlList.Add(picture);
                //this.Controls.Add(picture);

                layoutDone = true;
              }
              else
              {
                picture.Dispose();
              }


            }
          }
        }
        //if(

        textLabel.Visible = false;
      }
    }

    //public override void Enter(

    private void ShowActiveFamily()
    {
      if (printMode)
      {
        Debug.WriteLine("ImageViewPanel1::ShowActiveFamily " + this.CanFocus);
        Debug.WriteLine(" Controls.count = " + this.Controls.Count + " = " + controlList.Count);
      }
      this.Controls.Clear();
      controlList.Clear();


      if (!this.Visible || (familyTree == null) || (selectedIndividual == null))
      {
        return;
      }

      
      /*while(controlList.Count > 0)
      {
        Control ctrl = controlList[0];


        controlList.Remove(ctrl);
        this.Controls.Remove(ctrl);
        ctrl.Dispose();

        //ctrl.
      }*/


      //if (selectedIndividual != null)
      {
        //Button ctrl = new Button();
        //int pos = 0;

        //current.Width = 0;
        //current.Height = 0;

        //ctrl.AutoSize = true;
        //ctrl.Left = 0;
        //ctrl.Top = pos;
        //ctrl.Height = 100;
        //ctrl.Width = 400;
        //ctrl.Text = selectedIndividual.GetName();
        //ctrl.Height = 40;
        //ctrl.Width = 40;
        //ctrl.Show();
        //pos += ctrl.Height;

        if (!layoutDone)
        {
          ReadImages();
        }

        //this.Height = 0;
        //this.Width = 0;
        //this.Controls.Clear();

        if (layoutDone)
        {
          Size current = new Size(0,0);
          int currentLineHeight = 0;

          /*this.Left = 0;
          this.Top = 0;*/
          this.Width = 0;
          this.Height = 0;

          foreach (PictureBox picture in controlList)
          {
            if (picture.Image != null)
            {
              if (printMode)
              {
                Debug.WriteLine(" imageView next");
                Debug.WriteLine("  picture " + picture.Image.Width + "," + picture.Image.Height);
                Debug.WriteLine("  picture frame " + picture.Left + "," + picture.Top + "," + picture.Width + "," + picture.Height);
              }
              if (this.ClientSize.Width < current.Width + picture.Image.Width)
              {
                current.Width = 0;
                current.Height += currentLineHeight;
                currentLineHeight = 0;
              }
              picture.Left = current.Width;
              picture.Top = current.Height;
              current.Width += picture.Image.Width;
              if (picture.Image.Height > currentLineHeight)
              {
                currentLineHeight = picture.Image.Height;
              }

              picture.Width = picture.Image.Width;
              picture.Height = picture.Image.Height;

              this.Controls.Add(picture);

              if (this.Width < picture.Left + picture.Width)
              {
                this.Width = picture.Left + picture.Width;
              }
              if (this.Height < picture.Top + picture.Height)
              {
                this.Height = picture.Top + picture.Height;
              }

              if (printMode)
              {
                Debug.WriteLine("  client " + this.ClientSize.Width + "," + this.ClientSize.Height);
                Debug.WriteLine("  picture " + picture.Image.Width + "," + picture.Image.Height);
                Debug.WriteLine("  picture frame " + picture.Left + "," + picture.Top + "," + picture.Width + "," + picture.Height);
                Debug.WriteLine("  status " + current.Width + "," + current.Height + "," + currentLineHeight);
                Debug.WriteLine("  size " + this.Height + "x" + this.Width);
              }
            }



          }
          //return;
        }

        //controlList.Add(ctrl);

        //this.Controls.Add(ctrl);

        if (printMode)
        {
          Debug.WriteLine("imageView size " + this.Height + "x" + this.Width);
        }

      }



      //this.Show();
    }
    /*protected override void OnLayout(LayoutEventArgs levent)
    {
      base.OnLayout(levent);

      Control ctrl = levent.AffectedControl;
      Debug.WriteLine("ImageViewPanel1::OnLayout ctrl" + levent.AffectedControl.Left + "," + levent.AffectedControl.Top + "," + levent.AffectedControl.Width + "," + levent.AffectedControl.Height);
      Debug.WriteLine("ImageViewPanel1::OnLayout client" + levent.AffectedControl.ClientSize.Width + "," + levent.AffectedControl.ClientSize.Height);


      //if (!layoutDone)
      {
        //ShowActiveFamily();

      }
    }*/

    /*protected override void OnEnter(EventArgs e)
    {

      base.OnEnter(e);
    }*/


  }
}
