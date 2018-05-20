using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Diagnostics;
//using System.Data.OleDb;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.ComponentModel;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
//using FamilyStudioData.FamilyFileFormat;
using System.Web.Script.Serialization;
//using System.Web.Script.Serialization.JavaScriptConverter;
//using System.Collections.ObjectModel;
//using System.Web.UI.WebControls;
using FamilyStudioFormsGui.WindowsGui.FamilyWebBrowser;

namespace FamilyStudioData.FileFormats.MyHeritageCodec
{
  public class FamilyTreeStoreMyHeritage : FamilyTreeStoreBaseClass, IDisposable
  {
    private static TraceSource trace = new TraceSource("FamilyTreeStoreMyHeritage", SourceLevels.Warning);
    private Boolean printDecode;

    private string accessToken;
    private string refreshToken;
    private int expiryTime;
    private string authenticationToken;
    //private string geniTreeSize;
    private String sourceFileName;
    private bool authenticated;
    private FamilyWebBrowserClass authenticationWebBrowser;
    private JavaScriptSerializer serializer;
    private string rootPersonXref;

    protected virtual void Dispose(bool managed)
    {
      if (managed)
      {
        if (authenticationWebBrowser != null)
        {
          authenticationWebBrowser.Dispose();
        }
      }
    }
    public void Dispose()
    {
      Dispose(true);
    }


    private class FamilyCache
    {
      public IDictionary<string, IndividualClass> individuals;
      public IDictionary<string, FamilyClass> families;

      public FamilyCache()
      {
        individuals = new Dictionary<string, IndividualClass>();
        families = new Dictionary<string, FamilyClass>();
      }
    }

    FamilyCache cache;

    private HttpAuthenticateResponse authenticationResponse;
    private HttpGeniTreeSize geniTreeSize;

    private HttpSearchPersonResult searchPersonResult;

    private HttpGetIndividualResult getIndividualResult;

    private HttpFamilyResponse familyResponse;

    private HttpMaxFamilyResponse maxFamilyResponse;

    public class HttpAuthenticateResponse
    {
      public string access_token { get; set; }
      public string refresh_token { get; set; }
      public int expires_in { get; set; }
    }

    public class HttpGeniTreeSize
    {
      public string formatted_size { get; set; }
      public int size { get; set; }
    }


    public class HttpSearchPerson
    {
      public string id { get; set; }
      public string name { get; set; }
    }

    public class HttpSearchPersonResult 
    {
      public List<HttpSearchPerson> results { get; set; }
    }

    public class HttpDate
    {
      public string gedcom { get; set; }
      public string text { get; set; }
      public string date { get; set; }
    }

    public class HttpLocation
    {
      public string city { get; set; }
      public string place_name { get; set; }
      public string county { get; set; }
      public string state { get; set; }
      public string country { get; set; }
      public string country_code { get; set; }
      public double latitude { get; set; }
      public double longitude { get; set; }
    }

    public class HttpBirth
    {
      public HttpDate date { get; set; }
      public HttpLocation location { get; set; }
    }

    public class HttpDeath
    {
      public HttpDate date { get; set; }
    }

    public class HttpUnion
    {
      public string id { get; set; }
    }
    public class HttpFamily
    {
      public HttpUnion family { get; set; }
    }
    public class HttpPersonalPhoto
    {
      public string id { get; set; }
      public string name { get; set; }
    }
    public class HttpTree
    {
      public string id { get; set; }
      public string name { get; set; }
    }

    public class HttpSite
    {
      public string id { get; set; }
      public string name { get; set; }
    }

    public class HttpPerson
    {
      public string id { get; set; }
      public string name { get; set; }
      public bool is_alive { get; set; }
      public string first_name { get; set; }
      public string last_name { get; set; }
      public string married_surname { get; set; }
      public string former_name { get; set; }
      public string gender { get; set; }
      public HttpDate birth_date { get; set; }
      public HttpDate death_date { get; set; }
      public bool is_privatized { get; set; }
      public string link { get; set; }
      public List<HttpUnion> spouse_in_families { get; set; }
      public List<HttpFamily> child_in_families { get; set; }
      public int smartmatch_count { get; set; }
      public string data_language { get; set; }
      public HttpPersonalPhoto personal_photo { get; set; }
      public HttpTree tree { get; set; }
      public HttpSite site { get; set; }
    }

