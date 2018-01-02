using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.ComponentModel;
using FamilyStudioData.FamilyData;
//using FamilyStudioData.FamilyFileFormat;
using FamilyStudioData.FileFormats.GedcomCodec;
using FamilyStudioData.FamilyTreeStore;

namespace FamilyStudioData.FileFormats.GedcomCodec
{

  public class GedcomDecoder : FamilyFileTypeBaseClass
  {
    private static TraceSource trace = new TraceSource("GedcomDecoder", SourceLevels.Warning);
    private FamilyTreeStoreBaseClass familyTree;
    private BackgroundWorker backgroundWorker;
    private int parsedLines;
    private bool printMemory;
    private FileBufferClass fileBuffer;
    private GedcomLineStatus line;
    private GedcomFileCharacterSet characterSet;

    private MemoryClass memory;

    private GedcomParserUtility parser;

    public GedcomDecoder()
    {
      parser = new GedcomParserUtility();
    }

    void DebugStringAdd(ref GedcomImportResult importResult, string str)
    {
      /*if (importResult == null)
      {
        importResult = "";
      }*/
      //importResult += str + "\n";
      importResult.AddString(str);
      trace.TraceInformation(str);

    }


    private bool ReadFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree)
    {
      printMemory = false;// true;

      GedcomImportResult importResult = new GedcomImportResult();

      DebugStringAdd(ref importResult, "GedcomDecoder::Readfile(" + fileName + ") Start " + DateTime.Now);

      familyTree = inFamilyTree;

      familyTree.Print();

      if (printMemory)
      {
        memory = new MemoryClass();

        memory.PrintMemory();
      }


      fileBuffer = new FileBufferClass();

      fileBuffer.ReadFile(fileName);

      DebugStringAdd(ref importResult, "GedcomDecoder::Readfile() size " + fileBuffer.GetSize());
      if (printMemory)
      {
        memory.PrintMemory();
      }

      String HeadString = "";
      parsedLines = 0;

      if (fileBuffer.GetSize() < 12)
      {
        importResult = line.GetDebugString();
        DebugStringAdd(ref importResult, "gedcom file too small!: " + fileName + ", size:" + fileBuffer.GetSize());
        importResult.WriteToFile(fileName);
        return false;
      }

      Byte[] fileDataBuffer = fileBuffer.GetBuffer();

      //DebugStringAdd(ref importResult, "");

      string hdrStrText = "";
      string linefeed = "";
      bool linefeedFound = false;
      for (int i = 0; i < 20; i++)
      {
        char ch = (char)fileDataBuffer[i];
        //DebugStringAdd(ref importResult, fileDataBuffer[i].ToString());
        HeadString += ch;
        hdrStrText += ch.ToString() + " ";
        if (!linefeedFound)
        {
          if ((ch == '\n') || (ch == '\r'))
          {
            linefeed += ch;
          }
          else if (linefeed.Length > 0)
          {
            linefeedFound = true;
          }
        }
      }
      //DebugStringAdd(ref importResult, "");

      DebugStringAdd(ref importResult, "Header bytes: " + hdrStrText);

      if (!linefeedFound)
      {
        DebugStringAdd(ref importResult, " Did not find proper linefeed!: " + fileName + ", size:" + fileBuffer.GetSize() + ":" + HeadString);
        importResult.WriteToFile(fileName);
        return false;
      }

      line = new GedcomLineStatus(ref importResult, linefeed);

      if (HeadString.IndexOf("HEAD") < 0)
      {
        importResult = line.GetDebugString();
        DebugStringAdd(ref importResult, "gedcom file header missing!: " + fileName + ", size:" + fileBuffer.GetSize() + ":" + HeadString);
        importResult.WriteToFile(fileName);
        return false;
      }

      familyTree.SetSourceFileType("GEDCOM");

      if (printMemory)
      {
        memory.PrintMemory();
      }

      Parse(ref importResult);

      if (printMemory)
      {
        memory.PrintMemory();
      }
      familyTree.Print();
      DebugStringAdd(ref importResult, "GedcomDecoder::Readfile() Done " + DateTime.Now);

      importResult.WriteToFile(fileName);

      return true;

    }


