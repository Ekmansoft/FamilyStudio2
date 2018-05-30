# FamilyStudio2
Genealogy software based on dotnet and written in C#

# Introduction
FamilyStudio2 is a piece of software for handling your family tree research written for dotnet in C#.

The goal in the development has not been on the basic parts such as entering people or 
editing family relations, but on tools for analysing and comparing whole genealogy trees.

Thus here are a few features that are supported:
- Importing and exporting GEDCOM files from any source.
- Importing ANARKIV data files. (experimental)
- Connecting to the web tree Geni.com live, and performing the same operations on Geni.com as on local trees (read-only).
- Possibility to export cached data from geni.com as native or gedcom files.
- Saving and reading an internal file format (xml so far).
- Comparing whole trees from any source. Suspicious matches can easily be compared for easy manual transfer between the trees.
- Sanity check of any tree to a limited number of generations up to 19 generations, or an infinite number of generations. 
  Note that this can take quite some time if running against a web tree and many generations are selected.
- Checking for duplicate profiles in any tree. (This can take a long time especially against geni.com)

For a ready-built version, see http://endian.net/FamilyStudio/publish.htm

Todo: It doesn't have that great support for presenting trees in different forms, etc..

# Getting Started
TODO: Guide users through getting your code up and running on their own system. In this section you can talk about:
1.	Installation process

    - For using the geni.com interface, you need to create a geni app key at https://www.geni.com/platform/developer/apps
      This app key  includes an app id and an app secret that needs to be entered in the registry as strings at 
            HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio\\GeniAppId
            HKEY_CURRENT_USER\\Software\\endian.net\\FamilyStudio\\GeniAppSecret
      If you try to open a geni.com without this, it will ask you for the values. 

2.	Software dependencies
3.	Latest releases
4.	API references

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://www.visualstudio.com/en-us/docs/git/create-a-readme). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