    public class HttpGetIndividualResult
    {
      public HttpPerson focus { get; set; }
      public IDictionary<string, HttpPerson> nodes { get; set; }
    }

    public class HttpPersonXref
    {
      public string id { get; set; }
      public string name { get; set; }
    }
    public class HttpChild
    {
      public HttpPersonXref child { get; set; }
    }


    public class HttpMarriage
    {
      public HttpDate date { get; set; }
      public HttpLocation location { get; set; }
    }

    public class HttpFamilyResponse
    {
      public string id { get; set; }
      public string status { get; set; }
      public HttpPersonXref husband { get; set; }
      public HttpPersonXref wife { get; set; }
      public string link { get; set; }
      public HttpDate marriage_date { get; set; }
      public List<HttpChild> children { get; set; }
      public HttpTree tree { get; set; }
      public HttpSite site { get; set; }
    }

    public class HttpMaxFamilyResponse
    {
      public List<HttpPerson> results { get; set; }
      public int page { get; set; }
      public string next_page { get; set; }
    }


    public FamilyTreeStoreMyHeritage()
    {

      printDecode = false;
      printDecode = true;
      authenticated = false;

      if (printDecode)
      {
        trace.TraceInformation("FamilyTreeStoreMyHeritage");
      }
      accessToken = null;
      refreshToken = null;
      expiryTime = 0;
      authenticationToken = null;
      authenticationWebBrowser = null;
      geniTreeSize = null;

      serializer = new JavaScriptSerializer();

     //Authenticate();

      //FamilyTreeStoreGeni2 geni2 = new FamilyTreeStoreGeni2();
      //geni2.TestIndividualParsing();
      cache = new FamilyCache();

      //rootPersonXref = FetchRootPerson();

    }

    string FetchRootPerson()
    {
      string sLine = null;
      string sURL = "https://www.geni.com/api/user/max-family&&page=1&&per_page=3";

      Authenticate();
      GetTreeStats();

      if (printDecode)
      {
        trace.TraceInformation("FetchRootPerson()");
      }
      try
      {

        if (printDecode)
        {
          trace.TraceInformation("FetchRootPerson() url " + sURL);
        }
        WebRequest wrGETURL = WebRequest.Create(sURL);

        /*if (authenticationToken != null)
        {
          wrGETURL.Headers.Add("Authorization", String.Format("Bearer {0}", Uri.EscapeDataString(authenticationToken)));
        }*/

        Stream objStream = wrGETURL.GetResponse().GetResponseStream();

        StreamReader objReader = new StreamReader(objStream);

        sLine = objReader.ReadToEnd();
      }
      catch (WebException e)
      {
        trace.TraceData(TraceEventType.Warning, 0, "FetchRootPerson(" + sURL+") FAILED");
        trace.TraceData(TraceEventType.Warning, 0, "Exception: " + e.ToString());
        foreach (string key in e.Response.Headers)
        {
          trace.TraceData(TraceEventType.Warning, 0, "     " + key + "=" + e.Response.Headers[key]);
        }
        return null;
      }
      if (sLine != null)
      {
        if (printDecode)
        {
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
        }
        if ((sLine.StartsWith("<!DOCTYPE") || sLine.StartsWith("<HTML") || sLine.StartsWith("<html")))
        {
          trace.TraceInformation("Bad format. Don't parse.");
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
          return null;
        }

        maxFamilyResponse = serializer.Deserialize<HttpMaxFamilyResponse>(sLine);

        foreach(HttpPerson person in maxFamilyResponse.results)
        {
          if(person.id != null)
          {
            if (printDecode)
            {
              trace.TraceInformation("GetFamily() = " + person.id.Substring(8) + " " + DateTime.Now);
            }
            return person.id.Substring(8); // skip "profile-";
          }
        }
      }
      return null;
    }

    public void SetHomeIndividual(String xrefName)
    {

    }
    public string GetHomeIndividual()
    {
      return null;
    }

    private IndividualClass TransformRecordToIndividual(DataRow personRow)
    {
      return null;
    }

    public void AddFamily(FamilyClass tempFamily)
    {
    }

    public FamilyDateTimeClass DecodeDate(HttpDate inDate)
    {
      if (inDate != null)
      {
        FamilyDateTimeClass date = new FamilyDateTimeClass(inDate.gedcom);
        
      }
      return null;
    }