    private bool CheckBomMark(ref GedcomParserProgress progress, ref GedcomImportResult importResult)
    {
      Byte[] fileDataBuffer = fileBuffer.GetBuffer();

      Byte[] UTF_8_BOM = { 0xEF, 0xBB, 0xBF };
      Byte[] UTF_16BE_BOM = { 0xFE, 0xFF };
      Byte[] UTF_16LE_BOM = { 0xFF, 0xFE };

      bool match;

      match = true;
      for (int i = 0; match  && (i < UTF_8_BOM.Length); i++)
      {
        if (UTF_8_BOM[i] != fileDataBuffer[i])
        {
          match = false;
        }
      }
      if (match)
      {
        importResult.AddString("BOM says UTF-8!");
        //trace.TraceInformation();
        SetCharacterSet(GedcomFileCharacterSet.Utf8);
        progress.position = UTF_8_BOM.Length;
        return true;
      }
      match = true;
      for (int i = 0; match && (i < UTF_16BE_BOM.Length); i++)
      {
        if (UTF_16BE_BOM[i] != fileDataBuffer[i])
        {
          match = false;
        }
      }
      if (match)
      {
        importResult.AddString("BOM says UTF-8!");
        //trace.TraceInformation("BOM says UTF-16-BE!");
        SetCharacterSet(GedcomFileCharacterSet.Utf16BE);
        progress.position = UTF_16BE_BOM.Length;
        return true;
      }

      match = true;
      for (int i = 0; match && (i < UTF_16LE_BOM.Length); i++)
      {
        if (UTF_16LE_BOM[i] != fileDataBuffer[i])
        {
          match = false;
        }
      }
      if (match)
      {
        importResult.AddString("BOM says UTF-16-LE!");
        //trace.TraceInformation("BOM says UTF-16-LE!");
        SetCharacterSet(GedcomFileCharacterSet.Utf16LE);
        progress.position = UTF_16LE_BOM.Length;
        return true;
      }
      importResult.AddString("BOM not found!");

      return false;
    }

    public void SetCharacterSet(GedcomFileCharacterSet inCharacterSet)
    {
      characterSet = inCharacterSet;
      line.SetCharacterSet(inCharacterSet);
    }




    private void Parse(ref GedcomImportResult importResult)
    {
      bool bomFound = false;
      GedcomLineObject rootLineObject = new GedcomLineObject(0);

      GedcomParserProgress progress = new GedcomParserProgress(fileBuffer.GetBuffer(), fileBuffer.GetSize());

      double lastPrintPercent = 0.0;
      double printPercent;
      GedcomLineObject currentLineObject = rootLineObject;
      GedcomLineData prevLineData = null;
      int lineDiff = 0;
      uint lineLength;
      //string importResult = "";
      bomFound = CheckBomMark(ref progress, ref importResult);

      GedcomTreeDecoderClass treeDecoder = new GedcomTreeDecoderClass(ref familyTree, ref importResult);

      treeDecoder.SetCharacterSet(characterSet);

      while(!progress.IsEndOfFile())
      {
        GedcomLineData lineData;

        line.SetDebugString(importResult);
        lineLength = line.ReadLine(ref progress);
        lineData = line.DecodeLine();
        importResult = line.GetDebugString();
        parsedLines++;

        if((lineData != null) && lineData.valid)
        {
          if (lineData.level == (currentLineObject.GetLevel() + 1))
          {
            GedcomLineObject subLineObject = new GedcomLineObject(lineData.level);

            subLineObject.parent = currentLineObject;

            if (prevLineData != null)
            {
              prevLineData.child = subLineObject;
            }

            currentLineObject = subLineObject;

          }
          /*else if (currentLineObject.GetLevel() == lineData.level)
          {
          }*/
          else if (lineData.level < currentLineObject.GetLevel())
          {
            bool decodeDone = false;
            printPercent = 100.0 * (double)progress.position / (double)progress.size;

            if ((printPercent - lastPrintPercent) > 0.10)
            {
              if (backgroundWorker != null)
              {
                backgroundWorker.ReportProgress((int)printPercent, "Working...");
              }
              lastPrintPercent = printPercent;

              if (trace.Switch.Level.HasFlag(SourceLevels.Information))
              {
                trace.TraceInformation("Decode position 1 " + progress.position + " (" + progress.size + ") " + DateTime.Now.ToString());
                trace.TraceInformation("Lines " + parsedLines + " (" + treeDecoder.GetDecodedLines() + ") " + printPercent.ToString("F") + "%" );
                familyTree.PrintShort();
              }

            }
            do
            {
              if (currentLineObject.parent != null)
              {
                currentLineObject = currentLineObject.parent;

                if ((currentLineObject.GetLevel() == 0) && (parsedLines > 0))
                {
                  treeDecoder.DecodeObject(currentLineObject);

                  currentLineObject.Clear();

                  if (treeDecoder.GetCharacterSet() != characterSet)
                  {
                    if (bomFound)
                    {
                      treeDecoder.DebugStringAdd("Warning! BOM and character set in Gedcom part mismatches! " + treeDecoder.GetCharacterSet() + "," + characterSet);
                    }
                    SetCharacterSet(treeDecoder.GetCharacterSet());
                  }
                }
              }
              else
              {

                if ((parsedLines - 1) - treeDecoder.GetDecodedLines() - 1 > lineDiff)
                {
                  treeDecoder.DebugStringAdd("Decode position " + progress.position + " (" + progress.size + ")");
                  treeDecoder.DebugStringAdd("Lines " + parsedLines + " (" + treeDecoder.GetDecodedLines() + "," + lineDiff + ")");
                  treeDecoder.DebugStringAdd("New undecoded lines: " + (parsedLines - treeDecoder.GetDecodedLines() - lineDiff) + "!");
                  lineDiff = parsedLines - treeDecoder.GetDecodedLines();
                }
                if (trace.Switch.Level.HasFlag(SourceLevels.Information))
                {
                  trace.TraceInformation("Decode position 2 " + progress.position + " (" + progress.size + ")");
                  trace.TraceInformation("Lines " + parsedLines + " (" + treeDecoder.GetDecodedLines() + ")");
                  familyTree.PrintShort();
                }

                currentLineObject.gedcomLines.Clear();
                decodeDone = true;
              }
            } while ((currentLineObject.GetLevel() > lineData.level) && !decodeDone);
          }

          prevLineData = lineData;

          currentLineObject.gedcomLines.Add(lineData);
        }
        else
        {
          if (lineData != null)
          {
            treeDecoder.DebugStringAdd("Line:" + lineData.lineNo + " Error bad gedcom line [" + lineData + "]");
          }
          else
          {
            treeDecoder.DebugStringAdd("Line: " + parsedLines + ": Error bad gedcom line:no data found ");
          }
        }

        if (progress.IsEndOfFile())
        {
          treeDecoder.DecodeObject(currentLineObject);
          if (treeDecoder.GetCharacterSet() != characterSet)
          {
            if (bomFound)
            {
              treeDecoder.DebugStringAdd("Warning! BOM and character set in Gedcom part mismatches! " + treeDecoder.GetCharacterSet() + "," + characterSet);
            }
            SetCharacterSet(treeDecoder.GetCharacterSet());
          }
          treeDecoder.DebugStringAdd("Line:" + lineData.lineNo + " end of file " + parsedLines);

          if (lineData.level != 0)
          {
            treeDecoder.DebugStringAdd("Line:" + lineData.lineNo + "Error: The Gedcom file did not end correctly! Was it inomplete? = " + currentLineObject.gedcomLines.Count);
          }
          if (!treeDecoder.DecodingCompleted())
          {
            treeDecoder.DebugStringAdd("Line:" + lineData.lineNo + "Error: The Gedcom file did not end correctly! No trailer detected = " + currentLineObject.gedcomLines.Count);
          }

          treeDecoder.ShowUnknownTags();
        }
      }
      backgroundWorker = null;
      treeDecoder.DebugStringAdd("Gedcom file parsing finished at " + currentLineObject.gedcomLines.Count);
      importResult = treeDecoder.GetImportResult();

    }

