using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script;
using System.Web.Script.Serialization;
using FamilyStudioData.FamilyData;
using FamilyStudioData.FamilyTreeStore;
using FamilyStudioFormsGui.WindowsGui.FamilyWebBrowser;

namespace FamilyStudioData.FileFormats.GeniCodec
{
  [DataContract]
  public class FamilyTreeStoreGeni2 : FamilyTreeStoreBaseClass, IDisposable
  {
    private static TraceSource trace = new TraceSource("FamilyTreeStoreGeni2", SourceLevels.Warning);

    private String sourceFileName;
    private FamilyWebBrowserClass authenticationWebBrowser;
    private JavaScriptSerializer serializer;
    private const int CACHE_CLEAR_DELAY = 3600 * 24 * 7; // one week
    private FamilyTimer authenticationTimer;
    private GeniAccessStats stats;
    private string homePerson;
    private AppAuthentication appAuthentication;
    private UserAuthentication userAuthentication;
    private CompletedCallback completedCallback;
    private string clientId;
    private string clientSecret;
    private bool warningShown;

    protected virtual void Dispose(bool managed)
    {
      if (managed)
      {
        if (authenticationWebBrowser != null)
        {
          authenticationWebBrowser.Dispose();
        }
        if (authenticationTimer != null)
        {
          authenticationTimer.Dispose();
        }
      }
    }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public bool GetAppId()
    {
      clientId = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", "GeniAppId", null);
      clientSecret = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", "GeniAppSecret", null);

      if ((clientId == null) || (clientSecret == null))
      {
        if (!warningShown)
        {
          FamilyWebBrowserClass.ShowMessage("Please enter a valid valid Geni AppId and AppSecret separated by semicolon (id;secret)!");

          bool success = false;
          string regString = "";

          if(FamilyWebBrowserClass.ShowInputDialog("Two strings separated by only semicolon: like <AppId>;<AppSecret>!", ref regString) == System.Windows.Forms.DialogResult.OK)
          {
            trace.TraceData(TraceEventType.Warning, 1, "Entering appid and appsecret: " + regString);

            if (regString.Length > 3)
            {
              string[] subStrings = regString.Split(';');
              if (subStrings.Length == 2 && (subStrings[0].Length >= 1) && (subStrings[1].Length >= 20))
              {
                clientId = subStrings[0];
                clientSecret = subStrings[1];
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", "GeniAppId", clientId);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", "GeniAppSecret", clientSecret);
                FamilyWebBrowserClass.ShowMessage("Success entering keys! Now geni.com access should work, if you have provided valid values for app key and app secret!");
                success = true;
              }
            }

          }

          if (!success)
          {
            FamilyWebBrowserClass.ShowMessage("Failure! Geni.com access will not work!");
          }
          warningShown = true;
        }
        return false;
      }
      return true;
    }
    private class AppAuthentication
    {
      private HttpAuthenticateResponse response;
      private DateTime receptionTime;

      public AppAuthentication()
      {
        response = null;
        receptionTime = DateTime.MinValue;
      }

      public bool UpdateAuthenticationData(HttpAuthenticateResponse response)
      {
        trace.TraceInformation("authenticationResponse- access:" + response.access_token + " refresh:" + response.refresh_token + " expiry: " + response.expires_in + " now: " + DateTime.Now + " expiry: " + DateTime.Now.AddSeconds(Convert.ToInt32(response.expires_in)));
        this.response = response;
        receptionTime = DateTime.Now;

        if(this.response != null)
        {
          if (Convert.ToInt32(this.response.expires_in) < 0)
          {
            trace.TraceData(TraceEventType.Error, 0, "Geni.com returned an authentication token that already expired:" + response.ToString());
            response = null;
            receptionTime = DateTime.MinValue;
            return false;
          }
        }
        return true;
      }
      public bool IsValid()
      {
        if(response != null)
        {
          if (receptionTime.AddSeconds(Convert.ToInt32(this.response.expires_in)) > DateTime.Now)
          {
            return true;
          }
        }
        return false;
      }
    }

    private void CheckAuthentication()
    {
      if (!appAuthentication.IsValid())
      {
        AuthenticateApp();
      }
      if (userAuthentication.Request())
      {
        AuthenticateUser();
      }
      //GetTreeStats();
    }

    private class UserAuthentication
    {
      private string authenticationToken;
      private DateTime expiryTime;
      private bool requestAuthentication;

      public UserAuthentication()
      {
        authenticationToken = null;
        expiryTime = DateTime.MinValue;
        requestAuthentication = true;
      }

      public bool SetAuthentication(string token, string expiresIn)
      {
        if (Convert.ToInt32(expiresIn) > 0)
        {
          expiryTime = DateTime.Now.AddSeconds(Convert.ToInt32(expiresIn));
          trace.TraceInformation("expires at " + expiryTime);
          authenticationToken = token;
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "Geni.com returned an authentication token that already expired: Expiry in " + expiresIn + " seconds...");
          expiryTime = DateTime.MinValue;
          authenticationToken = null;
          return false;
        }
        return true;
      }

      public void Cancel()
      {
        requestAuthentication = false;
      }

      public bool Request()
      {
        if(IsValid())
        {
          return false;
        }
        return requestAuthentication;
      }