    public FamilyClass GetFamily(String familyXrefName)
    {
      if (familyXrefName == null)
      {
        trace.TraceInformation("GetFamily(null) = ");
        return null;
      }
      if (cache.families.ContainsKey(familyXrefName))
      {
        if (printDecode)
        {
          trace.TraceInformation("GetFamily(" + familyXrefName + ") cached");
        }
        return cache.families[familyXrefName];
      }
      if (printDecode)
      {
        trace.TraceInformation("GetFamily(" + familyXrefName + ") start " + DateTime.Now);
      }
      //IndividualClass individual = null;

      string sLine = null;
      try
      {
        string sURL = "https://www.geni.com/api/union-" + familyXrefName;

        WebRequest wrGETURL;
        wrGETURL = WebRequest.Create(sURL);
        if (authenticationToken != null)
        {
          wrGETURL.Headers.Add("Authorization", String.Format("Bearer {0}", Uri.EscapeDataString(authenticationToken)));
        }
        if (printDecode)
        {
          trace.TraceInformation("GetFamily() url " + sURL);
        }

        Stream objStream = wrGETURL.GetResponse().GetResponseStream();

        StreamReader objReader = new StreamReader(objStream);

        sLine = objReader.ReadToEnd();
      }
      catch
      {
        return null;
      }
      if (sLine != null)
      {
        if (printDecode)
        {
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
        }
        if ((sLine.StartsWith("<!DOCTYPE") || sLine.StartsWith("<HTML") || sLine.StartsWith("<html")))
        {
          trace.TraceInformation("Bad format. Don't parse.");
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
          return null;
        }

        familyResponse = serializer.Deserialize<HttpFamilyResponse>(sLine);

        if (familyResponse.id != null)
        {
          FamilyClass family = new FamilyClass();
          // Ignore "union-"
          family.SetXrefName(familyResponse.id.Substring(6));

          if (familyResponse.marriage_date != null)
          {
            family.AddEvent(new IndividualEventClass(IndividualEventClass.EventType.FamMarriage, new FamilyDateTimeClass(familyResponse.marriage_date.gedcom)));
          }
          if (familyResponse.husband != null)
          {
            int startPos = familyResponse.husband.id.IndexOf("individual-") + 11;

            family.AddRelation(new IndividualXrefClass(familyResponse.husband.id.Substring(startPos)), FamilyClass.RelationType.Parent);
          }
          if (familyResponse.wife != null)
          {
            int startPos = familyResponse.wife.id.IndexOf("individual-") + 11;

            family.AddRelation(new IndividualXrefClass(familyResponse.wife.id.Substring(startPos)), FamilyClass.RelationType.Parent);
          }
          if (familyResponse.children != null)
          {
            foreach (HttpChild child in familyResponse.children)
            {
              int startPos = child.child.id.IndexOf("individual-") + 11;
              // ignore "profile-" = 8 characters
              family.AddRelation(new IndividualXrefClass(child.child.id.Substring(startPos)), FamilyClass.RelationType.Child);
            }
          }
          if (family.GetXrefName() != "")
          {

            if (printDecode)
            {
              //family.Print();
              trace.TraceInformation("GetFamily(" + familyXrefName + ") done " + DateTime.Now);
            }

            cache.families.Add(family.GetXrefName(), family);

            return family;
          }
        }
        else
        {
          trace.TraceInformation("Error no data in familyresult:" + sLine);
        }

      }
      //Console.ReadLine();

      if (printDecode)
      {
        trace.TraceInformation("GetFamily(" + familyXrefName + ") = null" + DateTime.Now);
      }

      return null;
    }

    public bool AddIndividual(IndividualClass tempIndividual)
    {
      return false;
    }
    public bool UpdateIndividual(IndividualClass tempIndividual, PersonUpdateType updateType)
    {
      return false;
    }



    /*public void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {

      MessageBox.Show(((WebBrowser)sender).Url.ToString());
      if (printDecode)
      {
        trace.TraceInformation("WebBrowser_DocumentCompleted" + ((WebBrowser)sender).Url.ToString());
      }

    }*/

    private string GetUrlToken(string url, string token)
    {
      if (url != null)
      {
        trace.TraceInformation("FindUrlToken:" + url + " " + token);

        int tokenPos = url.IndexOf(token);
        if (tokenPos >= 0)
        {
          string tStr = url.Substring(tokenPos + token.Length);

          int endPos = tStr.IndexOf('&');
          if(endPos >= 0)
          {
            return tStr.Substring(0, endPos);
          }
          return tStr;
        }
        return null;
      }
      trace.TraceInformation("FindUrlToken:null " + token);
      return null;

    }

