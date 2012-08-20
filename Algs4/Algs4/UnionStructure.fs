namespace Algs4.UnionStructure




type QuickUnion (n:int) = 
   let entries = Array.init n id
   
   member x.IsConnected p q = entries.[p] = entries.[q]

   member x.Union (p:int) (q:int) = 
      if max p q > n - 1 then
         failwithf "max capacity reached %A %A" (max p q) n
      else
         let pcomp = entries.[p]
         let qcomp = entries.[q]
         entries |> Seq.iteri(fun i v -> if v = pcomp then entries.[i] <- qcomp)
   override x.ToString() = 
      entries.ToString()



type QuickFind (n:int) = 
   let entries = Array.init n id
   
   let rec root p = 
         if entries.[p] = p then p
         else
            root entries.[p]

   member x.IsConnected p q =
      root p = root q 

   member x.Union (p:int) (q:int) = 
      if max p q > n - 1 then
         failwithf "max capacity reached %A %A" (max p q) n
      else
         entries.[root p] <- root q
   override x.ToString() = 
      entries.ToString()
