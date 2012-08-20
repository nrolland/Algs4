﻿//This script generates 
//a file named __project.fsx, for each proejct which can be #load "__project.fsx" in script intending to use the same dependency graph as the code in VS
//a file named __solmerged.fsx, at the solution root which can be #load "__solmerged.fsx" in script intending to use the same dependency graph as the code in VS
//In both cases, this enforce that a script compiling in VS should work from within FSI 


#if INTERACTIVE
#r "System.Xml"
#r "System.Xml.Linq"
#endif

open System
open System.IO
open System.Xml.Linq

let rec findsolutiondir (p:DirectoryInfo) =  if (p.GetFiles("*.sln") |> Array.length > 0)  then p
                                             else findsolutiondir p.Parent

let root = findsolutiondir (DirectoryInfo(__SOURCE_DIRECTORY__))

type Dependency = 
   | GacLocation of String
   | ProjectFile of DirectoryInfo
   | DllFile of DirectoryInfo

let refsforaproject (dirproject:DirectoryInfo) =   [
         for fsProjFile in dirproject.GetFiles("*.fsproj") do
                 let getElemName name = XName.Get(name, "http://schemas.microsoft.com/developer/msbuild/2003")
                 let getElemValue name (parent:XElement) =
                     let elem = parent.Element(getElemName name)
                     if elem = null || String.IsNullOrEmpty elem.Value then None else Some(elem.Value)

                 let getAttrValue name (elem:XElement) =
                     let attr = elem.Attribute(XName.Get name)
                     if attr = null || String.IsNullOrEmpty attr.Value then None else Some(attr.Value)

                 let (|??) (option1: 'a Option) option2 =
                     if option1.IsSome then option1 else option2

                 let fsProjFile = dirproject.GetFiles("*.fsproj") |> Seq.head
                 let fsProjXml = XDocument.Load fsProjFile.FullName

                 let refspath = 
                     fsProjXml.Document.Descendants(getElemName "Reference")
                     |> Seq.choose (fun elem -> getElemValue "HintPath" elem)
                     |> Seq.map (fun ref ->  //if dirproject.Name.Contains("divsharp") then printfn "fulname : %A" ( DirectoryInfo(dirproject.FullName +  "\\" + ref).FullName)
                                             DllFile(DirectoryInfo(dirproject.FullName +  "\\" + ref)) )
                                             //("#r ", true, DirectoryInfo(dirproject.FullName +  "\\" + ref).FullName) )

                 let refsgac = 
                     fsProjXml.Document.Descendants(getElemName "Reference")
                     |> Seq.choose (fun elem -> if (getElemValue "HintPath" elem).IsNone then getAttrValue "Include" elem else None)
                     |> Seq.map (fun ref -> GacLocation(ref))

                 let fsFiles = 
                     fsProjXml.Document.Descendants(getElemName "Compile")
                     |> Seq.choose (fun elem -> //printfn "%A" elem
                                                getAttrValue "Include" elem)
                     |> Seq.map (fun fsFile -> ProjectFile( DirectoryInfo(dirproject.FullName +  "\\" + fsFile)))
                                               //#load ", true, DirectoryInfo(dirproject.FullName +  "\\" + fsFile).FullName))

                 let projDll = 
                     fsProjXml.Document.Descendants(getElemName "ProjectReference")
                     |> Seq.choose (fun elem -> getAttrValue "Include" elem)
                     |> Seq.map (fun projFile -> let refedPrjDir = DirectoryInfo(dirproject.FullName + "\\" + projFile).Parent
                                                 //("#r " ,  true, refedPrjDir.FullName +   "\\bin\\Debug\\" + refedPrjDir.Name + ".dll"))  //refedPrjDir.Name -> assembly name
                                                 DllFile(DirectoryInfo(refedPrjDir.FullName +   "\\bin\\Debug\\" + refedPrjDir.Name + ".dll")))

                 yield! refspath
                 yield! refsgac
                 yield! projDll
                 yield! fsFiles 
   ]

let toabsolute root rel = DirectoryInfo(root + rel).FullName
let f n = String.concat "." (Array.create (n+1) ".\\")

let writerelative root path = 
      let rec intwriterelative (root:DirectoryInfo) (path:DirectoryInfo) n = 
         if path.FullName.Contains(root.FullName + "\\" ) then f n + path.FullName.Remove(0,root.FullName.Length + 1)    //most common acestor = root
         else intwriterelative root.Parent path (n+1)
      intwriterelative root path 0

let getprojectdir (root:DirectoryInfo) = 
   let rec getdirs (root:DirectoryInfo) = seq {
      yield! root.GetDirectories() |> Array.filter(fun f -> f.GetFiles("*.fsproj") |> Array.length > 0 ) 
      yield! root.GetDirectories() |> Array.map(fun d -> getdirs d) |> Seq.concat}
   getdirs root   

let orderKey = function | GacLocation(n) -> "A" + n
                        | ProjectFile(n) -> "C" + n.FullName
                        | DllFile(n)     -> "B" + n.FullName

let tostrings rootwrite (dependencies) =
    dependencies |> Seq.distinct
                 |> Seq.map (fun dep  -> match dep with
                                         | GacLocation(n) -> "#r \"" + n + "\""
                                         | ProjectFile(n) -> "#load @\"" + writerelative rootwrite n + "\""
                                         | DllFile(n)     -> "#r @\"" + writerelative rootwrite n + "\"")

do 
   let projects = getprojectdir root |> Seq.map(fun p -> p, refsforaproject p)
   
   projects |> Seq.iter (fun (p,ds) ->  File.WriteAllLines(p.FullName+ "\\" + "__project.fsx", tostrings p (ds |> Seq.sortBy orderKey)))
   let s = projects |> Seq.map ( fun (p, dep) -> seq{ yield sprintf "//project %A" p.Name
                                                      yield!  dep |> Seq.sortBy orderKey |> tostrings root } )
                    |> Seq.concat

   File.WriteAllLines(root.FullName+ "\\" + "__solmerged.fsx",  s)