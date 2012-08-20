module testss

open Xunit
open Swensen.Unquote
open Algs4.UnionStructure


[<Fact>]
let ``QuickUnion `` () =
   let c2 = QuickUnion(10)
   c2.Union 4 3
   c2.Union 3 8 // 0 1 2 8 35
   c2.Union 6 5 
   c2.Union 9 4
   c2.Union 2 1
   c2.Union 5 0
   c2.Union 7 2
   c2.Union 6 1

   test <@  c2.IsConnected 4 3 &&
            c2.IsConnected 3 9 &&
            c2.IsConnected 4 4 &&

            c2.IsConnected 6 7 &&
            c2.IsConnected 5 7 &&
            c2.IsConnected 0 2 &&

            c2.IsConnected 5 4 = false @>


[<Fact>]
let ``QuickFind `` () =
   let c2 = QuickFind(10)
   c2.Union 4 3
   c2.Union 3 8 // 0 1 2 8 35
   c2.Union 6 5 
   c2.Union 9 4
   c2.Union 2 1
   c2.Union 5 0
   c2.Union 7 2
   c2.Union 6 1

   test <@  c2.IsConnected 4 3 &&
            c2.IsConnected 3 9 &&
            c2.IsConnected 4 4 &&

            c2.IsConnected 6 7 &&
            c2.IsConnected 5 7 &&
            c2.IsConnected 0 2 &&

            c2.IsConnected 5 4  = false@>