    public void wbrowser_AuthenticationEvent(object sender, FamilyAuthenticationEvent e)
    {

      //MessageBox.Show(((WebBrowser)sender).Url.ToString());

      if (printDecode)
      {
        trace.TraceInformation("wbrowser_AuthenticationEvent-1:" + e.url + " " + DateTime.Now);
      }

      const string tokenHeader = "access_token=";
      const string expiresInHeader = "expires_in=";

      string newAuthToken = GetUrlToken(e.url, tokenHeader);
      if (newAuthToken != null)
      {
      //int tokenPos = .IndexOf(tokenHeader);
        authenticationToken = newAuthToken;
        string expiryTime = GetUrlToken(e.url, expiresInHeader);
        if (printDecode)
        {
          trace.TraceInformation("wbrowser_AuthenticationEvent-3:" + authenticationToken + " " + DateTime.Now + expiryTime);
        }

        if (authenticationWebBrowser != null)
        {
          authenticationWebBrowser.Hide();

          authenticationWebBrowser = null;
        }
        else if (printDecode)
        {
          trace.TraceInformation("wbrowser_AuthenticationEvent-4-no auth window !");

        }        
      }
      else if (printDecode)
      {
        trace.TraceInformation("wbrowser_AuthenticationEvent-4-no token found!");

      }

      //trace.TraceInformation("WebBrowser_DocumentCompleted-2:" + ((WebBrowser)sender).Url.ToString());

    }



    private void navBtnClick()
    {
      if (printDecode)
      {
        trace.TraceInformation("navBtnClick=" + DateTime.Now);
      }
      //WebBrowser wbrowser = new GeniWebBrowserClass();
      authenticationWebBrowser = new FamilyWebBrowserClass();
      //wbrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(WebBrowser_DocumentCompleted);
      //wbrowser.webBrowser1_Navigated +=

      authenticationWebBrowser.AuthenticationEvent += wbrowser_AuthenticationEvent;
      authenticationWebBrowser.Navigate("https://www.geni.com/platform/oauth/authorize?client_id=88&response_type=token&display=desktop");

      authenticationWebBrowser.Show();


    }

    private void Authenticate()
    {
      navBtnClick();

      if (printDecode)
      {
        trace.TraceInformation("Authenticate() " + DateTime.Now);
      }
      string sLine = null;
      try
      {
        string sURL;
        sURL = "https://familygraph.myheritage.com/REQUEST?bearer_token";
        if (printDecode)
        {
          trace.TraceInformation("Authenticate() url " + sURL);
        }
        WebRequest wrGETURL;
        wrGETURL = WebRequest.Create(sURL);
        WebResponse webResponse = wrGETURL.GetResponse();
        Stream objStream = webResponse.GetResponseStream();

        StreamReader objReader = new StreamReader(objStream);

        sLine = objReader.ReadToEnd();
      }
      catch
      {
        trace.TraceInformation("Authenticate() FAIL " + " " + DateTime.Now);
        return;
      }
      if (sLine != null)
      {
        if (printDecode)
        {
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
        }
        if ((sLine.StartsWith("<!DOCTYPE") || sLine.StartsWith("<HTML") || sLine.StartsWith("<html")))
        {
          trace.TraceInformation("Bad format. Don't parse.");
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
          return;
        }

        authenticationResponse = serializer.Deserialize<HttpAuthenticateResponse>(sLine);

        if (authenticationResponse.access_token != null)
        {
          accessToken = authenticationResponse.access_token;
          refreshToken = authenticationResponse.refresh_token;
          expiryTime = authenticationResponse.expires_in;
          authenticated = true;
        }
        if (printDecode)
        {
          trace.TraceInformation("Authenticate() Done " + " " + DateTime.Now);
        }

      }

    }
    private void GetTreeStats()
    {

      if (printDecode)
      {
        trace.TraceInformation("GetTreeStats() " + " " + DateTime.Now);
      }
      string sLine = null;
      try
      {
        string sURL = "https://www.geni.com/api/stats/world-family-tree";

        WebRequest wrGETURL;
        wrGETURL = WebRequest.Create(sURL);
        if (authenticationToken != null)
        {
          wrGETURL.Headers.Add("Authorization", String.Format("Bearer {0}", Uri.EscapeDataString(authenticationToken)));
        }
        if (printDecode)
        {
          trace.TraceInformation("GetTreeStats() url " + sURL);
        }
        Stream objStream = wrGETURL.GetResponse().GetResponseStream();

        StreamReader objReader = new StreamReader(objStream);

        sLine = objReader.ReadToEnd();
      }
      catch
      {
        trace.TraceInformation("GetTreeStats() FAIL " + " " + DateTime.Now);
        return;
      }
      if (sLine != null)
      {
        if (printDecode)
        {
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
        }
        if ((sLine.StartsWith("<!DOCTYPE") || sLine.StartsWith("<HTML") || sLine.StartsWith("<html")))
        {
          trace.TraceInformation("Bad format. Don't parse.");
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
          trace.TraceInformation("GetTreeStats() FAIL " + " " + DateTime.Now);
          return;
        }

        geniTreeSize = serializer.Deserialize<HttpGeniTreeSize>(sLine);

        if (printDecode)
        {
          trace.TraceInformation("GetTreeStats() OK " + DateTime.Now);
        }
      }
    }

