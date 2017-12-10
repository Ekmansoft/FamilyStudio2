using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
//using System.Threading.Tasks;

namespace FamilyStudioData.FamilyTreeStore
{
  class FamilyTreeCompareResults
  {
  }

  [DataContract]
  public class TreeItems
  {
    [DataMember]
    public string item1;
    [DataMember]
    public string item2;

    public TreeItems(string i1, string i2)
    {
      item1 = i1;
      item2 = i2;
    }
  }

  [DataContract]
  public class SavedMatches
  {
    [DataMember]
    public string database1, database2;
    [DataMember]
    public IList<TreeItems> itemList;

    public SavedMatches()
    {
      itemList = new List<TreeItems>();
    }

  }


}