      public string GetToken()
      {
        if(IsValid())
        {
          return authenticationToken;
        }
        return null;
      }
      public bool IsValid()
      {
        if (authenticationToken != null)
        {
          if (expiryTime != null)
          {
            if (expiryTime > DateTime.Now)
            {
              return true;
            }
          }
        }
        return false;
      }
    }



    private class AccessStats
    {
      public int attempt;
      public int fetchSuccess;
      public int cacheSuccess;
      public int failureRetry;
      public int failure;
      public TimeSpan slowestFetch;

      public void Print()
      {
        trace.TraceInformation("attempts: " + attempt + " fetch success:" + fetchSuccess + " cache success:" + cacheSuccess + " fail/retry:" + failureRetry + " failed:" + failure + " slowest:" + slowestFetch);
      }
    }

    private class GeniAccessStats
    {
      public AccessStats GetIndividual;
      public AccessStats GetFamily;
      public AccessStats SearchIndividual;

      public GeniAccessStats()
      {
        GetIndividual = new AccessStats();
        GetFamily = new AccessStats();
        SearchIndividual = new AccessStats();

      }
    }

    [DataContract]
    private class GeniCache
    {
      [DataMember]
      private IDictionary<string, IndividualClass> individuals;
      [DataMember]
      private IDictionary<string, FamilyClass> families;
      [DataMember]
      private DateTime latestUpdate;

      class AddFamilyEvent : EventArgs
      {
        public FamilyClass family;
        public AddFamilyEvent(FamilyClass family)
        {
          this.family = family;
        }
      }
      class AddIndividualEvent : EventArgs
      {
        public IndividualClass individual;
        public AddIndividualEvent(IndividualClass individual)
        {
          this.individual = individual;
        }
      }

      public GeniCache()
      {
        individuals = new Dictionary<string, IndividualClass>();
        families = new Dictionary<string, FamilyClass>();
        latestUpdate = DateTime.Now;
      }

      void CacheFamily(FamilyClass family)
      {
        trace.TraceInformation("CacheFamily " + family.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);

        lock (families)
        {
          if (!families.ContainsKey(family.GetXrefName()))
          {
            trace.TraceInformation("cached family-2 " + family.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);

            latestUpdate = DateTime.Now;
            families.Add(family.GetXrefName(), family);
          }
          else
          {
            trace.TraceInformation("skipped family " + family.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);
          }
        }
      }

      void CacheIndividual(IndividualClass individual)
      {
        trace.TraceInformation("CacheIndidvidual " + individual.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);
        lock (individuals)
        {
          if (!individuals.ContainsKey(individual.GetXrefName()))
          {
            trace.TraceInformation("cached individual-2 " + individual.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);

            individuals.Add(individual.GetXrefName(), individual);
            latestUpdate = DateTime.Now;
          }
          else
          {
            trace.TraceInformation("skipped individual " + individual.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);
          }
        }
      }

      private void Clear()
      {
        Print();
        individuals.Clear();
        families.Clear();
        trace.TraceInformation(" Geni Cache cleared! " + families.Count + " families and " + individuals.Count + " people");
      }

      public bool CheckIndividual(string xrefName)
      {
        if (latestUpdate.AddSeconds(CACHE_CLEAR_DELAY) < DateTime.Now)
        {
          Clear();
        }
        return individuals.ContainsKey(xrefName);
      }
      public void AddIndividual(IndividualClass individual)
      {
        if (individual.GetXrefName().Length == 0)
        {
          trace.TraceEvent(TraceEventType.Error, 0, "AddIndividual():error: no xref!");
        }
        else
        {
          bool relations = false;
          trace.TraceInformation("cached individual " + individual.GetXrefName());

          if (individual.GetFamilyChildList() != null)
          {
            if(individual.GetFamilyChildList().Count > 0)
            {
              relations = true;
            }
          }
          if (individual.GetFamilySpouseList() != null)
          {
            if (individual.GetFamilySpouseList().Count > 0)
            {
              relations = true;
            }
          }
          if(!relations)
          {
            trace.TraceData(TraceEventType.Warning, 0, "Warning, person has no relations! " + individual.GetXrefName());
          }
          CacheIndividual(individual);
          latestUpdate = DateTime.Now;
        }
      }
      public IndividualClass GetIndividual(string xrefName)
      {
        if (CheckIndividual(xrefName))
        {
          return individuals[xrefName];
        }
        return null;
      }
      public IEnumerator<IndividualClass> GetIndividualIterator()
      {
        return individuals.Values.GetEnumerator();
      }
      public bool CheckFamily(string xrefName)
      {
        return families.ContainsKey(xrefName);
      }


      public void AddFamily(FamilyClass family)
      {
        if (family.GetXrefName().Length == 0)
        {
          trace.TraceEvent(TraceEventType.Error, 0, "error: no xref!");
        }
        else
        {
          if (!families.ContainsKey(family.GetXrefName()))
          {
            trace.TraceInformation("cached family " + family.GetXrefName() + " " + Thread.CurrentThread.GetApartmentState() + " " + Thread.CurrentThread.ManagedThreadId);

            CacheFamily(family);
          }
          else
          {
            trace.TraceData(TraceEventType.Information, 0, "family " + family.GetXrefName() + " already in cache!");
          }
        }
      }
      public FamilyClass GetFamily(string xrefName)
      {
        if (latestUpdate.AddSeconds(CACHE_CLEAR_DELAY) < DateTime.Now)
        {
          Clear();
        }
        if (CheckFamily(xrefName))
        {
          return families[xrefName];
        }
        return null;
      }
      public IEnumerator<FamilyClass> GetFamilyIterator()
      {
        return families.Values.GetEnumerator();
      }
      public int GetFamilyNo()
      {
        return families.Count;
      }
      public int GetIndividualNo()
      {
        return individuals.Count;
      }
      public void Print()
      {
        trace.TraceInformation(" Geni Cache includes " + families.Count + " families and " + individuals.Count + " people. Latest update " + latestUpdate + " now:" + DateTime.Now);
      }

    }

    [DataMember]
    GeniCache cache;

    private HttpGeniTreeSize geniTreeSize;

    //private HttpGetIndividualResult getIndividualResult;

    //private HttpFamilyResponse familyResponse;

    //private HttpMaxFamilyResponse maxFamilyResponse;

    private SynchronizationContext uiContext;

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
      public List<HttpPerson> results { get; set; }
    }

    public class HttpDate
    {
      public int day { get; set; }
      public int month { get; set; }
      public int year { get; set; }
      public bool circa { get; set; }
      public override string ToString()
      {
        return year + "-" + month + "-" + day;
        //return base.ToString();
      }
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

    public class HttpEvent
    {
      public HttpDate date { get; set; }
      public HttpLocation location { get; set; }
      public override string ToString()
      {
        if (date != null)
        {
          return date.ToString();
        }
        //return base.ToString();
        return "Event";
      }
    }

    public class HttpUnionRelation
    {
      public string rel { get; set; }
    }

    public class HttpMugshotUrls
    {
      public string large { get; set; }
      public string medium { get; set; }
      public string small { get; set; }
      public string thumb { get; set; }
      public string thumb2 { get; set; }
      public string print { get; set; }
      public string url { get; set; }
    }

    public class HttpPerson
    {
      public string id { get; set; }
      public string url { get; set; }
      public bool @public { get; set; }
      public bool is_alive { get; set; }
      public string cause_of_death { get; set; }
      public HttpLocation current_residence { get; set; }
      public bool deleted { get; set; }
      public string profile_url { get; set; }
      public string guid { get; set; }
      public string email { get; set; }
      public string language { get; set; }
      public string status { get; set; }
      public string name { get; set; }
      public string first_name { get; set; }
      public string middle_name { get; set; }
      public string maiden_name { get; set; }
      public string last_name { get; set; }
      public string display_name { get; set; }
      public List<string> unions { get; set; }
      public List<string> nicknames { get; set; }
      public string gender { get; set; }
      public string about_me { get; set; }
      public HttpEvent birth { get; set; }
      public HttpEvent baptism { get; set; }
      public HttpEvent death { get; set; }
      public HttpEvent burial { get; set; }
      public HttpMugshotUrls mugshot_urls { get; set; }
      public IDictionary<string, HttpUnionRelation> edges { get; set; }
      public override string ToString()
      {
        return id;
        //return base.ToString();
      }
    }

    public class HttpGetIndividualResult
    {
      public HttpPerson focus { get; set; }
      public IDictionary<string, HttpPerson> nodes { get; set; }
    }

    public class HttpFamilyResponse
    {
      public string id { get; set; }
      public string url { get; set; }
      public string guid { get; set; }
      public HttpEvent marriage { get; set; }
      public HttpEvent divorce { get; set; }
      public string status { get; set; }
      public List<string> partners { get; set; }
      public List<string> children { get; set; }
    }

    public class HttpMaxFamilyResponse
    {
      public List<HttpPerson> results { get; set; }
      public int page { get; set; }
      public string next_page { get; set; }
    }


    public FamilyTreeStoreGeni2(CompletedCallback callback)
    {
      trace.TraceInformation("FamilyTreeStoreGeni2");

      appAuthentication = new AppAuthentication();
      userAuthentication = new UserAuthentication();

      geniTreeSize = null;

      serializer = new JavaScriptSerializer();

      cache = new GeniCache();

      stats = new GeniAccessStats();

      if (GetAppId())
      {
        StartAuthenticationTimer();
      }
      this.completedCallback = callback;
    }

    private void CreateBrowser()
    {
      authenticationWebBrowser = new FamilyWebBrowserClass();
      authenticationWebBrowser.AuthenticationEvent += wbrowser_AuthenticationEvent;
    }


    private void StartAuthenticationTimer()
    {
      if (authenticationTimer == null)
      {
        authenticationTimer = new FamilyTimer();// System.Windows.Forms.Timer();

        authenticationTimer.Tick += AuthenticationTimer_Tick;

        authenticationTimer.Interval = 50;
        authenticationTimer.Start();
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "Authentication timer already started!");
      }
    }
    private void AuthenticationTimer_Tick(object sender, EventArgs e)
    {
      if (authenticationTimer != null)
      {
        CheckAuthentication();
        GetTreeStats();
        authenticationTimer.Stop();
        authenticationTimer = null;
      }
    }

    private string GetWebData(string mainURL, string secondaryURL, string requestDescription, int numberOfRetries)
    {
      string returnLine = null;
      bool failure = false;
      int retryCount = 0;
      GeniWebResultType resultClass = GeniWebResultType.Ok;

      do
      {
        string sURL = mainURL;
        failure = false;
        try
        {
          if ((resultClass == GeniWebResultType.FailedRetrySimple) && (secondaryURL != null))
          {
            sURL = secondaryURL;
          }

          WebRequest webRequestGetUrl;
          webRequestGetUrl = WebRequest.Create(sURL);
          if (userAuthentication.IsValid() )
          {
            webRequestGetUrl.Headers.Add("Authorization", String.Format("Bearer {0}", Uri.EscapeDataString(userAuthentication.GetToken())));
          }
          trace.TraceInformation(requestDescription + " = " + sURL + " " + DateTime.Now);

          WebResponse response = webRequestGetUrl.GetResponse();

          if(ClassifyWebResponse(response) == GeniWebResultType.OkTooFast)
          {
            trace.TraceData(TraceEventType.Warning, 0, "Running too fast...Breaking 100 ms! " + response.Headers);
            Thread.Sleep(100);
          }

          StreamReader objReader = new StreamReader(response.GetResponseStream());

          returnLine = objReader.ReadToEnd();
        }
        catch (WebException e)
        {
          stats.GetIndividual.failureRetry++;
          trace.TraceData(TraceEventType.Warning, 0, requestDescription + " " + sURL + " FAILURE " + retryCount + "/" + numberOfRetries);
          stats.GetIndividual.Print();
          trace.TraceData(TraceEventType.Warning, 0, "WebException: " + e.ToString());

          if (e.Response != null)
          {
            trace.TraceData(TraceEventType.Information, 0, "Exception.Response.Headers: " + e.Response.Headers);
          }
          else
          {
            trace.TraceData(TraceEventType.Information, 0, "Exception.Response == null");
          }

          resultClass = ClassifyErrorWebResponse(e.Response, requestDescription);

          failure = true;
          if (resultClass == GeniWebResultType.FailedReauthenticationNeeded)
          {
            CheckAuthentication();
          }
          else if (resultClass != GeniWebResultType.FailedRetrySimple)
          {
            Thread.Sleep(1000);
          }
        }
        catch (System.IO.IOException e)
        {
          stats.GetIndividual.failureRetry++;
          trace.TraceData(TraceEventType.Warning, 0, requestDescription + " " + sURL + " FAILURE " + retryCount + "/" + numberOfRetries);
          stats.GetIndividual.Print();
          trace.TraceData(TraceEventType.Warning, 0, "IOException: " + e.ToString());
          failure = true;
        }
      } while (failure && (retryCount++ < numberOfRetries));

      if (returnLine == null)
      {
        trace.TraceData(TraceEventType.Error, 0, requestDescription + " Failed to receive any valid response from the server despite " + numberOfRetries + " retries!");
        return null;
      }

      if ((returnLine.StartsWith("<!DOCTYPE") || returnLine.StartsWith("<HTML") || returnLine.StartsWith("<html")))
      {
        trace.TraceData(TraceEventType.Warning, 0, requestDescription + ":Bad response format. Don't parse.");
        trace.TraceData(TraceEventType.Warning, 0, "**********************************************************-start");
        trace.TraceData(TraceEventType.Warning, 0, "{0}:{1}", returnLine.Length, returnLine);
        trace.TraceData(TraceEventType.Warning, 0, "**********************************************************-end");
        CheckAuthentication();
        return null;
      }
      if (trace.Switch.Level.HasFlag(SourceLevels.Information))
      {
        trace.TraceInformation("**********************************************************-start:");
        trace.TraceInformation("{0}:{1}", returnLine.Length, returnLine);
        trace.TraceInformation("**********************************************************-end:");
      }

      return returnLine;
    }

    string FetchRootPerson()
    {
      string sLine = null;
      DateTime startTime = DateTime.Now;

      trace.TraceInformation("FetchRootPerson()");

      sLine = GetWebData("https://www.geni.com/api/user/max-family", null, "FetchRootPerson()", 5);

      if (sLine != null)
      {
        HttpMaxFamilyResponse maxFamilyResponse = serializer.Deserialize<HttpMaxFamilyResponse>(sLine);

        foreach (HttpPerson person in maxFamilyResponse.results)
        {
          if (person.id != null)
          {
            trace.TraceInformation("FetchRootPerson() = " + person.id + " " + (DateTime.Now - startTime) + "s");
            return person.id; 
          }
        }
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "FetchRootPerson() FAILED due to server problems (no data returned)! " + " " + (DateTime.Now - startTime) + "s");
      }
      return null;
    }

    private IndividualClass TransformRecordToIndividual(DataRow personRow)
    {
      return null;
    }
    public void SetHomeIndividual(String xrefName)
    {
      homePerson = xrefName;
    }
    public string GetHomeIndividual()
    {
      return homePerson;
    }

    public void AddFamily(FamilyClass tempFamily)
    {
    }

    enum GeniWebResultType
    {
      Ok,
      OkTooFast,
      FailedRetry,
      FailedRetrySimple,
      FailedReauthenticationNeeded
    }
    private GeniWebResultType ClassifyWebResponse(WebResponse response)
    {
      if (response != null)
      {
        if (response.Headers["X-API-Rate-Remaining"] != null)
        {
          if (Convert.ToInt32(response.Headers["X-API-Rate-Remaining"]) < 5)
          {
            return GeniWebResultType.OkTooFast;
          }
        }
      }
      return GeniWebResultType.Ok;
    }
    private GeniWebResultType ClassifyErrorWebResponse(WebResponse response, string request)
    {
      GeniWebResultType result = GeniWebResultType.Ok;
      if (response != null)
      {
        trace.TraceData(TraceEventType.Information, 0, "Exception url: " + response.ResponseUri);
        if (response.Headers["Status"] != null)
        {
          string rspStatus = response.Headers["Status"].ToString();
          if (rspStatus.IndexOf("403") >= 0)
          {
            result = GeniWebResultType.FailedRetrySimple;
          }
          else if (rspStatus.IndexOf("404") >= 0)
          {
            result = GeniWebResultType.FailedRetrySimple;
          }
          else if (rspStatus.IndexOf("401") >= 0)
          {
            result = GeniWebResultType.FailedReauthenticationNeeded;
          }
          else
          {
            result = GeniWebResultType.FailedRetry;
          }
          trace.TraceData(TraceEventType.Warning, 0, request + " failed with Cause:" + rspStatus + " result:" + result);
        }
        else
        {
          result = GeniWebResultType.FailedRetry;
          trace.TraceData(TraceEventType.Warning, 0, request + " failed with status=null: result:" + result);
        }
      }
      else
      {
        result = GeniWebResultType.FailedRetry;
        trace.TraceData(TraceEventType.Warning, 0, request + " failed with response=null: result:" + result);
      }
      return result;
    }


    public FamilyClass GetFamily(String familyXrefName)
    {
      stats.GetFamily.attempt++;
      DateTime startTime = DateTime.Now;
      if (familyXrefName == null)
      {
        trace.TraceInformation("GetFamily(null) = ");
        stats.GetFamily.failure++;
        stats.GetFamily.Print();
        return null;
      }
      if (familyXrefName.IndexOf("union-") < 0)
      {
        trace.TraceInformation("Warning: strange xref in GetFamily" + familyXrefName);
      }
      if (cache.CheckFamily(familyXrefName))
      {
        trace.TraceInformation("GetFamily(" + familyXrefName + ") cached");
        return cache.GetFamily(familyXrefName);
      }
      trace.TraceInformation("GetFamily(" + familyXrefName + ") start " + startTime);
      //IndividualClass individual = null;

      string sLine = GetWebData("https://www.geni.com/api/" + familyXrefName, null, "GetFamily " + familyXrefName, 5);
      if (sLine != null)
      {
        HttpFamilyResponse familyResponse = serializer.Deserialize<HttpFamilyResponse>(sLine);

        if (familyResponse.id != null)
        {
          FamilyClass family = new FamilyClass();
          // Ignore "union-"
          family.SetXrefName(familyResponse.id);
          if (familyResponse.marriage != null)
          {
            FamilyDateTimeClass date = null;
            if (familyResponse.marriage.date != null)
            {
              date = new FamilyDateTimeClass(familyResponse.marriage.date.year, familyResponse.marriage.date.month, familyResponse.marriage.date.day);
            }
            family.AddEvent(new IndividualEventClass(IndividualEventClass.EventType.FamMarriage, date));
          }
          if (familyResponse.divorce != null)
          {
            FamilyDateTimeClass date = null;
            if (familyResponse.divorce.date != null)
            {
              date = new FamilyDateTimeClass(familyResponse.divorce.date.year, familyResponse.divorce.date.month, familyResponse.divorce.date.day);
            }
            family.AddEvent(new IndividualEventClass(IndividualEventClass.EventType.FamDivorce, date));
          }
          if (familyResponse.partners != null)
          {
            foreach (string partner in familyResponse.partners)
            {
              // ignore "profile-" = 8 characters
              int startPos = partner.IndexOf("profile-");

              family.AddRelation(new IndividualXrefClass(partner.Substring(startPos)), FamilyClass.RelationType.Parent);
            }
          }
          if (familyResponse.children != null)
          {
            foreach (string child in familyResponse.children)
            {
              int startPos = child.IndexOf("profile-");
              // ignore "profile-" = 8 characters
              family.AddRelation(new IndividualXrefClass(child.Substring(startPos)), FamilyClass.RelationType.Child);
            }
          }
          if (family.GetXrefName() != "")
          {
            trace.TraceInformation("GetFamily(" + familyXrefName + ") done " + DateTime.Now);

            cache.AddFamily(family);

            stats.GetFamily.fetchSuccess++;

            if (familyXrefName != family.GetXrefName())
            {
              trace.TraceData(TraceEventType.Error, 0, "GetFamily() Error:wrong family returned:" + familyXrefName + "!=" + family.GetXrefName());
              trace.TraceData(TraceEventType.Error, 0, "Request:" + "https://www.geni.com/api/" + familyXrefName);
              trace.TraceData(TraceEventType.Error, 0, "response:" + sLine);
            }

            return family;
          }
        }
        else
        {
          trace.TraceEvent(TraceEventType.Error, 0, "GetFamily() FAILURE  (no data returned) no data in result:" + sLine);
        }

      }
      else
      {
        trace.TraceEvent(TraceEventType.Error, 0, "GetFamily() FAILURE: no data returned from server:" + sLine);
      }
      //Console.ReadLine();

      trace.TraceInformation("GetFamily(" + familyXrefName + ") = null" + DateTime.Now);
      stats.GetFamily.failure++;
      stats.GetFamily.Print();

      TimeSpan deltaTime = DateTime.Now - startTime;

      if (deltaTime > stats.GetFamily.slowestFetch)
      {
        stats.GetFamily.slowestFetch = deltaTime;
        trace.TraceInformation("GetFamily() slowest " + DateTime.Now + " " + deltaTime);
        stats.GetFamily.Print();
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

    private string GetUrlToken(string url, string token)
    {
      if (url != null)
      {
        //trace.TraceInformation("FindUrlToken:" + url + " " + token);

        int tokenPos = url.IndexOf(token);
        if (tokenPos >= 0)
        {
          string tStr = url.Substring(tokenPos + token.Length);

          int endPos = tStr.IndexOf('&');
          if (endPos >= 0)
          {
            return tStr.Substring(0, endPos);
          }
          return tStr;
        }
        trace.TraceInformation("GetUrlToken(" + url + " " + token + ") not found!");
        return null;
      }
      //trace.TraceInformation("FindUrlToken:null " + token);
      trace.TraceInformation("GetUrlToken(null," + token + ") not found!");
      return null;

    }


    public void wbrowser_AuthenticationEvent(object sender, FamilyAuthenticationEvent e)
    {
      trace.TraceInformation("wbrowser_AuthenticationEvent-1:" + e.url);

      string newAuthToken = GetUrlToken(e.url, "access_token=");

      if (newAuthToken != null)
      {
        string newExpiryTime = GetUrlToken(e.url, "expires_in=");

        if (newExpiryTime != null)
        {
          if (!userAuthentication.SetAuthentication(newAuthToken, newExpiryTime))
          {
            string result = GetWebData("https://www.geni.com/platform/oauth/invalidate_token?access_token=" + newAuthToken, null, "reset token", 5);
          }
        }
        trace.TraceInformation("wbrowser_AuthenticationEvent authenticated:" + newAuthToken);

        if (authenticationWebBrowser != null)
        {
          authenticationWebBrowser.Hide();
          authenticationWebBrowser = null;
        }
        string rootPerson = FetchRootPerson();

        if((rootPerson != null) && (rootPerson.Length > 0))
        {
          Debug.WriteLine("root = " + rootPerson);
          homePerson = rootPerson;

        }
        if (this.completedCallback != null)
        {
          this.completedCallback(true);
          this.completedCallback = null;
        }

      }
      else
      {
        newAuthToken = GetUrlToken(e.url, "canceled");
        if (newAuthToken != null)
        {
          trace.TraceData(TraceEventType.Information, 0, "UserAuthenticationEvent canceled:" + e.url);

          userAuthentication.Cancel();
          authenticationWebBrowser.Hide();
          authenticationWebBrowser = null;
          this.completedCallback(false);
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "UserAuthenticationEvent unexpected event...:" + e.url);
        }

      }
    }

    public bool CallbackArmed()
    {
      return completedCallback != null;
    }

    private void OpenAuthenticationWindow()
    {
      trace.TraceInformation("OpenAuthenticationWindow()=" + DateTime.Now);

      if (authenticationWebBrowser != null)
      {
        trace.TraceInformation("Authentication window already opened.");
        return;
      }
      CreateBrowser();

      string sURL = "https://www.geni.com/platform/oauth/authorize?client_id=88&response_type=token&display=desktop";
      trace.TraceInformation("OpenAuthenticationWindow() url " + sURL + " at " + DateTime.Now.ToString());
      authenticationWebBrowser.Navigate(sURL);

      authenticationWebBrowser.Show();
    }

    private void AuthenticateUser()
    {
      ApartmentState threadState = Thread.CurrentThread.GetApartmentState();

      if (uiContext == null)
      {
        uiContext = SynchronizationContext.Current;
        trace.TraceInformation("New context : this thread:" + Thread.CurrentThread.ManagedThreadId + " ctx " + uiContext.ToString());
      }

      if (threadState == ApartmentState.STA)
      {
        OpenAuthenticationWindow();
      }
      else
      {
        trace.TraceInformation("Starting new authentication thread!!.." + threadState);
        uiContext.Post(unusedArg => OpenAuthenticationWindow(), null);

        do
        {
          trace.TraceInformation("Waiting for authentication..");
          Thread.Sleep(10000);
        }
        while (!userAuthentication.IsValid());
        trace.TraceInformation("Authentication Done!");
      }

      trace.TraceInformation("AuthenticateUser() " + DateTime.Now);

    }

    private void AuthenticateApp()
    {
      DateTime startTime = DateTime.Now;
      string sLine = GetWebData("https://www.geni.com/platform/oauth/request_token?client_id=" + clientId + "&client_secret=" + clientSecret + "&grant_type=client_credentials", null, "AuthenticateApp()", 5);
      if (sLine != null)
      {
        HttpAuthenticateResponse authenticationResponse = serializer.Deserialize<HttpAuthenticateResponse>(sLine);

        if (authenticationResponse.access_token != null)
        {
          appAuthentication.UpdateAuthenticationData(authenticationResponse);
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "AuthenticateApp() failed!");
        }
        trace.TraceInformation("AuthenticateApp() Done " + " " + (DateTime.Now - startTime) + "s");
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "AuthenticateApp() FAILED due to server problems (no data returned)!" + " " + (DateTime.Now - startTime) + "s");
      }

    }
    private void GetTreeStats()
    {
      DateTime startTime = DateTime.Now;
      trace.TraceInformation("GetTreeStats() " + " " + DateTime.Now);

      string sLine = GetWebData("https://www.geni.com/api/stats/world-family-tree", null, "GetTreeStats()", 5);

      if (sLine != null)
      {
        geniTreeSize = serializer.Deserialize<HttpGeniTreeSize>(sLine);

        trace.TraceInformation("GetTreeStats() OK in " + (DateTime.Now - startTime) + "s");
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "GetTreeStats() FAILED due to server problems (no data returned)!" + " " + (DateTime.Now - startTime) + "s");
      }
    }

    AddressClass TranslateLocation(HttpLocation location)
    {
      AddressClass address = new AddressClass();

      if (location.place_name != null)
      {
        address.AddAddressPart(AddressPartClass.AddressPartType.Line1, location.place_name);
      }
      if (location.city != null)
      {
        address.AddAddressPart(AddressPartClass.AddressPartType.City, location.city);
      }
      if (location.county != null)
      {
        address.AddAddressPart(AddressPartClass.AddressPartType.State, location.county);
      }
      if (location.country != null)
      {
        address.AddAddressPart(AddressPartClass.AddressPartType.Country, location.country);
      }
      if (address.GetAddressPart(AddressPartClass.AddressPartType.Country) != null)
      {
        if (location.country_code != null)
        {
          address.AddAddressPart(AddressPartClass.AddressPartType.Country, location.country_code);
        }
      }
      if (location.state != null)
      {
        address.AddAddressPart(AddressPartClass.AddressPartType.State, location.state);
      }
      return address;
    }


    IndividualEventClass TranslateEvent(HttpEvent httpEv, IndividualEventClass.EventType type)
    {
      IndividualEventClass ev = new IndividualEventClass(type);

      if (httpEv.date != null)
      {
        FamilyDateTimeClass date = new FamilyDateTimeClass(
          httpEv.date.year,
          httpEv.date.month,
          httpEv.date.day);
        if (httpEv.date.circa)
        {
          date.SetApproximate(true);
        }
        ev.SetDate(date);
      }
      if (httpEv.location != null)
      {
        ev.AddAddress(TranslateLocation(httpEv.location));
      }
      return ev;
    }


    public IndividualClass DecodeIndividual(HttpPerson person)
    {
      if (person != null)
      {
        IndividualClass individual = new IndividualClass();
        PersonalNameClass name = new PersonalNameClass();

        individual.SetPublic(person.@public);
        if (person.id != null)
        {
          individual.SetXrefName(person.id);
        }
        if (person.first_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.GivenName, person.first_name);
        }
        if (person.middle_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.MiddleName, person.middle_name);
        }
        if (person.last_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.Surname, person.last_name);
        }
        if (person.maiden_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.BirthSurname, person.maiden_name);
        }
        if (person.display_name != null)
        {
          name.SetName(PersonalNameClass.PartialNameType.PublicName, person.display_name);
        }
        if (person.name != null)
        {
          if (name.GetName().Length < 2)
          {
            name.SetName(PersonalNameClass.PartialNameType.PublicName, person.name);
          }
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
        individual.SetIsAlive(person.is_alive);

        if (person.birth != null)
        {
          individual.AddEvent(TranslateEvent(person.birth, IndividualEventClass.EventType.Birth));
        }
        if (person.baptism != null)
        {
          individual.AddEvent(TranslateEvent(person.baptism, IndividualEventClass.EventType.Baptism));
        }

        if (person.death != null)
        {
          individual.AddEvent(TranslateEvent(person.death, IndividualEventClass.EventType.Death));
        }
        if (person.burial != null)
        {
          individual.AddEvent(TranslateEvent(person.burial, IndividualEventClass.EventType.Burial));
        }

        if (person.about_me != null)
        {
          individual.AddNote(new NoteClass(person.about_me.Replace("\n", "\r\n")));
        }
        if (person.current_residence != null)
        {
          individual.AddAddress(TranslateLocation(person.current_residence));
        }
        if(person.profile_url != null)
        {
          individual.AddUrl(person.profile_url);
        }
        if (person.mugshot_urls != null)
        {
          if (person.mugshot_urls.large != null)
          {
            if (person.mugshot_urls.large.IndexOf(".jpg") >= 0)
            {
              individual.AddMultimediaLink(new MultimediaLinkClass("image/jpeg", person.mugshot_urls.large));
            }
          }
          if (person.mugshot_urls.medium != null)
          {
            if (person.mugshot_urls.medium.IndexOf(".jpg") >= 0)
            {
              individual.AddMultimediaLink(new MultimediaLinkClass("image/jpeg", person.mugshot_urls.medium));
            }
          }
          if (person.mugshot_urls.thumb != null)
          {
            if (person.mugshot_urls.thumb.IndexOf(".jpg") >= 0)
            {
              individual.AddMultimediaLink(new MultimediaLinkClass("image/jpeg", person.mugshot_urls.thumb));
            }
          }
          if (person.mugshot_urls.thumb2 != null)
          {
            if (person.mugshot_urls.thumb2.IndexOf(".jpg") >= 0)
            {
              individual.AddMultimediaLink(new MultimediaLinkClass("image/jpeg", person.mugshot_urls.thumb2));
            }
          }
          if (person.mugshot_urls.print != null)
          {
            if (person.mugshot_urls.print.IndexOf(".jpg") >= 0)
            {
              individual.AddMultimediaLink(new MultimediaLinkClass("image/jpeg", person.mugshot_urls.print));
            }
          }
          if (person.mugshot_urls.url != null)
          {
            individual.AddMultimediaLink(new MultimediaLinkClass("text/html", person.mugshot_urls.url));
          }
        }
        if (!UpdateRelationsFromEdges(person.edges, ref individual))
        {
          trace.TraceInformation(" relations " + individual.GetXrefName() + " no relation updates..");
        }

        return individual;
      }

      return null;
    }

    bool UpdateRelations(IDictionary<string, HttpPerson> personList, ref IndividualClass individual)
    {
      bool updated = false;

      foreach (KeyValuePair<string, HttpPerson> nodePersonPair in personList)
      {
        if ((nodePersonPair.Key.IndexOf("profile-") == 0) && (nodePersonPair.Key == individual.GetXrefName()))
        {
          if (nodePersonPair.Value != null)
          {
            HttpPerson nodePerson = (HttpPerson)nodePersonPair.Value;

            if (!UpdateRelationsFromEdges(nodePerson.edges, ref individual))
            {
              trace.TraceInformation(" Added " + individual.GetXrefName() + " no relation updates..");
            }
          }
        }
      }
      return updated;
    }

    bool UpdateRelationsFromEdges(IDictionary<string, HttpUnionRelation> edgeList, ref IndividualClass individual)
    {
      bool updated = false;

      if (edgeList != null)
      {
        foreach (KeyValuePair<string, HttpUnionRelation> edgePair in edgeList)
        {
          if (edgePair.Value != null)
          {
            HttpUnionRelation union = edgePair.Value;

            if (union.rel == "child")
            {
              individual.AddRelation(new FamilyXrefClass(edgePair.Key), IndividualClass.RelationType.Child);
              updated = true;
            }
            else if (union.rel == "partner")
            {
              individual.AddRelation(new FamilyXrefClass(edgePair.Key), IndividualClass.RelationType.Spouse);
              updated = true;
            }
          }
        }
      }
      if (!updated)
      {
        if (trace.Switch.Level.HasFlag(SourceLevels.Information))
        {
          if (edgeList == null)
          {
            trace.TraceInformation("edgelist = null");
          }
          else
          {
            trace.TraceInformation("edgelist = " + edgeList + " " + edgeList.Count);
          }
        }
      }
      return updated;
    }



    public IndividualClass GetIndividual(String xrefName, uint index = (uint)SelectIndex.NoIndex, PersonDetail detailLevel = PersonDetail.PersonDetail_All)
    {
      DateTime startTime = DateTime.Now;
      stats.GetIndividual.attempt++;

      if (xrefName != null)
      {
        if (xrefName.IndexOf("profile-") < 0)
        {
          trace.TraceData(TraceEventType.Warning, 0, "Warning: strange xref name in GetIndividual" + xrefName);
        }
        if (cache.CheckIndividual(xrefName))
        {
          trace.TraceInformation("GetIndividual(" + xrefName + ") cached");
          stats.GetIndividual.cacheSuccess++;
          IndividualClass person = cache.GetIndividual(xrefName);

          if(person.GetXrefName() != xrefName)
          {
            trace.TraceData(TraceEventType.Error, 0, "Wrong person in cache!" + xrefName + " != " + person.GetXrefName());
          }
          return person;
        }
      }
      trace.TraceInformation("GetIndividual(" + xrefName + ") start " + DateTime.Now);

      if (xrefName == null)
      {
        trace.TraceInformation("GetIndividual(null==root) : not allowed on geni");
        return null;
      }

      string sLine;
      string getPersonUrl;
      if (userAuthentication.IsValid())
      {
        getPersonUrl = "https://www.geni.com/api/" + xrefName + "/immediate-family?only_ids=true&fields=first_name,middle_name,nicknames,last_name,maiden_name,name,suffix,occupation,gender,birth,baptism,death,burial,cause_of_death,unions,id,about_me,is_alive,profile_url,mugshot_urls,public";
        sLine = GetWebData(getPersonUrl, "https://www.geni.com/api/" + xrefName, "GetIndividual()" + xrefName, 5);
      }
      else
      {
        getPersonUrl = "https://www.geni.com/api/" + xrefName;
        sLine = GetWebData(getPersonUrl, null, "GetIndividual-simple()" + xrefName, 5);
      }


      if (sLine != null)
      {
        IndividualClass focusPerson = null;

        if (sLine.IndexOf("{\"focus\"") == 0)
        {
          HttpGetIndividualResult getIndividualResult = serializer.Deserialize<HttpGetIndividualResult>(sLine);
          if (getIndividualResult.focus != null)
          {
            if (getIndividualResult.focus.id != xrefName)
            {
              trace.TraceData(TraceEventType.Error, 0, "getIndividualResult.focus.id != xrefName" + getIndividualResult.focus.id + " " + xrefName + " "+ getPersonUrl);
            }

            foreach (KeyValuePair<string, HttpPerson> nodePersonPair in getIndividualResult.nodes)
            {
              if (nodePersonPair.Key.IndexOf("profile-") == 0)
              {
                if (!cache.CheckIndividual(nodePersonPair.Key))
                {
                  if (nodePersonPair.Value != null)
                  {
                    HttpPerson nodePerson = (HttpPerson)nodePersonPair.Value;

                    IndividualClass nodeIndividual = DecodeIndividual(nodePerson);

                    if (nodeIndividual != null)
                    {
                      cache.AddIndividual(nodeIndividual);
                    }
                  }
                }
                else
                {
                  trace.TraceInformation(" Person " + nodePersonPair.Key + " skipped, already cached");
                }
              }
              else if (nodePersonPair.Key.IndexOf("union-") == 0)
              {
                FamilyClass family = new FamilyClass();

                family.SetXrefName(nodePersonPair.Key);

                if (!cache.CheckFamily(family.GetXrefName()))
                {
                  if (nodePersonPair.Value != null)
                  {
                    HttpPerson nodePersonUnion = (HttpPerson)nodePersonPair.Value;

                    foreach (KeyValuePair<string, HttpUnionRelation> edgePair in nodePersonUnion.edges)
                    {
                      if (edgePair.Value != null)
                      {
                        HttpUnionRelation union = edgePair.Value;

                        if (union.rel == "child")
                        {
                          family.AddRelation(new IndividualXrefClass(edgePair.Key), FamilyClass.RelationType.Child);
                          trace.TraceInformation("  added child " + edgePair.Key);
                        }
                        else if (union.rel == "partner")
                        {
                          family.AddRelation(new IndividualXrefClass(edgePair.Key), FamilyClass.RelationType.Parent);
                          trace.TraceInformation("  added partner " + edgePair.Key);
                        }
                        // cache
                      }
                    }
                  }
                  cache.AddFamily(family);
                }
              }
            }
            if(!cache.CheckIndividual(xrefName))
            {
              if (getIndividualResult.focus != null)
              {
                trace.TraceData(TraceEventType.Warning, 0, "Warning!! focus person " + focusPerson.GetXrefName() + "not in nodes!!");
                trace.TraceData(TraceEventType.Error, 0, "request!" + getPersonUrl);
                trace.TraceData(TraceEventType.Error, 0, "returned!" + sLine);
                if (!cache.CheckIndividual(getIndividualResult.focus.id))
                {
                  focusPerson = DecodeIndividual(getIndividualResult.focus);
                  trace.TraceData(TraceEventType.Information, 0, "Setting focus person !" + xrefName + ", " + focusPerson.GetXrefName());

                  // For some reason there is no "edges" section within "focus"...
                  if (!UpdateRelations(getIndividualResult.nodes, ref focusPerson))
                  {
                    trace.TraceInformation("focusperson added " + focusPerson.GetXrefName() + " no relation updates..");
                  }
                  cache.AddIndividual(focusPerson);
                }
              }

            }
          }
        }
        else
        {
          HttpPerson individualResult = serializer.Deserialize<HttpPerson>(sLine);
          if (individualResult != null)
          {
            cache.AddIndividual(DecodeIndividual(individualResult));
          }
        }
        if (cache.CheckIndividual(xrefName))
        {
          focusPerson = cache.GetIndividual(xrefName);
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "Error: Requesed person " + xrefName + " not found!");
        }

        if (focusPerson.GetXrefName() != xrefName)
        {
          trace.TraceData(TraceEventType.Error, 0, "Wrong person returned!" + xrefName + " != " + focusPerson.GetXrefName());
          trace.TraceData(TraceEventType.Error, 0, "request!" + getPersonUrl);
          trace.TraceData(TraceEventType.Error, 0, "returned!" + sLine);
        }
        trace.TraceInformation("GetIndividual() done " + DateTime.Now);
        stats.GetIndividual.fetchSuccess++;
        TimeSpan deltaTime = DateTime.Now - startTime;

        if (deltaTime > stats.GetIndividual.slowestFetch)
        {
          stats.GetIndividual.slowestFetch = deltaTime;
          trace.TraceInformation("GetIndividual() slowest " + DateTime.Now + " " + deltaTime);
          stats.GetIndividual.Print();

        }
        return focusPerson;
        
      }
      else
      {
        trace.TraceData(TraceEventType.Error, 0, "GetIndividual() FAILED! due to server problems (no data returned) in " + (DateTime.Now - startTime) + "s");
      }

      stats.GetIndividual.failure++;
      stats.GetIndividual.Print();
      return null;
    }

    public IEnumerator<IndividualClass> SearchPerson(String individualName, ProgressReporter progressReporter = null)
    {
      DateTime startTime = DateTime.Now;

      stats.SearchIndividual.attempt++;
      trace.TraceInformation("SearchPerson(" + individualName + ") " + startTime);
      if ((individualName == null) || (individualName.Length == 0))
      {
        IEnumerator<IndividualClass> personIterator = cache.GetIndividualIterator();

        if (personIterator != null)
        {
          IList<IndividualClass> personList = new List<IndividualClass>();
          while (personIterator.MoveNext())
          {
            personList.Add(personIterator.Current);
            stats.SearchIndividual.fetchSuccess++;
          }

          foreach(IndividualClass person in personList)
          {
            yield return person;
          }
          trace.TraceInformation("SearchPerson():done " + (DateTime.Now - startTime) + "s");
        }
      }
      else
      {
        CheckAuthentication();

        string searchPersonUrl = "https://www.geni.com/api/profile/search?names=" + individualName + "&&fields=first_name,middle_name,nicknames,last_name,maiden_name,name,suffix,occupation,gender,birth,baptism,death,burial,cause_of_death,id,about_me,is_alive,mugshot_urls,public";
        string sLine = GetWebData(searchPersonUrl, null, "SearchIndividual()" + individualName, 5);
        if (sLine != null)
        {
          HttpSearchPersonResult searchPersonResult = serializer.Deserialize<HttpSearchPersonResult>(sLine);

          if ((searchPersonResult != null) && (searchPersonResult.results != null))
          {
            foreach (HttpPerson person in searchPersonResult.results)
            {
              stats.SearchIndividual.fetchSuccess++;
              yield return DecodeIndividual(person);
            }
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "SearchPerson() FAILED  (no data returned) in  " + (DateTime.Now - startTime) + "s");
          yield return null;
        }
      }
    }

    private void UpdateIndividualCache()
    {
      IEnumerator<FamilyClass> familyIterator = cache.GetFamilyIterator();
      List<FamilyClass> familyList = new List<FamilyClass>();

      while (familyIterator.MoveNext())
      {
        familyList.Add(familyIterator.Current);
      }
      foreach (FamilyClass family in familyList)
      {
        IList<IndividualXrefClass> parents = family.GetParentList();

        if (parents != null)
        {
          foreach (IndividualXrefClass personXref in parents)
          {
            if (!cache.CheckIndividual(personXref.GetXrefName()))
            {
              trace.TraceInformation("UpdateIndividualCache() " + personXref.GetXrefName());
              IndividualClass person = GetIndividual(personXref.GetXrefName());
            }
          }
        }
        IList<IndividualXrefClass> children = family.GetChildList();

        if (children != null)
        {
          foreach (IndividualXrefClass personXref in children)
          {
            if (!cache.CheckIndividual(personXref.GetXrefName()))
            {
              trace.TraceInformation("UpdateIndividualCache() " + personXref.GetXrefName());
              IndividualClass person = GetIndividual(personXref.GetXrefName());
            }
          }
        }
      }

    }
    private void UpdateFamilyCache()
    {
      IEnumerator<IndividualClass> individualIterator = cache.GetIndividualIterator();
      List<IndividualClass> individualList = new List<IndividualClass>();

      while (individualIterator.MoveNext())
      {
        individualList.Add(individualIterator.Current);
      }
      foreach (IndividualClass person in individualList)
      {
        IList<FamilyXrefClass> childFamilies = person.GetFamilyChildList();

        if (childFamilies != null)
        {
          foreach (FamilyXrefClass familyXref in childFamilies)
          {
            if (!cache.CheckFamily(familyXref.GetXrefName()))
            {
              trace.TraceInformation("UpdateFamilyCache() " + familyXref.GetXrefName());
              FamilyClass family = GetFamily(familyXref.GetXrefName());
            }
          }
        }
        IList<FamilyXrefClass> spouseFamilies = person.GetFamilySpouseList();
        if (spouseFamilies != null)
        {
          foreach (FamilyXrefClass familyXref in spouseFamilies)
          {
            if (!cache.CheckFamily(familyXref.GetXrefName()))
            {
              trace.TraceInformation("UpdateFamilyCache() " + familyXref.GetXrefName());
              FamilyClass family = GetFamily(familyXref.GetXrefName());
            }
          }
        }
      }

    }
    private void UpdateCaches()
    {
      UpdateIndividualCache();
      UpdateFamilyCache();
    }

    public IEnumerator<FamilyClass> SearchFamily(String familyXrefName = null, ProgressReporter progressReporter = null)
    {
      if (familyXrefName == null)
      {
        IEnumerator<FamilyClass> familyIterator = cache.GetFamilyIterator();

        if (familyIterator != null)
        {
          List<FamilyClass> familyList = new List<FamilyClass>();
          while (familyIterator.MoveNext())
          {
            familyList.Add(familyIterator.Current);
          }
          foreach(FamilyClass family in familyList)
          {
            yield return family;
          }
          trace.TraceInformation("SearchFamily():end:");
        }
      }
    }

    public void AddMultimediaObject(MultimediaObjectClass tempMultimediaObject)
    {
    }

    public IEnumerator<MultimediaObjectClass> SearchMultimediaObject(String mmoString = null, ProgressReporter progressReporter = null)
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
    public IEnumerator<NoteClass> SearchNote(String noteString = null, ProgressReporter progressReporter = null)
    {
      return null;
    }

    public void AddRepository(RepositoryClass tempRepository)
    {
    }

    public IEnumerator<RepositoryClass> SearchRepository(String repositoryString = null, ProgressReporter progressReporter = null)
    {
      return null;
    }

    public void AddSource(SourceClass tempSource)
    {
    }

    public IEnumerator<SourceClass> SearchSource(String sourceString = null, ProgressReporter progressReporter = null)
    {
      return null;
    }

    public void AddSubmission(SubmissionClass tempSubmission)
    {
    }

    public IEnumerator<SubmissionClass> SearchSubmission(String submissionString = null, ProgressReporter progressReporter = null)
    {
      return null;
    }

    public void AddSubmitter(SubmitterClass tempSubmitter)
    {
    }

    public void SetSubmitterXref(SubmitterXrefClass tempSubmitterXref)
    {
    }
    public IEnumerator<SubmitterClass> SearchSubmitter(String noteString = null, ProgressReporter progressReporter = null)
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
      cache.Print();
    }
    public void PrintShort()
    {
    }

    public String GetShortTreeInfo()
    {
      if (geniTreeSize != null)
      {
        string cacheInfo = "; Cached " + cache.GetIndividualNo() + " individuals and " + cache.GetFamilyNo() + " families...";

        return "Web tree includes " + geniTreeSize.size + " people" + cacheInfo;
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

    public FamilyTreeContentClass GetContents()
    {
      FamilyTreeContentClass contents = new FamilyTreeContentClass();

      contents.families = cache.GetFamilyNo();
      contents.individuals = cache.GetIndividualNo();
      return contents;
    }

    public void SetFile(String fileName)
    {
      trace.TraceInformation("SetFile("+fileName+"):");
    }

  }
}
