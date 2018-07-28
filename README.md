# CastleDB Unity Importer
*Requires Unity 2018 + Scripting Runtime 4.X*  
Define all your data in CastleDB and then directly access your data in code in Unity with full intellisense support!

## Why
*Read about this here*  
Unity doesn't provide an easy way to globally manage game data, so this plugin is an attempt to fix that.

## How it works  
By leveraging Unity's AssetImporter API, this plugin makes it so that Unity understands and recgonizes a CastleDB file (.cdb). Upon a .cdb file being imported or updated, this plugin generates (or regenerates) types from the sheets/rows/columns in your database.  

Because this does code generation, it's important to understand what is happening and structure your database accordingly. The main concepts employed are:
* A **sheet** is considered a **Type**
* A **column** in a sheet is considered the sheet's **fields**
* A **row** in a sheet is considered an **instance** of the sheet's Type.

## Limitations  
* A current annoyance is that if you add more columns to a given type, the solution in Unity will properly update but you're need to reload your solution in your editor. This bug manifests as previous mentions of your type in code saying "Can't be found". To fix this, just reload the assembly. In VSCode, this is as easy as just clicking your .sln file at the bottom of the window and reselecting it.
* Currently there is no validation of column names, so column names in CastleDB need to be valid field names in C#. I.E. don't use spaces in your column names, weird characters, etc.
* All sheets need to have some sort of GUID column that defines the name of a row. This defaults to "id" with a string type.


## TODO  
* Add in Color support
* Figure out a better guide for adding CastleDB Custom Trypes
* Document way to add in your own CustomType to match with a predefined Type in Unity
* Currently do not have file types or Image types implemented. This would require some preconfiguration steps that seem unique to every user so I'm not sure if it should be added.
* CastleDB also has a map/level creator that is not used at all here. Could be interesting to implement.

## Custom Types  

## References
[CastleDB Wedbsite](http://castledb.org/)
[Best CastleDB Tutorial/Walkthrough](https://translate.google.com/translate?sl=auto&tl=en&js=y&prev=_t&hl=en&ie=UTF-8&u=http%3A%2F%2Fhaxeflixel.2dgames.jp%2Findex.php%3FCastleDB%252FHaxe&edit-text=)