    public IndividualClass DecodeIndividual(HttpPerson person)
    {
      if (person != null)
      {
        IndividualClass individual = new IndividualClass();
        PersonalNameClass name = new PersonalNameClass();

        if (person.id != null)
        {
          individual.SetXrefName(person.id.Substring(8)); // skip "profile-"
        }
        if (person.first_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.GivenName, person.first_name);
        }
        if (person.last_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.Surname, person.last_name);
        }
        if (person.former_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.BirthSurname, person.former_name);
        }

        name.SanityCheck();

        individual.SetPersonalName(name);
        if (person.gender != null)
        {
          switch (person.gender)
          {
            case "male":
              individual.SetSex(IndividualClass.IndividualSexType.Male);
              break;
            case "female":
              individual.SetSex(IndividualClass.IndividualSexType.Female);
              break;
          }
        }

        if (person.birth_date != null)
        {
          FamilyDateTimeClass birthDate = new FamilyDateTimeClass(
            person.birth_date.gedcom);
          individual.AddEvent(new IndividualEventClass(IndividualEventClass.EventType.Birth, birthDate));
        }
        if (person.death_date != null)
        {
          FamilyDateTimeClass deathDate = new FamilyDateTimeClass(
            person.death_date.gedcom);
          individual.AddEvent(new IndividualEventClass(IndividualEventClass.EventType.Death, deathDate));
        }

