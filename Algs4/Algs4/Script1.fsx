#load "__project.fsx"


type Intunion (n:int) = 
   let entries = Array.init n id
   
   member x.IsConnected = ()

   member x.Union (p:int) (q:int) = 
      if max p q > n - 1 then
         failwithf "max capacity reached %A %A" (max p q) n
      else
         let pcomp = entries.[p]
         let qcomp = entries.[q]
         entries |> Seq.iteri(fun i v -> if v = pcomp then entries.[i] <- qcomp)
   override x.ToString() = 
      entries.ToString()


let c = Intunion(10)
c.Union 4 3
c.Union 3 8
c.Union 6 5 
c.Union 9 4
c.Union 2 1
c.Union 5 0
c.Union 7 2
c.Union 6 1
c