    public override bool IsKnownFileType(String fileName)
    {
      if (fileName.ToLower().Contains(".ged"))
      {
        return true;
      }
      return false;
    }

    public override FamilyTreeStoreBaseClass CreateFamilyTreeStore(String fileName, CompletedCallback callback)
    {
      trace.TraceInformation("GedcomDecoder::CreateFamilyTreeStore( " + fileName + ")");
      callback(true);
      return null;
    }

    public override bool OpenFile(String fileName, ref FamilyTreeStoreBaseClass inFamilyTree, CompletedCallback callback)
    {
      trace.TraceInformation("GedcomDecoder::OpenFile( " + fileName + ")");
      bool result = ReadFile(fileName, ref inFamilyTree);

      callback(result);
      return result;
    }
    public override bool SetProgressTarget(BackgroundWorker inBackgroundWorker)
    {
      trace.TraceInformation("GedcomCodec::SetProgressTarget 2");
      backgroundWorker = inBackgroundWorker;
      return true;
    }
    public override string GetFileTypeFilter(FamilyFileTypeOperation operation = FamilyFileTypeOperation.Open)
    {
      if (operation == FamilyFileTypeOperation.Import)
      {
        return "Gedcom|*.ged";
      }
      return null;
    }

  }

  public class GedcomImportResult
  {
    private IList<string> importResultList;

    public GedcomImportResult()
    {
      importResultList = new List<string>();
    }

    public void AddString(string str)
    {
      importResultList.Add(str);
    }

    public void WriteToFile(string filename)
    {
      using (StreamWriter writer = new StreamWriter(FamilyUtility.MakeFilename(filename + "_import_" + DateTime.Now.ToString() + ".txt")))
      {
        foreach (string str in importResultList)
        {
          writer.Write(str + "\r\n");
        }
      }
    }


  }

}
