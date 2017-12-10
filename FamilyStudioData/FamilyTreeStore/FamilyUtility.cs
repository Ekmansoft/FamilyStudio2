using Microsoft.Win32;
using System;
//using System.Collections.Generic;
using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace FamilyStudioData.FamilyTreeStore
{
  public class FamilyUtility
  {
    private string currentDirectory;

    public FamilyUtility()
    {
      currentDirectory = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", "UserDirectory", null);

      if (currentDirectory == null)
      {
        Registry.SetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", (string)"UserDirectory", Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\FamilyStudio");

        currentDirectory = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio", "UserDirectory", "null");
      }
      if (!Directory.Exists(currentDirectory))
      {
        Directory.CreateDirectory(currentDirectory);
      }
    }

    public string GetCurrentDirectory()
    {
      return currentDirectory;
    }

  }
}
