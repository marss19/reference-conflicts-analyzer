# Reference Conflicts Analyzer - Visual Studio Extension

This is an extension to Visual Studio for easy visual analysis of the "Could not load file or assembly or one of its dependencies" problem and issues related to referenced assemblies. The tool allows selecting an .Net assembly (.ddl or .exe file) and get a graph of all referenced assemblies with hightlighted conflicting references. 

After installation it is available in the main menu: Tools -&gt; Analyze Assembly Dependencies.

Download the latest release from Visual Studio Marketplace: https://marketplace.visualstudio.com/vsgallery/051172f3-4b30-4bbc-8da6-d55f70402734

Documentation: http://www.marss.co.ua/2017/05/reference-conflicts-analyzer-visual.html

##### Screenshot 1. Example of output
![alt tag](https://github.com/marss19/reference-conflicts-analyzer/blob/master/Docs/Screenshots/output.png)


There is also a command line version of the analyzer. It is intended to be used on production servers without Visual Studio installed. It generates a DGML file which can be opened on a different machine where Visual Studio with DGML viewer is installed. 

Example of usage:
```
ReferenceConflictAnalyzer.CommandLine.exe -file="C:\Program Files\Some App\someapp.exe" -config="C:\Program Files\Some App\someapp.exe.config" -output="C:\temp"
```

More details on parameters:
```
ReferenceConflictAnalyzer.CommandLine.exe help
```
Download the latest release of the command line utility: https://github.com/marss19/reference-conflicts-analyzer/releases