        return individual;
      }

      return null;
    }

    bool UpdateRelations(IDictionary<string,HttpPerson> personList, ref IndividualClass individual)
    {
      bool updated = false;

      foreach (KeyValuePair<string, HttpPerson> nodePersonPair in personList)
      {
        if ((nodePersonPair.Key.IndexOf("profile-") == 0) && (nodePersonPair.Key.Substring(8) == individual.GetXrefName()))
        {
          if (nodePersonPair.Value != null)
          {
            HttpPerson nodePerson = (HttpPerson)nodePersonPair.Value;

            /*if (nodePerson.edges != null)
            {
              foreach (KeyValuePair<string, HttpUnion> edgePair in nodePerson.edges)
              {
                if (edgePair.Value != null)
                {
                  HttpUnion union = (HttpUnion)edgePair.Value;

                  if (union.rel == "child")
                  {
                    individual.AddRelation(new FamilyXrefClass(edgePair.Key.Substring(6)), IndividualClass.RelationType.Child);
                    updated = true;
                  }
                  else if (union.rel == "partner")
                  {
                    individual.AddRelation(new FamilyXrefClass(edgePair.Key.Substring(6)), IndividualClass.RelationType.Spouse);
                    updated = true;
                  }
                }
              }
            }*/
          }
        }
      }
      return updated;
    }



    public IndividualClass GetIndividual(String xrefName, uint index = (uint)SelectIndex.NoIndex, PersonDetail detailLevel = PersonDetail.PersonDetail_All)
    {
      if (xrefName == null)
      {
        trace.TraceInformation("GetIndividual(root)");
        rootPersonXref = FetchRootPerson();
        xrefName = rootPersonXref;
      }
      if (xrefName == null)
      {
        trace.TraceInformation("GetIndividual(null)!!!" + DateTime.Now);
        return null;
      }

      if (cache.individuals.ContainsKey(xrefName))
      {
        if (printDecode)
        {
          trace.TraceInformation("GetIndividual(" + xrefName + ") cached");
        }
        return cache.individuals[xrefName];
      }
      if (printDecode)
      {
        trace.TraceInformation("GetIndividual(" + xrefName + ") start " + DateTime.Now);
      }

      if (!authenticated)
      {
        Authenticate();
        GetTreeStats();
      }

      string sLine = null; ;
      try
      {
        string sURL = "https://www.geni.com/api/profile-" + xrefName + "/immediate-family?only_ids=true&fields=first_name,middle_name,nicknames,last_name,maiden_name,gender,birth,death,id";

        WebRequest wrGETURL;
        wrGETURL = WebRequest.Create(sURL);
        if (authenticationToken != null)
        {
          wrGETURL.Headers.Add("Authorization", String.Format("Bearer {0}", Uri.EscapeDataString(authenticationToken)));
        }
        if (printDecode)
        {
          trace.TraceInformation("GetIndividual(" + xrefName + ") = " + sURL + " " + DateTime.Now);
        }
        Stream objStream = wrGETURL.GetResponse().GetResponseStream();

        StreamReader objReader = new StreamReader(objStream);

        sLine = objReader.ReadToEnd();
      }
      catch
      {
        return null;
      }

      if (sLine != null)
      {
        IndividualClass focusPerson;

        if (printDecode)
        {
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
        }
        if ((sLine.StartsWith("<!DOCTYPE") || sLine.StartsWith("<HTML") || sLine.StartsWith("<html")))
        {
          trace.TraceInformation("Bad format. Don't parse.");
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
          return null;
        }

        getIndividualResult = serializer.Deserialize<HttpGetIndividualResult>(sLine);

        if (getIndividualResult.focus != null)
        {
          focusPerson = DecodeIndividual(getIndividualResult.focus);

          if (focusPerson != null)
          {
            if (!UpdateRelations(getIndividualResult.nodes, ref focusPerson))
            {
              if (printDecode)
              {
                trace.TraceInformation("focusperson added " + focusPerson.GetXrefName() + " no relation updates..");
              }
            }
            cache.individuals.Add(focusPerson.GetXrefName(), focusPerson);
          }

          foreach (KeyValuePair<string, HttpPerson> nodePersonPair in getIndividualResult.nodes)
          {
            if (nodePersonPair.Key.IndexOf("profile-") == 0)
            {
              if (!cache.individuals.ContainsKey(nodePersonPair.Key.Substring(8)))
              {
                if (nodePersonPair.Value != null)
                {
                  HttpPerson nodePerson = (HttpPerson)nodePersonPair.Value;

                  IndividualClass nodeIndividual = DecodeIndividual(nodePerson);

                  if (nodeIndividual != null)
                  {
                    if (printDecode)
                    {
                      trace.TraceInformation(" Cache person:" + nodePersonPair.Key.Substring(8) + "=" + nodeIndividual.GetName());
                    }
                    if (!UpdateRelations(getIndividualResult.nodes, ref nodeIndividual))
                    {
                      trace.TraceInformation(" Added " + nodeIndividual.GetXrefName() + " no relation updates..");
                    }
                    /*if (printDecode)
                    {
                      nodeIndividual.Print();
                    }*/
                    cache.individuals.Add(nodeIndividual.GetXrefName(), nodeIndividual);
                  }
                }
              }
              else
              {
                if (printDecode)
                {
                  trace.TraceInformation(" Person " + nodePersonPair.Key.Substring(8) + " skipped, already cached");
                }
              }
            }
          }

          if (printDecode)
          {
            trace.TraceInformation("GetIndividual() done " + DateTime.Now);
          }
          return focusPerson;
        }
      }

      return null;
    }

    public IEnumerator<IndividualClass> SearchPerson(String individualName, ProgressReporterInterface progressReporter = null)
    {

      if (printDecode)
      {
        trace.TraceInformation("SearchPerson(" + individualName + ")");
      }
      if (!authenticated)
      {
        Authenticate();
        GetTreeStats();
      }

      string sLine = null;
      try
      {
        string sURL = "https://www.geni.com/api/profile/search?names=" + individualName + "&&fields=id,name";

        WebRequest wrGETURL;
        wrGETURL = WebRequest.Create(sURL);
        if (authenticationToken != null)
        {
          wrGETURL.Headers.Add("Authorization", String.Format("Bearer {0}", Uri.EscapeDataString(authenticationToken)));
        }

        if (printDecode)
        {
          trace.TraceInformation("SearchPerson(" + individualName + ") = " + sURL);
        }

        Stream objStream;
        objStream = wrGETURL.GetResponse().GetResponseStream();

        StreamReader objReader = new StreamReader(objStream);

        sLine = objReader.ReadToEnd();
      }
      catch
      {
        if (printDecode)
        {
          trace.TraceInformation("SearchPerson() FAIL");
        }
        //yield return new IndividualClass();
      }

      if (sLine != null)
      {
        if (printDecode)
        {
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
        }
        if ((sLine.StartsWith("<!DOCTYPE") || sLine.StartsWith("<HTML") || sLine.StartsWith("<html")))
        {
          trace.TraceInformation("Bad format. Don't parse.");
          trace.TraceInformation("**********************************************************-start");
          trace.TraceInformation("{0}:{1}", sLine.Length, sLine);
          trace.TraceInformation("**********************************************************-end");
          trace.TraceInformation("SearchPerson() FAIL");
          sLine = null;
          yield return null;

        }

        if (sLine != null)
        {
          searchPersonResult = serializer.Deserialize<HttpSearchPersonResult>(sLine);

          foreach (HttpSearchPerson person in searchPersonResult.results)
          {
            IndividualClass individual = new IndividualClass();
            PersonalNameClass name = new PersonalNameClass();

            name.SetName(PersonalNameClass.PartialNameType.NameString, person.name);
            individual.SetXrefName(person.id.Substring(8)); // skip "profile-"
            individual.SetPersonalName(name);
            yield return individual;

          }
        }

      }
    }

    public IEnumerator<FamilyClass> SearchFamily(String familyXrefName = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddMultimediaObject(MultimediaObjectClass tempMultimediaObject)
    {
    }

    public IEnumerator<MultimediaObjectClass> SearchMultimediaObject(String mmoString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddNote(NoteClass tempNote)
    {
    }

    public NoteClass GetNote(String xrefName)
    {
      return null;
    }
    public IEnumerator<NoteClass> SearchNote(String noteString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddRepository(RepositoryClass tempRepository)
    {
    }

    public IEnumerator<RepositoryClass> SearchRepository(String repositoryString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddSource(SourceClass tempSource)
    {
    }

    public IEnumerator<SourceClass> SearchSource(String sourceString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddSubmission(SubmissionClass tempSubmission)
    {
    }

    public IEnumerator<SubmissionClass> SearchSubmission(String submissionString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void AddSubmitter(SubmitterClass tempSubmitter)
    {
    }

    public void SetSubmitterXref(SubmitterXrefClass tempSubmitterXref)
    {
    }
    public IEnumerator<SubmitterClass> SearchSubmitter(String noteString = null, ProgressReporterInterface progressReporter = null)
    {
      return null;
    }

    public void SetSourceFileType(String type)
    {
    }
    public void SetSourceFileTypeVersion(String version)
    {
    }
    public void SetSourceFileTypeFormat(String format)
    {
    }

    public void SetSourceFileName(string file)
    {
      sourceFileName = file;
    }
    public string GetSourceFileName()
    {
      return sourceFileName;
    }
    public void SetSourceName(String source)
    {
    }

    public void SetCharacterSet(FamilyTreeCharacterSet charSet)
    {
    }

    public void SetDate(FamilyDateTimeClass inDate)
    {
    }

    public string CreateNewXref(XrefType type)
    {
      return "";
    }


    public void Print()
    {
    }
    public void PrintShort()
    {
    }

    public String GetShortTreeInfo()
    {
      if (geniTreeSize != null)
      {
        return "I:" + geniTreeSize.size;
      }
      FamilyTreeContentClass contents = GetContents();
      return "I:" + contents.individuals + " F:" + contents.families + " N:" + contents.notes;
    }

    public bool ValidateFamilies()
    {
      return false;
    }

    public bool ValidateIndividuals()
    {
      return false;
    }

    public bool ValidateTree()
    {
      return false;
    }

    //void SetProgressTarget(ProgressReporter inProgressReporter);

    public FamilyTreeContentClass GetContents()
    {
      return new FamilyTreeContentClass();
    }

    public void SetFile(String fileName)
    {
      //m_fileName = fileName;
    }

  }
}
